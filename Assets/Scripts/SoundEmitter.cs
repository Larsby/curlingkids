using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEmitter : MonoBehaviour {

	public enum EmitterType { RemoteControlled, Immediate, OnCollidePlayer, OnCollideActivePlayer, OnCollideAll, OnCollideOther, OnCollapse };
	public enum PlayerType { Player, PlayerHelper, Both }

	public EmitterType emitterType = EmitterType.OnCollideAll;
	public PlayerType playerType = PlayerType.Both;

	public SingleSfx [] singleSfx;
	public SfxRandomType randomSfx = SfxRandomType.None;

	public float soundPlayProbability = 1;

	public float volume = 1;
	public float pitch = -1;
	public bool randomPitch = false;
	public float startDelay = 0;
	public Vector2 immediateDelayRandomTime = Vector2.zero;

	public int repeatSoundNof = 0; // <0 = infinite
	private int repeatCounter = 0;
	public float repeatDelayExtraTime = 0;
	public Vector2 repeatDelayExtraRandomTime = Vector2.zero;
	public bool repeatSameSound = true;
	private int lastPlayedIndex = -1, repeatIndex = -1;
	public bool stopOnHitObjectStopTouch = false;
	private GameObject lastHitObject = null;
	public bool fadeStop = true;

	public float minTriggerDelay = 0.25f;
	private float minTriggerTimer = 0;

	private float orgVolume, orgRepeatDelay;

	public ToonDollHelper[] zombieWatch;
	public bool autoFillZombieWatch = false;

	public Rigidbody[] fallWatch;
	public Vector3 fallSpeedTresholds = Vector3.zero;
	public bool singleFall = true;
	private bool hasFallen = false;

	private bool stillHitting = false;
	private int frameCount = 0;

	public GameObject speechBubblePrefab = null;
	public float speechBubbleDuration = 0;
	public bool speechBubbleToCollidingPlayer = true;
	public bool speechBubbleToThisObject = false;
	private ToonDollHelper collidedPlayer = null;

	public float speechShowProbability = 1;


	void Start () {
		if (emitterType == EmitterType.Immediate) {
			repeatCounter = repeatSoundNof;
			Invoke ("PlaySoundLoop", Random.Range (immediateDelayRandomTime.x, immediateDelayRandomTime.y));
		}

		orgVolume = volume;
		orgRepeatDelay = repeatDelayExtraTime;

		if (autoFillZombieWatch) {
			ToonDollHelper [] allTdh = GameObject.FindObjectsOfType<ToonDollHelper> ();
			List<ToonDollHelper> zombies = new List<ToonDollHelper> ();
			foreach (ToonDollHelper tdh in allTdh) {
				if (tdh.zombieHitMode)
					zombies.Add (tdh);
			}
			if (zombies.Count > 0)
				zombieWatch = zombies.ToArray ();
		}
	}

	void Update () {
		if (minTriggerTimer > 0)
			minTriggerTimer -= Time.deltaTime;

		if (emitterType == EmitterType.OnCollapse && fallWatch != null && fallWatch.Length > 0 && fallSpeedTresholds != Vector3.zero && minTriggerTimer <= 0) {

			if (singleFall && hasFallen)
				return;

			int nofFalling = 0;
			foreach (Rigidbody rb in fallWatch) {
				if (fallSpeedTresholds.x != 0 && ((fallSpeedTresholds.x < 0 && rb.velocity.x < fallSpeedTresholds.x) || (fallSpeedTresholds.x > 0 && rb.velocity.x > fallSpeedTresholds.x)))
					nofFalling++;
				else if (fallSpeedTresholds.y != 0 && ((fallSpeedTresholds.y < 0 && rb.velocity.y < fallSpeedTresholds.y) || (fallSpeedTresholds.y > 0 && rb.velocity.y > fallSpeedTresholds.y)))
					nofFalling++;
				else if (fallSpeedTresholds.z != 0 && ((fallSpeedTresholds.z < 0 && rb.velocity.z < fallSpeedTresholds.z) || (fallSpeedTresholds.z > 0 && rb.velocity.z > fallSpeedTresholds.z)))
					nofFalling++;
				// print (rb.velocity);
			}

			if (nofFalling == fallWatch.Length) {
				hasFallen = true;
				minTriggerTimer = minTriggerDelay;
				PlaySound ();
			}
		}
	}
		
	void OnTriggerEnter(Collider collider)
	{
		ToonDollHelper doll = null;

		if (emitterType == EmitterType.RemoteControlled || emitterType == EmitterType.Immediate || emitterType == EmitterType.OnCollapse)
			return;

		if (lastHitObject != null && stopOnHitObjectStopTouch)
			return;

		if (minTriggerTimer > 0)
			return;

		bool doPlay = true;
		GameObject findMe = null;

		if (emitterType != EmitterType.OnCollideAll) {

			findMe = GameUtil.FindParentWithTag (collider.gameObject, "Player");
			if (findMe != null) doll = findMe.GetComponent<ToonDollHelper> ();

			if (findMe == null) {
				if (emitterType != EmitterType.OnCollideOther)
					doPlay = false;
			} else {
				if (doll != null) {
					if (playerType == PlayerType.Player && doll.IsPlayerHelper ())
						doPlay = false;
					if (emitterType == EmitterType.OnCollideActivePlayer && !doll.IsActive ())
						doPlay = false;
					if (playerType == PlayerType.PlayerHelper && !doll.IsPlayerHelper ())
						doPlay = false;
				}
			}
		}

		if (doPlay) {
			lastHitObject = findMe != null ?findMe : collider.gameObject;
			stillHitting = true;

			if (speechBubbleToCollidingPlayer)
				collidedPlayer = doll;
				
			PlaySound ();
			minTriggerTimer = minTriggerDelay;
		}
	}

	void OnCollisionEnter(Collision collision) {
		OnTriggerEnter (collision.collider);
	}

	void OnTriggerStay(Collider collider)
	{
		if (!stopOnHitObjectStopTouch || lastHitObject == null)
			return;

		GameObject findMe = GameUtil.FindParentWithTag (collider.gameObject, "Player");
		GameObject match = findMe != null ? findMe : collider.gameObject;

		if (lastHitObject == match) {
			stillHitting = true;
		}
	}

	void OnCollisionStay(Collision collision) {
		OnTriggerStay (collision.collider);
	}


	public void PlaySound() {
		if (!this.enabled)
			return;

		if (soundPlayProbability < 1 && Random.Range(0f, 1f) > soundPlayProbability)
			return;

		repeatCounter = repeatSoundNof;
		PlaySoundLoop ();

		if (speechBubblePrefab != null && speechBubbleDuration > 0 && Random.Range(0f, 1f) <= speechShowProbability) {

			if (speechBubbleToCollidingPlayer) {
				if (collidedPlayer != null)
					collidedPlayer.ShowSpeechBubble (speechBubblePrefab, speechBubbleDuration);
			}

			if (speechBubbleToThisObject) {
				ToonDollHelper tdh = GetComponent<ToonDollHelper> ();
				if (tdh != null) {
					tdh.ShowSpeechBubble (speechBubblePrefab, speechBubbleDuration);
				}
			}
		}

		collidedPlayer = null;
	}

	private void PlaySoundLoop() {
		float playTime = 0;

		if (zombieWatch != null && zombieWatch.Length > 0) {
			int activeZombies = 0;
			foreach (ToonDollHelper tdh in zombieWatch) {
				if (tdh.zombieHitMode && !tdh.ZombieWasHit ())
					activeZombies++;
			}

			float modVol = (float) activeZombies / (float) zombieWatch.Length;
			volume = orgVolume * modVol;
			repeatDelayExtraTime = orgRepeatDelay + (zombieWatch.Length - activeZombies) * 0.3f;
		}

		if (randomSfx != SfxRandomType.None) {
			if (repeatIndex != -1 && repeatSameSound)
				playTime = SoundManager.instance.PlayRandomFromType (randomSfx, repeatIndex, startDelay, volume, pitch, randomPitch);
			else {
				playTime = SoundManager.instance.PlayRandomFromType (randomSfx, -1, startDelay, volume, pitch, randomPitch);
				repeatIndex = SoundManager.instance.lastRandomIndex;
			}
		}
		else if (singleSfx != null && singleSfx.Length > 0) {
			lastPlayedIndex = Random.Range (0, singleSfx.Length);
			if (repeatIndex != -1 && repeatSameSound)
				lastPlayedIndex = repeatIndex;
			else
				repeatIndex = lastPlayedIndex;
			playTime = SoundManager.instance.PlaySingleSfx (singleSfx [lastPlayedIndex], randomPitch, false, startDelay, volume, pitch);
		}

		if (repeatCounter != 0 && playTime > 0) {
			repeatCounter--;
			Invoke ("PlaySoundLoop", playTime + repeatDelayExtraTime + Random.Range(repeatDelayExtraRandomTime.x, repeatDelayExtraRandomTime.y));
		} else
			repeatIndex = -1;
	}

	public void StopPlay(bool stopForced = true) {
		repeatCounter = 0;
		repeatIndex = -1;
		CancelInvoke ("PlaySoundLoop");
		if (stopForced) {
			if (randomSfx == SfxRandomType.None) {
				if (lastPlayedIndex >= 0) {
					if (fadeStop)
						SoundManager.instance.FadeSingleSfx (singleSfx [lastPlayedIndex]);
					else
						SoundManager.instance.StopSingleSfx (singleSfx [lastPlayedIndex]);
				}
			} else {
				if (fadeStop)
					SoundManager.instance.FadeRandomPlayingSfx (randomSfx);
				else
					SoundManager.instance.StopPlayingRandomSfx (randomSfx);
			}
		}
		lastPlayedIndex = -1;
	}

	public void SetSingleSfx(SingleSfx sfx) {
		singleSfx = new SingleSfx[] { sfx };
	}

	void LateUpdate()
	{
		if (stillHitting == false && lastHitObject != null && stopOnHitObjectStopTouch) {
			frameCount++;
			if (frameCount > 20) {
				lastHitObject = null;
				StopPlay ();
				frameCount = 0;
			}
		} else
			frameCount = 0;
		stillHitting = false;
	}

}
