﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OrbiterEnemy : RoomObjectBehaviour {

	public float preferredPersonalSpace;
	public float preferredOrbitalRadius;
	public Motivity motivity;
	public float speed;
	public float orbiticity;
	public float shootCooldown;

	public bool autoShoot;
	public bool autoMove;
    public bool moveWhileShooting;
	public bool turnWhileShooting;
	public bool aimAhead;

	bool redirecting;
	bool shooting;
	float lastShotTime = -10000;

	public BasePulsar pulsar;

	RaycastHit2D[] lineOfSight;
	bool hasLineOfSight;
	Vector2 aimPos;
	Vector2 relativeAimPos;

	Rigidbody2D rb;

	void Start() {
		pulsar.OnPulseStart += delegate {
			shooting = true;
		};
		pulsar.OnPulseEnd += delegate {
			lastShotTime = room.waveEngine.t;
			shooting = false;
			redirecting = true;
		};
		rb = GetComponent<Rigidbody2D>();
	}

	void Reset() {
		lastShotTime = -10000;
	}


	void Update() {

		aimPos = Player.instance.transform.position;
		relativeAimPos = aimPos - (Vector2)transform.position;

		if (aimAhead) {
			Vector2 v = Player.instance.GetComponent<Rigidbody2D>().velocity;
			float c = room.waveEngine.param.cScale;
			float c1 = v.sqrMagnitude - c*c;
			float c2 = 2 * Vector2.Dot(v, relativeAimPos);
			float c3 = relativeAimPos.sqrMagnitude;
			float t = (-c2 - Mathf.Sqrt(c2 * c2 - 4 * c1 * c3)) / 2 / c1;
			float netTime = t + pulsar.pulseLength / 2;
			aimPos = aimPos + v * netTime;
			relativeAimPos = aimPos - (Vector2)transform.position;
		}

		lineOfSight = Physics2D.RaycastAll(transform.position, -transform.up, relativeAimPos.magnitude);
		hasLineOfSight = !lineOfSight.Any((hit) => hit.collider.gameObject.CompareTag("View Blocking"));

		// Movement

		if (autoShoot && !shooting)
			if (hasLineOfSight)
				if (room.waveEngine.t - lastShotTime > shootCooldown)
					pulsar.Pulse();


	}

	void FixedUpdate() {

		if (!autoMove)
			return;

		if (!shooting || moveWhileShooting) {

			float radiusError = relativeAimPos.magnitude - preferredOrbitalRadius;
			Vector2 playerward = relativeAimPos.normalized;

			Vector2 radialComponent = playerward * radiusError * (hasLineOfSight ? 1 : 0.5f);
			Vector2 tangentialComponent = Vector2.Perpendicular(playerward) * orbiticity;
			Vector2 direction = (radialComponent + tangentialComponent).normalized;

			// I plead guilty to all charges of awful control flow
			List<ContactPoint2D> contacts = new List<ContactPoint2D>();
			rb.GetContacts(contacts);
			foreach (ContactPoint2D contact in contacts) {
				if (Vector3.Angle(-contact.normal, tangentialComponent) < 60) { // If almost perpendicular strike (of sidewalking component),
					orbiticity *= -1;                                           // switch direction.
				} else if (Vector3.Angle(-contact.normal, direction) < 90) {  // otherwise, if we really are running into the wall,
					Vector2 newAxis = Vector2.Perpendicular(contact.normal); // redirect to run along contact tangent.
					direction = newAxis * (Vector2.Angle(newAxis, tangentialComponent) > 90 ? -1 : 1);
				}
			}
			motivity.Motivate(rb, direction, room.timeScale);

		}

		if (!shooting || turnWhileShooting) {

			float targetAngle = Vector2.SignedAngle(Vector2.up, relativeAimPos);
			if (redirecting) {
				float currentAngle = Vector2.SignedAngle(Vector2.up, transform.up);
				float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, room.deltaTime * 270);
				transform.rotation = Quaternion.Euler(0, 0, newAngle);
				if (Mathf.Abs(newAngle - targetAngle) < 1) redirecting = false;
			} else {
				transform.rotation = Quaternion.Euler(0, 0, targetAngle);
			}

		}

	}

}
