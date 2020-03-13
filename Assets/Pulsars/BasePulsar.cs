using UnityEngine;
using System;

public enum PulsarState { Inactive, PrePulse, MidPulse }

public class BasePulsar : RoomObject {
	public float pulseLength;
	public bool manualActivate;
	public bool autoActivate;
	public float activationLag;
	protected float pulseStartTime = -10000;
	public PulsarState state;

	public Action<float> OnPulseStart;
	public Action<float> OnPulsing;
	public Action<float> OnPulseEnd;

	protected virtual void StartPulse(float t) { }
	protected virtual void Pulsing(float t) { }
	protected virtual void EndPulse(float t) { }

	protected void Awake() {
		OnPulseStart += StartPulse;
		OnPulsing += Pulsing;
		OnPulseEnd += EndPulse;
	}

	protected virtual void Reset() {
		pulseStartTime = -10000;
	}

	public void Pulse() {
		if (state != PulsarState.Inactive) return;
		pulseStartTime = room.waveEngine.t + activationLag;
		state = PulsarState.PrePulse;
	}

	protected void Update() {

		// Activation conditions
		if (manualActivate) {
			Pulse();
			manualActivate = false;
		}

		// Normalized time past activation
		float t = pulseLength == 0 ? 0 : (room.waveEngine.t - pulseStartTime) / pulseLength;

		if (autoActivate && state == PulsarState.Inactive) Pulse();

		// Behavior
		if (state == PulsarState.PrePulse && t >= 0) {
			OnPulseStart(t);
			state = PulsarState.MidPulse;
		}
		if (state == PulsarState.MidPulse && t <= 1) {
			OnPulsing(t);
		}
		if (state == PulsarState.MidPulse && t >= 1) {
			OnPulseEnd(t);
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