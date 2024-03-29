using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flipper : MonoBehaviour {

	private bool wasUsed = false;
	private Component[] rigidComponents = null;

	public float multiplier = 1;

	public float minRandomXForce = 0, maxRandomXForce = 0;
	public float minRandomYForce = 0, maxRandomYForce = 0;
	public float minRandomZForce = 0, maxRandomZForce = 0;

	public bool bExplodes = false;
	public bool bExplodesAtHitPont = true;

	public float loweredTimeStepTime = 0;
	public float loweredTimeStepValue = 0.004f;
	private int timeStepAffectIndex;

	private float mulMod = 1;

	private ForceMode forceMode = ForceMode.Force;

	public Color hitColor = Color.red;

	public bool forceGround = true;

	public bool bAffectsEntireBody = true;

	public float forcedMinSpeed = 0;
	public float forcedMaxSpeed = 0;

	public bool bounceRetarded = false;

	public bool disableOnHit = false;
	public bool enableOnReset = false;
	public float resetTime = 0.1f;

	public SingleSfx sfx = SingleSfx.Teleport2;
	public float sfxVolume = 1f;


	void Start () {
	}

	void Update () {
	}

	private Color oldColor;

	private void restoreColor() {
		MeshRenderer mr = this.GetComponent<MeshRenderer>();
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

	void OnCollisionEnter(Collision collision) {

		GameObject findMe = GameUtil.FindParentWithTag (collision.collider.gameObject, "Player");

		if (findMe != null && !wasUsed) {

			MeshRenderer mr = this.GetComponent<MeshRenderer>();
			oldColor = mr.material.color;
			mr.material.color = hitColor;
			Invoke ("restoreColor", 0.1f);
			mulMod = StaticManager.GetTimeStepMul ();

			if (loweredTimeStepTime > 0) {

				timeStepAffectIndex = StaticManager.PushFixedTimeStep (loweredTimeStepValue);
				Invoke ("restoreFixedTimeStep", loweredTimeStepTime);
			}

			if (bAffectsEntireBody)
				rigidComponents = findMe.gameObject.GetComponentsInChildren(typeof(Rigidbody));
			else
				rigidComponents = collision.collider.gameObject.GetComponentsInChildren(typeof(Rigidbody));

			foreach (Component c in rigidComponents)
			{
				Rigidbody rb = (Rigidbody)c;

				if (!bounceRetarded) {
					Vector3 reflected = Vector3.Reflect (rb.velocity, collision.contacts [0].normal);
					rb.velocity = reflected * multiplier;
				} else {
					rb.velocity = rb.velocity * -1 * multiplier;
				}

				if (forcedMinSpeed > 0 && rb.velocity.magnitude > 0 && rb.velocity.magnitude < forcedMinSpeed) {
					float mulMin = forcedMinSpeed / rb.velocity.magnitude;
					rb.velocity *= mulMin;
				}

				if (forcedMaxSpeed > 0 && rb.velocity.magnitude > 0 && rb.velocity.magnitude > forcedMaxSpeed) {
					float mulMax = forcedMaxSpeed / rb.velocity.magnitude;
					rb.velocity *= mulMax;
				}

				if (forceGround)
					rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
				
				float initialXForce, initialYForce, initialZForce;
				initialXForce = minRandomXForce + Random.Range (0, Mathf.Clamp (maxRandomXForce - minRandomXForce, 0, 999999));
				initialYForce = minRandomYForce + Random.Range (0, Mathf.Clamp (maxRandomYForce - minRandomYForce, 0, 999999));
				initialZForce = minRandomZForce + Random.Range (0, Mathf.Clamp (maxRandomZForce - minRandomZForce, 0, 999999));

				rb.AddForce (initialXForce * mulMod, initialYForce * mulMod, initialZForce * mulMod, forceMode);
			}

			if (sfx != SingleSfx.None)
				SoundManager.instance.PlaySingleSfx (sfx, false, false, 0, sfxVolume);

			if (bExplodes) {
				if (transform.childCount > 0) {
					Transform t = transform.GetChild (0);
					if (t != null) {
						t.gameObject.SetActive (true);
						if (bExplodesAtHitPont)
							t.gameObject.transform.position = collision.collider.attachedRigidbody.gameObject.transform.position;
					}
				}
			}

			wasUsed = true;
			Invoke ("resetBoost", resetTime);

			// Not using this at the moment and it produces lots of warnings if animator present but not this state. Possibly use public string for this instead
			/* Animator animator = GetComponent<Animator> ();
			if (animator) {
				animator.Play ("PlayerHit");
			}*/

			if (disableOnHit) {
				GetComponent<MeshRenderer> ().enabled = false;
			}
		}

	}
		
}
