using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinaryMultiEnablementPulsar : BasePulsar
{
	public List<Component> enableables;
	public List<bool> defaultEnabled;

	protected void Start() {
		for (int i = 0; i < enableables.Count; i++)
			Utilities.TrySetEnabled(enableables[i], defaultEnabled[i]);
	}
	protected override void StartPulse(float t) {
		for (int i = 0; i < enableables.Count; i++)
			Utilities.TrySetEnabled(enableables[i], !defaultEnabled[i]);
	}
	protected override void EndPulse(float t) {
		for (int i = 0; i < enableables.Count; i++)
			Utilities.TrySetEnabled(enableables[i], defaultEnabled[i]);
	}
}
