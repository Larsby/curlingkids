using UnityEngine;

public class Booster : InvokeWithObject {

	private bool wasUsed = false;
	private Component[] rigidComponents = null;

	public Vector3 multiplier = Vector3.one;

	public float zAdd = 0;
	public float zLength = 0;
	public float yAdd = 0;
	public float yLength = 0;
	public float xAdd = 0;
	public float xLength = 0;

	public float minRandomXForce = 0;
	public float maxRandomXForce = 0;

	public float minRandomYForce = 0;
	public float maxRandomYForce = 0;

	public float minRandomZForce = 0;
	public float maxRandomZForce = 0;

	public bool bExplodes = false;
	public bool bExplodesAtHitPont = true;

	public bool bAffectsEntireBody = true;

	public float loweredTimeStepTime = 0;
	public float loweredTimeStepValue = 0.004f;
	private int timeStepAffectIndex;

	public float temporaryCollisionDetectionModeTime = 0;
	public CollisionDetectionMode temporaryCollisionDetectionMode = (CollisionDetectionMode)(-1); // non-existing mode = not set
	private int cdmAffectIndex;

	private float zSum = 0;
	private float ySum = 0;
	private float xSum = 0;

	private float mulMod = 1;
	private float orgMulMod;

	private ForceMode forceMode = ForceMode.Force;

	public bool changeColorOnHit = true;
	public Color hitColor = Color.red;

	public Vector3 forcedRotation = Vector3.zero;

	public bool disableOnHit = false;
	public bool enableOnReset = false;
	public float resetTime = 1.2f;

	public bool forceVelocity = false;

	public float boostDelay = 0;

	public bool enableCollisionStay = false;

	public float randomizerValue = 0;

	public bool allowOnlyPlayerHelpers = false;
	public bool allowNoPlayerHelpers = false;

	public SingleSfx hitSound = SingleSfx.Booster1;
	public bool randomPitch = false;
	public float volume = 1;
	private SoundEmitter soundEmitter = null;


	void Start () {
		soundEmitter = GetComponent<SoundEmitter> ();

		if (hitSound == SingleSfx.Booster1 && (multiplier.x + multiplier.y + multiplier.z < 2.0f)) // normally boosters that slow down objects should not have a sound, so remove if not explicitly set to nonstandard sound
			hitSound = SingleSfx.None;

		if (hitSound == SingleSfx.Booster1 && volume > 0.55f)
			volume = 0.55f;

			
	}
	
