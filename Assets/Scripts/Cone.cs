using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cone : MonoBehaviour {

	public float LiftForce = 90;

	private bool wasUsed = false;

	void Start () {
		// this would normally be in the prefab. But no Cone prefab.
		// I am also assuming here that only cones use Cone.cs
		AwardTrigger trigger = GetComponent<AwardTrigger>();
		if(trigger == null) {
			trigger = gameObject.AddComponent<AwardTrigger>();
			trigger.taskType = StaticTaskManager.TaskType.Cones;
			trigger.destroyOnHit = true;
		}

	}

	void Update () {}

	void OnCollisionEnter(Collision coll) {
		if (wasUsed)
			return;

		Collider other = coll.collider;

		GameObject findMe = GameUtil.FindParentWithTag (other.gameObject, "Player");

		if (findMe != null) {

			Rigidbody rb = this.gameObject.GetComponent<Rigidbody> ();
			if (rb != null && other.attachedRigidbody != null) {
				rb.AddForce (0, LiftForce * StaticManager.GetTimeStepMul() * other.attachedRigidbody.velocity.magnitude / 16f, 0);
			}

			wasUsed = true;
			Invoke ("ResetUsed", 1f);
		}
	}

	void ResetUsed() {
		wasUsed = false;
	}

}
