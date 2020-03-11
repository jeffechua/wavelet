using System;
using UnityEngine;

public class SinePulsar : BasePulsar {

	public float intensity;

	protected override void Start() {
		base.Start();
		material.color = Color.black;
	}

	protected override void StartPulse(float t) {
		material.color = new Color(intensity, 0.5f / pulseLength / WaveEngine.instance.sourceFrequencyScale, -((pulseStartTime / pulseLength / 2) % 1));
	}

	protected override void EndPulse(float t) {
		material.color = Color.black;
	}

}
