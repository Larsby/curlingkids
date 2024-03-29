using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Teleporter : MonoBehaviour, PushButton {

	public enum TransportPointSelect
	{
		Random, Progressive, PushButtonProgressive
	};

	public GameObject [] teleportPoints;
	public TransportPointSelect randomPoint = TransportPointSelect.Random;
	public bool teleportPlayerHelpers = false;
	private int destinationIndex = 0;
	public int pushButtonIndex = -1;
	public bool resetProgressiveOnNewDoll = false;
	private bool wasUsed = false;
	public float resetUsedTime = 0.6f;
	public SingleSfx sfx = SingleSfx.Teleport;
	public SingleSfx sfxPush = SingleSfx.Button1;

	public Color inactivePointColor = Color.gray;
	public Color activePointColor = Color.white;

	public bool inactiveIndicator = false;

	void Start () {
		SetColors ();
		SetIndicators();
	}
	
	void Update () {
	}

	void ResetUsed() {
		wasUsed = false;
	}
 
	void OnTriggerEnter(Collider other) {

		if (teleportPoints == null || teleportPoints.Length < 1)
			return;

		if (wasUsed)
			return;

		if (!this.enabled)
			return;

		GameObject findMe = GameUtil.FindParentWithTag (other.gameObject, "Player");

		if (findMe != null) {

			if (!teleportPlayerHelpers) {
				ToonDollHelper tdh = findMe.GetComponent<ToonDollHelper> ();
				if (tdh != null && tdh.IsPlayerHelper ())
					return;
			}

			Transform rootT = findMe.transform.Find ("Root");
			if (rootT != null) {
				if (randomPoint == TransportPointSelect.Random) {
					rootT.gameObject.transform.position = teleportPoints [Random.Range (0, teleportPoints.Length)].transform.position;
				} else {
					rootT.gameObject.transform.position = teleportPoints [destinationIndex].transform.position;
				}

				wasUsed = true;
				Invoke ("ResetUsed", resetUsedTime);

				if (sfx != SingleSfx.None)
				{
					SoundManager.instance.PlaySingleSfx(sfx, false, false, 0, 1f);
				}

				if (randomPoint == TransportPointSelect.Progressive) {
					destinationIndex++;
					if (destinationIndex >= teleportPoints.Length)
						destinationIndex = 0;
				}
			}

		}
	}

	private void SetColors() {
		for (int i = 0; i < teleportPoints.Length; i++) {
			ParticleSystem ps = teleportPoints [i].GetComponentInParent<ParticleSystem> ();
			if (ps != null) {
				ParticleSystem.MainModule main = ps.main;
				main.startColor = i == destinationIndex ? activePointColor : inactivePointColor;
			}
		}
	}

	private void SetIndicators()
	{
		for (int i = 0; i < teleportPoints.Length; i++)
		{
			Transform t = teleportPoints[i].transform.Find("INDICATOR");
			if (t != null)
			{
				if (inactiveIndicator == false)
					t.gameObject.SetActive(i == destinationIndex ? true : false);
				else
					t.gameObject.SetActive(i == destinationIndex ? false : true);
			}
		}
	}

	public void Switch() {
		destinationIndex++;
		if (destinationIndex >= teleportPoints.Length)
			destinationIndex = 0;

		if (sfxPush != SingleSfx.None)
			SoundManager.instance.PlaySingleSfx (sfxPush, false, false, 0, 0.8f);

		SetColors ();
		SetIndicators();
	}

	public void AddButtonListener(Button b) {
		b.onClick.AddListener (Switch);
	}


	public int GetPushButtonIndex() {
		return pushButtonIndex;
	}

	public void ResetState() {
		if (resetProgressiveOnNewDoll)
			destinationIndex = 0;
	}


}
