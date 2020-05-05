using System;
using System.Collections.Generic;
using UnityEngine;

public class HP : RoomObjectBehaviour {

	public float maxHealth;
	public float health;
	public float damageThreshold;
	public bool dead;
	public Hitbox hitbox;

	public SpriteRenderer[] tintSubjects;
	bool currentlyTinted;

	public bool destroyOnDeath;
	public List<Component> disableOnDeath;
	public Action OnDeath = delegate { };
	public Action OnRevival = delegate { };

	// Start is called before the first frame update
	void Start() {
		health = maxHealth;
	}

	void Reset() {
		health = maxHealth;
	}

	// Update is called once per frame
	void Update() {
		if (!dead && hitbox.damageDensity > damageThreshold) {
			health -= room.deltaTime;
			if (!currentlyTinted) {
				foreach (SpriteRenderer subj in tintSubjects)
					subj.color = new Color(1, 0.5f, 0.5f);
				currentlyTinted = true;
			}
		} else if (currentlyTinted) {
			foreach (SpriteRenderer subj in tintSubjects)
				subj.color = Color.white;
			currentlyTinted = false;
		}

		if (health < 0 && !dead) {
			dead = true;
			foreach (Component component in disableOnDeath) {
				Utilities.TrySetEnabled(component, false);
			}
			OnDeath();
			if (destroyOnDeath)
				Destroy(gameObject);
		}
		if (health > 0 && dead) {
			dead = false;
			foreach (Component component in disableOnDeath) {
				Utilities.TrySetEnabled(component, true);
			}
			OnRevival();
		}
	}
}
