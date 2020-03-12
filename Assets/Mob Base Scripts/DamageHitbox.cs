using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class DamageHitbox : MonoBehaviour {

	public float radius;

	public Texture2D snapshot;

	int lastUpdateTime;
	float _damageIntegral;
	public float damageIntegral {
		get {

			if (WaveEngine.instance.shaderSpace_t == lastUpdateTime)
				return _damageIntegral;
			lastUpdateTime = WaveEngine.instance.shaderSpace_t;

			Vector2Int tsPosition = Vector2Int.RoundToInt((transform.position - WaveEngine.instance.transform.position +
				WaveEngine.instance.transform.localScale / 2) / WaveEngine.instance.pixelSize);
			int tsRadius = Mathf.RoundToInt(radius / WaveEngine.instance.pixelSize);
			int tsDiameter = tsRadius * 2 + 1;

			if (snapshot == null || snapshot.height != tsRadius)
				snapshot = new Texture2D(tsDiameter, tsDiameter, TextureFormat.RFloat, false);

			RenderTexture.active = WaveEngine.instance.systemDisplayTexture;
			// Take the intersect of the capture rect and the actual readable texture rect
			Rect captureRect = new Rect(tsPosition - new Vector2(tsRadius, tsRadius), new Vector2(tsDiameter, tsDiameter));
			Rect boundingRect = new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height);
			Vector2 min = Vector2.Max(captureRect.min, boundingRect.min);
			Vector2 max = Vector2.Min(captureRect.max, boundingRect.max);
			Rect rect = new Rect(min, max-min);
			// Read from that rect to avoid reading out of bounds
			snapshot.ReadPixels(rect, 0, 0);
			Color[] pixels = snapshot.GetPixels(0, 0, (int)rect.width, (int)rect.height);
			RenderTexture.active = null;

			_damageIntegral = pixels.Aggregate<Color, float>(0, (sum, color) => sum + DamageFunction(color.r));

			return _damageIntegral;

		}
	}

	public float DamageFunction(float val) {
		if (Mathf.Abs(val) * WaveEngine.instance.amplitudeScale > WaveEngine.instance.amplitudeThreshold) {
			return 1;
		}
		return 0;
	}

}
