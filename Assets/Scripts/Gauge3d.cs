using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gauge3d : MonoBehaviour {

	public enum ListenType { CLICK, DOWN, UP }

	public enum Process { Idle, Ongoing, Completed };

	public float speed = 1;
	public float pos = 0;
	public float rotationRange = 12.5f;
	public bool forward = true;
	public Process state = Process.Idle;
	public bool rotation = true;

	private float orgPos, orgSpeed, orgRange;
	private bool orgForward;

	private bool bOrgValuesSet = false;

	private float rotationSkew = 0;

	private float tossForceX = 1;

	public ListenType keyListenType = ListenType.CLICK;

	private PointerListener pointerListener;
	private bool wasClicked = false;

	void Awake () {
		SetOriginalValues ();
	}

	public void SetTossForceX(float value) {
		tossForceX = value;
	}

	private void SetOriginalValues() {
		if (!bOrgValuesSet) {
			orgPos = pos;
			orgSpeed = speed;
			orgRange = rotationRange;
			orgForward = forward;
			bOrgValuesSet = true;
		}
	}

	public void SetPointerListener(PointerListener pl) {
		Button b = pl.gameObject.GetComponent<Button> ();
		if (b != null)
			b.onClick.AddListener (OnClickedButton);
		pointerListener = pl;
	}

	public void OnClickedButton() {
		wasClicked = true;
	}

	void Update () {

		if (state != Process.Ongoing)
			return;

		if (keyListenType == ListenType.CLICK && wasClicked) {
			state = Process.Completed;
		}
		if (keyListenType == ListenType.DOWN && pointerListener.isPressed()) {
			state = Process.Completed;
		}
		if (keyListenType == ListenType.UP && !pointerListener.isPressed()) {
			state = Process.Completed;
		}
		wasClicked = false;

		pos += forward? speed * Time.deltaTime : -speed * Time.deltaTime;
		if (pos > 1.0f) {
			pos = 1;
			forward = false;
		}
		if (pos < 0) {
			pos = 0;
			forward = true;
		}

		if (rotation)
			this.transform.rotation = Quaternion.Euler (90, (180 + rotationSkew) - (pos * 2 - 1) * (-rotationRange * tossForceX), 0);
		else
			this.transform.localScale = new Vector3 (0.1f, 0.0033f + (pos / 10), 0.1f);
	}

	public void Enable() {
		state = Process.Ongoing;
	}

	public void SetPosition(float x, float z) {
		transform.position = new Vector3 (x, transform.position.y, z);

		rotationSkew = -x * 3.25f;

		this.transform.rotation = Quaternion.Euler (90, 180 + rotationSkew, 0);
	}

	public void SetSkew(float skew) {
		rotationSkew = skew;
	}
	public float GetSkew() {
		return rotationSkew;
	}

	public void Reset() {
		SetOriginalValues ();

		state = Process.Idle;
		pos = orgPos;
		speed = orgSpeed;
		rotationRange = orgRange;
		forward = orgForward;
		this.transform.rotation = Quaternion.Euler (90, 180, 0);
		rotationSkew = 0;
	}
		
}
