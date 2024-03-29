﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockableTrailer : RoomObjectBehaviour {

	public BasePulsar controller;

	public float afterlockDelay;
	public float trailSpeed;

	Transform parent;

	Vector2 offset;
	Quaternion rotOffset;

	Vector2 targetPos;
	Quaternion targetRot;

	bool locked;
	float unlockTime = -9999;

	void Start() {
		offset = transform.localPosition;
		rotOffset = transform.localRotation;
		parent = transform.parent;
		controller.OnPulseStart += delegate {
			locked = true;
		};
		controller.OnPulseEnd += delegate {
			locked = false;
			unlockTime = waveEngine.t;
		};
		transform.SetParent(parent.parent);
	}

	// Update is called once per frame
	void FixedUpdate() {
		if (!locked && waveEngine.t - unlockTime > afterlockDelay) {
			targetPos = parent.position;
			targetRot = parent.rotation;
		}
		transform.position = Vector2.Lerp(transform.position, targetPos + (Vector2)(targetRot * offset), trailSpeed * room.deltaTime);
		transform.rotation = Quaternion.Lerp(transform.rotation, rotOffset * targetRot, room.deltaTime * trailSpeed);
	}
}
