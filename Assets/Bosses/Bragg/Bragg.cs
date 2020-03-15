using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Bragg : RoomObject {

	public float angularVelocity;
	public int sideAtomCount;
	public float atomicSeparation;
	public Vector2 atomicSeparationBounds;
	public Vector2 timeBetweenResizeBounds;

	float targetAtomicSeparation;
	float timeToNextResize;

	ParticleSystem ps;

	void Start() {
		ps = GetComponent<ParticleSystem>();
		Reparticle();
	}

	void Reparticle() {
		ps.Clear();
		for (int x = 0; x < sideAtomCount; x++) {
			for (int y = 0; y < sideAtomCount; y++) {
				Vector3 gridCoord = new Vector3(x, y) - Vector3.one * (sideAtomCount - 1) / 2;
				ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
				p.position = Vector3.Scale(gridCoord, transform.localScale) * atomicSeparation;
				ps.Emit(p, 1);
			}
		}
	}

	// Update is called once per frame
	void FixedUpdate() {
		transform.Rotate(0, 0, angularVelocity * room.deltaTime);
		timeToNextResize -= room.deltaTime;
		if (timeToNextResize < 0) {
			timeToNextResize = UnityEngine.Random.Range(timeBetweenResizeBounds.x, timeBetweenResizeBounds.y);
			targetAtomicSeparation = UnityEngine.Random.Range(atomicSeparationBounds.x, atomicSeparationBounds.y);
		}
		atomicSeparation = Mathf.Lerp(atomicSeparation, targetAtomicSeparation, room.deltaTime * 10);
		Reparticle();
	}
}
