using UnityEngine;

public enum PulsarState { Inactive, PrePulse, MidPulse }

public delegate void pulseDel(float t);

public class BasePulsar : MonoBehaviour {
	public float pulseLength;
	public bool manualActivate;
	public bool autoActivate;
	public float activationLag;
	protected float pulseStartTime;
	public PulsarState state;

	public pulseDel pulseStartDel;
	public pulseDel pulsingDel;
	public pulseDel pulseEndDel;

	protected virtual void StartPulse(float t) { }
	protected virtual void Pulsing(float t) { }
	protected virtual void EndPulse(float t) { }

	protected void Awake() {
		pulseStartDel += StartPulse;
		pulsingDel += Pulsing;
		pulseEndDel += EndPulse;
	}

	public void Pulse() {
		if (state != PulsarState.Inactive) return;
		pulseStartTime = WaveEngine.instance.t + activationLag;
		state = PulsarState.PrePulse;
	}

	protected void Update() {

		// Activation conditions
		if (manualActivate) {
			Pulse();
			manualActivate = false;
		}

		// Normalized time past activation
		float t = pulseLength == 0 ? 0 : (WaveEngine.instance.t - pulseStartTime) / pulseLength;

		if (autoActivate && state == PulsarState.Inactive) Pulse();

		// Behavior
		if (state == PulsarState.PrePulse && t >= 0) {
			pulseStartDel(t);
			state = PulsarState.MidPulse;
		}
		if (state == PulsarState.MidPulse && t <= 1) {
			pulsingDel(t);
		}
		if (state == PulsarState.MidPulse && t >= 1) {
			pulseEndDel(t);
			state = PulsarState.Inactive;
		}

	}

	public Material material {
		get {
			if (_material == null) {
				SpriteRenderer maybeSR = GetComponent<SpriteRenderer>();
				_material = maybeSR ? maybeSR.material : GetComponent<LineRenderer>().material;
			}
			return _material;
		}
	}
	Material _material;

}