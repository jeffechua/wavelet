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

		List<Vector2[]> positions = new List<Vector2[]>();
		positions.Add(new Vector2[2] { new Vector2(height / 2, 0), new Vector2(slittedHeight / 2, 0) });
		for (int i = 1; i < numSlits; i++) {
			float currentDisplacement = 1.0f * i * (gapSize + slitSize);
			positions.Add(new Vector2[2] {
				new Vector2(slittedHeight / 2 - currentDisplacement + gapSize, 0),
				new Vector2(slittedHeight / 2 - currentDisplacement, 0)
			});
		}
		positions.Add(new Vector2[2] { new Vector2(-height / 2, 0), new Vector2(-slittedHeight / 2, 0) });

		LineSegmenter.DrawTo(lr, positions);
		lr.widthMultiplier = width;

	}

	// Update is called once per frame
	void Update() {
		if (apply) {
			Apply();
			apply = false;
		}
	}
}
