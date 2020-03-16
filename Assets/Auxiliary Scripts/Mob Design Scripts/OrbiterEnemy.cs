using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbiterEnemy : RoomObject {

	public float preferredPersonalSpace;
	public float preferredOrbitalRadius;
	public Motivity motivity;
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

	RaycastHit2D lineOfSight; // it's a RaycastHit2D from the player towards self and not a geometric line of sight, but 
							  // line of sight logic is what it's being used for.
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

	// Update is called once per frame
	void Update() {

		Vector3 playerPos = Player.instance.transform.position;
		lineOfSight = Physics2D.Raycast(playerPos, transform.position - playerPos, Mathf.Infinity, LayerMask.GetMask("View Blocking"));

		// Movement

		if (autoShoot && !shooting)
			if (lineOfSight.collider == null || lineOfSight.distance >= ((Vector2)(playerPos - transform.position)).magnitude)
				if (room.waveEngine.t - lastShotTime > shootDelay)
					pulsar.Pulse();


	}

	void FixedUpdate() {

		if (autoMove && (!shooting || moveWhileShooting) ) {
			float targetOrbitalRadius = lineOfSight.collider == null ? preferredOrbitalRadius : Mathf.Min(lineOfSight.distance, preferredOrbitalRadius);

			Vector2 playerward = Player.instance.transform.position - transform.position;
			float radiusError = playerward.magnitude - targetOrbitalRadius;
			playerward = playerward.normalized;

			Vector2 radialComponent = playerward * radiusError;
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
					print(direction);
				}
			}
			motivity.Motivate(rb, direction);

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

}
