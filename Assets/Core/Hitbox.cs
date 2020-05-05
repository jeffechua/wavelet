using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;

public class Hitbox : RoomObjectBehaviour {

	public float thresholdOverride = -1;
	float threshold { get => thresholdOverride == -1 ? waveEngine.param.amplitudeThreshold : thresholdOverride; }
	public Vector2 radius; // secretly actually relative radius to width
	public Vector2Int pxRadius { get => Vector2Int.RoundToInt(Vector2.Scale(radius, transform.localScale) / waveEngine.param.pixelSize); }
	public Vector2Int pxDiameter { get => pxRadius * 2 + Vector2Int.one; }
	public bool computeDamage;
	public bool computeGradient;
	public float damageIntegralRaw; // area covered of intensity above threshold, in units of pixels
	public float damageDensity;     // fractional area with intensity above threshold
	public Vector2 gradientIntegralRaw; // area integral of wavefunction * position from rect center
	public Vector2 gradientRaw; // grad(wavefunction) in pixel space units
	public Vector2 gradient; // grad(wavefunction) in world space units

	AsyncGPUReadbackRequest? currentRequest;

	public void FixedUpdate() {
		if (!currentRequest.HasValue || currentRequest.Value.hasError || currentRequest.Value.done) {
			LaunchEvaluationRequest();
		}
	}

	private void OnDisable() {
		damageDensity = 0;
		damageIntegralRaw = 0;
		gradient = Vector2.zero;
		gradientRaw = Vector2.zero;
		gradientIntegralRaw = Vector2.zero;
	}

	void LaunchEvaluationRequest(System.Action callback = null) {

		if (!waveEngine)
			return;

		Vector2Int pxPosition = waveEngine.WorldToTexturePoint(transform.position);

		// Take the intersect of the capture rect and the actual readable texture rect
		RectInt captureRect = new RectInt(pxPosition - pxRadius, pxDiameter);
		RectInt boundingRect = new RectInt(0, 0, waveEngine.systemTexture.width, waveEngine.systemTexture.height);
		Vector2Int min = Vector2Int.Max(captureRect.min, boundingRect.min);
		Vector2Int max = Vector2Int.Min(captureRect.max, boundingRect.max);
		RectInt rect = new RectInt(min, max - min);

		// Read from that rect to avoid reading out of bounds
		if (rect.width <= 0 || rect.height <= 0) {
			damageIntegralRaw = 0;
		} else {
			currentRequest = AsyncGPUReadback.Request(waveEngine.systemTexture, 0, rect.xMin, rect.width, rect.yMin, rect.height, 0, 1,
				delegate (AsyncGPUReadbackRequest request) {
					if (this == null)
						return;
					PerformEvaluations(request.GetData<float>(), rect.size);
					callback?.Invoke();
				}
			);
		}

	}

	void PerformEvaluations(NativeArray<float> data, Vector2Int dims) {
		if (computeDamage) {
			damageIntegralRaw = data.Sum((f) => Mathf.Abs(f) * waveEngine.param.amplitudeScale > threshold ? 1 : 0);
			damageDensity = damageIntegralRaw / pxDiameter.x / pxDiameter.y;
		}
		if (computeGradient) {
			gradientIntegralRaw = Vector2.zero;
			for(int i=0; i<data.Length; i++) {
				int x = i % dims.x - dims.x/2;
				int y = i / dims.x - dims.y/2;
				gradientIntegralRaw += data[i] * new Vector2(x, y);
			}
			gradientRaw = gradientIntegralRaw / Mathf.Pow(pxDiameter.x*pxDiameter.y, 2) * 12;
			gradient = gradientRaw / waveEngine.param.pixelSize;
		}
	}

}
