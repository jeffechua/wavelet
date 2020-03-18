using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;

public class DamageHitbox : RoomObject {

	public float radius;

	public float damageIntegral;

	AsyncGPUReadbackRequest? currentRequest;

	public float DamageFunction(float val) {
		if (Mathf.Abs(val) * room.waveEngine.amplitudeScale > room.waveEngine.amplitudeThreshold) {
			return 1;
		}
		return 0;
	}

	public void FixedUpdate() {
		if(!currentRequest.HasValue || currentRequest.Value.hasError || currentRequest.Value.done) {
			LaunchEvaluationRequest();
		}
	}

	void LaunchEvaluationRequest (System.Action callback = null) {

		if (!room)
			return;

		Vector2Int tsPosition = Vector2Int.RoundToInt((transform.position - room.waveEngine.transform.position +
			room.waveEngine.transform.localScale / 2) / room.waveEngine.pixelSize);
		int tsRadius = Mathf.RoundToInt(radius / room.waveEngine.pixelSize);
		int tsDiameter = tsRadius * 2 + 1;

		// Take the intersect of the capture rect and the actual readable texture rect
		RectInt captureRect = new RectInt(tsPosition - new Vector2Int(tsRadius, tsRadius), new Vector2Int(tsDiameter, tsDiameter));
		RectInt boundingRect = new RectInt(0, 0, room.waveEngine.systemTexture.width, room.waveEngine.systemTexture.height);
		Vector2Int min = Vector2Int.Max(captureRect.min, boundingRect.min);
		Vector2Int max = Vector2Int.Min(captureRect.max, boundingRect.max);
		RectInt rect = new RectInt(min, max - min);

		// Read from that rect to avoid reading out of bounds
		if (rect.width <= 0 || rect.height <= 0) {
			damageIntegral = 0;
		} else {
			currentRequest = AsyncGPUReadback.Request(room.waveEngine.systemTexture, 0, rect.xMin, rect.width, rect.yMin, rect.height, 0, 1,
				delegate (AsyncGPUReadbackRequest request) {
					if (this == null)
						return;
					damageIntegral = request.GetData<float>().Sum(DamageFunction);
					callback?.Invoke();
				}
			);
		}

	}

}
