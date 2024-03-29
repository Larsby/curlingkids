using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleOnOff : MonoBehaviour {
	public bool activeObj = false;
	public GameObject switchObject;
	// Use this for initialization
	void Start () {
		
	}
	public void Toggle() {
		activeObj =!activeObj;
		switchObject.SetActive(activeObj);
	}


	// Update is called once per frame
	void Update () {
		
	}
}
