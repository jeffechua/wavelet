using UnityEngine;
using UnityEngine.UI;

public class Draw : MonoBehaviour {

	public float drawRadius;
	public Color drawColor;

	Texture2D tex;
	int width;
	int height;

	Vector2 mousePosOld;

	void Start() {
		tex = (Texture2D)GetComponent<RawImage>().texture;
		width = tex.width;
		height = tex.height;
		Clean();
	}

	void Update() {

		switch (Input.inputString) {
			case "1":
				drawColor = new Color(1, 0, 0, 1);
				break;
			case "2":
				drawColor = new Color(0, 1, 0, 1);
				break;
			default:
				break;
		}

		if (Input.GetMouseButtonDown(0)) {
			mousePosOld = LocalMousePosition;
		} else if (Input.GetMouseButton(0)) {
			Vector2 mousePos = LocalMousePosition;
			int intDrawRadius = Mathf.CeilToInt(drawRadius);
			for (Vector2 pos = mousePosOld; pos != mousePos; pos = Vector2.MoveTowards(pos, mousePos, 1))
				for (int i = -intDrawRadius; i <= intDrawRadius; i++)
					for (int j = -intDrawRadius; j <= intDrawRadius; j++)
						if (Mathf.Sqrt(i * i + j * j) <= drawRadius)
							tex.SetPixel(Mathf.RoundToInt(pos.x) + i, Mathf.RoundToInt(pos.y) + j, drawColor);
			mousePosOld = mousePos;
			tex.Apply();
		}

		drawRadius += Input.GetAxis("Mouse ScrollWheel");

		if(Input.GetKeyDown(KeyCode.R)) {
			Clean();
		}

	}

	void Clean() {
		tex.LoadRawTextureData(new byte[width * height * 16]);
		tex.Apply();
	}


	Vector2 LocalMousePosition {
		get => (Vector2)Input.mousePosition + GetComponent<RectTransform>().rect.min - (Vector2)transform.position;
	}

}
