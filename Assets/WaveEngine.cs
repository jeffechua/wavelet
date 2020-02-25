using UnityEngine;
using UnityEngine.UI;
using System;

public class WaveEngine : MonoBehaviour {

	public RawImage ICDisplay;
	public RawImage systemDisplay;
	RenderTexture systemDisplayTexture;

	public ComputeShader waveCompute;
	public Texture2D initialConditions;
	public RenderTexture systemTexture;

	public int width;
	public int height;

	public float c;
	public float dt;
	public int f;

	int resetKernel;
	int dispKernel;
	int veloKernel;

	Vector2 mousePosOld;

	ComputeBuffer forceWaitBuffer1;
	ComputeBuffer forceWaitBuffer2;

	void Awake() {

		systemTexture = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
		systemTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
		systemTexture.volumeDepth = 4;
		systemTexture.enableRandomWrite = true;
		systemTexture.Create();

		initialConditions = new Texture2D(width, height, TextureFormat.RGFloat, false);
		ICDisplay.texture = initialConditions;

		resetKernel = waveCompute.FindKernel("Reset");
		dispKernel = waveCompute.FindKernel("ComputeDisplacement");
		veloKernel = waveCompute.FindKernel("ComputeVelocity");
		waveCompute.SetTexture(resetKernel, "IC", initialConditions);
		waveCompute.SetTexture(resetKernel, "System", systemTexture);
		waveCompute.SetTexture(dispKernel, "System", systemTexture);
		waveCompute.SetTexture(veloKernel, "System", systemTexture);

		forceWaitBuffer1 = new ComputeBuffer(1, 4);
		forceWaitBuffer2 = new ComputeBuffer(1, 4);
		waveCompute.SetBuffer(dispKernel, "forceWait", forceWaitBuffer1);
		waveCompute.SetBuffer(veloKernel, "forceWait", forceWaitBuffer2);

		systemDisplayTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
		systemDisplayTexture.Create();
		systemDisplay.texture = systemDisplayTexture;

	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			waveCompute.Dispatch(resetKernel, width / 8, height / 8, 1);
		}
		for (int i = 0; i < f; i++) {
			int[] forceWaitResult = new int[1];
			waveCompute.SetFloat("c2", c * c);
			waveCompute.SetFloat("dt", dt);
			waveCompute.Dispatch(veloKernel, width / 8, height / 8, 1);
			forceWaitBuffer1.GetData(forceWaitResult);
			waveCompute.Dispatch(dispKernel, width / 8, height / 8, 1);
			forceWaitBuffer2.GetData(forceWaitResult);
		}
		Graphics.Blit(systemTexture, systemDisplayTexture, 0, 0);
	}

	private void OnDestroy() {
		forceWaitBuffer1.Dispose();
		forceWaitBuffer2.Dispose();
	}

}
