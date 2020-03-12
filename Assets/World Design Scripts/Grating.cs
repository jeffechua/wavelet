using System;
using System.Collections.Generic;
using UnityEngine;

public class Grating : MonoBehaviour {

	public float height;
	public float width;
	public float slitSize;
	public float gapSize;
	public float numSlits;

	public bool apply;

	LineRenderer lr;

	// Start is called before the first frame update
	void Start() {
		lr = GetComponent<LineRenderer>();
	}

	public void Apply() {

		float slittedHeight = slitSize * numSlits + gapSize * (numSlits - 1);
		if (height < slittedHeight) height = slittedHeight;

		float totalLineDistance = height + 2 * numSlits;

		List<Keyframe> keyFrames = new List<Keyframe>();

		List<Vector3> positions = new List<Vector3>();
		positions.Add(new Vector3(height / 2, 0, 0));
		for (int i = 0; i < numSlits; i++) {

			float currentDisplacement = 1.0f * i * (gapSize + slitSize);
			float currentLineDistance = (height - slittedHeight) / 2 + i * (gapSize + 1 + slitSize + 1);

			positions.Add(new Vector3(slittedHeight / 2 - currentDisplacement, 0, 0));
			keyFrames.Add(new Keyframe((currentLineDistance) / totalLineDistance, 1));

			positions.Add(new Vector3(slittedHeight / 2 - currentDisplacement - 0.00001f, 0, -1)); // Need the 0.01 for it to render properly
			keyFrames.Add(new Keyframe((currentLineDistance + 1) / totalLineDistance, 0));

			positions.Add(new Vector3(slittedHeight / 2 - currentDisplacement - slitSize + 0.00001f, 0, -1));
			keyFrames.Add(new Keyframe((currentLineDistance + 1 + slitSize) / totalLineDistance, 0));

			positions.Add(new Vector3(slittedHeight / 2 - currentDisplacement - slitSize, 0, 0));
			keyFrames.Add(new Keyframe((currentLineDistance + 1 + slitSize + 1) / totalLineDistance, 1));
		}
		positions.Add(new Vector3(-height / 2, 0, 0));

		lr.widthCurve = new AnimationCurve(keyFrames.ToArray());
		lr.widthMultiplier = width;
		lr.alignment = LineAlignment.TransformZ;
		lr.positionCount = positions.Count;
		lr.SetPositions(positions.ToArray());
		lr.useWorldSpace = false;

	}

	// Update is called once per frame
	void Update() {
		if (apply) {
			Apply();
			apply = false;
		}
	}
}
