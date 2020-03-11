using UnityEngine;

public class SineAmplitudePulsar : BasePulsar {

	public bool r;
	public bool g;
	public bool b;
	public bool a;

	protected override void Start() {
		base.Start();
		material.color = new Color(r ? 0 : material.color.r,
								   g ? 0 : material.color.g,
								   b ? 0 : material.color.b,
								   a ? 0 : material.color.a);
	}

	protected override void Pulsing(float t) {
		float amp = Mathf.Sin(t * Mathf.PI);
		material.color = new Color(r ? amp : material.color.r,
								   g ? amp : material.color.g,
								   b ? amp : material.color.b,
								   a ? amp : material.color.a);
	}

	protected override void EndPulse(float t) {
		material.color = new Color(r ? 0 : material.color.r,
								   g ? 0 : material.color.g,
								   b ? 0 : material.color.b,
								   a ? 0 : material.color.a);
	}
}
