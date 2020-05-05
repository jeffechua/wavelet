using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Kineticity {
	public float range;
	public float tolerance;
	public float force;
	public float breakForce { get => tolerance * force; }
}

public class Telekinesis : RoomObjectBehaviour {

	public LineRenderer rangeCircle;
	public LineRenderer dragLine;
	public TrailRenderer[] telekinesisTrails; // dest, cappedDest
	Material[] trailMaterials;

	public Kineticity kineticity;

	public Rigidbody2D manipulated;

	public float reactionForce;
	public float GottenREactionForce;

	DistanceJoint2D joint;

	// These don't change over a drag
	Vector2 grip;              // on manipulated, relative to the manipulated
	Vector2 dragStartDest;     // grip at drag start, relative to the player
	Vector2 dragStartMousePos; // relative to camera

	// These change every frame
	Vector2 currentMouseWorldPos;
	Vector2 currentMousePos;
	Vector2 cappedCurrentMouseWorldPos;
	Vector2 dest;
	Vector2 cappedDest;
	Vector2 cappedWorldSpaceDest;
	float timeSinceDragStart;

	// Start is called before the first frame update
	void Start() {
		Utilities.DrawCircle(rangeCircle, kineticity.range);
		trailMaterials = new Material[] { telekinesisTrails[0].material, telekinesisTrails[1].material };
		telekinesisTrails[0].sharedMaterial = trailMaterials[0];
		telekinesisTrails[1].sharedMaterial = trailMaterials[1];
		telekinesisTrails[0].GetComponent<SpriteRenderer>().sharedMaterial = trailMaterials[0];
		telekinesisTrails[1].GetComponent<SpriteRenderer>().sharedMaterial = trailMaterials[1];
	}

	void Update() {

		// Fade things
		rangeCircle.material.color = rangeCircle.material.color - new Color(0, 0, 0, Mathf.Clamp(room.deltaTime, 0, rangeCircle.material.color.a));
		trailMaterials[0].color = trailMaterials[0].color - new Color(0, 0, 0, room.deltaTime * 3);
		trailMaterials[1].color = trailMaterials[1].color - new Color(0, 0, 0, room.deltaTime * 3);

		// Calculate mouse position
		currentMouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		currentMousePos = Camera.main.transform.InverseTransformPoint(currentMouseWorldPos);
		cappedCurrentMouseWorldPos = Vector2.MoveTowards(transform.position, currentMouseWorldPos, kineticity.range);

		// Player input
		timeSinceDragStart += Time.deltaTime;
		if (Input.GetMouseButtonDown(1)) {
			if (!manipulated)
				TryGrip();
			else
				Ungrip();
		}
		if(Input.GetMouseButtonUp(1) && timeSinceDragStart>0.3f) {
			Ungrip();
		}

		// Calculate drag destination
		dest = dragStartDest + (currentMousePos - dragStartMousePos);
		cappedDest = dest.magnitude > kineticity.range ? dest.normalized * kineticity.range : dest;
		cappedWorldSpaceDest = transform.TransformPoint(cappedDest);

	}

	// Update is called once per frame
	void FixedUpdate() {
		if (manipulated) {
			Drag();
		}
	}

	void TryGrip() {

		// Prep guide points
		telekinesisTrails[0].transform.position = cappedCurrentMouseWorldPos;
		telekinesisTrails[1].transform.position = cappedCurrentMouseWorldPos;
		telekinesisTrails[0].Clear();
		telekinesisTrails[1].Clear();

		// Show warning circle if attempted dest is too far.
		// We *cannot* use dest.magnitude here since dest was computed based on dragStartDest, which is outdated at this moment.
		// Also note this is not redundant with the one in Drag, since this triggers for unsuccessful drag starts.
		if (Vector2.Distance(currentMouseWorldPos, transform.position) > kineticity.range) {
			rangeCircle.material.color = new Color(1, 1, 1, 0.5f);
			rangeCircle.transform.rotation = Quaternion.FromToRotation(Vector3.up, currentMouseWorldPos - (Vector2)transform.position);
		}

		// Set persistent reference positions
		dragStartMousePos = currentMousePos;
		dragStartDest = transform.InverseTransformPoint(cappedCurrentMouseWorldPos);

		// Attempt to locate manipulated and calculate grip
		manipulated = Physics2D.OverlapPoint(cappedCurrentMouseWorldPos, 1 << LayerMask.NameToLayer("Manipulable"))?.attachedRigidbody;
		if (manipulated) {
			grip = manipulated.transform.InverseTransformPoint(cappedCurrentMouseWorldPos);
			joint = Player.instance.gameObject.AddComponent<DistanceJoint2D>();
			joint.enableCollision = true;
			joint.autoConfigureDistance = false;
			joint.maxDistanceOnly = true;
			joint.distance = kineticity.range;
			joint.connectedBody = manipulated;
			joint.connectedAnchor = grip;
			Utilities.DrawCircle(rangeCircle, kineticity.range); // this can probably be put somewhere else.
			timeSinceDragStart = 0;
		} else {
			// Only explicitly show guide points if we didn't grab something;
			// if something was, the cappedDest point is redundant with the drag line, and the dest point is shown in FixedUpdate.
			trailMaterials[0].color = new Color(1, 1, 1, 0.5f);
			trailMaterials[1].color = new Color(1, 1, 1, 0.5f);
		}

	}

	void Drag() {

		// Calculate grip position
		Vector2 worldSpaceGrip = manipulated.transform.TransformPoint(grip);
		float distance = (cappedWorldSpaceDest - worldSpaceGrip).magnitude;

		// Do physics
		Vector2 force = (cappedWorldSpaceDest - worldSpaceGrip).normalized * kineticity.force * Mathf.Atan(distance * 10); // atan damps near approach
		manipulated.AddForceAtPosition(force, worldSpaceGrip);
		Player.instance.GetComponent<Rigidbody2D>().AddForce(-force);


		// Update drag destination guide point
		telekinesisTrails[0].transform.localPosition = dest;
		trailMaterials[0].color = new Color(1, 1, 1, 0.5f);

		// Show drag line
		dragLine.gameObject.SetActive(true); // probably don't need to do this every frame but if done in TryGrip it flashes with the old positions.
		dragLine.SetPositions(new Vector3[] { (Vector3)worldSpaceGrip + Vector3.forward, (Vector3)cappedWorldSpaceDest + Vector3.forward });

		// Show warning circle if trying to drag outside
		if (dest.magnitude > kineticity.range) {
			rangeCircle.material.color = new Color(1, 1, 1, 0.5f);
			rangeCircle.transform.rotation = Quaternion.FromToRotation(Vector3.up, dest);
		}

		// Show warning circle if we're tugging on range limit
		if (joint.reactionForce.magnitude > 0) {
			rangeCircle.material.color = new Color(1, 1.0f - joint.reactionForce.magnitude / kineticity.breakForce, 0, 0.5f);
			rangeCircle.transform.rotation = Quaternion.FromToRotation(Vector2.up, (Vector2)transform.InverseTransformPoint(worldSpaceGrip));
		}

		// Break condition
		if (joint.reactionForce.magnitude > kineticity.breakForce) {
			rangeCircle.material.color = Color.red;
			Ungrip();
		}

	}

	void Ungrip() {
		manipulated = null;
		grip = Vector2.zero;
		dragLine.gameObject.SetActive(false);
		Destroy(joint);
	}
}
