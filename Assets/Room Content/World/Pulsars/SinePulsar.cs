using System;
using UnityEngine;
using System.Linq;

public class SinePulsar : BasePulsar {

	public float intensity;
	public bool fullwave;
	public bool polarity;

	protected void Start() {
		material.color = new Color(0, 0, 0, 0);
	}

	public override void Configure(string[] args) {
		if (args.Contains("-"))
			polarity = true;
		if (args.Contains("fullwave"))
			fullwave = true;
	}

	protected override void StartPulse(float t) {
		material.color = new Color(intensity, (fullwave ? 1.0f : 0.5f) / pulseLength / waveEngine.param.sourceFrequencyScale,
									-(((fullwave ? 1.0f : 0.5f) * pulseStartTime / pulseLength) % 1) + (polarity ? 0.5f : 0f));
	}

	protected override void EndPulse(float t) {
		material.color = new Color(0, 0, 0, 0);
	}

}
