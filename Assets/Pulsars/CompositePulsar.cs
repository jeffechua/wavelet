using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositePulsar : BasePulsar {

	public List<BasePulsar> subPulsars;

    // Start is called before the first frame update
    protected override void StartPulse(float t)
    {
        foreach (BasePulsar pulsar in subPulsars) pulsar.Pulse();
    }

}
