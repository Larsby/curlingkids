using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkRenderer : MonoBehaviour {

	public float interval1 = 0.5f;
	public float interval2 = 0.5f;

	private Renderer myRenderer;

	private IEnumerator Blink() {

		while (true) {
			myRenderer.enabled = false;
			yield return new WaitForSeconds(interval1);
			myRenderer.enabled = true;
			yield return new WaitForSeconds(interval2);
		}

	}


	void Start () {
		myRenderer = GetComponent<Renderer> ();
		if (myRenderer != null) {
			StartCoroutine (Blink());
		}
	}
	
	void Update () {
		
	}
}
