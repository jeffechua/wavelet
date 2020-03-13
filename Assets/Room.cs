using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class RoomObject : MonoBehaviour {
	Room _room;
	protected Room room {
		get {
			if (!_room) {
				_room = transform.root.GetComponent<Room>();
				if (!_room)
					throw new MissingComponentException("Root of room.waveEngineSubject is not a room.");
			}
			return _room;
		}
	}
	public void OverrideRoom(Room newRoom) {
		_room = newRoom;
	}
}

public class Room : MonoBehaviour
{

	public WaveEngine waveEngine;
	public float timeScale;

	public void ResetRoom() {
		gameObject.BroadcastMessage("Reset",SendMessageOptions.DontRequireReceiver);
	}

	private void Update() {

		if (Input.GetKeyDown(KeyCode.R)) {
			ResetRoom();
		}

		// Couple player hitbox if overlapping
		if (new Rect(waveEngine.transform.position - waveEngine.transform.localScale / 2, waveEngine.transform.transform.localScale).Contains(Player.instance.transform.position)) {
			Player.instance.GetComponent<HP>().OverrideRoom(this); // probably don't need to do this every frame
			Player.instance.GetComponent<DamageHitbox>().OverrideRoom(this); // probably don't need to do this every frame
		}

	}

}
