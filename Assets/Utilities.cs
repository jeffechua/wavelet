using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
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
}
