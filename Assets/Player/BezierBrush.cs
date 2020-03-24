using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class BezierBrush : RoomObjectBehaviour {
	public TrailRenderer guide;
	public LineRenderer mechanical;
	public LineRenderer graphical;
	public float maxLength;
	public float lifetime;
	public float diameter;
	public float slowFactor;
	float _slowFactor;

	Color mColor0;
	Color gColor0;
	Room slowedRoom;
	bool drawing;
	float lifetimeLeft;

	Vector2[] bezierPoints;

	void Start() {
		transform.SetParent(null);
		mColor0 = mechanical.material.color;
		gColor0 = graphical.material.color;
	}

	void Slow() {
		if (slowedRoom) {
			Unslow();
		}
		_slowFactor = slowFactor;
		if (!Room.active)
			return;
		slowedRoom = Room.active;
		//slowedRoom.timeScale /= _slowFactor;
	}

	void Unslow() {
		//slowedRoom.timeScale *= _slowFactor;
		slowedRoom = null;
	}

	void Update() {

		transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward;
		if (Input.GetMouseButtonDown(1)) {
			StartDraw();
		}
		if (Input.GetMouseButtonUp(1)) {
			StopDraw();
		}

		// Length stop condition
		Vector3[] points = new Vector3[guide.positionCount];
		guide.GetPositions(points);
		if (Utilities.PathLength(guide) > maxLength) {
			StopDraw();
		}

		// Fading
		lifetimeLeft -= Time.deltaTime;
		Color c = graphical.material.color;
		graphical.material.color = new Color(gColor0.r, gColor0.g, gColor0.b, (1 - Mathf.Pow(Mathf.Clamp01(1 - lifetimeLeft/lifetime), 3))*gColor0.a);
		c = mechanical.material.color;
		mechanical.material.color = new Color(mColor0.r, mColor0.g, mColor0.b, lifetimeLeft > 0 ? mColor0.a : 0);
	}

	void StartDraw() {
		guide.Clear();
		guide.emitting = true;
		Slow();
		drawing = true;
	}

	void StopDraw() {

		if (!drawing)
			return;
		drawing = false;

		if (guide.positionCount >= 4) {
			List<Vector2> path = Utilities.GetPositionsToPathLength(guide, maxLength);
			Vector2[] points = new Vector2[] {
				path[0],
				path[(path.Count-1)/3],
				path[(path.Count-1)*2/3],
				path[(path.Count-1)],
			};
			int n = 50;
			Vector3[] interpolated = new bool[n].Select<bool, Vector3>((useless, i) => Utilities.CubicBezierThroughPoints(i * 1.0f / n, points)).ToArray();
			mechanical.positionCount = n;
			graphical.positionCount = n;
			mechanical.widthMultiplier = diameter;
			graphical.widthMultiplier = diameter;
			mechanical.SetPositions(interpolated);
			graphical.SetPositions(interpolated);
		}

		guide.emitting = false;
		guide.Clear();
		Unslow();

		lifetimeLeft = lifetime;

	}
}