	void Update () {
		if (rigidComponents != null) {

			mulMod = orgMulMod * StaticManager.GetTimeStepMul ();

			if (zSum < zAdd * zLength) {
				foreach (Component c in rigidComponents)
				{
					Rigidbody rb = (Rigidbody)c;
					float addVal = zAdd * Time.deltaTime;

					if (rb != null) {
						float addMulMod = 1;
						if (zSum + addVal >= zAdd * zLength)
						{
							float addMod = (zSum + addVal) - (zAdd * zLength);
							addVal -= addMod;
							addMulMod = addVal / (zAdd * Time.deltaTime);
						}

						// Debug.Log ("ZV: " + rb.velocity.z);
						addMulMod /= Mathf.Clamp(0 + rb.velocity.z / 6, 1, 999);

						rb.AddForce(0, 0, zAdd * Time.deltaTime * mulMod * addMulMod, forceMode);
					}
					zSum += addVal;
				}
			}

			if (ySum < yAdd * yLength) {
				foreach (Component c in rigidComponents)
				{
					Rigidbody rb = (Rigidbody)c;
					float addVal = yAdd * Time.deltaTime;

					if (rb != null) {
						float addMulMod = 1;
						if (ySum + addVal >= yAdd * yLength)
						{
							float addMod = (ySum + addVal) - (yAdd * yLength);
							addVal -= addMod;
							addMulMod = addVal / (yAdd * Time.deltaTime);
						}

						// Debug.Log ("YV: " + rb.velocity.y);
						addMulMod /= Mathf.Clamp(0 + rb.velocity.y / 6, 1, 999);

						rb.AddForce(0, yAdd * Time.deltaTime * mulMod * addMulMod, 0, forceMode);
					}
					ySum += addVal;
				}
			}

			if (xSum < xAdd * xLength) {
				foreach (Component c in rigidComponents)
				{
					Rigidbody rb = (Rigidbody)c;
					float addVal = xAdd * Time.deltaTime;

					if (rb != null) {
						float addMulMod = 1;
						if (xSum + addVal >= xAdd * xLength)
						{
							float addMod = (xSum + addVal) - (xAdd * xLength);
							addVal -= addMod;
							addMulMod = addVal / (xAdd * Time.deltaTime);
						}

						//	Debug.Log ("XV: " + rb.velocity.x);
						addMulMod /= Mathf.Clamp(0 + rb.velocity.x / 6, 1, 999);

						rb.AddForce(xAdd * Time.deltaTime * mulMod * addMulMod, 0, 0, forceMode);
					}
					xSum += addVal;
				}
			}

		}

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

		if (enableOnReset) {
			GetComponent<MeshRenderer> ().enabled = true;
		} else if (disableOnHit) {
			gameObject.SetActive (false);
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
			if (changeColorOnHit) {
				oldColor = mr.material.color;
				mr.material.color = hitColor;
				Invoke("restoreColor", 0.1f);
			}


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

			if (bAffectsEntireBody)
				rigidComponents = findMe.gameObject.GetComponentsInChildren(typeof(Rigidbody));
			else
				rigidComponents = other.gameObject.GetComponentsInChildren(typeof(Rigidbody));

			mulMod = bAffectsEntireBody? 1 : 1.0f / (Mathf.Clamp(rigidComponents.Length / 4f, 1, 666 ));
			orgMulMod = mulMod;
			mulMod = orgMulMod * StaticManager.GetTimeStepMul ();

			foreach (Component c in rigidComponents)
			{
				Rigidbody rb = (Rigidbody)c;

//				rb.velocity = rb.velocity * multiplier;
				rb.velocity = new Vector3(rb.velocity.x * multiplier.x, rb.velocity.y * multiplier.y, rb.velocity.z * multiplier.z);

				float initialXForce = minRandomXForce + Random.Range (0, Mathf.Clamp (maxRandomXForce - minRandomXForce, 0, 999999));
				float initialYForce = minRandomYForce + Random.Range (0, Mathf.Clamp (maxRandomYForce - minRandomYForce, 0, 999999));
				float initialZForce = minRandomZForce + Random.Range (0, Mathf.Clamp (maxRandomZForce - minRandomZForce, 0, 999999));

				if (forceVelocity == false) {
					rb.AddForce (initialXForce * mulMod + Random.Range(0f,randomizerValue) - randomizerValue/2, initialYForce * mulMod + Random.Range(0f,randomizerValue) - randomizerValue/2, initialZForce * mulMod + Random.Range(0f,randomizerValue) - randomizerValue/2, forceMode);
				} else {
					Vector3 newVelocity = Vector3.zero;
					if (initialXForce != 0)
						newVelocity = GameUtil.SetX (newVelocity, initialXForce + Random.Range(0f,randomizerValue) - randomizerValue/2);
					if (initialYForce != 0)
						newVelocity = GameUtil.SetY (newVelocity, initialYForce + Random.Range(0f,randomizerValue) - randomizerValue/2);
					if (initialZForce != 0)
						newVelocity = GameUtil.SetZ (newVelocity, initialZForce + Random.Range(0f,randomizerValue) - randomizerValue/2);

					rb.velocity = newVelocity;
				}

				float maxForcedRot = Mathf.Max (forcedRotation.x, forcedRotation.y, forcedRotation.z);
				if (maxForcedRot > 7)
					rb.maxAngularVelocity = maxForcedRot;
				if (maxForcedRot > 0) {
					rb.AddTorque (forcedRotation);
					findMe.GetComponent<ToonDollHelper> ().EnableAngularVelocityCheck ();
				}
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
			zSum = ySum = xSum = 0;
			Invoke ("resetBoost", resetTime);

			if (disableOnHit) {
				GetComponent<MeshRenderer> ().enabled = false;
			}

		}
	}

	void OnCollisionEnter(Collision collision)
	{
		OnTriggerEnter (collision.collider);
	}

	void OnTriggerStay(Collider other) {
		if (enableCollisionStay)
			OnTriggerEnter (other);
	}

	void OnCollisionStay(Collision collision)
	{
		OnTriggerStay (collision.collider);
	}

	void OnTriggerExit(Collider other) {
/*		GameObject findMe = FindParentWithTag (other.gameObject, "Player");

		if (findMe != null && wasUsed) {
			Debug.Log ("2");
			wasUsed = false;
		} */
	}
}
