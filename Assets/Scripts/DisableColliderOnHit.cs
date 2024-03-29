using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableColliderOnHit : MonoBehaviour {

	public float disableWaitTime = 0.3f;
	public float reEnableWaitTime = 2f;
	private bool wasUsed = false;
	private Collider coll = null;

	void Start () {
		coll = GetComponentInChildren<Collider> ();
	}
	
	void Update () {
	}

	private void Reset() {
		wasUsed = false;
		coll.enabled = true;
	}

	private void Disable() {
		Invoke ("Reset", reEnableWaitTime);
		coll.enabled = false;
	}

	void OnCollisionEnter(Collision collision) {

		GameObject findMe = GameUtil.FindParentWithTag (collision.collider.gameObject, "Player");

		if (findMe != null && !wasUsed) {

			Invoke ("Disable", disableWaitTime);
			wasUsed = true;
		}
	}
}
