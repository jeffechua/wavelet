using UnityEngine;
using System;

public class WaveEngine : MonoBehaviour {

	public static WaveEngine instance;

	// Rendering parameters
	public float amplitudeScale;
	public float amplitudeThreshold;
	public float subThresholdMultiplier;

	// Main compute shader
	public ComputeShader waveCompute;

	// Working and rendering assets
	public Camera mediumCamera, sourcesCamera;
	public RenderTexture systemTexture, mediumTexture, sourcesTexture, systemDisplayTexture; // Do not assign

	// Simulation parameters
	public float pixelSize;
	int width, height;

	public float timeScale = 1;

	public int frequency;
	int FrameFrequency { get => Mathf.RoundToInt(frequency * Time.fixedDeltaTime); }
	float Dt { get => 1.0f / frequency; }

	int _shaderSpace_t;
	public int shaderSpace_t { get => _shaderSpace_t; }
	public float t { get => _shaderSpace_t * Dt; }

	// Simulation parameters
	public float cScale;
	public float dampingScale;
	public float sourceFrequencyScale;

	float ShaderSpace_cScale { get => cScale / pixelSize * Dt; }
	float ShaderSpace_dampingScale { get => dampingScale * Dt; }
	float ShaderSpace_sourceFrequencyScale { get => sourceFrequencyScale * Dt * 2 * Mathf.PI; }

	// Compute shader data
	int resetKernel, dispKernel, veloKernel, testKernel;
	uint tgsX, tgsY, tgsZ;
	public int xGroups { get => Mathf.CeilToInt((float)width / tgsX); }
	public int yGroups { get => Mathf.CeilToInt((float)height / tgsY); }

	// Events
	public Action OnReset = delegate {};

	void Awake() {

		if (Mathf.Abs((frequency * Time.fixedDeltaTime) - FrameFrequency) > 0.01)
			print("Warning: true simulation frequency per game frame is not a whole number. Rounding.");

		instance = this;

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
			waveCompute.Dispatch(resetKernel, xGroups, yGroups, 1);
			_shaderSpace_t = 0;
			OnReset();
		}

		// Set some simulation and rendering parameters
		waveCompute.SetFloat("c2Scale", ShaderSpace_cScale * ShaderSpace_cScale);
		waveCompute.SetFloat("dampingScale", ShaderSpace_dampingScale);
		waveCompute.SetFloat("frequencyScale", ShaderSpace_sourceFrequencyScale);
		GetComponent<MeshRenderer>().material.SetFloat("_IntensityScale", amplitudeScale);
		GetComponent<MeshRenderer>().material.SetFloat("_Threshold", amplitudeThreshold);
		GetComponent<MeshRenderer>().material.SetFloat("_STMult", subThresholdMultiplier);

		// Simulate
		for (int i = 0; i < FrameFrequency * timeScale; i++) {
			waveCompute.SetInt("t", _shaderSpace_t);
			waveCompute.Dispatch(veloKernel, xGroups, yGroups, 1);
			waveCompute.Dispatch(dispKernel, xGroups, yGroups, 1);
			_shaderSpace_t++;
		}

		// Render
		Graphics.Blit(systemTexture, systemDisplayTexture, 0, 0);

	}

}
