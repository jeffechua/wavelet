using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Motivity {
	public float force;
	public float velocityHalfLife;
	public float walkSpeed;
	public float currentSpeed;
	public void Motivate(Rigidbody2D rb, Vector2 direction, float timeScale = 1) {

		/* a = -µv + F/m
         * Therefore v = (1 - exp(-µt)) * F/µm
         * v_max = F/µm

         * We want to specify force F, maximum speed v_max and half life of speed w'.
		 * This makes mass m a dependent variable.

         * But we want to control half-life. Let characteristic time be w such that at t=w, v = v_max/e.
         * i.e. µ = 1/w.
         *      F = F
         *      m = F / µ / v_max
         *        = Fw / v_max
		 */

		rb.AddForce(direction.normalized * force);
		rb.drag = 1 / velocityHalfLife;
		rb.mass = force * velocityHalfLife / (walkSpeed * timeScale);

		currentSpeed = rb.velocity.magnitude;

	}
}

public class Player : MonoBehaviour {

	public static Player instance;

	public Motivity motivity;

	Rigidbody2D rb;

	// Start is called before the first frame update
	void Start() {
		instance = this;
		rb = gameObject.GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	void FixedUpdate() {

		if (Input.GetKeyDown(KeyCode.Space)) {
			BroadcastMessage("Reset");
		}

		/* a = -µv + F/m
         * Therefore v = (1 - exp(-µt)) * F/µm
         * v_max = F/µm

         * We want to specify force F, maximum speed v_max and half life of speed w'.
		 * This makes mass m a dependent variable.

         * But we want to control half-life. Let characteristic time be w such that at t=w, v = v_max/e.
         * i.e. µ = 1/w.
         *      F = F
         *      m = F / µ / v_max
         *        = Fw / v_max
		 */


		Vector2 direction = new Vector2();
		if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) direction += Vector2.up;
		if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) direction += Vector2.down;
		if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) direction += Vector2.right;
		if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) direction += Vector2.left;

		motivity.Motivate(rb, direction, Room.active?.timeScale ?? 1);

	}
}
