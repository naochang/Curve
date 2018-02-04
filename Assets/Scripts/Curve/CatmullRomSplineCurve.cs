using UnityEngine;
using System;

namespace Curve {
	public class CatmullRomSplineCurve : CurveBase {
		/// <summary>
		/// Catmull-Romの計算。
		/// </summary>
		static Vector3 CatmullRom(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
			Vector3 a = -p0 + 3f * p1 - 3f * p2 + p3;
			Vector3 b = 2f * p0 - 5f * p1 + 4f * p2 - p3;
			Vector3 c = -p0 + p2;
			Vector3 d = 2f * p1;

			return 0.5f * ((a * t * t * t) + (b * t * t) + (c * t) + d);
		}

		/// <summary>
		/// Catmull-Romの最初のポイント。
		/// </summary>
		static Vector3 CatmullRomFirst(float t, Vector3 p0, Vector3 p1, Vector3 p2) {
			Vector3 b = p0 - 2f * p1 + p2;
			Vector3 c = -3f * p0 + 4f * p1 - p2;
			Vector3 d = 2f * p0;

			return 0.5f * ((b * t * t) + (c * t) + d);
		}

		/// <summary>
		/// Catmull-Romの最後のポイント.
		/// </summary>
		static Vector3 CatmullRomLast(float t, Vector3 p0, Vector3 p1, Vector3 p2) {
			Vector3 b = p0 - 2f * p1 + p2;
			Vector3 c = -p0 + p2;
			Vector3 d = 2f * p1;

			return 0.5f * ((b * t * t) + (c * t) + d);
		}

		/// <summary>
		/// 線を引く点を評価し返す。
		/// </summary>
		public override Vector3[] Evaluate() {
			if (this.points == null || this.points.Length < 2)
				throw new Exception("必要な数の点が設定されていません。");

			int lineCount = this.points.Length + (this.closed ? 1 : 0) - 1;

			if (this.dividedCount % lineCount != 0)
				this.dividedCount = lineCount * Mathf.FloorToInt(this.dividedCount / lineCount);

			if (this.dividedCount < 1)
				this.dividedCount = lineCount;


			Vector3[] linePoints = new Vector3[this.dividedCount + (!this.closed ? 1 : 0)];
			int eachDividedCount = this.dividedCount / lineCount;
			float step = 1f / eachDividedCount;

			for (int p = 0; p < lineCount; ++p) {
				for (int i = 0; i < eachDividedCount; ++i) {
					Vector3 point;

					if (!this.closed && p == 0) {
						point = CatmullRomFirst(i * step,
							this.points[p    ],
							this.points[p + 1],
							this.points[p + 2]
						);
					}
					else if (!this.closed && p == this.points.Length - 2) {
						point = CatmullRomLast(i * step,
							this.points[p - 1],
							this.points[p    ],
							this.points[p + 1]
						);
					}
					else {
						point = CatmullRom(i * step,
							this.points[p == 0 ? this.points.Length - 1 : p - 1],
							this.points[p],
							this.points[(p + 1) % this.points.Length],
							this.points[(p + 2) % this.points.Length]
						);
					}

					linePoints[p * eachDividedCount + i] = point;
				}
			}

			if (!this.closed)
				linePoints[linePoints.Length - 1] = this.points[this.points.Length - 1];

			return linePoints;
		}
	}
}