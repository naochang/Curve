using UnityEngine;
using System;

namespace Curve {
	public class BSplineCurve : CurveBase {
		// 参照 : http://d.hatena.ne.jp/nakamura001/20150117/1421501942
		protected static float Epsilon = 1.192093E-07f;

		/// <summary>
		/// Bスプライン基底関数。
		/// </summary>
		protected static float BSplineBasisFunc(int i, int degree, float t, float[] knots) {
			if (degree == 0) {
				if (t >= knots[i] && t < knots[i + 1])
					return 1f;
				else
					return 0f;
			}

			float w1 = 0f;
			float w2 = 0f;
			float denominatorA = knots[i + degree] - knots[i];
			float denominatorB = knots[i + degree + 1] - knots[i + 1];

			if (denominatorA != 0f)
				w1 = (t - knots[i]) / denominatorA;

			if (denominatorB != 0f)
				w2 = (knots[i + degree + 1] - t) / denominatorB;

			return w1 * BSplineCurve.BSplineBasisFunc(i, degree - 1, t, knots) + w2 * BSplineCurve.BSplineBasisFunc(i + 1, degree - 1, t, knots);
		}

		/// <summary>
		/// 次数。
		/// </summary>
		[SerializeField, Range(1, 10)]
		protected int degree = 3;
		public int Degree {
			get {
				return this.degree;
			}
			set {
				this.degree = value;
				this.OnValidate();
			}
		}

		/// <summary>
		/// 端点を通るかどうか。
		/// </summary>
		[SerializeField]
		protected bool passOnEdge = false;
		public virtual bool PassOnEdge {
			get {
				return this.passOnEdge;
			}
			set {
				this.passOnEdge = value;
				this.OnValidate();
			}
		}

		/// <summary>
		/// ノット。
		/// </summary>
		[SerializeField]
		protected float[] knots;

		/// <summary>
		/// 点の設定。
		/// </summary>
		public override void SetPoints(Vector3[] points) {
			base.SetPoints(this.closed ? this.CreatePointsAsClosed(points) : points);

			this.ResetKnots();
		}

		/// <summary>
		/// 線を引く点を評価し返す。
		/// </summary>
		public override Vector3[] Evaluate() {
			if (this.degree < 1)
				throw new Exception("次数の設定が相応しくありません。");
			else if (this.points == null || this.points.Length < 2 || this.points.Length < this.degree + 1)
				throw new Exception("必要な数の点が設定されていません。");


			// 分割数のチェック
			if (this.dividedCount < 1)
				this.dividedCount = 1;

			// 1次の場合は直線で結ぶ
			if (this.degree == 1) {
				return this.points;
			}


			Vector3[] linePoints;

			// 2次もしくは3次で一様であれば分割数を再定義
			if (this.GetType() == typeof(BSplineCurve) && (this.degree == 2 || this.degree == 3) && !this.PassOnEdge) {
				int lineCount = this.points.Length - 1;
				int repeatCount = this.points.Length - this.degree;

				// 分割数のチェック
				if (this.dividedCount % repeatCount != 0)
					this.dividedCount = repeatCount * Mathf.FloorToInt(this.dividedCount / repeatCount);

				if (this.dividedCount < 1)
					this.dividedCount = repeatCount;

				linePoints = new Vector3[this.dividedCount + 1];

				int eachDividedCount = this.dividedCount / repeatCount;
				float step = 1f / eachDividedCount;

				for (int i = 0; i < repeatCount; ++i) {
					for (int p = 0; p < eachDividedCount; ++p) {
						linePoints[i * eachDividedCount + p] = this.degree == 2 ? this.CalcurateUniformBSplinePointWithDegree2(i, p  * step) : this.CalcurateUniformBSplinePointWithDegree3(i, p  * step);
					}

					if (i == repeatCount - 1)
						linePoints[(i + 1) * eachDividedCount] = this.degree == 2 ? this.CalcurateUniformBSplinePointWithDegree2(i, 1f) : this.CalcurateUniformBSplinePointWithDegree3(i, 1f);
				}

				return linePoints;
			}
			else {
				linePoints = new Vector3[this.dividedCount + 1];

				float lowKnot = this.knots[this.degree];
				float highKnot = this.knots[this.points.Length];
				float step = (highKnot - lowKnot) / this.dividedCount;

				for (int p = 0; p <= this.dividedCount; ++p) {
					float t = p * step + lowKnot;
					if (t >= highKnot) t = highKnot - BSplineCurve.Epsilon;

					linePoints[p] = this.CalcuratePoint(t);
				}
			}

			return linePoints;
		}

		/// <summary>
		/// 指定の位置の座標を取得。
		/// </summary>
		protected virtual Vector3 CalcuratePoint(float t) {
			Vector3 linePoint = Vector3.zero;

			for (int i = 0; i < this.points.Length; ++i) {
				float bs = BSplineCurve.BSplineBasisFunc(i, this.degree, t, this.knots);
				linePoint += bs * this.points[i];
			}

			return linePoint;
		}

		/// <summary>
		/// 2次での一様なBスプライン基底関数。
		/// </summary>
		Vector3 CalcurateUniformBSplinePointWithDegree2(int i, float t) {
			return 0.5f * (t * t - 2f * t + 1f) * this.points[i] +
				0.5f * (-2f * t * t + 2f * t + 1f) * this.points[i + 1] +
				0.5f * t * t * this.points[i + 2];
		}

		/// <summary>
		/// 3次での一様なBスプライン基底関数。
		/// </summary>
		Vector3 CalcurateUniformBSplinePointWithDegree3(int i, float t) {
			return 1f/6f * (-this.points[i] + 3f * this.points[i + 1] - 3f * this.points[i + 2] + this.points[i + 3]) * t * t * t +
				0.5f * (this.points[i] - 2f * this.points[i + 1] + this.points[i + 2]) * t * t +
				0.5f * (-this.points[i] + this.points[i + 2]) * t +
				1f/6f * (this.points[i] + 4f * this.points[i + 1] + this.points[i + 2]);
		}

		/// <summary>
		/// ノットをリセット。
		/// </summary>
		protected void ResetKnots() {
			if (this.points == null)
				return;
			
			this.knots = new float[this.points.Length + this.degree + 1];

			float knot = 0;

			for (int i = 0; i < this.knots.Length; ++i) {
				this.knots[i] = knot;
				if (!this.passOnEdge || (i >= this.degree && i <= this.knots.Length - this.degree - 2)) {
					++knot;
				}
			}
		}

		/// <summary>
		/// 最終の座標が始点と同じになるポイントの配列を作る。
		/// </summary>
		protected Vector3[] CreatePointsAsClosed(Vector3[] points) {
			Vector3[] tmpPoints = new Vector3[points.Length + this.degree];
			points.CopyTo(tmpPoints, 0);

			for (int i = 0; i < this.degree; ++i) {
				tmpPoints[tmpPoints.Length - (this.degree - i)] = tmpPoints[i];
			}

			return tmpPoints;
		}

		/// <summary>
		/// OnValidate.
		/// </summary>
		protected override void OnValidate() {
			if (this.closed)
				this.passOnEdge = false;

			if (this.points != null)
				this.ResetKnots();
			
			base.OnValidate();
		}
	}
}