using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageHitbox : MonoBehaviour
{

	float radius;

	public float GetDamageIntegral () {
		Vector2Int tsPosition = Vector2Int.RoundToInt((transform.position - WaveEngine.instance.transform.position) / WaveEngine.instance.pixelSize);
		int tsRadius = Mathf.RoundToInt(radius / WaveEngine.instance.pixelSize);
		float integral = 0;
		for(int x = tsPosition.x - tsRadius; x <= tsPosition.x + tsRadius; x++) {
			for(int y = tsPosition.y - tsRadius; y <= tsPosition.y + tsRadius; y++) {
				integral += WaveEngine.
			}
		}
		return integral;
	}

}
