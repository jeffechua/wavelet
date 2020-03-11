using System.Collections;
using System;
using UnityEngine;

public class SizePulsar : BasePulsar {

	public Vector3 restSize;
	public Vector3 maxSize;

	protected override void Start() {
		base.Start();
		transform.localScale = restSize;
	}

	protected override void Pulsing(float t) {
		transform.localScale = Vector3.Lerp(restSize, maxSize, Mathf.Sin(t * Mathf.PI));
	}

	protected override void EndPulse(float t) {
		transform.localScale = restSize;
	}

}
