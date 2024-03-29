using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddAwardTriggerOnChildren : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
        AwardTrigger trigger = GetComponent<AwardTrigger>();
		bool addTrigger = true;
		foreach (Transform child in transform)
		{
			AwardTrigger[] oldTriggers = child.gameObject.GetComponents<AwardTrigger>();
			for (int i = 0; i < oldTriggers.Length; i++)
			{
				if (oldTriggers[i] != null && oldTriggers[i].taskType == trigger.taskType)
				{
					addTrigger = false;
					// child object already have the trigger. Don't add it. 
				}
			}
			if(addTrigger) {
				AwardTrigger childTrigger = child.gameObject.AddComponent<AwardTrigger>();
					childTrigger.taskType = trigger.taskType;
					childTrigger.isTrigger = trigger.isTrigger;
					childTrigger.destroyOnHit = trigger.destroyOnHit;
				childTrigger.description = trigger.description;
			}
		}
        Destroy(trigger);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
