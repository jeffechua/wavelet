using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockableTrailer : MonoBehaviour {

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
		OrbiterEnemy parentEnemy = parent.GetComponent<OrbiterEnemy>();
		if (parentEnemy) {
			parentEnemy.pulsar.OnPulseStart += delegate {
				locked = true;
			};
			parentEnemy.pulsar.OnPulseEnd += delegate {
				locked = false;
				unlockTime = WaveEngine.instance.t;
			};
		}
		transform.SetParent(null);
	}

	// Update is called once per frame
	void FixedUpdate() {
		if (!locked && WaveEngine.instance.t - unlockTime > afterlockDelay) {
			targetPos = parent.position;
			targetRot = parent.rotation;
		}
		transform.position = Vector2.Lerp(transform.position, targetPos + (Vector2)(targetRot * offset), Time.deltaTime * trailSpeed * WaveEngine.instance.timeScale);
		transform.rotation = Quaternion.Lerp(transform.rotation, rotOffset * targetRot, Time.deltaTime * trailSpeed);
	}
}
