using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class DamageHitbox : RoomObject {

	public float radius;

	Texture2D snapshot;

	int lastUpdateTime;
	float _damageIntegral;
	public float damageIntegral {
		get {

			if (!room)
				return 0;

			if (room.waveEngine.shaderSpace_t == lastUpdateTime)
				return _damageIntegral;
			lastUpdateTime = room.waveEngine.shaderSpace_t;

			Vector2Int tsPosition = Vector2Int.RoundToInt((transform.position - room.waveEngine.transform.position +
				room.waveEngine.transform.localScale / 2) / room.waveEngine.pixelSize);
			int tsRadius = Mathf.RoundToInt(radius / room.waveEngine.pixelSize);
			int tsDiameter = tsRadius * 2 + 1;

			if (snapshot == null || snapshot.height != tsRadius)
				snapshot = new Texture2D(tsDiameter, tsDiameter, TextureFormat.RFloat, false);

			RenderTexture.active = room.waveEngine.systemDisplayTexture;
			// Take the intersect of the capture rect and the actual readable texture rect
			Rect captureRect = new Rect(tsPosition - new Vector2(tsRadius, tsRadius), new Vector2(tsDiameter, tsDiameter));
			Rect boundingRect = new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height);
			Vector2 min = Vector2.Max(captureRect.min, boundingRect.min);
			Vector2 max = Vector2.Min(captureRect.max, boundingRect.max);
			Rect rect = new Rect(min, max-min);
			// Read from that rect to avoid reading out of bounds
			if (rect.width < 0 || rect.height < 0) {
				_damageIntegral = 0;
			} else {
				snapshot.ReadPixels(rect, 0, 0);
				Color[] pixels = snapshot.GetPixels(0, 0, (int)rect.width, (int)rect.height);
				_damageIntegral = pixels.Aggregate<Color, float>(0, (sum, color) => sum + DamageFunction(color.r));
			}
			RenderTexture.active = null;
			return _damageIntegral;

		}
	}

	public float DamageFunction(float val) {
		if (Mathf.Abs(val) * room.waveEngine.amplitudeScale > room.waveEngine.amplitudeThreshold) {
			return 1;
		}
		return 0;
	}

}
