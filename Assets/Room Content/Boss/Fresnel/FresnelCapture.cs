using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FresnelCapture : RoomObjectBehaviour {
	public string exportPath = "~/Test";
	public Hitbox[] hitboxes;
	RenderTexture intermediate1;
	RenderTexture intermediate2;
	public static Texture2D capture;
	public UnityEngine.UI.RawImage preview;
	public UnityEngine.UI.Button primeButton;
	public Material redMapper;
	public Material blueMapper;
	public static bool primed;

	void Capture() {
		RenderTexture system = waveEngine.systemTexture;
		Graphics.Blit(system, intermediate1, 0, 0);
		Graphics.Blit(intermediate1, intermediate2, redMapper);
		Graphics.Blit(system, intermediate1, 1, 0);
		Graphics.Blit(intermediate1, intermediate2, blueMapper);
		RenderTexture.active = intermediate2;
		capture.ReadPixels(new Rect(0, 0, system.width, system.height), 0, 0);
		capture.Apply();
		RenderTexture.active = null;
	}

	public void SetExportName(string name) {
		exportPath = "~/" + name;
	}

	public void Export() {
		string path = exportPath.StartsWith("~") ? exportPath.Replace("~", "Assets/Resources/Boss/Fresnel") : exportPath;
		while (File.Exists(path + ".bytes"))
			path += "\'";
		path += ".bytes";
		File.WriteAllBytes(path, capture.GetRawTextureData());
	}

	public void Prime() {
		((Room)room).ResetRoom();
		capture.LoadRawTextureData(new byte[capture.width * capture.height * 32 * 4]);
		capture.Apply();
		Graphics.Blit(capture, intermediate1);
		Graphics.Blit(capture, intermediate2);
		primeButton.interactable = false;
		StartCoroutine(PrimeSoon());
	}

	IEnumerator PrimeSoon() {
		yield return new WaitForSeconds(0.5f);
		primed = true;
	}

	void Start() {
		RenderTexture system = waveEngine.systemTexture;
		intermediate1 = new RenderTexture(system.width, system.height, 0, RenderTextureFormat.ARGBFloat, 0);
		intermediate1.filterMode = FilterMode.Point;
		intermediate1.Create();
		intermediate2 = new RenderTexture(system.width, system.height, 0, RenderTextureFormat.ARGBFloat, 0);
		intermediate2.filterMode = FilterMode.Point;
		intermediate2.Create();
		capture = new Texture2D(system.width, system.height, TextureFormat.RGFloat, false);
		capture.filterMode = FilterMode.Point;
		preview.texture = capture;
	}

	// Update is called once per frame
	void Update() {
		if (primed) {
			foreach (Hitbox hitbox in hitboxes) {
				if (hitbox.damageIntegralRaw > 0) {
					Capture();
					primed = false;
					primeButton.interactable = true;
					return;
				}
			}
		}
	}
}
