using UnityEngine;

namespace Sample {
	public class AutoMove : MonoBehaviour {
		[SerializeField, Range(0f, 5f)]
		float moveMultiplier = 1f;
		Vector3 firstPos;
		Vector3 moveAngle = Vector3.zero;
		Vector3 rotateSpeed;
		float moveDistance;

		void Start() {
			this.firstPos = this.transform.localPosition;
			this.moveDistance = Random.Range(0.5f, 2f);
			this.rotateSpeed = new Vector3(
				Random.Range(-1f, 1f),
				Random.Range(-1f, 1f),
				Random.Range(-1f, 1f)
			);
		}

		void Update () {
			this.transform.localPosition = this.firstPos + this.moveMultiplier * this.moveDistance * new Vector3(
				Mathf.Sin(this.moveAngle.x),
				Mathf.Sin(this.moveAngle.y),
				Mathf.Sin(this.moveAngle.z)
			);

			this.moveAngle += this.rotateSpeed * Time.deltaTime;
		}
	}
}