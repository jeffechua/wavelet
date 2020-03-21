using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Photoswitch : RoomObjectBehaviour, Configurable {

	public float threshold;
	public Component[] targets;
	public bool[] defaultEnablementState;
	public bool activated;
	Hitbox hb;

	void Start() {
		hb = GetComponent<Hitbox>();
	}

	public override void Configure(string[] args) {
		targets = new Component[args.Length];
		defaultEnablementState = new bool[args.Length];
		for (int i = 0; i < args.Length; i++) {
			targets[i] = ((Room)room).ParseNamedReference(args[i], 1, out string[] def);
			defaultEnablementState[i] = bool.Parse(def[0]);
			Utilities.TrySetEnabled(targets[i], defaultEnablementState[i]);
		}
	}

	void FixedUpdate() {
		if (hb.damageDensity > threshold && !activated) {
			activated = true;
			for (int i = 0; i < targets.Length; i++)
				Utilities.TrySetEnabled(targets[i], !defaultEnablementState[i]);
		} else if (hb.damageDensity < threshold && activated) {
			activated = false;
			for (int i = 0; i < targets.Length; i++)
				Utilities.TrySetEnabled(targets[i], defaultEnablementState[i]);
		}
	}
}
