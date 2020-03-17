using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnablementProxy : MonoBehaviour
{

    public Component[] subjects;
    public bool[] invert;

	private void Start() {
		for (int i = 0; i < subjects.Length; i++) {
			Utilities.TrySetEnabled(subjects[i], !invert[i]);
		}
	}
	private void OnEnable() {
		for(int i=0; i<subjects.Length; i++) {
			Utilities.TrySetEnabled(subjects[i], !invert[i]);
		}
	}
	private void OnDisable() {
		for (int i = 0; i < subjects.Length; i++) {
			Utilities.TrySetEnabled(subjects[i], invert[i]);
		}
	}
}
