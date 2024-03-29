using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour {

	private bool wasUsed = false;
	private Component[] rigidComponents = null;

	public float shootForce = 2000;

	public bool bExplodes = false;

	public float loweredTimeStepTime = 0;
	private int timeStepAffectIndex;

	private float mulMod = 1;

	public Vector3 CannonRotation = new Vector3(60, 0, 0);

	public Color hitColor = Color.red;

	public float shootDelay = 0;


	void Start () {}

	void Update () {
	}

	private Color oldColor;

	private void restoreColor() {
		MeshRenderer mr = this.GetComponent<MeshRenderer>();
		mr.material.color = oldColor;
	}

	private void resetBoost() {
		wasUsed = false;

		if (transform.childCount > 0) {
			Transform t = transform.GetChild (0);
			if (t != null)
				t.gameObject.SetActive (false);
		}
	}

	private void restoreFixedTimeStep() {
		StaticManager.RestoreTimeStep(timeStepAffectIndex);
	}


	private GameObject findMe;

	void Shoot() {

		rigidComponents = findMe.gameObject.GetComponentsInChildren(typeof(Rigidbody));

		foreach (Component c in rigidComponents)
		{
			Rigidbody rb = (Rigidbody)c;
			rb.useGravity = true;
			rb.velocity = Vector3.zero;
//			rb.angularVelocity = Vector3.zero;
			rb.MoveRotation (Quaternion.Euler(CannonRotation));
			rb.AddRelativeForce (Vector3.up * shootForce * mulMod);

			// Debug.Break ();
		}

		if (bExplodes) {
			if (transform.childCount > 0) {
				Transform t = transform.GetChild (0);
				if (t != null)
					t.gameObject.SetActive (true);
			}
		}
	}


	void OnTriggerEnter(Collider other) {

		findMe = GameUtil.FindParentWithTag (other.gameObject, "Player");

		if (findMe != null && !wasUsed) {

			MeshRenderer mr = this.GetComponent<MeshRenderer>();
			oldColor = mr.material.color;
			mr.material.color = hitColor;
			Invoke ("restoreColor", 0.1f);
			mulMod = StaticManager.GetTimeStepMul ();

			if (loweredTimeStepTime > 0) {

				timeStepAffectIndex = StaticManager.PushFixedTimeStep (0.002f);
				Invoke ("restoreFixedTimeStep", loweredTimeStepTime);
			}

			rigidComponents = findMe.gameObject.GetComponentsInChildren(typeof(Rigidbody));

			foreach (Component c in rigidComponents)
			{
				Rigidbody rb = (Rigidbody)c;
				rb.useGravity = false;
				rb.velocity = Vector3.zero;
//				rb.angularVelocity = Vector3.zero;
				rb.MoveRotation (Quaternion.Euler(CannonRotation));
			}

			Invoke ("Shoot", shootDelay);

			wasUsed = true;
			Invoke ("resetBoost", 1f);
		}

	}

}
