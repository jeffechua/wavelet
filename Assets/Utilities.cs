using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    // Start is called before the first frame update
    public static void TrySetEnabled(Component component, bool enabled) {
		if (component is Behaviour)
			((Behaviour)component).enabled = enabled;
		if (component is Renderer)
			((Renderer)component).enabled = enabled;
	}
}
