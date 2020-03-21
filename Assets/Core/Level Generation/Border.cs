using System;
using System.Collections.Generic;
using UnityEngine;

public class Border : RoomObjectBehaviour {

	public LineRenderer innerGraphical;
	public LineRenderer innerMechanical;
	public LineRenderer outer;
	public const float doorWidth = 2.25f;
	public const float exteriorThickness = 0.5f;

	public Color[] borderTypeColors;
	public Material[] borderTypeMaterials;

	public void Draw(BorderData param, int direction) {

		// Minor transform adjustments to graphical gameObjects
		innerGraphical.transform.localPosition = new Vector3(0, param.thickness / 2, 0);
		innerGraphical.widthMultiplier = param.thickness;
		innerGraphical.material = borderTypeMaterials[(int)param.type];
		innerGraphical.material.color = borderTypeColors[(int)param.type];
		innerMechanical.transform.localPosition = new Vector3(0, param.thickness / 2, 0);
		innerMechanical.widthMultiplier = param.thickness;
		innerMechanical.material = borderTypeMaterials[(int)param.type];
		innerMechanical.material.color = borderTypeColors[(int)param.type];
		outer.transform.localPosition = new Vector3(0, -exteriorThickness / 2, 0);
		outer.widthMultiplier = exteriorThickness;

		// Set border orientation
		float length;
		switch (direction) { // S, E, N, W
			case 0:
				length = room.size.x;
				transform.localPosition = Vector3.down * room.size.y / 2;
				break;
			case 1:
				length = room.size.y;
				transform.localPosition = Vector3.right * room.size.x / 2;
				transform.localRotation = Quaternion.Euler(0, 0, 90);
				break;
			case 2:
				length = room.size.x;
				transform.localPosition = Vector3.up * room.size.y / 2;
				transform.localRotation = Quaternion.Euler(0, 0, 180);
				break;
			case 3:
				length = room.size.y;
				transform.localPosition = Vector3.left * room.size.x / 2;
				transform.localRotation = Quaternion.Euler(0, 0, 270);
				break;
			default:
				throw new ArgumentException("Invalid border direction.");
		}

		// Draw border graphics proper
		List<Vector2[]> innerSegments = param.door ?
			new List<Vector2[]> {
				new Vector2[2] {new Vector2(-length/2, 0), new Vector2(-doorWidth/2, 0)},
				new Vector2[2] {new Vector2(doorWidth/2, 0), new Vector2(length/2, 0)}
			} :
			new List<Vector2[]> {
				new Vector2[2] { new Vector2(-length/2, 0), new Vector2(length/2, 0) }
			};
		List<Vector2[]> outerSegments = param.door ?
			new List<Vector2[]> {
				new Vector2[2] {new Vector2(-length/2-exteriorThickness, 0), new Vector2(-doorWidth/2, 0)},
				new Vector2[2] {new Vector2(doorWidth/2, 0), new Vector2(length/2+exteriorThickness, 0)}
			} :
			new List<Vector2[]> {
				new Vector2[2] { new Vector2(-length/2-exteriorThickness, 0), new Vector2(length/2+exteriorThickness, 0) }
			};
		LineSegmenter.DrawTo(innerGraphical, innerSegments);
		LineSegmenter.DrawTo(innerMechanical, innerSegments);
		LineSegmenter.DrawTo(outer, outerSegments);

		// Create colliders for exterior border
		float yLength = exteriorThickness + param.thickness;
		float yCenter = param.thickness / 2 - exteriorThickness / 2;
		if (param.door) {
			float xLength = (length - doorWidth) / 2 + exteriorThickness;
			float xCenter = -(length + doorWidth) / 4 - exteriorThickness / 2;
			BoxCollider2D collider1 = gameObject.AddComponent<BoxCollider2D>();
			BoxCollider2D collider2 = gameObject.AddComponent<BoxCollider2D>();
			collider1.offset = new Vector2(-xCenter, yCenter);
			collider1.size = new Vector2(xLength, yLength);
			collider2.offset = new Vector2(xCenter, yCenter);
			collider2.size = new Vector2(xLength, yLength);
			GameObject door = new GameObject("Door");
			door.layer = LayerMask.NameToLayer("Room Door");
			door.transform.SetParent(transform);
			door.transform.localPosition = new Vector2(0, yCenter);
			door.transform.localRotation = Quaternion.identity;
			door.AddComponent<BoxCollider2D>().size = new Vector2(doorWidth, yLength);
		} else {
			BoxCollider2D collider0 = gameObject.AddComponent<BoxCollider2D>();
			collider0.offset = new Vector2(0, yCenter);
			collider0.size = new Vector2(length + exteriorThickness * 2, yLength);
		}


	}
}
