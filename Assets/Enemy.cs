using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

	public float preferredPersonalSpace;
	public float preferredOrbitalRadius;
	public float speed;
	public float orbiticity;
	public float shootDelay;

	public bool autoShoot;
	public bool autoMove;

	bool redirecting;
	bool shooting;
	float lastShotTime = -10000;

	public BasePulsar pulsar;

	void Start() {
		WaveEngine.instance.OnReset += Reset;
		pulsar.OnPulseStart += delegate {
			shooting = true;
		};
		pulsar.OnPulseEnd += delegate {
			lastShotTime = WaveEngine.instance.t;
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
			if (hit.OverlapPoint(transform.position)) {
				print(":((");
				break;
			}
			Vector2 displacement = (Vector2)transform.position - hit.ClosestPoint(transform.position);
			transform.position += (Vector3)displacement.normalized * (preferredPersonalSpace - displacement.magnitude);
		}
		if (ilim == i) print(":(");


		// Movement

		if (shooting)
			return;

		if (autoShoot) {
			if (radiusCast.collider == null || radiusCast.distance >= ((Vector2)(playerPos - transform.position)).magnitude) {
				if (WaveEngine.instance.t - lastShotTime > shootDelay) {
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
			float distance = Time.deltaTime * speed * WaveEngine.instance.timeScale;

			// I plead guilty to all charges of awful control flow
			int j = 0;
			const int jlim = 10;
			while (j < jlim && CastObstructions(direction, distance, out RaycastHit2D hit)) {
				j++;
				float impactAngle = Vector2.Angle(tangentialComponent, -hit.normal);
				if (impactAngle < 60) {        // If hit is almost perpendicular to collidee, flip orbit
					orbiticity *= -1;          // and set move distance to 0 so if we're in a corner we don't jitter
					distance = 0;			   // and break. We don't translate this frame.
					break;
				} else if (impactAngle < 90) { // Else if hit is glancing, deflect to skim along contact surface.
					Vector2 newAxis = Vector2.Perpendicular(hit.normal); // (can be deflected up to ilim times)
					direction = newAxis * (Vector2.Angle(newAxis, tangentialComponent) > 90 ? -1 : 1);
				} else {
					// If impactAngle > 90, and we weren't caught by the CastOverlaps, we're probably actually inside
					// another collider, so just keep going and pretend nothing's wrong.
				}
				break;
			}
			transform.position += (Vector3)direction * distance;

			float targetAngle = Vector2.SignedAngle(Vector2.down, playerward);
			if (redirecting) {
				float currentAngle = Vector2.SignedAngle(Vector2.down, -transform.up);
				float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * WaveEngine.instance.timeScale * 10);
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
