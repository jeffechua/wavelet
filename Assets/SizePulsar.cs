using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizePulsar : MonoBehaviour
{
    public Vector3 restSize;
	public Vector3 maxSize;

	public bool manualActivate;
	public bool cyclic;
	public float pulseLength;
	public float pulseGap; // use if cyclic

	float refTime;

	private void Start() {
		transform.localScale = restSize;
	}

	void Update() {

		if (manualActivate) {
			refTime = WaveEngine.s_instance.t;
			manualActivate = false;
		}

		float t = WaveEngine.s_instance.t - refTime;
		if (cyclic || t < pulseLength + pulseGap) {
			float cycleCoordinate = Mathf.Clamp(t % (pulseLength + pulseGap) / pulseLength, 0, 1);
			transform.localScale = Vector3.Lerp(restSize, maxSize, Mathf.Sin(cycleCoordinate * Mathf.PI));
		}

	}

}
