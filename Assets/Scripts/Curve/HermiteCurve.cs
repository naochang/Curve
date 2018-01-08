using UnityEngine;
using System;

namespace Curve {
	public class HermiteCurve : CurveBase {
		/// <summary>
		/// 指定の条件でのポイントの取得。
		/// </summary>
		static Vector3 CubicHermiteCurve(Vector3 p0, Vector3 v0, Vector3 p1, Vector3 v1, float t) {
			float t2 = t*t;
			float t3 = t*t*t;

			return (2f * p0 - 2f * p1 + v0 + v1) * t3 + (-3f * p0 + 3f * p1 - 2f * v0 -v1) * t2 + v0 * t + p0;
		}

		/// <summary>
		/// 各点ごとのベクトル。
		/// </summary>
		Vector3[] vectors;

		/// <summary>
		/// 点とベクトルの設定。
		/// </summary>
		public void SetPoints(Vector3[] points, Vector3[] vectors) {
			base.SetPoints(points);
			this.vectors = vectors;
		}

		/// <summary>
		/// 線を引く点を評価し返す。
		/// </summary>
		public override Vector3[] Evaluate() {
			if (this.points == null || this.points.Length < 2)
				throw new Exception("必要な数の点が設定されていません。");
			else if (this.points.Length != this.vectors.Length)
				throw new Exception("点とベクトルの数が一致しません。");


			int lineCount = this.points.Length + (this.closed ? 1 : 0) - 1;

			if (this.dividedCount < lineCount)
				this.dividedCount = lineCount;
			else if (this.dividedCount % lineCount != 0)
				this.dividedCount = lineCount * Mathf.FloorToInt(this.dividedCount / lineCount);

			Vector3[] linePoints = new Vector3[this.dividedCount + 1];

			// 1次の場合は直線で結ぶ
			if (lineCount == this.dividedCount) {
				this.points.CopyTo(linePoints, 0);
				if (this.closed)
					linePoints[linePoints.Length - 1] = linePoints[0];
				return linePoints;
			}
			
			int eachDividedCount = this.dividedCount / lineCount;
			float step = 1f / eachDividedCount;

			for (int p = 0; p < lineCount; ++p) {
				int np = p + 1;

				if (this.closed && p == lineCount - 1)
					np = 0;

				for (int i = 0; i < eachDividedCount; ++i) {
					linePoints[p * eachDividedCount + i] = HermiteCurve.CubicHermiteCurve(
						this.points[p],
						this.vectors[p],
						this.points[np],
						this.vectors[np],
						i * step
					);
				}

				if (p == lineCount - 1) {
					linePoints[(p + 1) * eachDividedCount] = HermiteCurve.CubicHermiteCurve(
						this.points[p],
						this.vectors[p],
						this.points[np],
						this.vectors[np],
						1f
					);
				}
			}

			return linePoints;
		}
	}
}