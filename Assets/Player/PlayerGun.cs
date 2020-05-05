using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGun : RoomObjectBehaviour
{

    public float cooldown;
    public float cooldownTimer;
    public BasePulsar pulsar;

	private void Start() {
        pulsar.OnPulseStart += delegate { Player.instance.motivity.walkSpeed /= 2; };
        pulsar.OnPulseEnd += delegate { Player.instance.motivity.walkSpeed *= 2; };
    }

	void Update()
    {
        transform.rotation = Quaternion.Euler(0,0,Vector2.SignedAngle(Vector2.up,
			Camera.main.ScreenToWorldPoint(Input.mousePosition)-transform.position));
        cooldownTimer -= room.deltaTime;
		if(cooldownTimer < 0 && (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))) {
            cooldownTimer = cooldown;
            pulsar.Pulse();
		}
    }
}
