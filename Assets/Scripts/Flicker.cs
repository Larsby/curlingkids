using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flicker : MonoBehaviour {
	Light myLight;
	bool lightOn = true;
	public float onLightFrom = 0.3f;
	public float onLightEnd = 1.0f;
	public float offLightFrom = 0.1f;
	public float offLightEnd = 0.2f;
	// Use this for initialization
	void Start () {
		myLight = GetComponent<Light>();
		StartCoroutine(SetLightState());
	}

	IEnumerator SetLightState() {
		if (this.enabled)
		{
			lightOn = !lightOn;
			float startV = onLightFrom;
			float endV = onLightEnd;
			if (!lightOn)
			{
				startV = offLightFrom;
				endV = offLightEnd;
			}
			float delay = Random.Range(startV, endV);

			yield return new WaitForSeconds(delay);
			if (myLight)
			{
				myLight.enabled = lightOn;
			}
			StartCoroutine(SetLightState());
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
