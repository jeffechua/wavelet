using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class LineSegmenter {

	public static void DrawTo(LineRenderer lr, List<Vector2[]> segments) {

		List<Vector3> positions = new List<Vector3>();

		float netDistance = 0;
		List<float> distances = new List<float>();

		for (int i = 0; i < segments.Count; i++) {

			const float epsilon = 0.001f;

			List<Vector3> newPositions = new List<Vector3>();

			if (i > 0) {
				netDistance += (segments[i][0] - segments[i - 1][segments[i - 1].Length - 1]).magnitude;
			}
			distances.Add(netDistance);
			newPositions.Add((Vector3)segments[i][0] + Vector3.forward);

			netDistance += 1; // Technically should be sqrt(1+epsilon^2) since we add an epsilon displacement to position later
			distances.Add(netDistance);
			newPositions.Add(segments[i][0]);

			for (int j = 1; j < segments[i].Length; j++) {
				netDistance += (segments[i][j] - segments[i][j - 1]).magnitude;
				newPositions.Add(segments[i][j]);
			}
			distances.Add(netDistance);

			netDistance += 1;
			distances.Add(netDistance);
			newPositions.Add((Vector3)segments[i][segments[i].Length - 1] + Vector3.forward);

			if (segments[i].Length > 1) {
				newPositions[0] += (newPositions[1] - newPositions[2]).normalized * epsilon;
				newPositions[newPositions.Count - 1] += (newPositions[newPositions.Count - 2] - newPositions[newPositions.Count - 3]).normalized * epsilon;
			}

			positions.AddRange(newPositions);

		}

		float[] frameWidthOrder = { 0f, 1f, 1f, 0f };

		Keyframe[] keyframes = new Keyframe[distances.Count];
		for (int i = 0; i < keyframes.Length; i++)
			keyframes[i] = new Keyframe(distances[i] / netDistance, frameWidthOrder[i % 4]);

		lr.widthCurve = new AnimationCurve(keyframes);
		lr.alignment = LineAlignment.TransformZ;
		lr.positionCount = positions.Count;
		lr.SetPositions(positions.ToArray());
		lr.useWorldSpace = false;

	}
}
