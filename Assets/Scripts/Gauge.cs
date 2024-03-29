using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gauge : MonoBehaviour {

	public enum Process { Idle, Ongoing, Completed };

	public RectTransform gauge;
	public float speed = 1;
	public float pos = 0;
	public float rotationRange = 90;
	public bool forward = true;
	public Process state;

	private float orgPos, orgSpeed, orgRange;
	private bool orgForward;

	void Awake () {
		orgPos = pos; orgSpeed = speed; orgRange = rotationRange;
		orgForward = forward;
	}
	
	void Update () {
		if (state != Process.Ongoing)
			return;

		if (Input.GetKeyDown (KeyCode.N)) {
		} else
		if (Input.anyKeyDown) {
			state = Process.Completed;
		}

		pos += forward? speed * Time.deltaTime : -speed * Time.deltaTime;
		if (pos > 1.0f) {
			pos = 1;
			forward = false;
		}
		if (pos < 0) {
			pos = 0;
			forward = true;
		}

		gauge.rotation = Quaternion.Euler (0, 0, (pos * 2 - 1) * -rotationRange);
	}

	public void Enable() {
		state = Process.Ongoing;
	}

	public void Reset() {
		state = Process.Idle;
		pos = orgPos;
		speed = orgSpeed;
		rotationRange = orgRange;
		forward = orgForward;
		gauge.rotation = Quaternion.identity;
	}
}
