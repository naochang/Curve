using System;
using UnityEngine;

namespace Curve {
	/// <summary>
	/// カーブではなくそのまま直線で結ぶ点を返す。
	/// </summary>
	public class StraightLine : CurveBase {
		/// <summary>
		/// 線を引く点の取得。
		/// </summary>
		public override Vector3[] Evaluate() {
			if (this.points == null || this.points.Length < 2)
				throw new Exception("必要な数の点が設定されていません。");


			Vector3[] linePoints = new Vector3[this.points.Length + (this.closed ? 1 : 0)];
			points.CopyTo(linePoints, 0);
			if (this.closed)
				linePoints[linePoints.Length - 1] = linePoints[0];

			this.dividedCount = linePoints.Length - 1;

			return linePoints;
		}
	}
}