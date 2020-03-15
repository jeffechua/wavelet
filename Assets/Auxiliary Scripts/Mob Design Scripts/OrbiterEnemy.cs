using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbiterEnemy : RoomObject {

	public float preferredPersonalSpace;
	public float preferredOrbitalRadius;
	public float speed;
	public float orbiticity;
	public float shootDelay;

	public bool autoShoot;
	public bool autoMove;
    public bool moveWhileShooting;

	bool redirecting;
	bool shooting;
	float lastShotTime = -10000;

	public BasePulsar pulsar;

	void Start() {
		pulsar.OnPulseStart += delegate {
			shooting = true;
		};
		pulsar.OnPulseEnd += delegate {
			lastShotTime = room.waveEngine.t;
			shooting = false;
			redirecting = true;
		};
	}

	void Reset() {
		lastShotTime = -10000;
	}

	// Update is called once per frame
	void Update() {


		Vector3 playerPos = Player.instance.transform.position;
		RaycastHit2D radiusCast = Physics2D.Raycast(playerPos, transform.position - playerPos, Mathf.Infinity, LayerMask.GetMask("ViewBlocking"));


		// Collision detection
		int i = 0;
		const int ilim = 10;
		while (i < ilim && CastOverlaps(out Collider2D hit)) {
			i++;
			if (hit.OverlapPoint(transform.position))
				break;
			Vector2 displacement = (Vector2)transform.position - hit.ClosestPoint(transform.position);
			transform.position += (Vector3)displacement.normalized * (preferredPersonalSpace*1.0001f - displacement.magnitude);
		}


		// Movement

		if (shooting && !moveWhileShooting)
			return;

		if (autoShoot) {
			if (radiusCast.collider == null || radiusCast.distance >= ((Vector2)(playerPos - transform.position)).magnitude) {
				if (room.waveEngine.t - lastShotTime > shootDelay) {
					pulsar.Pulse();
				}
			}
		}

		if (autoMove) {
			float targetOrbitalRadius = radiusCast.collider == null ? preferredOrbitalRadius : Mathf.Min(radiusCast.distance, preferredOrbitalRadius);

			Vector2 playerward = playerPos - transform.position;
			float radiusError = playerward.magnitude - targetOrbitalRadius;
			playerward = playerward.normalized;

			Vector2 radialComponent = playerward * radiusError;
			Vector2 tangentialComponent = Vector2.Perpendicular(playerward) * orbiticity;
			Vector2 direction = radialComponent + tangentialComponent;
			direction = direction.normalized;
			float distance = room.deltaTime * speed;

			// I plead guilty to all charges of awful control flow
			int j = 0;
			const int jlim = 10;
			while (j < jlim && CastObstructions(direction, distance, out RaycastHit2D hit)) {
				j++;
				if (Vector2.Angle(tangentialComponent, -hit.normal) < 60) {
					orbiticity *= -1;   // If our sidewalking is almost perpendicular to surface, flip orbit
					distance = 0;       // and set move distance to 0 so that if we're in a corner we don't jitter
					break;              // and then break. We don't translate this frame.
				} else if (Vector2.Angle(direction, -hit.normal) < 90) { // Else if hit is sensible, deflect to skim along contact surface.
					Vector2 newAxis = Vector2.Perpendicular(hit.normal); // (can be deflected up to ilim times)
					direction = newAxis * (Vector2.Angle(newAxis, tangentialComponent) > 90 ? -1 : 1);
				} else { // If hit is not sensible, be sad.
					print(":(");
					distance = 0;
					break;
				}
				break;
			}
			transform.position += (Vector3)direction * distance;

			float targetAngle = Vector2.SignedAngle(Vector2.down, playerward);
			if (redirecting) {
				float currentAngle = Vector2.SignedAngle(Vector2.down, -transform.up);
				float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, room.deltaTime * 10);
				transform.rotation = Quaternion.Euler(0, 0, newAngle);
				if (Mathf.Abs(newAngle - targetAngle) < 1) redirecting = false;
			} else {
				transform.rotation = Quaternion.Euler(0, 0, targetAngle);
			}
		}


	}

	public bool CastOverlaps(out Collider2D next) {
		Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, preferredPersonalSpace);
		next = null;
		foreach (Collider2D hit in hits) {
			if (hit.transform != transform) {
				next = hit;
				return true;
			}
		}
		return false;
	}

	public bool CastObstructions(Vector2 direction, float distance, out RaycastHit2D closest) {
		RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, preferredPersonalSpace, direction, distance);
		closest = new RaycastHit2D();
		foreach (RaycastHit2D hit in hits)
			if (hit.transform != transform && (closest.collider == null || hit.distance < closest.distance))
				closest = hit;
		return closest.collider != null;
	}

}
