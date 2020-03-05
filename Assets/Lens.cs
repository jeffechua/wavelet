using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public readonly struct LensData {

	// 1/f = ∆n(2/R - t∆n/(nR^2))
	// With parameters f, n, R, H, t

	// R^2 = H^2 + (R-t/2)^2
	// H^2 = Rt - t^2/4
	// So eliminate R because nobody cares

	// Four parameters f, n, H, t with therefore 3 degrees of freedom

	public readonly float f;
	public readonly float n;
	public readonly float H;
	public readonly float t;
	public readonly float R;
	public readonly bool isReal;

	public float invN { get => 1 / n; }

	public LensData(float f, float n, float H, float t, float R) {
		this.f = f;
		this.n = n;
		this.H = H;
		this.t = t;
		this.R = R;
		float checksum = 1 / f - (n - 1) * (2 / R - (n - 1) * (t) / n / R / R);
		if (Mathf.Abs(checksum) > 1e-6) {
			isReal = false;
			Debug.Log("Lens is not real!\n" + ToString());
		} else {
			isReal = true;
		}
	}

	public static LensData fHt(float f, float H, float t) {
		float R = H * H / t / 4 + t / 4;
		// quadratic for n:
		float a = 2 * R - t;
		float b = 2 * t - 2 * R - R * R / f;
		float c = -t;
		float n = (-b + Mathf.Sqrt(b * b - 4 * a * c)) / 2 / a;
		return new LensData(f, n, H, t, R);
	}

	public static LensData fHn(float f, float H, float n) {

		// I don't like math. Let's do interval bisection instead.

		Tuple<float, LensData>[] broadScan = Array.ConvertAll(
			new float[] { .02f, .05f, .1f, .2f, .3f, .5f, .7f, .8f, .9f, 1f },
			(float i) => new Tuple<float, LensData>(i * H, fHt(f, H, i * H))
		);

		Tuple<float, LensData> lower = null;
		Tuple<float, LensData> higher = null;

		for (int i = 0; i < broadScan.Length - 1; i++) {
			if (broadScan[i].Item2.isReal && broadScan[i + 1].Item2.isReal &&
				(broadScan[i].Item2.n - n) * (broadScan[i + 1].Item2.n - n) < 0) {
				lower = broadScan[i];
				higher = broadScan[i + 1];
				break;
			}
		}

		if (lower == null) {
			Debug.Log("Interval could not be initialized :(");
			return fHt(f, H, 0.2f * H);
		}

		int j = 0;
		while (Mathf.Abs(lower.Item1 - higher.Item1) > WaveEngine.instance.pixelSize && j < 500) {
			float t_m = (lower.Item1 + higher.Item1) / 2;
			LensData n_m = fHt(f, H, t_m);
			if (Mathf.Sign(lower.Item2.n - n) == Mathf.Sign(n_m.n - n)) {
				lower = new Tuple<float, LensData>(t_m, n_m);
			} else {
				higher = new Tuple<float, LensData>(t_m, n_m);
			}
			j++;
		}

		return lower.Item2;
	}



	public float GetWidthAt(float distanceFromAxis) {
		return t / 2 - (R - Mathf.Sqrt(R * R - distanceFromAxis * distanceFromAxis));
	}

	public override string ToString() {
		return "f: " + f.ToString() + "\nn: " + n.ToString() + "\nH: " + H.ToString() + "\nt: " + t.ToString() + "\nR: " + R.ToString();
	}

}

public class Lens : MonoBehaviour {

	public LensData lensData;

	public LineRenderer mechanical;
	public LineRenderer graphical;

	public Gradient graphicalColor; // invN coordinate

	public Vector3 spec;
	public bool applyFHT; // R and n dependent
	public bool applyFHN; // R and t dependent


	// Start is called before the first frame update
	void DrawLens(LensData lens) {
		lensData = lens;
		print(lens.ToString());
		int vertexCount = (int)(lens.H * 10);
		Vector3[] positions = new Vector3[vertexCount];
		Keyframe[] widths = new Keyframe[vertexCount];
		for (int i = 0; i < vertexCount; i++) {
			float normY = 1.0f * i / (vertexCount - 1);
			float actualY = lens.H * (normY - 0.5f);
			positions[i] = new Vector3(actualY, 0, 0);
			widths[i] = new Keyframe(normY, lens.GetWidthAt(actualY));
		}
		mechanical.positionCount = vertexCount;
		mechanical.SetPositions(positions);
		mechanical.widthCurve = new AnimationCurve(widths);
		mechanical.startColor = new Color(0, lens.invN * lens.invN, 1);
		mechanical.endColor = mechanical.startColor;
		graphical.positionCount = vertexCount;
		graphical.SetPositions(positions);
		graphical.widthCurve = new AnimationCurve(widths);
		graphical.startColor = graphicalColor.Evaluate(lens.invN);
		graphical.endColor = graphical.startColor;
	}

	void Update() {
		// For some reason +1 to focal length works well. :/
		if (applyFHT) {
			DrawLens(LensData.fHt(spec.x+1, spec.y, spec.z));
			applyFHT = false;
		}
		if (applyFHN) {
			DrawLens(LensData.fHn(spec.x+1, spec.y, spec.z));
			applyFHN = false;
		}
	}
}
