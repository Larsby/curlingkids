using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyMaker : MonoBehaviour {

	public int nofCredits = 1;
	public float removeTime = -1;
	private bool repeatedReward = false;
	private float repeatedDelay = 1f;
	public bool regenerate = false;

	private bool wasHit = false;
	private float repeatTimer = 0;
	public bool isJewel = false;

	public SingleSfx sfx = SingleSfx.GetMoney;

	void Start () {
	}
	
	void Update () {
		if (repeatTimer > 0)
			repeatTimer -= Time.deltaTime;
	}

	void OnCollisionEnter(Collision collision)
	{
		OnTriggerEnter (collision.collider);
	}

	void OnTriggerEnter(Collider collider) {

		if (wasHit && repeatedReward == false)
			return;
		if (repeatTimer > 0)
			return;

		GameObject findMe = GameUtil.FindParentWithTag (collider.gameObject, "Player");

		ToonDollHelper tdh = null;
		if (findMe != null)
			tdh = findMe.GetComponent<ToonDollHelper>();

		if (findMe != null && !wasHit && tdh != null && !tdh.isPlayerHelper && !tdh.zombieHitMode) {

			if (removeTime >= 0)
				Destroy (this.gameObject, removeTime);
			
			repeatTimer = repeatedDelay;
			wasHit = true;

			int multi = 1;
			if (tdh != null)
			{
				multi = tdh.extra_creditMultiplier;
				tdh.ChangeFace(ToonDollHelper.FaceType.Happy);
				tdh.ChangeFace(ToonDollHelper.FaceType.Neutral, 1.5f);
			}

			StaticManager.AddTemporaryCredits (nofCredits * multi, regenerate? null : this);

			SoundManager.instance.PlaySingleSfx (sfx, true, false, 0, 0.7f);
		}
	}

}
