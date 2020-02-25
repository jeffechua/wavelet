using UnityEngine;
using UnityEngine.UI;
using System;

public class WaveEngine : MonoBehaviour {

	public ComputeShader waveCompute;
	public Material systemRender;

	public Camera mediumCamera;
	public Camera sourcesCamera;
	public RenderTexture systemTexture;
	public RenderTexture mediumTexture;
	public RenderTexture sourcesTexture;

	RenderTexture systemDisplayTextureIntermediate;
	RenderTexture systemDisplayTexture;

	public float pixelSize;
	int width;
	int height;

	public float cScale;
	public float dampScale;
	public float frequencyScale;
	float Sim_cScale { get => cScale / pixelSize; }

	public float dt;
	public int f;

	float t;

	int resetKernel;
	int dispKernel;
	int veloKernel;

	ComputeBuffer forceWaitBuffer1;
	ComputeBuffer forceWaitBuffer2;

	void Awake() {

		width = Mathf.RoundToInt(transform.localScale.x / pixelSize);
		height = Mathf.RoundToInt(transform.localScale.y / pixelSize);

		// Create and initialize working RenderTextures
		systemTexture = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
		systemTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
		systemTexture.volumeDepth = 2;
		systemTexture.filterMode = FilterMode.Point;
		systemTexture.enableRandomWrite = true;
		systemTexture.Create();

		mediumTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
		mediumTexture.filterMode = FilterMode.Point;
		mediumTexture.Create();
		mediumCamera.targetTexture = mediumTexture;
		mediumCamera.orthographicSize = transform.localScale.y / 2;

		sourcesTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
		sourcesTexture.filterMode = FilterMode.Point;
		sourcesTexture.Create();
		sourcesCamera.targetTexture = sourcesTexture;
		sourcesCamera.orthographicSize = transform.localScale.y / 2;

		// Set up kernels
		resetKernel = waveCompute.FindKernel("Reset");
		dispKernel = waveCompute.FindKernel("ComputeDisplacement");
		veloKernel = waveCompute.FindKernel("ComputeVelocity");

		waveCompute.SetTexture(resetKernel, "System", systemTexture);

		waveCompute.SetTexture(dispKernel, "System", systemTexture);
		waveCompute.SetTexture(dispKernel, "Medium", mediumTexture);
		waveCompute.SetTexture(dispKernel, "Sources", sourcesTexture);

		waveCompute.SetTexture(veloKernel, "System", systemTexture);
		waveCompute.SetTexture(veloKernel, "Medium", mediumTexture);

		// Set up ComputeBuffers to force waits
		forceWaitBuffer1 = new ComputeBuffer(1, 4);
		forceWaitBuffer2 = new ComputeBuffer(1, 4);
		waveCompute.SetBuffer(dispKernel, "forceWait", forceWaitBuffer1);
		waveCompute.SetBuffer(veloKernel, "forceWait", forceWaitBuffer2);

		// Set up display texture
		systemDisplayTextureIntermediate = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
		systemDisplayTextureIntermediate.Create();
		systemDisplayTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
		systemDisplayTexture.Create();
		GetComponent<MeshRenderer>().material.SetTexture("_MainTex", systemDisplayTexture);

	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			waveCompute.Dispatch(resetKernel, width / 8, height / 8, 1);
		}
		mediumCamera.Render();
		sourcesCamera.Render();
		for (int i = 0; i < f; i++) {
			int[] forceWaitResult = new int[1];
			waveCompute.SetFloat("c2Scale", Sim_cScale * Sim_cScale);
			waveCompute.SetFloat("dampScale", dampScale);
			waveCompute.SetFloat("frequencyScale", frequencyScale);
			waveCompute.SetFloat("dt", dt);
			waveCompute.SetFloat("t", t);
			waveCompute.Dispatch(veloKernel, width / 8, height / 8, 1);
			forceWaitBuffer1.GetData(forceWaitResult);
			waveCompute.Dispatch(dispKernel, width / 8, height / 8, 1);
			forceWaitBuffer2.GetData(forceWaitResult);
			t += dt;
		}
		Graphics.Blit(systemTexture, systemDisplayTextureIntermediate, 0, 0);
		Graphics.Blit(systemDisplayTextureIntermediate, systemDisplayTexture, systemRender);
	}

	private void OnDestroy() {
		forceWaitBuffer1.Dispose();
		forceWaitBuffer2.Dispose();
	}

}
