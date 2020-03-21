using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Kineticity {
	public float range;
	public float tolerance;
	public float force;
}

public class Telekinesis : RoomObjectBehaviour {

	public LineRenderer rangeCircle;
	public LineRenderer dragLine;
	public Transform[] telekinesisTrails; // dest, cappedDest

	public Kineticity kineticity;

	Rigidbody2D manipulated;
	Vector2 grip; // on manipulated, relative to the manipulated
	Vector2 dragStartDest; // grip at drag start, relative to the player
	Vector2 dragStartMousePos; // relative to camera


	// Start is called before the first frame update
	void Start() {
		Utilities.DrawCircle(rangeCircle, kineticity.range);
	}

	void Update() {
		rangeCircle.material.color = rangeCircle.material.color - new Color(0, 0, 0, room.deltaTime);
	}

	// Update is called once per frame
	void FixedUpdate() {

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
			// Calculate grip and show guide lines and points
			if (manipulated) {
				grip = manipulated.transform.InverseTransformPoint(cappedCurrentMouseWorldPos);
				dragLine.gameObject.SetActive(true);
				Utilities.DrawCircle(rangeCircle, kineticity.range);
			} else {
				telekinesisTrails[1].gameObject.SetActive(true);
			}
			telekinesisTrails[0].gameObject.SetActive(true);
		}
		Vector2 dest = dragStartDest + (currentMousePos - dragStartMousePos);
		if (Input.GetMouseButton(0)) {
			Vector2 cappedDest = dest.magnitude > kineticity.range ? dest.normalized * kineticity.range : dest;
			Vector2 cappedWorldSpaceDest = transform.TransformPoint(cappedDest);
			telekinesisTrails[0].transform.localPosition = dest;
			telekinesisTrails[1].transform.localPosition = cappedDest;
			// Warning circle
			if (dest.magnitude > kineticity.range) {
				rangeCircle.material.color = Color.white;
				rangeCircle.transform.rotation = Quaternion.FromToRotation(Vector3.up, dest);
			}
			if (manipulated) {
				Vector2 worldSpaceGrip = manipulated.transform.TransformPoint(grip);
				float distance = (cappedWorldSpaceDest - worldSpaceGrip).magnitude;
				Vector2 force = (cappedWorldSpaceDest - worldSpaceGrip).normalized * kineticity.force * Mathf.Atan(distance * 10); // atan damps near approach
				manipulated.AddForceAtPosition(force, worldSpaceGrip);
				Player.instance.GetComponent<Rigidbody2D>().AddForce(-force);
				dragLine.SetPositions(new Vector3[] { (Vector3)worldSpaceGrip + Vector3.forward, (Vector3)cappedWorldSpaceDest + Vector3.forward });
				// Warning circle
				if ((worldSpaceGrip - (Vector2)transform.position).magnitude > kineticity.range) {
					rangeCircle.material.color = Color.yellow;
					rangeCircle.transform.rotation = Quaternion.FromToRotation(Vector3.up, dest);
				}
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
