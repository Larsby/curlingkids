using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadDown : MonoBehaviour {

	public float downTime = 2;
	public bool enableHeadup = false;
	public bool disableHeadup = false;

	void Start () {
	}

	void OnCollisionEnter(Collision collision)
	{
		OnTriggerEnter (collision.collider);
	}

	void OnTriggerEnter(Collider collider) {

		GameObject findMe = GameUtil.FindParentWithTag (collider.gameObject, "Player");

		if (findMe != null) {

			ToonDollHelper th = findMe.GetComponent<ToonDollHelper> ();
			if (th != null) {
				if (enableHeadup)
					th.bGazing = true;
				if (disableHeadup)
					th.bGazing = false;
				else
					th.ForceHeadDown (downTime);
			}
		}
	}

}
