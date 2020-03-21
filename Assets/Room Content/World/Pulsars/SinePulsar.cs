using System;
using UnityEngine;

public class SinePulsar : BasePulsar {

	public float intensity;
	public bool polarity;

	protected void Start() {
		material.color = new Color(0, 0, 0, 0);
	}

	protected override void StartPulse(float t) {
		material.color = new Color(intensity, 0.5f / pulseLength / waveEngine.param.sourceFrequencyScale, -((0.5f * pulseStartTime / pulseLength) % 1) + (polarity ? 0.5f : 0f));
	}

	protected override void EndPulse(float t) {
		material.color = new Color(0, 0, 0, 0);
	}

}
