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

	public void Configure(string[] args) {
		targets = new Component[args.Length];
		defaultEnablementState = new bool[args.Length];
		for (int i = 0; i < args.Length; i++) {
			// Format: root.child[...].child.component.defaultEnablementState
			string[] halves = args[i].Split('.');
			Transform obj = room.named[halves[0]].transform;
			for (int j = 1; j < halves.Length - 2; j++)
				obj = obj.Find(halves[j]);
			targets[i] = obj.gameObject.GetComponent(halves[halves.Length - 2]);
			defaultEnablementState[i] = bool.Parse(halves[halves.Length - 1]);
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
