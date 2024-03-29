using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetInactiveIfZombieNotBought : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (StaticManager.WorldPurchased(1) == false)
			gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
