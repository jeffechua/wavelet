using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulsar : MonoBehaviour {

	public float pulseLength;
	public float intensity;
	public bool manualActivate;
	public bool cyclic;
	public float pulseGap; // use if cyclic

	float startTime;
	Material material;

	private void Start() {
		material = GetComponent<MeshRenderer>().material;
		material.color = Color.black;
	}

	void Pulse() {
		startTime = WaveEngine.s_instance.t;
		material.color = new Color(intensity, Mathf.PI / pulseLength / WaveEngine.s_instance.sourceFrequencyScale, -((startTime / pulseLength / 2) % 1));
	}

	void Update() {
		if (manualActivate) {
			Pulse();
			manualActivate = false;
		}
		if (cyclic && WaveEngine.s_instance.t > startTime + pulseLength + pulseGap) {
			Pulse();
		}
		if (WaveEngine.s_instance.t - startTime > pulseLength) {
			material.color = Color.black;
		}
	}

}
