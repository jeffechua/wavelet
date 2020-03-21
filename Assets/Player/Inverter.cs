using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Inverter : RoomObjectBehaviour {

	public ComputeShader shader;
	public LineRenderer circle;
	public float radius;
	public float cooldown;

	int kernel;

	// Start is called before the first frame update
	void Start() {
		kernel = shader.FindKernel("Main");
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetKey(KeyCode.Space) && circle.material.color.a <= 0) {
			int pixelRadius = Mathf.RoundToInt(radius / waveEngine.param.pixelSize / 4) * 4;
			Vector2Int pixelPosition = waveEngine.WorldToTexturePoint(transform.position);
			shader.SetTexture(kernel, "System", room.waveEngine.systemTexture);
			shader.SetInt("centerX", pixelPosition.x);
			shader.SetInt("centerY", pixelPosition.y);
			shader.SetInt("radius", pixelRadius);
			shader.Dispatch(kernel, pixelRadius / 4, pixelRadius / 4, 1);
			Utilities.DrawCircle(circle, radius);
			circle.material.color = Color.green;
		}
		circle.material.color -= new Color(0, 0, 0, room.deltaTime / cooldown);
	}
}
