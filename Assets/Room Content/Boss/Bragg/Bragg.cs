using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Bragg : RoomObjectBehaviour {

	public bool hexagonal;
	public float angularVelocity;
	public int sideAtomCount;
	public float atomicSeparation;
	public Vector2 atomicSeparationBounds;
	public Vector2 timeBetweenResizeBounds;

	float targetAtomicSeparation;
	float timeToNextResize;

	public ParticleSystem mechanical;
	public ParticleSystem graphical;

	void Start() {
		Reparticle();
	}

	void Reparticle() {
		mechanical.Clear();
		graphical.Clear();
		if (hexagonal) { // Draw hexagonal
			int radius = (sideAtomCount - 1) / 2;
			for (int x = -radius; x <= radius; x++) {
				int n = radius + 1 + (radius - Math.Abs(x));
				float xPos = Mathf.Sqrt(3)/2 * atomicSeparation * x;
				float yTop = n * atomicSeparation / 2;
				for (int y = 0; y < n; y++) {
					Vector3 localPosition = new Vector3(xPos, yTop - y * atomicSeparation);
					ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
					p.position = Vector3.Scale(localPosition, transform.localScale);
					mechanical.Emit(p, 1);
					graphical.Emit(p, 1);
				}
			}
		} else { // Draw square
			for (int x = 0; x < sideAtomCount; x++) {
				for (int y = 0; y < sideAtomCount; y++) {
					Vector3 gridCoord = new Vector3(x, y) - Vector3.one * (sideAtomCount - 1) / 2;
					ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
					p.position = Vector3.Scale(gridCoord, transform.localScale) * atomicSeparation;
					mechanical.Emit(p, 1);
					graphical.Emit(p, 1);
				}
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
