using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformLerp : MonoBehaviour
{

    public Vector2 origin;
    public Vector2 displacement;
    public float originRot;
    public float deltaRot;
    public Vector2 baseScale;
    public AnimationCurve xScale;
    public AnimationCurve yScale;
    public bool replay;
    public float duration;
    float t;
        
    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
		if (replay) {
            replay = false;
            t = 0;
		}
        transform.localPosition = origin + displacement * t / duration;
        transform.localRotation = Quaternion.Euler(0,0, originRot + deltaRot* t / duration);
        transform.localScale = new Vector3(xScale.Evaluate(t/duration) * baseScale.x, yScale.Evaluate(t/duration) * baseScale.y, 1);
    }
}
