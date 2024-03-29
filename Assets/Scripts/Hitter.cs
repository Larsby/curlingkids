using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface PushButton {
	void AddButtonListener (Button b);
	int GetPushButtonIndex ();
	void ResetState ();
}

public class Hitter : MonoBehaviour , PushButton {

	public float scalePlus = 1;
	public float hitSpeed = 0.1f;
	public bool retreat = true;
	public float retreatSpeed = 0.5f;
	public float loweredTimeStepTime = 0;
	public float loweredTimeStepValue = 0.004f;
	private int timeStepAffectIndex;
	public int pushButtonIndex = 1;

	private Vector3 orgScale;

	public SingleSfx sfx = SingleSfx.Button2;
		

	void Start () {
		orgScale = transform.localScale;
	}

	private void Retreat() {
		LeanTween.scaleX (this.gameObject, orgScale.x, retreatSpeed);
	}

	private void restoreFixedTimeStep() {
		StaticManager.RestoreTimeStep(timeStepAffectIndex);
	}

	public void AddButtonListener(Button b) {
		b.onClick.AddListener (Hit);
	}

	public int GetPushButtonIndex() {
		return pushButtonIndex;
	}

	public void ResetState() {
		Invoke ("Retreat", hitSpeed);
	}

	public void Hit() {

		if (sfx != SingleSfx.None)
			SoundManager.instance.PlaySingleSfx (sfx, false, false, 0, 0.8f);

		LeanTween.cancel (gameObject);

		if (loweredTimeStepTime > 0) {
			timeStepAffectIndex = StaticManager.PushFixedTimeStep (loweredTimeStepValue);
			Invoke ("restoreFixedTimeStep", loweredTimeStepTime);
		}

		LeanTween.scaleX (this.gameObject, orgScale.y + scalePlus, hitSpeed);

		if (retreat)
			Invoke ("Retreat", hitSpeed);
	}

	void Update () {

		if (Input.GetKeyDown (KeyCode.L)) {
			Hit ();
		}
	}

}
