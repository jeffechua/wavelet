using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class GlassTrail : RoomObjectBehaviour {
	public TrailRenderer guide;
	public LineRenderer mechanical;
	public LineRenderer graphical;
	public float slowFactor;
	float _slowFactor;

	Room slowedRoom;

	Vector2[] bezierPoints;

	void Start() {
		transform.SetParent(null);
	}

	void Slow () {
		if(slowedRoom) {
			Unslow();
		}
		_slowFactor = slowFactor;
		if (!Room.active)
			return;
		slowedRoom = Room.active;
		slowedRoom.timeScale /= _slowFactor;
	}

	void Unslow () {
		slowedRoom.timeScale *= _slowFactor;
		slowedRoom = null;
	}

	void Update() {
		transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward;
		if (Input.GetMouseButtonDown(1)) {
			guide.Clear();
			guide.emitting = true;
			Slow();
		}
		if (Input.GetMouseButtonUp(1)) {
			if (guide.positionCount >= 4) {
				Vector2[] points = new Vector2[] {
					guide.GetPosition(0),
					guide.GetPosition((guide.positionCount-1)/3),
					guide.GetPosition((guide.positionCount-1)*2/3),
					guide.GetPosition(guide.positionCount-1),
				};
				int n = 50;
				Vector3[] interpolated = new bool[n].Select<bool, Vector3>((useless, i) => Utilities.CubicBezierThroughPoints(i * 1.0f / n, points)).ToArray();
				mechanical.positionCount = n;
				graphical.positionCount = n;
				mechanical.SetPositions(interpolated);
				graphical.SetPositions(interpolated);
			}
			guide.emitting = false;
			guide.Clear();
			Unslow();
		}
	}
}
