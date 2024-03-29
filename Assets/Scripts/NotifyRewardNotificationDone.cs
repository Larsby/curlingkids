using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotifyRewardNotificationDone : MonoBehaviour {
	public bool Done = false;
	void Start() {
		DontDestroyOnLoad(gameObject.transform.parent.gameObject);
	}
	// Update is called once per frame
	void Update () {
		if(Done) {
			DoDone();
			Done = false;
		}
	}

	public void DoDone() {

		CharacterManager.instance.ShowNotificationDone();
		Destroy(gameObject.transform.parent.gameObject);
	}

}
