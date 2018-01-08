using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sample {
	/// <summary>
	/// ハンドルを最大2つまで持つ点。
	/// 片方のハンドルの移動でもう片方のハンドルも点対称に移動する。
	/// independentHandlesをtrueにすることで、独立して移動可能。
	/// 点ごとの重みも持つ。
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(LineRenderer))]
	public class HandlePoint : MonoBehaviour {
		/// <summary>
		/// ハンドルのタイプ。
		/// なしか、点との差分のベクトルを持つか、2点持つか。
		/// </summary>
		public enum HandleType {
			None,
			One,
			Two
		}

		/// <summary>
		/// ハンドルを使うかどうか。
		/// </summary>
		HandleType handle = HandleType.None;
		public HandleType Handle {
			get {
				return this.handle;
			}
			set {
				this.handle = value;

				this.UpdateHandles();
			}
		}

		/// <summary>
		/// ハンドルを独立するか。
		/// </summary>
		public bool independentHandles = false;

		/// <summary>
		/// ポイントの取得。
		/// </summary>
		public Vector3 Point {
			get {
				return this.transform.localPosition;
			}
		}

		/// <summary>
		/// ハンドル。
		/// </summary>
		Transform handle1Trans, handle2Trans;
		public Transform Handle1Trans {
			get {
				if (this.handle1Trans == null)
					this.handle1Trans = this.transform.GetChild(0);

				return this.handle1Trans;
			}
		}

		public Transform Handle2Trans {
			get {
				if (this.handle2Trans == null)
					this.handle2Trans = this.transform.GetChild(1);

				return this.handle2Trans;
			}
		}

		/// <summary>
		/// 重み。
		/// </summary>
		[Range(0f, 2f)]
		public float handle1Weight = 1f;
		[Range(0f, 2f)]
		public float pointWeight = 1f;
		[Range(0f, 2f)]
		public float handle2Weight = 1f;

		/// <summary>
		/// 前回のハンドルの位置。
		/// </summary>
		Vector3 prevHandle1Pos;
		Vector3 prevHandle2Pos;

		/// <summary>
		/// LineRenderer.
		/// </summary>
		LineRenderer lineRenderer;

		/// <summary>
		/// Awake.
		/// </summary>
		void Awake() {
			this.lineRenderer = this.GetComponent<LineRenderer>();
			this.lineRenderer.useWorldSpace = false;
			this.prevHandle1Pos = this.Handle1Trans.localPosition;
			this.prevHandle2Pos = this.Handle2Trans.localPosition;

			this.UpdateHandles();
		}

		/// <summary>
		/// Update
		/// </summary>
		void Update() {
			if (this.handle != HandleType.None && (transform.hasChanged || this.Handle1Trans.hasChanged || this.Handle2Trans.hasChanged)) {
				this.UpdateHandles();
				this.transform.hasChanged = this.Handle1Trans.hasChanged = this.Handle2Trans.hasChanged = false;
			}
		}

		/// <summary>
		/// ハンドルとの差分のベクトルを取得する。
		/// </summary>
		public Vector3 GetHandle(int handleIndex = 1) {
			if (handleIndex < 1 || handleIndex > 2)
				throw new Exception("ハンドルのインデックスは1か2である必要があります。");

			Transform handleTrans = handleIndex == 1 ? this.Handle1Trans : this.Handle2Trans;
			return transform.parent.InverseTransformPoint(handleTrans.position);
		}

		/// <summary>
		/// ハンドルの更新。
		/// </summary>
		void UpdateHandles() {
			this.lineRenderer.enabled = true;
			Vector3[] linePoints = null;

			this.Handle1Trans.gameObject.SetActive(this.handle != HandleType.None);
			this.Handle2Trans.gameObject.SetActive(this.handle == HandleType.Two);

			if (!this.independentHandles && this.handle == HandleType.Two) {
				if (this.Handle1Trans.localPosition != this.prevHandle1Pos) {
					this.Handle2Trans.localPosition = -1f * this.Handle1Trans.localPosition;
					this.prevHandle1Pos = this.Handle1Trans.localPosition;
				}
				else if (this.Handle2Trans.localPosition != this.prevHandle2Pos) {
					this.Handle1Trans.localPosition = -1f * this.Handle2Trans.localPosition;
					this.prevHandle2Pos = this.handle2Trans.localPosition;
				}
			}

			if (this.handle == HandleType.One) {
				linePoints = new Vector3[2];
				linePoints[0] = this.Handle1Trans.localPosition;
				linePoints[1] = Vector3.zero;
				this.lineRenderer.positionCount = linePoints.Length;
				this.lineRenderer.SetPositions(linePoints);
			}
			else if (this.handle == HandleType.Two) {
				linePoints = new Vector3[3];
				linePoints[0] = this.Handle1Trans.localPosition;
				linePoints[1] = Vector3.zero;
				linePoints[2] = this.Handle2Trans.localPosition;
				this.lineRenderer.positionCount = linePoints.Length;
				this.lineRenderer.SetPositions(linePoints);
			}
			else {
				this.lineRenderer.enabled = false;
				this.lineRenderer.positionCount = 0;
			}
		}

		/// <summary>
		/// OnValiddate.
		/// </summary>
		void OnValidate() {
			this.transform.hasChanged = true;
		}
	}
}