using System;
using System.Collections.Generic;
using UnityEngine;

public class Border : RoomObjectBehaviour {

	public SpriteRenderer innerGraphical;
	public SpriteRenderer innerMechanical;
	public Transform inner;
	public SpriteRenderer outer;
	public Transform segment;
	public Transform door;
	public const float doorWidth = 2.25f;
	public const float exteriorThickness = 0.5f;

	public Color[] borderTypeColors;
	public Sprite[] borderTypeSprites;

	public void Draw(BorderData param, int direction) {

		// Minor transform adjustments to graphical gameObjects
		innerGraphical.sprite = borderTypeSprites[(int)param.type];
		innerMechanical.sprite = borderTypeSprites[(int)param.type];
		innerGraphical.color = borderTypeColors[(int)param.type];
		innerMechanical.color = borderTypeColors[(int)param.type];
		inner.localPosition = new Vector3(0, -param.thickness / 2, 0);
		inner.localScale = new Vector3(1, param.thickness, 1);
		outer.transform.localPosition = new Vector3(0, exteriorThickness / 2, 0);
		outer.transform.localScale = new Vector3(1, exteriorThickness, 1);

		// Set border orientation
		float length;
		switch (direction) { // N, W, S, E
			case 0:
				length = room.size.x;
				transform.localPosition = Vector3.up * room.size.y / 2;
				break;
			case 1:
				length = room.size.y;
				transform.localPosition = Vector3.left * room.size.x / 2;
				transform.localRotation = Quaternion.Euler(0, 0, 90);
				break;
			case 2:
				length = room.size.x;
				transform.localPosition = Vector3.down * room.size.y / 2;
				transform.localRotation = Quaternion.Euler(0, 0, 180);
				break;
			case 3:
				length = room.size.y;
				transform.localPosition = Vector3.right * room.size.x / 2;
				transform.localRotation = Quaternion.Euler(0, 0, 270);
				break;
			default:
				throw new ArgumentException("Invalid border direction.");
		}

		// Create colliders for exterior border
		if (param.door) {
			float subLength = (length - doorWidth) / 2 + exteriorThickness;
			float subCenter = -(length + doorWidth) / 4 - exteriorThickness / 2;
			Transform segment2 = Instantiate(segment, transform);
			segment.localPosition = new Vector2(-subCenter, 0);
			segment.localScale = new Vector3(subLength, 1, 1);
			segment2.localPosition = new Vector2(subCenter, 0);
			segment2.localScale = new Vector3(subLength, 1, 1);
			door.gameObject.SetActive(true);
			door.localPosition = new Vector2(0, (param.thickness - exteriorThickness) / 2);
			door.localRotation = Quaternion.identity;
			door.localScale = new Vector3(doorWidth, param.thickness + exteriorThickness, 1);
		} else {
			BoxCollider2D collider0 = gameObject.AddComponent<BoxCollider2D>();
			segment.localScale = new Vector3(length + exteriorThickness * 2, 1, 1);
		}


	}
}
