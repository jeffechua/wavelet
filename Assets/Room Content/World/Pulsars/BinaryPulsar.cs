﻿using UnityEngine;

public class BinaryPulsar : BasePulsar {

	protected void Start() {
		material.color = new Color(material.color.r, material.color.g, material.color.b, 0);
	}
	protected override void StartPulse(float t) {
		material.color = new Color(material.color.r, material.color.g, material.color.b, 1);
	}
	protected override void EndPulse(float t) {
		material.color = new Color(material.color.r, material.color.g, material.color.b, 0);
	}
}
