using UnityEngine;

public class TriggerGhostAnim : InvokeWithObject {
	public string ghostAnimName = "";
	private bool wasUsed = false;
	public float blendTime = 0.6f;
	public bool immediateStop = false;
	public float animTime = 0;
	public float reEnableTriggerWaitTime = 2f;
	public float triggerProbability = 1f;
	public SingleSfx sfx = SingleSfx.None;
	public SfxRandomType sfxRandom = SfxRandomType.None;
	public bool randomPitch = false;
	public float volume = 1f;
	public float stopOnHitMinTime = -1f;

	void Start () {}

	private void Reset() {
		wasUsed = false;
	}

	void OnCollisionEnter(Collision collision) {
		OnTriggerEnter (collision.collider);
	}

	void OnTriggerEnter(Collider other) {

		GameObject findMe = GameUtil.FindParentWithTag (other.gameObject, "Player");

		if (findMe != null && !wasUsed) {

			ToonDollHelper tdh = findMe.GetComponent<ToonDollHelper> ();

			if (tdh != null && !tdh.isPlayerHelper && !tdh.zombieHitMode && Random.Range(0f, 1f) < triggerProbability) {

				if (ghostAnimName.Length > 0) {
					tdh.PlayGhostAnim (ghostAnimName, blendTime, true, stopOnHitMinTime);

					if (sfx != SingleSfx.None)
						SoundManager.instance.PlaySingleSfx(sfx, randomPitch, false, 0, volume);
					else if (sfxRandom != SfxRandomType.None)
						SoundManager.instance.PlayRandomFromType(sfxRandom, -1, 0, volume, -1, randomPitch);

					if (animTime > 0) {
						InvokeWO (StopAnim, animTime, tdh);
					}

					Invoke ("Reset", reEnableTriggerWaitTime);
				} else {
					if (immediateStop)
						tdh.StopGhostAnim ();
					else
						tdh.FadeGhostAnim ();
				}

				wasUsed = true;
			}
		}
	}

	private void StopAnim(System.Object myObject) {
		ToonDollHelper myTdh = myObject as ToonDollHelper;

		if (myTdh != null) {
			if (immediateStop)
				myTdh.StopGhostAnim ();
			else
				myTdh.FadeGhostAnim ();
		}
	}

}
