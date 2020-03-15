﻿using UnityEngine;
using System;

[Serializable]
public struct WaveEngineParams {
	public float amplitudeScale;
	public float amplitudeThreshold;
	public float subThresholdMultiplier;
	public float pixelSize;
	public int frequency;
	public float cScale;
	public float dampingScale;
	public float sourceFrequencyScale;
}

public class WaveEngine : RoomObject {

	public static WaveEngine active;

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

	public int frequency;
	int FrameFrequency { get => Mathf.RoundToInt(frequency * Time.fixedDeltaTime * (room ? room.timeScale : 1)); }
	float Dt { get => 1.0f / frequency; } // conversion factor between one shader space time unit and a room second.

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


	void Awake () {
		if(pixelSize != 0) {
			Initialize();
			SetActive();
		}
	}

	public void SetActive () {
		active = this;
		waveCompute.SetTexture(resetKernel, "System", systemTexture);

		waveCompute.SetTexture(dispKernel, "System", systemTexture);
		waveCompute.SetTexture(dispKernel, "Medium", mediumTexture);
		waveCompute.SetTexture(dispKernel, "Sources", sourcesTexture);

		waveCompute.SetTexture(veloKernel, "System", systemTexture);
		waveCompute.SetTexture(veloKernel, "Medium", mediumTexture);

		waveCompute.SetTexture(testKernel, "System", systemTexture);
	}

	public void LoadParams (WaveEngineParams p, Vector2 size) {
		amplitudeScale = p.amplitudeScale;
		amplitudeThreshold = p.amplitudeThreshold;
		subThresholdMultiplier = p.subThresholdMultiplier;
		pixelSize = p.pixelSize;
		frequency = p.frequency;
		cScale = p.cScale;
		dampingScale = p.dampingScale;
		sourceFrequencyScale = p.sourceFrequencyScale;
		transform.localScale = new Vector3(size.x, size.y, 1);
	}

	public void Initialize() {

		if(systemTexture != null) {
			print("Warning: wave engine has already been initialized. Aborting.");
			return;
		}
		
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

		// Set up display texture
		systemDisplayTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
		systemDisplayTexture.Create();
		GetComponent<MeshRenderer>().material.SetTexture("_MainTex", systemDisplayTexture);

	}

	void Reset() {
		waveCompute.Dispatch(resetKernel, xGroups, yGroups, 1);
		_shaderSpace_t = 0;
	}

	void FixedUpdate() {

		if (active != this)
			return;

		// Set some simulation and rendering parameters
		waveCompute.SetFloat("c2Scale", ShaderSpace_cScale * ShaderSpace_cScale);
		waveCompute.SetFloat("dampingScale", ShaderSpace_dampingScale);
		waveCompute.SetFloat("frequencyScale", ShaderSpace_sourceFrequencyScale);
		GetComponent<MeshRenderer>().material.SetFloat("_IntensityScale", amplitudeScale);
		GetComponent<MeshRenderer>().material.SetFloat("_Threshold", amplitudeThreshold);
		GetComponent<MeshRenderer>().material.SetFloat("_STMult", subThresholdMultiplier);

		// Simulate
		for (int i = 0; i < FrameFrequency; i++) {
			waveCompute.SetInt("t", _shaderSpace_t);
			waveCompute.Dispatch(veloKernel, xGroups, yGroups, 1);
			waveCompute.Dispatch(dispKernel, xGroups, yGroups, 1);
			_shaderSpace_t++;
		}

		// Render
		Graphics.Blit(systemTexture, systemDisplayTexture, 0, 0);

	}

}
