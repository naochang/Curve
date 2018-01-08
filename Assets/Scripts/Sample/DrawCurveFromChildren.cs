using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using Curve;

namespace Sample {
	/// <summary>
	/// 子要素にあるHandlePointからカーブを描く。
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(LineRenderer))]
	public class DrawCurveFromChildren : MonoBehaviour {
		/// <summary>
		/// カーブタイプ。
		/// </summary>
		enum CurveType {
			None,
			StraightLine,
			HermiteCurve,
			BezierCurve,
			RationalBezierCurve,
			BSplineCurve,
			NURBSCurve,
			CatmullRomSplineCurve
		}
			
		/// <summary>
		/// カーブのタイプ。
		/// </summary>
		[SerializeField]
		CurveType curveType;

		/// <summary>
		/// 表示用。
		/// </summary>
		[SerializeField]
		Text typeTxt;

		/// <summary>
		/// カーブのロジック。
		/// </summary>
		CurveBase curve;

		/// <summary>
		/// 線の描画。
		/// </summary>
		LineRenderer lineRenderer;

		/// <summary>
		/// 初期化済みかどうか。
		/// </summary>
		bool initFlag = false;

		/// <summary>
		/// Awake.
		/// </summary>
		void Awake() {
			if (!this.initFlag) {
				this.curve = this.gameObject.GetComponent<CurveBase>();
				this.lineRenderer = this.gameObject.GetComponent<LineRenderer>();

				if (this.curve != null) {
					this.curve.ValueChanged += this.OnCurveEvaluatorValueChanged;
					this.typeTxt.text = this.curveType.ToString();
					this.DrawCurve();
				}

				this.initFlag = true;
			}
		}

		/// <summary>
		/// Update.
		/// </summary>
		void Update() {
			this.DrawCurve();
		}

		/// <summary>
		/// OnValidate.
		/// </summary>
		void OnValidate() {
			if (this.initFlag)
				this.ChangeCurveType(this.curveType);
		}

		/// <summary>
		/// 閉じるかどうかを設定する。
		/// </summary>
		public void SetClosed(bool closed) {
			if (this.curve != null) {
				this.curve.closed = closed;
			}
		}

		/// <summary>
		/// カーブタイプの変更。
		/// </summary>
		void ChangeCurveType(CurveType curveType) {
			if (this.curve != null &&
				((curveType == CurveType.StraightLine && this.curve.GetType() == typeof(StraightLine)) ||
				(curveType == CurveType.HermiteCurve && this.curve.GetType() == typeof(HermiteCurve)) ||
				(curveType == CurveType.BezierCurve && this.curve.GetType() == typeof(BezierCurve)) ||
				(curveType == CurveType.RationalBezierCurve && this.curve.GetType() == typeof(RationalBezierCurve)) ||
				(curveType == CurveType.BSplineCurve && this.curve.GetType() == typeof(BSplineCurve)) ||
				(curveType == CurveType.NURBSCurve && this.curve.GetType() == typeof(NURBSCurve))) ||
				(curveType == CurveType.CatmullRomSplineCurve && this.curve.GetType() == typeof(CatmullRomSplineCurve))) {

				return;
			}

			this.curveType = curveType;
			this.typeTxt.text = this.curveType.ToString();

			// see : http://anchan828.hatenablog.jp/entry/2013/11/18/012021
			EditorApplication.delayCall += () => {
				if (this.curve != null) {
					this.curve.ValueChanged -= this.OnCurveEvaluatorValueChanged;
					DestroyImmediate(this.curve);
					this.curve = null;
				}

				switch (curveType) {
					case CurveType.None :
						return;
					break;

					case CurveType.StraightLine :
						this.curve = this.gameObject.AddComponent<StraightLine>();
					break;

					case CurveType.HermiteCurve :
						this.curve = this.gameObject.AddComponent<HermiteCurve>();
					break;

					case CurveType.BezierCurve :
						this.curve = this.gameObject.AddComponent<BezierCurve>();
					break;

					case CurveType.RationalBezierCurve :
						this.curve = this.gameObject.AddComponent<RationalBezierCurve>();
					break;

					case CurveType.BSplineCurve :
						this.curve = this.gameObject.AddComponent<BSplineCurve>();
					break;

					case CurveType.NURBSCurve :
						this.curve = this.gameObject.AddComponent<NURBSCurve>();
					break;

					case CurveType.CatmullRomSplineCurve :
						this.curve = this.gameObject.AddComponent<CatmullRomSplineCurve>();
					break;
				}

				if (this.curve != null) {
					this.curve.ValueChanged += this.OnCurveEvaluatorValueChanged;
					this.DrawCurve();
				}
			};
		}

		/// <summary>
		/// 描画。
		/// </summary>
		void DrawCurve() {
			HandlePoint[] handlePoints = this.transform.GetComponentsInChildren<HandlePoint>();
			Vector3[] points = null;
			points = new Vector3[handlePoints.Length];
			for (int i = 0; i < handlePoints.Length; ++i) {
				points[i] = handlePoints[i].Point;
			}

			switch (this.curveType) {
				case CurveType.StraightLine :
					Array.ConvertAll(handlePoints, handlePoint => handlePoint.Handle = HandlePoint.HandleType.None);
					this.curve.SetPoints(points);
				break;

				case CurveType.HermiteCurve :
					Array.ConvertAll(handlePoints, handlePoint => handlePoint.Handle = HandlePoint.HandleType.One);

					Vector3[] vectors = new Vector3[handlePoints.Length];
					for (int i = 0; i < handlePoints.Length; ++i) {
						vectors[i] = handlePoints[i].GetHandle();
					}

					((HermiteCurve)this.curve).SetPoints(points, vectors);
				break;

				case CurveType.BezierCurve :
					int BezierDegree = 3;
					int BezierHandleCount = BezierDegree - 1;
					Vector3[] bezierHandles = new Vector3[handlePoints.Length * BezierHandleCount];

					for (int i = 0; i < handlePoints.Length; ++i) {
						bezierHandles[i * BezierHandleCount] = handlePoints[i].GetHandle(1);
						bezierHandles[i * BezierHandleCount + 1] = handlePoints[i].GetHandle(2);
					}

					Array.ConvertAll(handlePoints, handlePoint => handlePoint.Handle = HandlePoint.HandleType.Two);
					((BezierCurve)this.curve).SetPoints(points, bezierHandles);
				break;

				case CurveType.RationalBezierCurve :
					int RationalBezierDegree = 3;
					int RationalBezierHandleCount = RationalBezierDegree - 1;
					Vector3[] rationalBezierHandles = new Vector3[handlePoints.Length * RationalBezierHandleCount];
					float[] rationalBezierWeights = new float[handlePoints.Length * RationalBezierDegree];

					for (int i = 0; i < handlePoints.Length; ++i) {
						rationalBezierHandles[i * RationalBezierHandleCount] = handlePoints[i].GetHandle(1);
						rationalBezierHandles[i * RationalBezierHandleCount + 1] = handlePoints[i].GetHandle(2);

						rationalBezierWeights[i * RationalBezierDegree] = handlePoints[i].handle1Weight;
						rationalBezierWeights[i * RationalBezierDegree + 1] = handlePoints[i].pointWeight;
						rationalBezierWeights[i * RationalBezierDegree + 2] = handlePoints[i].handle2Weight;
					}

					Array.ConvertAll(handlePoints, handlePoint => handlePoint.Handle = HandlePoint.HandleType.Two);
					((RationalBezierCurve)this.curve).SetPoints(points, rationalBezierHandles, rationalBezierWeights);
					break;

				case CurveType.BSplineCurve :
				case CurveType.NURBSCurve :
				case CurveType.CatmullRomSplineCurve :
					Array.ConvertAll(handlePoints, handlePoint => handlePoint.Handle = HandlePoint.HandleType.None);
					this.curve.SetPoints(points);
				break;

				default :
				break;
			}

			if (this.curve != null) {
				Vector3[] linePoints = this.curve.Evaluate();

				if (linePoints != null) {
					this.lineRenderer.enabled = true;
					this.lineRenderer.loop = this.curve.closed;
					this.lineRenderer.useWorldSpace = false;
					this.lineRenderer.positionCount = linePoints.Length;
					this.lineRenderer.SetPositions(linePoints);
				}
				else {
					this.lineRenderer.enabled = false;
					this.lineRenderer.positionCount = 0;
				}
			}
			else {
				this.lineRenderer.enabled = false;
				this.lineRenderer.positionCount = 0;
			}
		}

		/// <summary>
		/// CurveEvaluatorの設定値が変わった。
		/// </summary>
		void OnCurveEvaluatorValueChanged(object sender, EventArgs evt) {
			this.DrawCurve();
		}
	}
}