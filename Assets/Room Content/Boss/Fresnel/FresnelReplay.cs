using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FresnelReplay : RoomObjectBehaviour {

	public Material displacementReplay;
	public Material velocityReplay;
	public Texture2D[] holograms;
	public GameObject replayWidget;
	public Stack<GameObject> widgets = new Stack<GameObject>();
	public GameObject dampers;
	public float renderTime;
	float t;

	public void Import() {
		TextAsset[] hologramData = Resources.LoadAll<TextAsset>("Boss/Fresnel/");
		holograms = new Texture2D[hologramData.Length];
		while (widgets.Count > 0)
			Destroy(widgets.Pop());
		for (int i = 0; i < hologramData.Length; i++) {
			holograms[i] = new Texture2D(waveEngine.systemTexture.width, waveEngine.systemTexture.height,
										  TextureFormat.RGFloat, false);
			holograms[i].LoadRawTextureData(hologramData[i].bytes);
			holograms[i].Apply();
			GameObject widget = Instantiate(replayWidget, Vector3.zero, Quaternion.identity, GameObject.Find("Canvas").transform);
			widget.GetComponent<RectTransform>().anchoredPosition = new Vector3(10, -135 - 45 * i, 0);
			widget.transform.Find("Image").GetComponent<UnityEngine.UI.RawImage>().texture = holograms[i];
			widget.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = hologramData[i].name;
			int ii = i;
			widget.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => Replay(ii));
			widgets.Push(widget);
		}
	}

	public void Replay(int i) {
		((Room)room).ResetRoom();
		dampers.SetActive(false);
		Texture2D replayed = i == -1 ? GetComponent<FresnelCapture>().capture : holograms[i];
		Graphics.Blit(replayed, waveEngine.systemTexture, displacementReplay, -1, 0);
		Graphics.Blit(replayed, waveEngine.systemTexture, velocityReplay, -1, 1);
		t = renderTime;
	}

	void Start() {
		Import();
		float slowFactor = Mathf.Sqrt(5.0f / 255);
		//float flatDist = (2.4f - 0.05f) * 2 / 3;
		//float slopeDist = flatDist / 2;
		//renderTime = flatDist / (waveEngine.param.cScale * slowFactor) +
		//			 slopeDist * Mathf.Log(1.0f / slowFactor) / waveEngine.param.cScale / (1 - slowFactor);
		renderTime = 1 / waveEngine.param.cScale / slowFactor;
	}

	void Update() {
		t -= Time.deltaTime;
		if (t < 0 && t > -renderTime) {
			if (!dampers.activeSelf)
				dampers.SetActive(true);
		} else if (dampers.activeSelf)
			dampers.SetActive(false);
	}
}
