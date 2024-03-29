using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slomo : MonoBehaviour {

	public float slowTo = 0.5f;
	public float maxSlowTime = 2;
	public bool singleHit = true;

	private bool wasUsed = false;
	private List<GameObject> hasHitObjects = new List<GameObject>();

	void Start () {
		maxSlowTime /= (1 / slowTo);
	}

	void ResetNormalTimeScale() {
		Time.timeScale = 1;
		wasUsed = false;
	}

	void ResetUsed() {
		wasUsed = false;
	}

	void OnTriggerEnter(Collider other) {

		GameObject findMe = GameUtil.FindParentWithTag (other.gameObject, "Player");
		if (findMe == null)
			return;

		bool firstHit = true;
		if (singleHit) {
			foreach (GameObject g in hasHitObjects) {
				if (g == findMe.gameObject)
					firstHit = false;
			}
		}

		ToonDollHelper tdh = findMe.GetComponent<ToonDollHelper> ();

		if (!wasUsed && firstHit && tdh.IsActive()) {

			hasHitObjects.Add (findMe);

			Time.timeScale = slowTo;
			if (maxSlowTime > 0)
				Invoke ("ResetNormalTimeScale", maxSlowTime);
			else
				Invoke ("ResetUsed", 2);
			wasUsed = true;
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		OnTriggerEnter (collision.collider);
	}
}
