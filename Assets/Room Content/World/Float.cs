using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Float : RoomObjectBehaviour
{

    public float sensitivity;

    Hitbox hb;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        hb = GetComponent<Hitbox>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.AddForce(hb.gradient * sensitivity);
    }
}
