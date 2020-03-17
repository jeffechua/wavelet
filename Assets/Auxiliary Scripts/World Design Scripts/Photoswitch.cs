using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Photoswitch : RoomObject, Configurable {

	public float threshold;
	public Component[] targets;
	public bool[] defaultEnablementState;
	public bool activated;
	DamageHitbox hb;

	void Start() {
		hb = GetComponent<DamageHitbox>();
	}

	public void Configure(string[] args) {
		targets = new Component[args.Length - 1];
		defaultEnablementState = new bool[args.Length - 1];
		for (int i = 1; i < args.Length; i++) {
			// Format: root.child[...].child.component.defaultEnablementState
			string[] halves = args[i].Split('.');
			Transform obj = room.named[halves[0]].transform;
			for (int j = 1; j < halves.Length - 2; j++)
				obj = obj.Find(halves[j]);
			targets[i - 1] = obj.gameObject.GetComponent(halves[halves.Length - 2]);
			defaultEnablementState[i - 1] = bool.Parse(halves[halves.Length - 1]);
			Utilities.TrySetEnabled(targets[i - 1], defaultEnablementState[i - 1]);
		}
	}

	void Update() {
		if (hb.damageIntegral > threshold && !activated) {
			activated = true;
			for (int i = 0; i < targets.Length; i++)
				Utilities.TrySetEnabled(targets[i], !defaultEnablementState[i]);
		} else if (hb.damageIntegral < threshold && activated) {
			activated = false;
			for (int i = 0; i < targets.Length; i++)
				Utilities.TrySetEnabled(targets[i], defaultEnablementState[i]);
		}
	}
}
