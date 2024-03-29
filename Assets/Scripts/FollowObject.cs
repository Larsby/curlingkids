using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour {

	public enum FollowType { Always, PhysicsOnly, KinematicOnly };
 
	public GameObject followThis = null;
	public bool followPosX = false;
	public bool followPosY = false;
	public bool followPosZ = true;
	public Vector3 positionOffset = Vector3.zero;
	public FollowType followType = FollowType.Always;
	public bool followRotation = false;
	public bool followScaleX = false;
	public bool followScaleY = false;
	public bool followScaleZ = false;

	public bool inheritDisable = false;

	private Rigidbody rb = null;

	void Start () {
		SetFollowThis(followThis);
	}
	
	void Update () {
		if (followThis != null) {

			if (followType != FollowType.Always && rb != null) {
				if (followType == FollowType.PhysicsOnly && rb.isKinematic)
					return;
				if (followType == FollowType.KinematicOnly && !rb.isKinematic)
					return;
			}

			if (followPosX)
				transform.position = GameUtil.SetX(transform.position, followThis.transform.position.x + positionOffset.x);
			if (followPosY)
				transform.position = GameUtil.SetY(transform.position, followThis.transform.position.y + positionOffset.y);
			if (followPosZ)
				transform.position = GameUtil.SetZ(transform.position, followThis.transform.position.z + positionOffset.z);
			if (followRotation)
				transform.rotation = followThis.transform.rotation;
			if (followScaleX)
				transform.localScale = GameUtil.SetX(transform.localScale, followThis.transform.localScale.x);
			if (followScaleY)
				transform.localScale = GameUtil.SetY(transform.localScale, followThis.transform.localScale.y);
			if (followScaleZ)
				transform.localScale = GameUtil.SetZ(transform.localScale, followThis.transform.localScale.z);

			if (inheritDisable && followThis.activeSelf == false)
				gameObject.SetActive(false); // we will not awake from this even if followThis becomes active again
		}
	}

	public void SetFollowThis(GameObject g) {
		followThis = g;
		if (followThis != null)
			rb = followThis.GetComponent<Rigidbody>();
	}
}
