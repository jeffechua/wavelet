using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveAmplitudePulsar : BasePulsar {

	// Note that all amplitude pulsars modify material color
	// Therefore, the sprite/line color should be used to specify operating parameters,
	// and the gradient used as a multiplying factor.

	public Color restColor;
	public Gradient gradient;
	Color originalColor;

	protected override void Start() {
		base.Start();
		originalColor = material.color;
		material.color = Multiply(originalColor, restColor);
	}

	protected override void Pulsing(float t) {
		material.color = Multiply(originalColor, gradient.Evaluate(t));
	}

	protected override void EndPulse(float t) {
		material.color = Multiply(originalColor, restColor);
	}

	static Color Multiply (Color c1, Color c2) {
		return new Color(
			c1.r * c2.r,
			c1.g * c2.g,
			c1.b * c2.b,
			c1.a * c2.a
		);
	}
}