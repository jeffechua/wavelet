using System;
using System.Collections.Generic;
using UnityEngine;

public class Border : RoomObject
{

    public LineRenderer inner;
    public LineRenderer outer;
    public const float doorWidth = 2f;
    public const float exteriorThickness = 0.5f;

    public Color[] borderTypeColors;
    public Material[] borderTypeMaterials;

    public void Draw(BorderParams param, int direction)
    {

        // Minor transform adjustments to graphical gameObjects
        inner.transform.localPosition = new Vector3(0, param.thickness / 2, 0);
        inner.widthMultiplier = param.thickness;
        inner.material = borderTypeMaterials[(int)param.type];
        inner.material.color = borderTypeColors[(int)param.type];
        outer.transform.localPosition = new Vector3(0, -exteriorThickness / 2, 0);
        outer.widthMultiplier = exteriorThickness;

        // Set border orientation
        float length;
        switch (direction)
        { // S, E, N, W
            case 0:
                length = room.size.x;
                transform.position = Vector3.down * room.size.y / 2;
                break;
            case 1:
                length = room.size.y;
                transform.position = Vector3.right * room.size.x / 2;
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case 2:
                length = room.size.x;
                transform.position = Vector3.up * room.size.y / 2;
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case 3:
                length = room.size.y;
                transform.position = Vector3.left * room.size.y / 2;
                transform.rotation = Quaternion.Euler(0, 0, 270);
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
        LineSegmenter.DrawTo(inner, innerSegments);
        LineSegmenter.DrawTo(outer, outerSegments);

        // Create colliders for exterior border
        if (param.door)
        {
            float wallLength = (length - doorWidth) / 2 + exteriorThickness;
            float center = (length + doorWidth) / 4 + exteriorThickness / 2;
            BoxCollider2D collider1 = gameObject.AddComponent<BoxCollider2D>();
            BoxCollider2D collider2 = gameObject.AddComponent<BoxCollider2D>();
            collider1.offset = new Vector2(-center, -exteriorThickness / 2);
            collider1.size = new Vector2(wallLength, exteriorThickness);
            collider2.offset = new Vector2(center, -exteriorThickness / 2);
            collider2.size = new Vector2(wallLength, exteriorThickness);
        }
        else
        {
            BoxCollider2D collider0 = gameObject.AddComponent<BoxCollider2D>();
            collider0.offset = new Vector2(0, -exteriorThickness / 2);
            collider0.size = new Vector2(length + exteriorThickness * 2, exteriorThickness);
        }


    }
}
