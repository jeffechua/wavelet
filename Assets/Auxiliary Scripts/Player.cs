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

[Serializable]
public class Kineticity {
	public float range;
	public float tolerance;
	public float force;
}

public class Player : MonoBehaviour {

	public static Player instance;

	public LineRenderer rangeCircle;
	public LineRenderer dragLine;
	public Transform[] telekinesisTrails; // dragStartMousePos, currentMousePos, dragStartDest, dest, cappedDest, grip

	public Motivity motivity;
	public Kineticity kineticity;

	Rigidbody2D rb;

	Rigidbody2D manipulated;
	Vector2 grip; // on manipulated, relative to the manipulated
	Vector2 dragStartDest; // grip at drag start, relative to the player
	Vector2 dragStartMousePos; // relative to camera

	void Start() {
		instance = this;
		rb = gameObject.GetComponent<Rigidbody2D>();
		telekinesisTrails[0].SetParent(Camera.main.transform);
		telekinesisTrails[1].SetParent(Camera.main.transform);
		RedrawCircle();
	}

	void RedrawCircle () {
		Vector3[] points = new Vector3[Mathf.RoundToInt(kineticity.range * 50)];
		for (int i = 0; i < points.Length; i++) {
			points[i] = Quaternion.Euler(0, 0, 360 * i / points.Length) * Vector3.up * kineticity.range;
		}
		rangeCircle.positionCount = points.Length;
		rangeCircle.SetPositions(points);
	}

	void Update() {
		rangeCircle.material.color = rangeCircle.material.color - new Color(0, 0, 0, Room.active?.deltaTime ?? 1);
	}

	void FixedUpdate() {

		if (Input.GetKeyDown(KeyCode.Space)) {
			BroadcastMessage("Reset");
		}

		// Motion

		Vector2 direction = new Vector2();
		if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) direction += Vector2.up;
		if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) direction += Vector2.down;
		if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) direction += Vector2.right;
		if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) direction += Vector2.left;

		motivity.Motivate(rb, direction, Room.active?.timeScale ?? 1);

		// Telekinesis

		Vector2 currentMouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 currentMousePos = Camera.main.transform.InverseTransformPoint(currentMouseWorldPos);
		if (Input.GetMouseButtonDown(0)) {
			// Try to locate a manipulated
			Vector2 cappedCurrentMouseWorldPos;
			if (Vector2.Distance(dragStartMousePos, transform.position) > kineticity.range) {
				cappedCurrentMouseWorldPos = Vector2.MoveTowards(transform.position, currentMouseWorldPos, kineticity.range);
				rangeCircle.material.color = Color.white;
				rangeCircle.transform.rotation = Quaternion.FromToRotation(Vector3.up, currentMouseWorldPos - (Vector2)transform.position);
			} else {
				cappedCurrentMouseWorldPos = dragStartMousePos;
			}
			dragStartMousePos = currentMousePos;
			dragStartDest = transform.InverseTransformPoint(cappedCurrentMouseWorldPos);
			manipulated = Physics2D.OverlapPoint(cappedCurrentMouseWorldPos, 1 << LayerMask.NameToLayer("Manipulable"))?.attachedRigidbody;
			// Always show guide points
			telekinesisTrails[0].transform.localPosition = (Vector3)dragStartMousePos + Vector3.forward;
			telekinesisTrails[2].transform.localPosition = dragStartDest;
			for (int i = 0; i < 5; i++)
				telekinesisTrails[i].gameObject.SetActive(true);
			// Calculate grip and show grip point if manipulated != null
			if (manipulated) {
				telekinesisTrails[5].transform.SetParent(manipulated.transform);
				grip = manipulated.transform.InverseTransformPoint(cappedCurrentMouseWorldPos);
				telekinesisTrails[5].transform.localPosition = grip;
				telekinesisTrails[5].gameObject.SetActive(true);
				dragLine.gameObject.SetActive(true);
				RedrawCircle();
			}
		}
		Vector2 dest = dragStartDest + (currentMousePos - dragStartMousePos);
		if (Input.GetMouseButton(0)) {
			Vector2 cappedDest;
			if (dest.magnitude > kineticity.range) {
				cappedDest = dest.normalized * kineticity.range;
				rangeCircle.material.color = Color.white;
				rangeCircle.transform.rotation = Quaternion.FromToRotation(Vector3.up, dest);
			} else {
				cappedDest = dest;
			}
			Vector2 cappedWorldSpaceDest = transform.TransformPoint(cappedDest);
			telekinesisTrails[1].transform.localPosition = (Vector3)currentMousePos + Vector3.forward;
			telekinesisTrails[3].transform.localPosition = dest;
			telekinesisTrails[4].transform.localPosition = cappedDest;
			if (manipulated) {
				Vector2 worldSpaceGrip = manipulated.transform.TransformPoint(grip);
				Vector2 force = (cappedWorldSpaceDest - worldSpaceGrip).normalized * kineticity.force;
				manipulated.AddForceAtPosition(force, worldSpaceGrip);
				rb.AddForce(-force);
				dragLine.SetPositions(new Vector3[] { telekinesisTrails[4].transform.position, telekinesisTrails[5].transform.position });
			}
		}
		if (Input.GetMouseButtonUp(0) || (manipulated &&
			Vector2.Distance(manipulated.transform.TransformPoint(grip), transform.position) > kineticity.range * (1f + kineticity.tolerance))) {
			manipulated = null;
			grip = Vector2.zero;
			foreach (Transform trail in telekinesisTrails)
				trail.gameObject.SetActive(false);
			dragLine.gameObject.SetActive(false);
		}

	}
}
