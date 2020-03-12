﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class HP : MonoBehaviour
{

    public float maxHealth;
    public float health;
    public bool dead;
    public DamageHitbox hitbox;

    public bool destroyOnDeath;
    public List<Component> disableOnDeath;
    public Action OnDeath = delegate { };
    public Action OnRevival = delegate { };

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        WaveEngine.instance.OnReset += Reset;
    }

	void OnDestroy() {
        WaveEngine.instance.OnReset -= Reset;
	}

	void Reset() {
        health = maxHealth;
	}

	// Update is called once per frame
	void Update()
    {
		if(!dead)
			health -= hitbox.damageIntegral * Time.deltaTime * WaveEngine.instance.timeScale;

		if (health < 0 && !dead) {
            dead = true;
			foreach(Component component in disableOnDeath) {
                Utilities.TrySetEnabled(component, false);
			}
            OnDeath();
            if (destroyOnDeath)
                Destroy(gameObject);
        }
		if(health > 0 && dead) {
            dead = false;
            foreach (Component component in disableOnDeath) {
                Utilities.TrySetEnabled(component, true);
            }
            OnRevival();
        }
    }
}
