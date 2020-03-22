using UnityEngine;
using System;

// Note on rendering:

// FOR GRAPHICAL:

// All things should be either
// 1) z = 0, rendering in front of the wave plane
// 2) z = 1, which should only be the wave plane, and
// 3) z = 2, rendering behind the wave plane (mainly border, and things that block the border

// In z=0, opaque things should be in sorting order 0. Transparent things then go either in front or behind.
// The border should be in sorting order -10.

// FOR MECHANICAL:

// All things should have z=0
// The border should have sorting order -1.


[Serializable]
public class WaveEngineParams { // Intensive properties of the simulation
	public float amplitudeScale = 5;
	public float amplitudeThreshold = 0.5f;
	public float subThresholdMultiplier = 0.5f;
	public float pixelSize = 0.025f;
	public int frequency = 500;
	public float cScale = 7;
	public float dampingScale = 75;
	public float sourceFrequencyScale = 60;
}

public class WaveEngine : RoomObjectBehaviour {

	public static WaveEngine active;

	public bool autoInitialize;

	// All intensive parameters of the simulation
	public WaveEngineParams param;

	// Main compute shader
	public ComputeShader waveCompute;

	// Working and rendering assets
	public Camera mediumCamera, sourcesCamera;
	public RenderTexture systemTexture, mediumTexture, sourcesTexture, systemDisplayTexture; // Do not assign
	MeshRenderer renderer;
	
	// Dependent space properties
	int width, height; // in simulation pixels

	// Dependent time properties
	int FrameFrequency { get => Mathf.RoundToInt(param.frequency * room.deltaTime); }
	float Dt { get => 1.0f / param.frequency; } // conversion factor between one shader space time unit and a room second.

	// Conversions to values for shader calculations.
	int _shaderSpace_t;
	public int shaderSpace_t { get => _shaderSpace_t; }
	public float t { get => _shaderSpace_t * Dt; }
	float ShaderSpace_cScale { get => param.cScale / param.pixelSize * Dt; }
	float ShaderSpace_dampingScale { get => param.dampingScale * Dt; }
	float ShaderSpace_sourceFrequencyScale { get => param.sourceFrequencyScale * Dt * 2 * Mathf.PI; }

	// Compute shader data
	int resetKernel, dispKernel, veloKernel, testKernel;
	uint tgsX, tgsY, tgsZ;
	public int xGroups { get => Mathf.CeilToInt((float)width / tgsX); }
	public int yGroups { get => Mathf.CeilToInt((float)height / tgsY); }


	void Awake () {
		renderer = GetComponent<MeshRenderer>();
		renderer.sortingLayerName = "WavePlane";
		if (autoInitialize) {
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

	public void Initialize() {

		if(systemTexture != null) {
			print("Warning: wave engine has already been initialized. Aborting.");
			return;
		}
		
		width = Mathf.RoundToInt(transform.localScale.x / param.pixelSize);
		height = Mathf.RoundToInt(transform.localScale.y / param.pixelSize);

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
		renderer.material.SetTexture("_MainTex", systemDisplayTexture);

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
		renderer.material.SetFloat("_IntensityScale", param.amplitudeScale);
		renderer.material.SetFloat("_Threshold", param.amplitudeThreshold);
		renderer.material.SetFloat("_STMult", param.subThresholdMultiplier);

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

	public Vector2Int WorldToTexturePoint (Vector2 point) {
		return Vector2Int.RoundToInt(((Vector3)point - transform.position +
			transform.localScale / 2) / param.pixelSize);
	}

}
