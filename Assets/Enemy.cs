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


	bool shooting;
	float lastShotTime;

	public CompositePulsar pulsar;

	void Start() {
		pulsar.pulseStartDel += delegate {
			shooting = true;
		};
		pulsar.pulseEndDel += delegate {
			lastShotTime = WaveEngine.instance.t;
			shooting = false;
		};
	}

	// Update is called once per frame
	void Update() {
		

		Vector3 playerPos = Player.instance.transform.position;
		RaycastHit2D radiusCast = Physics2D.Raycast(playerPos, transform.position - playerPos, Mathf.Infinity, LayerMask.GetMask("ViewBlocking"));

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
			float distance = Time.deltaTime * speed;

			int i = 0;
			const int ilim = 10;
			RaycastHit2D hit;
			do {
				i++;
				hit = Physics2D.CircleCast(transform.position, preferredPersonalSpace, direction, distance);
				if (hit.collider == null) {
					transform.position += (Vector3)direction * distance;
					break;
				}
				if (Vector2.Angle(tangentialComponent, -hit.normal) < 60) {
					orbiticity *= -1;
					break;
				}
				Vector2 newAxis = Vector2.Perpendicular(hit.normal);
				direction = newAxis * (Vector2.Angle(newAxis, tangentialComponent) > 90 ? -1 : 1);
			} while (hit.collider != null && i <= ilim);

			transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.down, playerward));
		}

	}

}
