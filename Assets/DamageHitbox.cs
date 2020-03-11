using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class DamageHitbox : MonoBehaviour {

	public bool recomputeEachFrame;
	public float radius;
	public Vector2Int tsPosition;
	public int tsRadius;
	public float damageIntegral;
	Texture2D snapshot;

	static List<DamageHitbox> damageHitboxes = new List<DamageHitbox>();
	static int lastUpdateTime;

	void Start() {
		damageHitboxes.Add(this);
	}

	void OnDestroy() {
		damageHitboxes.Remove(this);
	}

	void Update() {
		if (recomputeEachFrame) {
			RecomputeAndGetDamageIntegralNonBatch();
		}
	}

	public float DamageFunction(float val) {
		if (Mathf.Abs(val) * WaveEngine.instance.amplitudeScale > WaveEngine.instance.amplitudeThreshold) {
			return 1;
		}
		return 0;
	}

	public void RecomputeDamageIntegral() {
		tsPosition = Vector2Int.RoundToInt((transform.position - WaveEngine.instance.transform.position + WaveEngine.instance.transform.localScale / 2) / WaveEngine.instance.pixelSize);
		tsRadius = Mathf.RoundToInt(radius / WaveEngine.instance.pixelSize);
		int tsDiameter = tsRadius * 2 + 1;
		if (snapshot == null || snapshot.height != tsRadius) snapshot = new Texture2D(tsRadius * 2 + 1, tsRadius * 2 + 1, TextureFormat.RFloat, false);
		snapshot.ReadPixels(new Rect((Vector2)tsPosition, new Vector2(tsDiameter, tsDiameter)), 0, 0);

		Color[] pixels = snapshot.GetPixels(0, 0, snapshot.width, snapshot.height);
		damageIntegral = pixels.Aggregate<Color, float>(0, (sum, color) => sum + DamageFunction(color.r));
	}

	public float RecomputeAndGetDamageIntegralNonBatch() {
		RenderTexture.active = WaveEngine.instance.systemDisplayTexture;
		if (WaveEngine.instance.shaderSpace_t != lastUpdateTime)
			RecomputeDamageIntegral();
		RenderTexture.active = null;
		return damageIntegral;
	}

	public float GetDamageIntegral() {
		if (WaveEngine.instance.shaderSpace_t != lastUpdateTime) {
			lastUpdateTime = WaveEngine.instance.shaderSpace_t;
			RenderTexture.active = WaveEngine.instance.systemDisplayTexture;
			foreach (DamageHitbox hitbox in damageHitboxes)
				hitbox.RecomputeDamageIntegral();
			RenderTexture.active = null;
		}
		return damageIntegral;
	}

}
