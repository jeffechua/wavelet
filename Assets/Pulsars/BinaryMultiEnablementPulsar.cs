using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinaryMultiEnablementPulsar : BasePulsar
{
	public List<GameObject> enableables;

	protected override void Start() {
		base.Start();
		foreach (GameObject enableable in enableables)
			enableable.SetActive(false);
	}
	protected override void StartPulse(float t) {
		foreach(GameObject enableable in enableables)
			enableable.SetActive(true);
	}
	protected override void EndPulse(float t) {
		foreach (GameObject enableable in enableables)
			enableable.SetActive(false);
	}
}
