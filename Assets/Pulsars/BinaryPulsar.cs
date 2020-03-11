using UnityEngine;

public class BinaryPulsar : BasePulsar {

	protected override void Start() {
		base.Start();
		material.color = new Color(material.color.r, material.color.g, material.color.b, 0);
	}
	protected override void StartPulse(float t) {
		material.color = new Color(material.color.r, material.color.g, material.color.b, 1);
	}
	protected override void EndPulse(float t) {
		material.color = new Color(material.color.r, material.color.g, material.color.b, 0);
	}
}
