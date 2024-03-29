using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestroy : MonoBehaviour {

	public float time = 1f;
	public bool onlyHide = false;
	public GameObject altObject = null; 
	public bool showNotHide = false;

	void Start () {
		if (time >= 0)
			Invoke("KillMe", time);
	}

	void KillMe() {
		GameObject g = gameObject;
		if (altObject != null)
			g = altObject;

		if (showNotHide)
		{
			g.SetActive(true);
			return;
		}

		if (onlyHide)
			g.SetActive(false);
		else
			Destroy(g);
	}
}
