using UnityEngine;
using System;
using System.Collections.Generic;

namespace Curve {
	public class BezierCurve : CurveBase {
		/// <summary>
		/// バーンスタイン基底関数。
		/// </summary>
		protected static float Bernstein(int n, int i, float t) {
			return Biominal(n, i) * Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i);
		}

		/// <summary>
		/// 二項係数。
		/// </summary>
		static float Biominal(int n, int k) {
			return Factorial(n) / (Factorial(k) * Factorial(n - k));
		}

		/// <summary>
		/// 累乗。
		/// </summary>
		static float Factorial(int n) {
			float result = 1f;

			for (int i = 2; i <= n; ++i)
				result *= i;

			return result;
		}

		/// <summary>
		/// 指定のポイントの取得。
		/// </summary>
		static Vector3 GetPoint(Vector3[] points, float t) {
			if (points.Length < 3)
				throw new Exception("点は3つ以上指定する必要があります。");

			// 計算済みの次元ごとの式から返す
			if (points.Length == 3) {		// 2次
				return GetPointWithDegree2(points, t);
			}
			else if (points.Length == 4) {	// 3次
				return GetPointsWithDegree3(points, t);
			}

			Vector3 result = Vector3.zero;
			int n = points.Length;
			for (int i = 0; i < n; ++i) {
				result += points[i] * Bernstein(n - 1, i, t);
			}

			return result;
		}

		/// <summary>
		/// 2次でのポイントの取得。
		/// </summary>
		static Vector3 GetPointWithDegree2(Vector3[] points, float t) {
			if (points.Length != 3)
				throw new Exception("2次の場合は点の数が3つである必要があります。");
			
			return (1f - t) * (1f - t) * points[0] + 2f * (1f - t) * t * points[1] + t * t * points[2];
		}

		/// <summary>
		/// 3次でのポイントの取得。
		/// </summary>
		static Vector3 GetPointsWithDegree3(Vector3[] points, float t) {
			if (points.Length != 4)
				throw new Exception("3次の場合は点の数が4つである必要があります。");
			
			return (1f - t) * (1f - t) * (1f - t) * points[0] +
				3f * (1f - t) * (1f - t) * t * points[1] +
				3f * (1f - t) * t * t * points[2] +
				t * t * t * points[3];
		}

		/// <summary>
		/// 次数。引数で取る点とハンドルの個数で自動的に判定。
		/// </summary>
		protected int degree;

		/// <summary>
		/// 点をコントロールするハンドル。
		/// </summary>
		protected Vector3[] handles;

		/// <summary>
		/// 点と数の設定。
		/// </summary>
		public void SetPoints(Vector3[] points, Vector3[] handles) {
			base.SetPoints(points);

			if (handles.Length % points.Length != 0)
				throw new Exception("点とハンドルの数が相応しくありません。");

			this.handles = handles;
			this.degree = Mathf.FloorToInt(this.handles.Length / points.Length) + 1;
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
				for (int i = 0; i < eachDividedCount; ++i) {
					linePoints[p * eachDividedCount + i] = this.CalcuratePoint(partLinePoints, i * step);
				}

				if (p == lineCount - 1)
					linePoints[(p + 1) * eachDividedCount] = this.CalcuratePoint(partLinePoints, 1f);
			}

			return linePoints;
		}

		/// <summary>
		/// 指定の位置の座標を取得。
		/// </summary>
		protected Vector3 CalcuratePoint(Vector3[] partLinePoints, float t) {
			return BezierCurve.GetPoint(partLinePoints, t);
		}

		/// <summary>
		/// インデックスからラインを作るための座標を取得する。
		/// </summary>
		protected Vector3[] GetPartLinePoints(int index) {
			int lineCount = this.points.Length + (this.closed ? 1 : 0) - 1;
			int handleCount = this.degree - 1;
			int next = index + 1;

			if (this.closed && index == lineCount - 1)
				next = 0;

			Vector3[] partLinePoints = new Vector3[this.degree + 1];
			partLinePoints[0] = this.points[index];

			for (int h = 0; h < handleCount; ++h) {
				int handleIndex = index * handleCount + h + 1;

				if (this.closed && index == lineCount - 1 && h == handleCount - 1) 
					handleIndex = 0;

				partLinePoints[h + 1] = this.handles[handleIndex];
			}

			partLinePoints[this.degree] = this.points[next];

			return partLinePoints;
		}
	}
}