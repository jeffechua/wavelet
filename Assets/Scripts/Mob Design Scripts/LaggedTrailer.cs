using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LaggedTrailer : RoomObject {

	public float delay;

	Transform parent;

	Vector2 offset;
	Quaternion angleOffset;

	Queue<Vector2> posQueue = new Queue<Vector2>();
	Queue<Quaternion> angleQueue = new Queue<Quaternion>();

	void Start() {
		offset = transform.localPosition;
		angleOffset = transform.localRotation;
		parent = transform.parent;
		transform.SetParent(parent.parent);
	}

	// Update is called once per frame
	void FixedUpdate() {
		posQueue.Enqueue(parent.position);
		angleQueue.Enqueue(parent.rotation);
		if (room.waveEngine.t > delay) {
			transform.position = posQueue.Dequeue();
			transform.rotation = angleQueue.Dequeue();
			transform.Translate(offset);
			transform.rotation = angleOffset * transform.rotation;
		}
	}
}
