using UnityEngine;

public class ObjectTracker : ExternalHit {

	public GameObject trackTarget = null;

	public Rigidbody followIfPhysicsEnabled = null;

	public bool trackX = true;
	public bool trackY = true;
	public bool trackZ = true;

	public float trackingSpeed = 1;
	public float trackingRotationSpeed = 0;

	public Vector3 trackingDelta = Vector3.zero;

	public bool stopOnExternalHit = true;

	public float minDistance = 0;

	private bool stopMe = false;

	public bool useDynamicTrackingSpeed = false;
	public float dynamicTrackingSpeedMaxMul = 10;
	public float dynamicTrackingSpeedMaxDistance = 100;

	private Vector3 oldPos;
	private Quaternion oldRot;
	public bool allowExternalMinDistanceMod = true;

	void Start () {
		oldPos = transform.position;
		oldRot = transform.rotation;
	}
	
	void Update () {
		if (stopMe)
			return;

		if (trackTarget)
		{
			Vector3 trackPos = trackTarget.transform.position;
			if (followIfPhysicsEnabled != null && followIfPhysicsEnabled.isKinematic == false)
				trackPos = followIfPhysicsEnabled.gameObject.transform.position;
			
			if (!trackX)
				trackPos = GameUtil.SetX(trackPos, transform.position.x);
			trackPos = GameUtil.AddX(trackPos, trackingDelta.x);

			if (!trackY)
				trackPos = GameUtil.SetY(trackPos, transform.position.y);
			trackPos = GameUtil.AddY(trackPos, trackingDelta.y);

			if (!trackZ)
				trackPos = GameUtil.SetZ(trackPos, transform.position.z);
			trackPos = GameUtil.AddZ(trackPos, trackingDelta.z);

			float distance = Vector3.Distance(transform.position, trackPos);

			float trackSpeed = trackingSpeed;
			if (useDynamicTrackingSpeed) {
				//print(distance);
				float dist = Mathf.Clamp(distance, 0, dynamicTrackingSpeedMaxDistance);
				float effect = 1.0f - (dist / dynamicTrackingSpeedMaxDistance);
				effect *= dynamicTrackingSpeedMaxMul;
				if (effect < 1) effect = 1;
				trackSpeed *= effect;
			}

			Vector3 currOldPos = transform.position;
			transform.position = Vector3.MoveTowards(transform.position, trackPos, trackSpeed * Time.deltaTime);
			if (distance < minDistance) {
				if (allowExternalMinDistanceMod)
					transform.position = currOldPos;
				else {
					transform.position = oldPos;
					transform.rotation = oldRot;
				}
			}

			Vector3 targetDir = trackPos - transform.position;
			Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, trackingRotationSpeed * Time.deltaTime, 0.0f);
			transform.rotation = Quaternion.LookRotation(newDir);
		}
	}

	void LateUpdate()
	{
		oldPos = transform.position;
		oldRot = transform.rotation;
	}

	public override void OnExternalHit()
	{
		if (stopOnExternalHit)
			stopMe = true;
	}
	public override void OnExternalMinorHit()
	{
	}
}
