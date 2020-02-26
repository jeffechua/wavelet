using UnityEngine;
using UnityEngine.UI;
using System;

public class WaveEngine : MonoBehaviour {

	public static WaveEngine s_instance;

	public float t;

	public ComputeShader waveCompute;

	public Camera mediumCamera, sourcesCamera;
	public RenderTexture systemTexture, mediumTexture, sourcesTexture; // Do not assign

	RenderTexture systemDisplayTexture;

	// Simulation parameters
	public float pixelSize;
	int width, height;
	public float cScale;
	public float dampScale;
	public float intensityScale;
	public float frequencyScale;
	public float dt;
	public int f;


	float Sim_cScale { get => cScale / pixelSize; }

	int resetKernel, dispKernel, veloKernel, testKernel;
	uint tgsX, tgsY, tgsZ;

	void Awake() {

		s_instance = this;

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
		testKernel = waveCompute.FindKernel("Test");
		waveCompute.GetKernelThreadGroupSizes(dispKernel, out tgsX, out tgsY, out tgsZ);

		waveCompute.SetTexture(resetKernel, "System", systemTexture);

		waveCompute.SetTexture(dispKernel, "System", systemTexture);
		waveCompute.SetTexture(dispKernel, "Medium", mediumTexture);
		waveCompute.SetTexture(dispKernel, "Sources", sourcesTexture);

		waveCompute.SetTexture(veloKernel, "System", systemTexture);
		waveCompute.SetTexture(veloKernel, "Medium", mediumTexture);

		waveCompute.SetTexture(testKernel, "System", systemTexture);

		// Set up display texture
		systemDisplayTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
		systemDisplayTexture.Create();
		GetComponent<MeshRenderer>().material.SetTexture("_MainTex", systemDisplayTexture);

	}

	void FixedUpdate() {

		if (Input.GetKeyDown(KeyCode.Space)) {
			waveCompute.Dispatch(resetKernel, width / (int)tgsX, height / (int)tgsY, 1);
		}

		// Set some simulation and rendering parameters
		waveCompute.SetFloat("c2Scale", Sim_cScale * Sim_cScale);
		waveCompute.SetFloat("dampScale", dampScale);
		waveCompute.SetFloat("intensityScale", intensityScale);
		waveCompute.SetFloat("frequencyScale", frequencyScale);
		waveCompute.SetFloat("dt", dt);
		GetComponent<MeshRenderer>().material.SetFloat("_IntensityScale", intensityScale);

		// Simulate
		for (int i = 0; i < f; i++) {
			waveCompute.SetFloat("t", t);
			waveCompute.Dispatch(veloKernel, width / (int)tgsX, height / (int)tgsY, 1);
			waveCompute.Dispatch(dispKernel, width / (int)tgsX, height / (int)tgsY, 1);
			t += dt;
		}

		// Render
		Graphics.Blit(systemTexture, systemDisplayTexture, 0, 0);

	}

}
