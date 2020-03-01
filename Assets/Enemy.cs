using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

	public float preferredPersonalSpace;
	public float preferredOrbitalRadius;
	public float speed;
	public float orbiticity;
	public float shootDelay;

	public bool manualActivate;

	public List<BasePulsar> shootPulsars; // Shoot is assumed complete when first item in list returns.

	bool shooting;

	void Start() {
		shootPulsars[0].pulseEndDel += SetShootingFalse;
	}

	void Shoot() {
		shooting = true;
		foreach (BasePulsar shootPulsar in shootPulsars) shootPulsar.Pulse();
	}

	void SetShootingFalse(float t) => shooting = false;

	// Update is called once per frame
	void Update() {
		if (manualActivate) {
			Shoot();
			manualActivate = false;
		}


		Vector2 playerward = Player.instance.transform.position - transform.position;
		float radiusError = playerward.magnitude - preferredOrbitalRadius;
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
			if (Vector2.Angle(tangentialComponent, -hit.normal) < 45) {
				orbiticity *= -1;
				break;
			}
			Vector2 newAxis = Vector2.Perpendicular(hit.normal);
			direction = newAxis * (Vector2.Angle(newAxis, tangentialComponent) > 90 ? -1 : 1);
		} while (hit.collider != null && i <= ilim);

		transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.down, playerward));

	}

}
