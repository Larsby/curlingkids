using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shooter : MonoBehaviour, PushButton, CollisionListener {

	public int pushButtonIndex = 1;

	public float rotationRange = -60;
	public float shootSpeed = 0.1f;
	public float retreatSpeed = 0.2f;
	public bool alwaysCompleteHit = true;

	private Vector3 toPos;
	private float initialRotation = float.MaxValue;
	private PointerListener pointerListener = null;

	private bool wasPressed = false;
	private bool reachedTop = false;

	public KeyCode keyCode = KeyCode.Z;
	public float speedupTime = 1;
	public float speedupForce = 10;

	public SingleSfx sfx = SingleSfx.Button2;

	private bool pressedLastFrame = false;

	public float loweredTimeStepTime = 0;
	public float loweredTimeStepValue = 0.002f;
	private int timeStepAffectIndex;


	void Start () {
		Vector3 rotPos = transform.localRotation.eulerAngles;
		toPos = GameUtil.AddY (rotPos, rotationRange);

		ColliderBridge cb = GetComponentInChildren<ColliderBridge> ();
		if (cb != null)
			cb.Initialize (this);
	}

	void Update () {
		Vector3 rotPos = transform.localRotation.eulerAngles;

		float cmp1 = initialRotation + rotationRange;
		float cmp2 = 360 + (initialRotation + rotationRange);
		if (rotPos.y == cmp1 || rotPos.y == cmp2) {
			reachedTop = true;
		}

		/* KEY STUFF FOR DEV ONLY */
		if (Input.GetKey (keyCode) && !wasPressed) {
			Shoot ();
		}
		if (wasPressed && !Input.GetKey(keyCode) && (reachedTop || !alwaysCompleteHit)) {
			LeanTween.cancel(gameObject);
			LeanTween.rotateLocal (gameObject, GameUtil.SetY (rotPos, initialRotation), retreatSpeed);
			wasPressed = false;
			reachedTop = true;
		}
		/* */

		if (!wasPressed && pointerListener != null && pointerListener.isPressed()) {
			Shoot ();
		}

		if (wasPressed && pointerListener != null && !pointerListener.isPressed() && (reachedTop || !alwaysCompleteHit)) {
			LeanTween.cancel(gameObject);
			LeanTween.rotateLocal (gameObject, GameUtil.SetY (rotPos, initialRotation), retreatSpeed);
			wasPressed = false;
			reachedTop = true;
		}

		if (!LeanTween.isTweening(gameObject) && rotPos.y != initialRotation && rotPos.y != cmp1 && rotPos.y != cmp2) {
			LeanTween.cancel(gameObject);
			LeanTween.rotateLocal(gameObject, GameUtil.SetY(rotPos, initialRotation), retreatSpeed);
			wasPressed = false;
			reachedTop = true;
		}

//		Debug.Log (wasPressed + "  " + LeanTween.isTweening(gameObject));
//		Debug.Log (rotPos.y + "  " + cmp1 + "   " + cmp2);

		pressedLastFrame = pointerListener.isPressed ();
	}

	private void restoreFixedTimeStep()
	{
		StaticManager.RestoreTimeStep(timeStepAffectIndex);
	}

	public void AddButtonListener(Button b) {
		// b.onClick.AddListener (Shoot);
		pointerListener = b.GetComponent<PointerListener> ();
	}

	public int GetPushButtonIndex() {
		return pushButtonIndex;
	}

	public void ResetState() {
		Vector3 rotPos = transform.localRotation.eulerAngles;
		if (initialRotation == float.MaxValue) initialRotation = rotPos.y;

		transform.localRotation = Quaternion.Euler(GameUtil.SetY(rotPos, initialRotation));
	}

	public void Shoot() {

		if (sfx != SingleSfx.None && pressedLastFrame == false)
			SoundManager.instance.PlaySingleSfx (sfx, false, false, 0, 0.8f);

		LeanTween.cancel(gameObject);

		Vector3 rotPos = transform.localRotation.eulerAngles;
		LeanTween.rotateLocal(gameObject, toPos, shootSpeed);

		wasPressed = true;
		reachedTop = false;

		if (loweredTimeStepTime > 0 && loweredTimeStepValue > 0)
		{
			timeStepAffectIndex = StaticManager.PushFixedTimeStep(loweredTimeStepValue);
			Invoke("restoreFixedTimeStep", loweredTimeStepTime);
		}
	}

	public void OnCollisionExit(Collision collision) {

		GameObject findMe = GameUtil.FindParentWithTag (collision.collider.gameObject, "Player");

		if (findMe != null) {
			ToonDollHelper tdh = findMe.GetComponent<ToonDollHelper> ();
			if (tdh != null) {
				tdh.SetSpeedupTimer (speedupTime, speedupForce);
			}
		}
	}

	public void OnCollisionStay(Collision collision) {

		GameObject findMe = GameUtil.FindParentWithTag (collision.collider.gameObject, "Player");

		if (findMe != null) {
			ToonDollHelper tdh = findMe.GetComponent<ToonDollHelper> ();
			if (tdh != null) {
				tdh.SetStillStandTime (0.5f, true);
			}
		}
	}

	public void OnCollisionEnter(Collision collision) {}
	public void OnTriggerEnter(Collider collider) {}
	public void OnTriggerExit(Collider collider) {}
	public void OnTriggerStay(Collider collider) {}
	public void SetAffectedColliderBridgeObject(GameObject go) {}

}
