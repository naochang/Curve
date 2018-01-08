using UnityEngine;
using System;
using System.Collections.Generic;

namespace Curve {
	public class RationalBezierCurve : BezierCurve {
		/// <summary>
		/// 指定のポイントの取得。
		/// </summary>
		static Vector3 GetPointWithWeights(Vector3[] points, float[] weights, float t) {
			if (points.Length < 3)
				throw new Exception("点は3つ以上指定する必要があります。");
			if (points.Length != weights.Length)
				throw new Exception("点と重みの数が違います。");

			Vector3 result = Vector3.zero;
			float weight = 0f;

			int n = points.Length;
			for (int i = 0; i < n; ++i) {
				float bs = Bernstein(n - 1, i, t);
				result += weights[i] * points[i] * bs;
				weight += weights[i] * bs;
			}

			return result / weight;
		}

		/// <summary>
		/// 重み。
		/// </summary>
		[SerializeField]
		float[] weights;

		/// <summary>
		/// 点と数の設定。
		/// </summary>
		public void SetPoints(Vector3[] points, Vector3[] handles, float[] weights) {
			base.SetPoints(points, handles);

			if (handles.Length + this.points.Length != weights.Length)
				throw new Exception("ハンドルと重みの数に違いがあります。");

			this.weights = weights;
		}

		/// <summary>
		/// 評価する。
		/// </summary>
		public override Vector3[] Evaluate() {
			if (this.points == null || this.points.Length < 2)
				throw new Exception("必要な数の点が設定されていません。");
			else if (this.points.Length * (this.degree - 1) != this.handles.Length)
				throw new Exception("点の数とハンドルの数が相応しくありません。");


			int lineCount = this.points.Length + (this.closed ? 1 : 0) - 1;

			if (this.dividedCount < lineCount)
				this.dividedCount = lineCount;
			else if (this.dividedCount % lineCount != 0)
				this.dividedCount = lineCount * Mathf.FloorToInt(this.dividedCount / lineCount);

			Vector3[] linePoints = new Vector3[this.dividedCount + 1];

			// 1次の場合は直線で結ぶ
			if (lineCount == this.dividedCount) {
				this.points.CopyTo(linePoints, 0);
				if (this.closed) linePoints[linePoints.Length - 1] = linePoints[0];
				return linePoints;
			}

			int eachDividedCount = this.dividedCount / lineCount;
			float step = 1f / eachDividedCount;

			for (int p = 0; p < lineCount; ++p) {
				Vector3[] partLinePoints = this.GetPartLinePoints(p);
				float[] weights = this.GetPartWeights(p);

				for (int i = 0; i < eachDividedCount; ++i) {
					linePoints[p * eachDividedCount + i] = this.CalcuratePoint(partLinePoints, weights, i * step);
				}

				if (p == lineCount - 1)
					linePoints[(p + 1) * eachDividedCount] = this.CalcuratePoint(partLinePoints, weights, 1f);
			}

			return linePoints;
		}

		/// <summary>
		/// 指定の位置の座標を取得。
		/// </summary>
		protected Vector3 CalcuratePoint(Vector3[] partLinePoints, float[] weights, float t) {
			return RationalBezierCurve.GetPointWithWeights(partLinePoints, weights, t);
		}

		/// <summary>
		/// インデックスからラインを作るための重みを取得する。
		/// </summary>
		float[] GetPartWeights(int index) {
			int lineCount = this.points.Length + (this.closed ? 1 : 0) - 1;
			float[] weights = new float[this.degree + 1];

			for (int i = 0; i < this.degree + 1; ++i) {
				int weightIndex = index * this.degree + i + 1;

				if (this.closed && weightIndex > this.weights.Length - 1)
					weightIndex %= this.weights.Length;
				
				weights[i] = this.weights[weightIndex];
			}

			return weights;
		}
	}
}