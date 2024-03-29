using UnityEngine;

public class BoosterSimple : InvokeWithObject {

	private bool wasUsed = false;
	private Component[] rigidComponents = null;

	public float multiplier = 1;

	public bool forceVelocity = false;
	public float minRandomYForce = 0;
	public float maxRandomYForce = 0;

	public bool bExplodes = false;
	public bool bExplodesAtHitPont = true;

	public float loweredTimeStepTime = 0;
	public float loweredTimeStepValue = 0.004f;
	private int timeStepAffectIndex;

	public float temporaryCollisionDetectionModeTime = 0;
	public CollisionDetectionMode temporaryCollisionDetectionMode = (CollisionDetectionMode)(-1); // non-existing mode = not set
	private int cdmAffectIndex;

	private float mulMod = 1;

	private ForceMode forceMode = ForceMode.Force;

	public bool changeColorOnHit = true;
	public Color hitColor = Color.red;

	public float resetTime = 1.2f;

	public float boostDelay = 0;

	public float randomizerValue = 0; 

	public bool allowOnlyPlayerHelpers = false;
	public bool allowNoPlayerHelpers = false;

	public SingleSfx hitSound = SingleSfx.Booster1;
	public bool randomPitch = false;
	public float volume = 1;
	private SoundEmitter soundEmitter = null;


	void Start () {
		soundEmitter = GetComponent<SoundEmitter> ();

		if (hitSound == SingleSfx.Booster1 && volume > 0.55f)
			volume = 0.55f;
	}
	
	void Update () {
	}

	private Color oldColor;

	private void restoreColor() {
		MeshRenderer mr = this.GetComponent<MeshRenderer>();
		if (changeColorOnHit)
			mr.material.color = oldColor;
	}

	private void resetBoost() {
		wasUsed = false;

		if (transform.childCount > 0) {
			Transform t = transform.GetChild (0);
			if (t != null)
				t.gameObject.SetActive (false);
		}
	}

	private void restoreFixedTimeStep() {
		StaticManager.RestoreTimeStep(timeStepAffectIndex);
	}

	private void restoreCDM(System.Object dollObject) {
		if (dollObject == null) return;
		StaticManager.RestoreCollisionDetectionMode (cdmAffectIndex, (ToonDollHelper)dollObject);
	}

	private bool waitForDelay = false;
	private bool actualTrig = false;
	private Collider storedCollider = null;

	void delayedBoost() {
		waitForDelay = false;
		actualTrig = true;
		OnTriggerEnter (storedCollider);
	}
		
	void OnTriggerEnter(Collider other) {

		if (!this.enabled)
			return;

		GameObject findMe = GameUtil.FindParentWithTag (other.gameObject, "Player");

		if (findMe != null && !wasUsed && boostDelay > 0 && waitForDelay == false && actualTrig == false) {
			storedCollider = other;
			waitForDelay = true;
			Invoke ("delayedBoost", boostDelay);
			return;
		}

		if (waitForDelay)
			return;

		if (findMe != null && (allowOnlyPlayerHelpers || allowNoPlayerHelpers)) {
			bool isOkHitter = true;
			ToonDollHelper doll = findMe.GetComponent<ToonDollHelper> ();
			if (doll != null) {
				if (allowOnlyPlayerHelpers) {
					isOkHitter = doll.IsPlayerHelper ();
				} else if (allowNoPlayerHelpers) {
					isOkHitter = doll.IsRagDoll () && !doll.IsPlayerHelper ();
				} else {
					isOkHitter = doll.IsRagDoll () || doll.IsPlayerHelper ();
				}
			}
			if (!isOkHitter)
				return;
		}

		actualTrig = false;

		if (findMe != null && !wasUsed) {

			MeshRenderer mr = this.GetComponent<MeshRenderer>();
			oldColor = mr.material.color;
			if (changeColorOnHit)
				mr.material.color = hitColor;
			Invoke ("restoreColor", 0.1f);

			if (loweredTimeStepTime > 0 && loweredTimeStepValue > 0) {

				timeStepAffectIndex = StaticManager.PushFixedTimeStep (loweredTimeStepValue);
				Invoke ("restoreFixedTimeStep", loweredTimeStepTime);
			}

			if (temporaryCollisionDetectionModeTime > 0 && (temporaryCollisionDetectionMode == CollisionDetectionMode.Continuous || temporaryCollisionDetectionMode == CollisionDetectionMode.ContinuousDynamic || temporaryCollisionDetectionMode == CollisionDetectionMode.Discrete)) {
				ToonDollHelper doll = findMe.GetComponent<ToonDollHelper> ();
				if (doll != null) {
					cdmAffectIndex = StaticManager.PushCollisionDetectionMode (temporaryCollisionDetectionMode, doll);
					InvokeWO (restoreCDM, temporaryCollisionDetectionModeTime, doll);
				}
			}

			rigidComponents = findMe.gameObject.GetComponentsInChildren(typeof(Rigidbody));

			mulMod = 1 * StaticManager.GetTimeStepMul ();

			foreach (Component c in rigidComponents)
			{
				Rigidbody rb = (Rigidbody)c;

				if (forceVelocity) {
					rb.velocity = new Vector3 (transform.forward.x * 2 * multiplier / 200 + Random.Range (0f, randomizerValue) - randomizerValue / 2, 0, transform.forward.z * 2 * multiplier / 200 + Random.Range (0f, randomizerValue) - randomizerValue / 2); // *2 is a leftover from a bug, kept for continuity. Velocity should not be multiplied by Timestep at all
				} else
					rb.AddForce (transform.forward.x * mulMod * multiplier + Random.Range(0f,randomizerValue) - randomizerValue/2, 0, transform.forward.z * mulMod * multiplier + Random.Range(0f,randomizerValue) - randomizerValue/2, forceMode);

				float initialYForce = minRandomYForce + Random.Range (0, Mathf.Clamp (maxRandomYForce - minRandomYForce, 0, 999999));
				if (initialYForce != 0)
					rb.AddForce (0, initialYForce * mulMod + Random.Range(0f,randomizerValue) - randomizerValue/2, 0, forceMode);
			}

			if (soundEmitter != null && soundEmitter.emitterType == SoundEmitter.EmitterType.RemoteControlled)
				soundEmitter.PlaySound ();
			else
				SoundManager.instance.PlaySingleSfx (hitSound, randomPitch, false, 0, volume);
			
			if (bExplodes) {
				if (transform.childCount > 0) {
					Transform t = transform.GetChild (0);
					if (t != null) {
						t.gameObject.SetActive (true);
						if (bExplodesAtHitPont)
							t.gameObject.transform.position = other.attachedRigidbody.gameObject.transform.position;
					}
				}
			}

			wasUsed = true;
			Invoke ("resetBoost", resetTime);

		}
	}

	void OnCollisionEnter(Collision collision)
	{
		OnTriggerEnter (collision.collider);
	}

	void OnTriggerStay(Collider other) {
	}

	void OnCollisionStay(Collision collision)
	{
		OnTriggerStay (collision.collider);
	}

	void OnTriggerExit(Collider other) {
	}
}
