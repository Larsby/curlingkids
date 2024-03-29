using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// note: for a playerhelper object which is in kinematic mode, a rigidbody must be added to the deathbox for a collison to occur

public class Deathbox : MonoBehaviour {

	public bool actualDestroyForAllObjects = false;

	void Start () {
	}
	
	void Update () {
	}

	void OnTriggerEnter(Collider other) {
	
		if (actualDestroyForAllObjects) {
			Destroy(other.gameObject);
			return;
		}

		GameObject findMe = GameUtil.FindParentWithTag (other.gameObject, "Player");

		if (findMe != null) {

			ToonDollHelper rh = findMe.GetComponent<ToonDollHelper> ();
			if (rh != null) {
				if (rh.IsPlayerHelper())
					findMe.gameObject.SetActive (false);
				else
					rh.SetOutOfBounds ();
			}
		}
	}

}
