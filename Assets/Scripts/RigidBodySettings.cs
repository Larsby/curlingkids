using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodySettings : MonoBehaviour {

	public bool setRecursive = false;
	public string startChild = string.Empty;

	public bool setMaxDepenetrationVelocity = false;
	public float maxDepenetrationVelocity = float.MaxValue;

	public bool detectCollisions = true;

	public bool setSleepTreshold = false;
	public float sleepTreshold = 0;

	void Start () {

		Transform startT = transform;

		if (startChild != string.Empty) {
			Transform t = transform.Find (startChild);
			if (t != null)
				startT = t;
		}

		SetValues (startT);
	}

	void SetValues(Transform t) {

		Rigidbody rb = t.GetComponent<Rigidbody> ();

		if (rb != null) {

			if (setMaxDepenetrationVelocity) rb.maxDepenetrationVelocity = maxDepenetrationVelocity;
			rb.detectCollisions = detectCollisions;
			if (setSleepTreshold)rb.sleepThreshold = sleepTreshold;
		}

		if (setRecursive) {
			foreach (Transform tc in t)
				SetValues (tc);
		}
	}


}
