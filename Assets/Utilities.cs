using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities {
	// Start is called before the first frame update
	public static bool TrySetEnabled(Component component, bool enabled) {
		if (component is Behaviour)
			((Behaviour)component).enabled = enabled;
		else if (component is Renderer)
			((Renderer)component).enabled = enabled;
		else
			return false;
		return true;
	}
	public static void DrawCircle(LineRenderer lr, float radius) {
		Vector3[] points = new Vector3[Mathf.RoundToInt(radius * 50)];
		for (int i = 0; i < points.Length; i++) {
			points[i] = Quaternion.Euler(0, 0, 360 * i / points.Length) * Vector3.up * radius;
		}
		lr.positionCount = points.Length;
		lr.SetPositions(points);
	}
	public static Vector2 CubicBezierThroughPoints(float t, params Vector2[] p) {
		Vector2 c1 = (-5 * p[0] + 18 * p[1] - 9 * p[2] + 2 * p[3]) / 6;
		Vector2 c2 = (2 * p[0] - 9 * p[1] + 18 * p[2] - 5 * p[3]) / 6;
		return CubicBezier(t, p[0], c1, c2, p[3]);
	}
	public static Vector2 CubicBezier(float t, params Vector2[] p) {
		float T = 1 - t;
		return p[0] * T * T * T + 3 * p[1] * T * T * t + 3 * p[2] * T * t * t + p[3] * t * t * t;
	}
}
