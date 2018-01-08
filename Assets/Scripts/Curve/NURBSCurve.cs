using UnityEngine;
using System;

namespace Curve {
	/// <summary>
	/// Bスプラインを継承し、ノットと重みの値の変更ができるように。
	/// </summary>
	public class NURBSCurve : BSplineCurve {
		/// <summary>
		/// 重み。
		/// </summary>
		[SerializeField]
		float[] weights;

		/// <summary>
		/// 点の設定。
		/// 数に変更がなければ値を維持。
		/// </summary>
		public override void SetPoints(Vector3[] points) {
			this.points = this.closed ? this.CreatePointsAsClosed(points) : points;

			if (this.knots == null || this.points.Length + this.degree + 1 != this.knots.Length)
				this.ResetKnots();

			if (this.weights == null || this.weights.Length != this.points.Length + (this.closed ? this.degree : 0))
				this.ResetWeights();
		}

		/// <summary>
		/// 重みも考慮し、指定の位置の座標を取得。
		/// </summary>
		protected override Vector3 CalcuratePoint(float t) {
			Vector3 linePoint = Vector3.zero;
			float weight = 0f;

			for (int i = 0; i < this.points.Length; ++i) {
				float bs = this.BSplineBasisFunc(i, this.degree, t);
				linePoint += bs * this.weights[i] * this.points[i];
				weight += bs * this.weights[i];
			}

			return linePoint / weight;
		}

		/// <summary>
		/// 重みのリセット。
		/// </summary>
		void ResetWeights() {
			if (this.points == null)
				return;

			this.weights = new float[this.points.Length + (this.closed ? this.degree : 0)];

			for (int i = 0; i < this.weights.Length; ++i) {
				this.weights[i] = 1f;
			}
		}

		/// <summary>
		/// OnValidate.
		/// </summary>
		protected override void OnValidate() {
			if (this.closed || this.passOnEdge) {
				Debug.LogWarning("サポートされていません。knots及びweightsを直接操作する必要があります。");
				this.closed = false;
				this.passOnEdge = false;
			}

			float prevKnot = int.MinValue;
			foreach (float knot in this.knots) {
				if (knot < prevKnot)
					throw new Exception("ノットの値が前後のノットより小さい、もしくは大きくなっています。");
				prevKnot = knot;
			}

			if (this.points != null) {
				if (this.knots.Length != this.points.Length + this.degree + 1)
					this.ResetKnots();

				if (this.weights.Length != this.points.Length)
					this.ResetWeights();
			}

			this.DispatchValueChanged();
		}
	}
}