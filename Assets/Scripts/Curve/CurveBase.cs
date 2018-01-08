using System;
using UnityEngine;

namespace Curve {
	/// <summary>
	/// 任意の点からカーブを描画するための座標を返すインターフェース。
	/// </summary>
	[DisallowMultipleComponent]
	abstract public class CurveBase : MonoBehaviour {
		public event EventHandler ValueChanged;

		/// <summary>
		/// 閉包したカーブとするか。
		/// </summary>
		public bool closed;

		/// <summary>
		/// ラインの分割数。
		/// </summary>
		public int dividedCount = 100;

		/// <summary>
		/// 任意の点。
		/// </summary>
		protected Vector3[] points;

		/// <summary>
		/// Onvalidate.
		/// </summary>
		protected virtual void OnValidate() {
			if (this.points != null)
				this.DispatchValueChanged();
		}

		/// <summary>
		/// 値が変わったイベントの発行。
		/// </summary>
		protected void DispatchValueChanged() {
			if (this.ValueChanged != null)
				this.ValueChanged(this, EventArgs.Empty);
		}

		/// <summary>
		/// 点の設定。
		/// </summary>
		public virtual void SetPoints(Vector3[] points) {
			this.points = points;
		}

		/// <summary>
		/// 線を引く点を評価し返す。
		/// </summary>
		public abstract Vector3[] Evaluate();
	}
}