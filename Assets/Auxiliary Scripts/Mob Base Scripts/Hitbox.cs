using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;

public class Hitbox : RoomObjectBehaviour {

	public float radius; // secretly actually relative radius to width
	public int pxRadius { get => Mathf.RoundToInt(radius * transform.localScale.x / waveEngine.param.pixelSize); }
	public int pxDiameter { get => pxRadius * 2 + 1; }
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

	void LaunchEvaluationRequest(System.Action callback = null) {

		if (!waveEngine)
			return;

		Vector2Int pxPosition = Vector2Int.RoundToInt((transform.position - waveEngine.transform.position +
			waveEngine.transform.localScale / 2) / waveEngine.param.pixelSize);

		// Take the intersect of the capture rect and the actual readable texture rect
		RectInt captureRect = new RectInt(pxPosition - new Vector2Int(pxRadius, pxRadius), new Vector2Int(pxDiameter, pxDiameter));
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
			damageIntegralRaw = data.Sum((f) => Mathf.Abs(f) * waveEngine.param.amplitudeScale > waveEngine.param.amplitudeThreshold ? 1 : 0);
			damageDensity = damageIntegralRaw / pxDiameter / pxDiameter;
		}
		if (computeGradient) {
			gradientIntegralRaw = Vector2.zero;
			for(int i=0; i<data.Length; i++) {
				int x = i % dims.x - dims.x/2;
				int y = i / dims.x - dims.y/2;
				gradientIntegralRaw += data[i] * new Vector2(x, y);
			}
			gradientRaw = gradientIntegralRaw / Mathf.Pow(pxDiameter, 4) * 12;
			gradient = gradientRaw / waveEngine.param.pixelSize;
		}
	}

}
