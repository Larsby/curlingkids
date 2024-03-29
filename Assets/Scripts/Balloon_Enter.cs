using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon_Enter : MonoBehaviour {

	public GameObject killBalloon;
	public GameObject boomParticle;

	public SingleSfx singleSfx = SingleSfx.Baloon2;

	private SoundEmitter soundEmitter = null;

	void Start () {
		soundEmitter = GetComponent<SoundEmitter> ();
	}

	private void OnTriggerEnter(Collider collider)
	{
		GameObject findMe = GameUtil.FindParentWithTag (collider.gameObject, "Player");
		ToonDollHelper tdh = null;
		if (findMe != null)
			tdh = findMe.GetComponent<ToonDollHelper>();

		if (findMe != null && tdh != null && !tdh.isPlayerHelper && !tdh.zombieHitMode)
		{
			killBalloon.SetActive (false);	
			boomParticle.SetActive (true);

			if (soundEmitter == null) {
				SoundManager.instance.PlaySingleSfx (singleSfx, true, false, 0, 0.44f);
			} else {
				if (soundEmitter.emitterType == SoundEmitter.EmitterType.RemoteControlled)
					soundEmitter.PlaySound ();
			}
		}
	}

}
