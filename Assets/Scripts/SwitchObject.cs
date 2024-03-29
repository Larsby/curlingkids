using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchObject : InvokeWithObject, PushButton {

	// Note: if using SwitchOnPressAndRelease, disabling yourself will not work since then Update stops running! In this case, place the script on another object instead 

	public enum SwitchType { SwitchOnTap, SwitchOnReset, SwitchOnPressAndRelease, Periodic_NoPushbutton, SingleSwitch };

	public int pushButtonIndex = 1;
	public GameObject [] activateObjects;
	public GameObject [] disableObjects;
	private Vector3 [] orgPosVectors;
	private Vector3 [] destPosVectors;
	private Vector3 [] orgRotVectors;
	private Vector3 [] destRotVectors;
	private Vector3 [] orgScaleVectors;
	private Vector3 [] destScaleVectors;
	private int nofActivateObjects;
	public float resetTime = 0.1f;
	public float activateTime = 0;
	public float disableAnimTime = 0;
	public float enableAnimTime = 0;
	public SwitchType switchType = SwitchType.SwitchOnTap;
	public KeyCode keyCodeForDebug = KeyCode.None;
	private PointerListener pointerListener = null;

	private bool simpleAnim = false;
	public Vector3 moveVector = Vector3.zero;
	public Vector3 rotVector = Vector3.zero;
	public Vector3 scaleVector = Vector3.zero;
	public bool localRotation = false;
	public LeanTweenType easeType = LeanTweenType.linear;
	public float animTime = 0.3f;
	private int objIndex;

	private bool switched = false;
	private bool isReady = true;
	private bool wasPressed = false;

	public bool noActivationOrDeactivation = false;

	public List<GameObject> sendMessageObjects;
	public bool sendOnce = false;
	public List<GameObject> addObjectsOnCollide;
	public bool reCollide = true;
	public float reCollideTime = 1f;

	public string switchMessage;
	public string messageParamSwitchOn;
	public string messageParamSwitchOff;
	public string messageParamOnCollide;

	public bool flipColliderActiveOnSwitch = false;

	public SingleSfx sfx = SingleSfx.Button2;


	void Start () {

		if (moveVector != Vector3.zero || rotVector != Vector3.zero || scaleVector != Vector3.zero)
			simpleAnim = true;

		if (activateObjects.Length == 0 && disableObjects.Length == 0)
			activateObjects = new GameObject[] { this.gameObject };

		if (simpleAnim && (activateObjects.Length > 0 || disableObjects.Length > 0)) {
			nofActivateObjects = activateObjects.Length;
			orgPosVectors = new Vector3[activateObjects.Length + disableObjects.Length];
			destPosVectors = new Vector3[activateObjects.Length + disableObjects.Length];
			orgRotVectors = new Vector3[activateObjects.Length + disableObjects.Length];
			destRotVectors = new Vector3[activateObjects.Length + disableObjects.Length];
			orgScaleVectors = new Vector3[activateObjects.Length + disableObjects.Length];
			destScaleVectors = new Vector3[activateObjects.Length + disableObjects.Length];

			for (int i = 0; i < activateObjects.Length; i++) {
				orgPosVectors [i] = activateObjects [i].transform.position;
				destPosVectors [i] = activateObjects [i].transform.position + moveVector;
				orgRotVectors [i] = localRotation? activateObjects [i].transform.localRotation.eulerAngles : activateObjects [i].transform.rotation.eulerAngles;
				destRotVectors [i] = localRotation? activateObjects [i].transform.localRotation.eulerAngles + rotVector : activateObjects [i].transform.rotation.eulerAngles + rotVector;
				orgScaleVectors [i] = activateObjects [i].transform.localScale;
				destScaleVectors [i] = activateObjects [i].transform.localScale + scaleVector;
			}
			for (int i = 0; i < disableObjects.Length; i++) {
				orgPosVectors [i + nofActivateObjects] = disableObjects [i].transform.position - moveVector;
				destPosVectors [i + nofActivateObjects] = disableObjects [i].transform.position;
				orgRotVectors [i + nofActivateObjects] = localRotation? disableObjects [i].transform.localRotation.eulerAngles - rotVector : disableObjects [i].transform.rotation.eulerAngles - rotVector;
				destRotVectors [i + nofActivateObjects] = localRotation? disableObjects [i].transform.localRotation.eulerAngles : disableObjects [i].transform.rotation.eulerAngles;
				orgScaleVectors [i + nofActivateObjects] = disableObjects [i].transform.localScale - scaleVector;
				destScaleVectors [i + nofActivateObjects] = disableObjects [i].transform.localScale;
			}
		}

		if (switchType == SwitchType.Periodic_NoPushbutton) {
			pushButtonIndex = -1;
			Switch ();
		}
	}
	
	void Update () {
		if (Input.GetKeyDown (keyCodeForDebug))
			Switch ();

		if (wasPressed && pointerListener != null && !pointerListener.isPressed()) {
			Switch ();
			wasPressed = false;
		}

		if (!wasPressed && pointerListener != null && pointerListener.isPressed()) {
			Switch ();
			wasPressed = true;
		}
	}

	public void AddButtonListener(Button b) {
		if (switchType == SwitchType.SwitchOnPressAndRelease)
			pointerListener = b.GetComponent<PointerListener> ();
		else if (switchType != SwitchType.Periodic_NoPushbutton)
			b.onClick.AddListener (Switch);
	}


	public int GetPushButtonIndex() {
		return pushButtonIndex;
	}

	public void ResetState(){
		ResetInitial ();
	}

	private void ActivateObject(GameObject go) {
		if (!noActivationOrDeactivation)
			go.SetActive (true);

		// Not using this at the moment and it produces lots of warnings if animator present but not this state. Possibly use public string for this instead
		/* Animator animator = go.GetComponent<Animator> ();
		if (animator != null) {
			animator.Play ("Activated");
		} */

		if (simpleAnim) {
			LeanTween.cancel (go);
			if (moveVector != Vector3.zero)
				LeanTween.move (go, destPosVectors[objIndex], animTime).setEase(easeType);
			if (rotVector != Vector3.zero) {
				if (localRotation)
					LeanTween.rotateLocal (go, destRotVectors [objIndex], animTime).setEase (easeType);
				else
					LeanTween.rotate (go, destRotVectors [objIndex], animTime).setEase (easeType);
			}
			if (scaleVector != Vector3.zero)
				LeanTween.scale (go, destScaleVectors[objIndex], animTime).setEase(easeType);
		}
	}

	private IEnumerator DelayedDisable(float delayTime, GameObject go) {
		yield return new WaitForSeconds(delayTime);
		if (!noActivationOrDeactivation)
			go.SetActive (false);
	}

	private void DisableObject(GameObject go) {
		if (disableAnimTime < 0)
			;
		else if (disableAnimTime == 0 && !noActivationOrDeactivation)
			go.SetActive (false);
		else if (disableAnimTime > 0) {
			StartCoroutine(DelayedDisable(disableAnimTime, go));
		}

		// Not using this at the moment and it produces lots of warnings if animator present but not this state. Possibly use public string for this instead
		/* Animator animator = go.GetComponent<Animator> ();
		if (animator != null) {
			animator.Play ("Disabled");
		}*/

		if (simpleAnim) {
			LeanTween.cancel (go);
			if (moveVector != Vector3.zero)
				LeanTween.move (go, orgPosVectors[objIndex], animTime).setEase(easeType);
			if (rotVector != Vector3.zero) {
				if (localRotation)
					LeanTween.rotateLocal (go, orgRotVectors [objIndex], animTime).setEase (easeType);
				else
					LeanTween.rotate (go, orgRotVectors [objIndex], animTime).setEase (easeType);
			}
			if (scaleVector != Vector3.zero)
				LeanTween.scale (go, orgScaleVectors[objIndex], animTime).setEase(easeType);
			
		}

	}

	private void Reset() {
		isReady = true;
	}

	private void ResetInitial() {

		if (activateObjects != null && disableAnimTime >= 0) {
			foreach (GameObject g in activateObjects) {
				if (!noActivationOrDeactivation)
					g.SetActive(false);
			}
		}
		if (disableObjects != null) {
			foreach (GameObject g in disableObjects) {
				if (!noActivationOrDeactivation)
					g.SetActive(true);
			}
		}
		switched = false;
		isReady = true;
	}

	private void DelayedReady () {
		isReady = true;
	}


	private void ResetTimed() {

		if (activateObjects != null) {
			objIndex = 0;
			foreach (GameObject g in activateObjects) {
				DisableObject (g);
				objIndex++;
			}
		}
		if (disableObjects != null) {
			objIndex = 0;
			foreach (GameObject g in disableObjects) {
				ActivateObject (g);
				objIndex++;
			}
		}

		switched = false;

		if (enableAnimTime <= 0)
			isReady = true;
		else
			Invoke ("DelayedReady", enableAnimTime);

		if (switchType == SwitchType.Periodic_NoPushbutton)
			Invoke ("Switch", resetTime);
	}

	public void Switch() {

		if (!isReady) 
			return;

		if (sfx != SingleSfx.None)
			SoundManager.instance.PlaySingleSfx (sfx, false, false, 0, 0.8f);


		if (switchMessage != null && sendMessageObjects != null && switchMessage.Length > 0 && sendMessageObjects.Count > 0) {
			string switchParam = switched ? messageParamSwitchOff : messageParamSwitchOn;

			foreach (GameObject g in sendMessageObjects) {
				g.SendMessage (switchMessage, switchParam);

				collideResets.Add (g);
				InvokeWO (RemoveReset, reCollideTime, g);
			}
			if (sendOnce)
				sendMessageObjects.Clear ();
		}


		if (activateObjects != null) {
			objIndex = 0;
			foreach (GameObject g in activateObjects) {
				if (!switched) ActivateObject (g); else DisableObject (g);
				objIndex++;
			}
		}
		if (disableObjects != null) {
			objIndex = nofActivateObjects;
			foreach (GameObject g in disableObjects) {
				if (!switched) DisableObject (g); else ActivateObject (g);
				objIndex++;
			}
		}

		if (flipColliderActiveOnSwitch) {
			foreach (GameObject g in activateObjects) {
				Collider c = g.GetComponent<Collider> ();
				if (c != null)
					c.enabled = !c.enabled;
			}
			foreach (GameObject g in disableObjects) {
				Collider c = g.GetComponent<Collider> ();
				if (c != null)
					c.enabled = !c.enabled;
			}
		}


		if (switchType == SwitchType.SwitchOnTap) {
			Invoke ("Reset", resetTime);
		} else if (switchType == SwitchType.SwitchOnReset || switchType == SwitchType.Periodic_NoPushbutton) {
			Invoke ("ResetTimed", resetTime);
		} else if (switchType == SwitchType.SwitchOnPressAndRelease) {
			Invoke ("Reset", 0);
		}

		isReady = false;
		switched = !switched;
	}


	private List<GameObject> collideResets = new List<GameObject> ();

	void RemoveReset(object param) {
		collideResets.Remove((GameObject)param);
	}

	void OnTriggerEnter(Collider other) {
		if (switchMessage.Length > 0 && addObjectsOnCollide.Count > 0) {

			foreach (GameObject g in addObjectsOnCollide) {
				if (g == other.gameObject && !collideResets.Contains(g)) {

					g.SendMessage (switchMessage, messageParamOnCollide, SendMessageOptions.DontRequireReceiver);
					// g.transform.position = GameUtil.AddZ (g.transform.position, 1);

					if (!sendMessageObjects.Contains(g))
						sendMessageObjects.Add (g);
				}
			}
			if (!reCollide)
				addObjectsOnCollide.Remove (other.gameObject);

		}

	}

	void OnCollisionEnter(Collision collision)
	{
		OnTriggerEnter (collision.collider);
	}

}
