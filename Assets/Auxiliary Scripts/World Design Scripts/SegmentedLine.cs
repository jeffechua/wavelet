using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Segment {
	public Vector2[] points;
}

public class SegmentedLine : MonoBehaviour
{
	public List<Segment> segments;
    public bool apply;
    LineRenderer lr;

	void Start() {
		lr = GetComponent<LineRenderer>();
	}

	// Update is called once per frame
	void Update()
    {
		if (apply) {
			apply = false;
			LineSegmenter.DrawTo(lr, segments.ConvertAll((segment) => segment.points));
		}
    }
}
