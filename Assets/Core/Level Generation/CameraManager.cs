using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {
	public float switchRoomSpeed;

	// Update is called once per frame
	void Update() {
		if (Room.active)
			transform.position = (Vector3)Vector2.Lerp(transform.position, Room.active.transform.position, Time.deltaTime * switchRoomSpeed) + Vector3.forward * transform.position.z;
	}
}
