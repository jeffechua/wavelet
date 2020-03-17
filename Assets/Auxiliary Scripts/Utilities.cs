using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    // Start is called before the first frame update
    public static bool TrySetEnabled(Component component, bool enabled) {
		if (component is Behaviour)
			((Behaviour)component).enabled = enabled;
		else if (component is Renderer)
			((Renderer)component).enabled = enabled;
		else
			return false;
		return true;
	}
}
