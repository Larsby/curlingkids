using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {

	public enum AutoFillGoal1RequirementsGoalType { None, ToonDollHelpers, Zombies };
	public enum EnableDisableType { OnFinished, Immediate, Progressive };

	public bool setColor = true;
	public Color hitColor = Color.green;
	public GameObject altColorizeObject = null;
	public int RequiredHits = 1;
	public bool isActive = true;
	public GameObject altActivationObject = null;
	public bool isEnabled = true;
	public GameObject [] activateObjects;
	public AutoFillGoal1RequirementsGoalType autoFillGoal1ActivationRequirementsGoalType = AutoFillGoal1RequirementsGoalType.None;
	public Goal[] object1ActivationRequiredGoals;
	public bool useRequirementForAllActivateObjects = false;
	public EnableDisableType enableType = EnableDisableType.OnFinished;
	public string activateTransformString = string.Empty;
	public SingleSfx objectEnableSound = SingleSfx.None;
	public GameObject [] disableObjects;
	public AutoFillGoal1RequirementsGoalType autoFillGoal1DisableRequirementsGoalType = AutoFillGoal1RequirementsGoalType.None;
	public Goal[] object1DisableRequiredGoals;
	public bool useRequirementForAllDisableObjects = false;
	public SingleSfx objectDisableSound = SingleSfx.None;
	public bool disableSelf = false;
	public float disableTime = 0;
	public EnableDisableType disableType = EnableDisableType.OnFinished;
	public string disableTransformString = string.Empty;
	private Color oldColor;
	private int nofHits = 0;
	private bool wasUsed = false;
	public int requiredHitAtTossNumber = 0;
	public bool resetOnMissedGoal = false;
	private int goalIndex = 0;
	public bool allowInactivePlayerHits = true;
	public bool includeInGoalCount = true;
	public float resetTime = 2;
	public bool oneGoalhitPerPlayer = true;
	public bool allowOnlyStillObjects = false;
	public bool allowOnlyPlayerHelpers = false;
	public bool allowNoPlayerHelpers = false;
	private List<GameObject> hasHitObjects = new List<GameObject>();
	public GameObject[] excludedAffectors;
	public float mimimumHitterVelocity = 0;
	public float nextPlayerTime = -1;
	public bool allowLowerLegsAndArms = true;
	public SingleSfx hitSound = SingleSfx.GoalHit;
	public SfxRandomType hitRandomSound = SfxRandomType.None;
	public SingleSfx altGoalFinishSound = SingleSfx.None;
	public bool randomPitch = false;
	public float volume = 1;
	public float pitch = -1;
	private SoundEmitter soundEmitter = null;
	public bool hitIsDeath = false;
	public bool fxWhenHit = false;
	private MeshRenderer mr;
	private bool wasHitThisFrame = false;

	public GameObject comboTextPrefab = null;
	public GameObject powerIndicatorPrefab = null;
	public enum PowerIndicatorType { None, Temporary, Persistant, Forever, PersistantFromStart, ForeverFromStart };
	public PowerIndicatorType powerIndicatorType = Goal.PowerIndicatorType.Temporary;
	public Vector3 powerIndicatorPos = new Vector3(0, 2f, -1.5f);
	public Vector3 powerIndicatorScaleMul = new Vector3(1f, 1f, 1f);
	PowerLevelHandler plh = null;

	public bool unlinkFromParentOnGoalDone = false;

	void Start () {
		if (altActivationObject == null)
			gameObject.SetActive (isActive);
		else
			altActivationObject.SetActive (isActive);
		
		soundEmitter = GetComponent<SoundEmitter> ();

		mr = this.GetComponent<MeshRenderer> ();
		if (altColorizeObject != null)
			mr = altColorizeObject.GetComponent<MeshRenderer>();
		
		if (mr != null)
			oldColor = mr.material.color;

		if (allowOnlyStillObjects)
			oneGoalhitPerPlayer = true;

		if (comboTextPrefab == null) {
			Game game = GameObject.FindObjectOfType<Game> ();
			if (game != null)
				comboTextPrefab = game.comboTextPrefab;
		}

		if (powerIndicatorPrefab == null)
		{
			Game game = GameObject.FindObjectOfType<Game>();
			if (game != null)
				powerIndicatorPrefab = game.powerIndicatorPrefab;
		}

		if ((object1ActivationRequiredGoals == null || object1ActivationRequiredGoals.Length < 1) && autoFillGoal1ActivationRequirementsGoalType != AutoFillGoal1RequirementsGoalType.None) {
			object1ActivationRequiredGoals = GameObject.FindObjectsOfType<Goal> ();
			if (object1ActivationRequiredGoals != null) {
				List<Goal> restrictedGoals = new List<Goal> ();
				foreach (Goal g in object1ActivationRequiredGoals) {
					ToonDollHelper tdh = g.gameObject.GetComponent<ToonDollHelper> ();
					if (tdh != null) {
						if (autoFillGoal1ActivationRequirementsGoalType == AutoFillGoal1RequirementsGoalType.ToonDollHelpers || (autoFillGoal1ActivationRequirementsGoalType == AutoFillGoal1RequirementsGoalType.Zombies && tdh.zombieHitMode == true))
							restrictedGoals.Add (g);
					}
				}
				object1ActivationRequiredGoals = null;
				if (restrictedGoals.Count > 0)
					object1ActivationRequiredGoals = restrictedGoals.ToArray ();
			}
		}

		if ((object1DisableRequiredGoals == null || object1DisableRequiredGoals.Length < 1) && autoFillGoal1DisableRequirementsGoalType != AutoFillGoal1RequirementsGoalType.None) {
			object1DisableRequiredGoals = GameObject.FindObjectsOfType<Goal> ();
			if (object1DisableRequiredGoals != null) {
				List<Goal> restrictedGoals = new List<Goal> ();
				foreach (Goal g in object1DisableRequiredGoals) {
					ToonDollHelper tdh = g.gameObject.GetComponent<ToonDollHelper> ();
					if (tdh != null) {
						if (autoFillGoal1DisableRequirementsGoalType == AutoFillGoal1RequirementsGoalType.ToonDollHelpers || (autoFillGoal1DisableRequirementsGoalType == AutoFillGoal1RequirementsGoalType.Zombies && tdh.zombieHitMode == true))
							restrictedGoals.Add (g);
					}
				}
				object1DisableRequiredGoals = null;
				if (restrictedGoals.Count > 0)
					object1DisableRequiredGoals = restrictedGoals.ToArray ();
			}
		}

		if (powerIndicatorPrefab != null && (powerIndicatorType == PowerIndicatorType.ForeverFromStart || powerIndicatorType == PowerIndicatorType.PersistantFromStart) && RequiredHits > 1) {
			ShowPowerLevelIndicator();
		}

	}

	void Update () {}

	private void restoreColor() {
		if (mr != null && setColor)
			mr.material.color = oldColor;
	}

	public void ResetGoal() {
		wasUsed = false;

		if (mr != null && setColor)
			mr.material.color = oldColor;
		nofHits = 0;

		foreach (GameObject g in activateObjects)
			g.SetActive (false);

		foreach (GameObject g in disableObjects)
			g.SetActive (true);
		
		if (altActivationObject == null)
			gameObject.SetActive (isActive);
		else
			altActivationObject.SetActive (isActive);
	}

	private void resetGoalAfterHit() {
		wasUsed = false;

		if (fxWhenHit) {
			if (transform.childCount == 1)
				transform.GetChild (0).gameObject.SetActive (false);
		}
	}

	public bool HasFailedRequiredToss(int tossNumber) {
		if (requiredHitAtTossNumber <= 0)
			return false;
	
		if (nofHits >= RequiredHits)
			return false;

		return (tossNumber >= requiredHitAtTossNumber); 
	}

	public int GetRequiredHitAtTossNumber() {
		return requiredHitAtTossNumber;
	}

	private void ActivateObject(GameObject go) {
		if(go == null) {
			Debug.Log("Could not activate object since its' null" + gameObject.name);
			return;
		}
		
		go.SetActive (true);

		if (objectEnableSound != SingleSfx.None)
			SoundManager.instance.PlaySingleSfx (objectEnableSound);

		Goal g = go.GetComponent<Goal> ();
		if (g != null) {
			g.SetEnabled (true);
		}

		if (activateTransformString != string.Empty)
			go.SendMessage ("OnBrokenContainer", activateTransformString, SendMessageOptions.DontRequireReceiver);
		
	}

	private IEnumerator DelayedDisable(float delayTime, GameObject go) {
		yield return new WaitForSeconds(delayTime);
		go.SetActive (false);
		if (objectDisableSound != SingleSfx.None)
			SoundManager.instance.PlaySingleSfx (objectDisableSound);
	}

	private void DisableObject(GameObject go) {
		if (disableTime == 0) {
			go.SetActive (false);
			if (objectDisableSound != SingleSfx.None)
				SoundManager.instance.PlaySingleSfx (objectDisableSound);
		} else if (disableTime > 0) {
			StartCoroutine(DelayedDisable(disableTime, go));
		}

		if (disableTransformString != string.Empty)
			go.SendMessage ("OnBrokenContainer", disableTransformString, SendMessageOptions.DontRequireReceiver);
	}

	public void SetEnabled(bool enabled) {
		isEnabled = enabled;
	}

	private bool exception = false;

	void OnTriggerEnter(Collider other) {

		if (allowOnlyStillObjects && exception == false)
			return;
		exception = false;

		bool firstHit = true;
		ToonDollHelper doll = null;
		GameObject findMe = GameUtil.FindParentWithTag (other.gameObject, "Player");
		if (findMe != null) doll = findMe.GetComponent<ToonDollHelper> ();

		if (excludedAffectors != null && excludedAffectors.Length > 0) {
			foreach (GameObject g in excludedAffectors) {
				if (g == findMe)
					return;
			}
		}

		if (!allowLowerLegsAndArms) {
			if (other.gameObject.name.Contains("Fore") || other.gameObject.name == "RightLeg" || other.gameObject.name == "LeftLeg")
				return;
		}

		if (mimimumHitterVelocity > 0) {
			if (other.attachedRigidbody != null && other.attachedRigidbody.velocity.magnitude < mimimumHitterVelocity)
				return;
		}

		if (oneGoalhitPerPlayer && doll != null) {
			foreach (GameObject g in hasHitObjects) {
				if (g == doll.gameObject)
					firstHit = false;
			}
		}

		bool isOkHitter = true;
		if (doll != null) {
			if (allowOnlyPlayerHelpers) {
				isOkHitter = doll.IsPlayerHelper ();
			} else if (allowNoPlayerHelpers) {
				isOkHitter = doll.IsRagDoll () && !doll.IsPlayerHelper ();
			} else {
				isOkHitter = doll.IsRagDoll () || doll.IsPlayerHelper ();
			}
		} else
			isOkHitter = false;

		if (firstHit && isEnabled && findMe != null && nofHits < RequiredHits && wasUsed == false && isOkHitter && (allowInactivePlayerHits || doll.IsActive())) {

			wasHitThisFrame = true;

			if (setColor)
				mr.material.color = hitColor;

			if (oneGoalhitPerPlayer)
				hasHitObjects.Add(doll.gameObject);

			nofHits++;

			if (nofHits == RequiredHits && altGoalFinishSound != SingleSfx.None) {
				SoundManager.instance.PlaySingleSfx (altGoalFinishSound, randomPitch, false, 0, volume, pitch);
			} else {
				if (soundEmitter != null && soundEmitter.emitterType == SoundEmitter.EmitterType.RemoteControlled)
					soundEmitter.PlaySound ();
				else {
					if (hitRandomSound != SfxRandomType.None) {
						SoundManager.instance.PlayRandomFromType (hitRandomSound, -1, 0, volume, pitch, randomPitch);
					} else {
						SoundManager.instance.PlaySingleSfx (hitSound, randomPitch, false, 0, volume, pitch);
					}
				}
			}

			if (nofHits < RequiredHits)
				Invoke ("restoreColor", 0.3f);
			else {
				if (hitIsDeath) {
					Game game = GameObject.FindObjectOfType<Game> ();
					if (game != null)
						game.ForceInstantLevelDeath ();
				}

				if (unlinkFromParentOnGoalDone) {
					transform.SetParent(null);
				}
			}
			
			doll.HitGoal (goalIndex);

			if (activateObjects != null && (nofHits >= RequiredHits || enableType != EnableDisableType.OnFinished)) {
				if (activateObjects.Length > 0) {

					bool requirementsMet = true;

					if (object1ActivationRequiredGoals.Length > 0) {
						foreach (Goal g in object1ActivationRequiredGoals) {
							if (g != null && g.GetNofTimesHit () < g.GetNofRequiredHits ())
								requirementsMet = false;
						}
					}

					for (int index = 0; index < activateObjects.Length; index++)
					{
						bool activate = false;
						if (index == 0 && requirementsMet)
							activate = true;
						else if (index > 0 && (useRequirementForAllActivateObjects == false || requirementsMet))
							activate = true;
						if (enableType == EnableDisableType.Progressive && nofHits != (index + 1))
							activate = false;

						if (activate)
							ActivateObject(activateObjects[index]);
					}
				}
			}

			if (disableObjects != null && (nofHits >= RequiredHits || disableType != EnableDisableType.OnFinished)) {
				if (disableObjects.Length > 0) {

					bool requirementsMet = true;

					if (object1DisableRequiredGoals.Length > 0) {
						foreach (Goal g in object1DisableRequiredGoals) {
							if (g != null && g.GetNofTimesHit() < g.GetNofRequiredHits())
								requirementsMet = false;
						}
					}

					for (int index = 0; index < disableObjects.Length; index++)
					{
						bool disable = false;
						if (index == 0 && requirementsMet)
							disable = true;
						else if (index > 0 && (useRequirementForAllDisableObjects == false || requirementsMet))
							disable = true;
						if (disableType == EnableDisableType.Progressive && nofHits != (index + 1))
							disable = false;

						if (disable)
							DisableObject(disableObjects[index]);
					}
				}
			}

			if (disableSelf && nofHits >= RequiredHits || disableType != EnableDisableType.OnFinished) {
				DisableObject (gameObject);
			}

			if (comboTextPrefab != null && !hitIsDeath) {
				StaticManager.comboTossGoalsHit++;
				if (StaticManager.comboTossGoalsHit >= StaticManager.showComboHitTextTreshold) {
					float yPlus = 1.2f, textSize = 0.035f;
					int borderSize = 8;

					GameObject comboText = Instantiate (comboTextPrefab);
					comboText.transform.position = new Vector3 (transform.position.x, transform.position.y + yPlus, transform.position.z + 0.02f);
					comboText.transform.localScale = new Vector3 (textSize, textSize, textSize);
					TextMesh tm = comboText.GetComponent<TextMesh> ();
					if (tm != null) {
						tm.text = "x" + StaticManager.comboTossGoalsHit;
						tm.color = Color.black;
						tm.fontSize += borderSize;
					}
					
					comboText = Instantiate (comboTextPrefab);
					comboText.transform.position = new Vector3 (transform.position.x, transform.position.y + yPlus, transform.position.z);
					comboText.transform.localScale = new Vector3 (textSize, textSize, textSize);
					tm = comboText.GetComponent<TextMesh> ();
					if(tm != null)
						tm.text = "x" + StaticManager.comboTossGoalsHit;
				}
			}

			if (powerIndicatorPrefab != null && powerIndicatorType != PowerIndicatorType.None && nofHits <= RequiredHits && RequiredHits > 1) {
				ShowPowerLevelIndicator();
			}

			wasUsed = true;

			if (fxWhenHit) {
				if (transform.childCount == 1)
					transform.GetChild (0).gameObject.SetActive (true);
			}

			Invoke ("resetGoalAfterHit", resetTime);

			if (nextPlayerTime > 0) {
				currTdh = doll;
				Invoke ("forceNextPlayer", nextPlayerTime);
			}
		}
	}

	private void ShowPowerLevelIndicator() {
		float relativeSize = (float)nofHits / (float)RequiredHits;

		GameObject powInd;

		if (plh == null)
		{
			powInd = Instantiate(powerIndicatorPrefab);
			plh = powInd.GetComponent<PowerLevelHandler>();
			powInd.transform.localScale = new Vector3(powInd.transform.localScale.x * transform.localScale.x * powerIndicatorScaleMul.x, powInd.transform.localScale.y * transform.localScale.z * powerIndicatorScaleMul.z, powInd.transform.localScale.z * transform.localScale.y * powerIndicatorScaleMul.y);
		}
		else
			powInd = plh.gameObject;

		plh.SetPos(transform, powerIndicatorPos);

		//LeanTween.scaleX(powInd.transform.GetChild(0).gameObject, relativeSize, 0.5f); // not very visible so why bother
		powInd.transform.GetChild(0).localScale = new Vector3(relativeSize, 1, 1);

		if (plh != null && powerIndicatorType == PowerIndicatorType.Temporary)
		{
			plh.Kill(2f);
			plh = null;
		}
		else if (plh != null && (powerIndicatorType == PowerIndicatorType.Persistant || powerIndicatorType == PowerIndicatorType.PersistantFromStart) && nofHits == RequiredHits)
		{
			plh.Kill(2f);
			plh = null;
		}
	}


	void OnCollisionEnter(Collision collision) {
		OnTriggerEnter (collision.collider);
	}

	void OnTriggerStay(Collider other) {
		if (allowOnlyStillObjects == false)
			return;

		ToonDollHelper doll = null;
		GameObject findMe = GameUtil.FindParentWithTag (other.gameObject, "Player");
		if (findMe != null) doll = findMe.GetComponent<ToonDollHelper> ();
		if (doll == null)
			return;

		foreach (GameObject g in hasHitObjects) {
			if (g == doll.gameObject)
				return;
		}

		if (doll.IsMoving ())
			return;

		exception = true;
		OnTriggerEnter (other);
	}

	void OnCollisionStay(Collision collision) {
		OnTriggerStay (collision.collider);
	}


	public int GetNofTimesHit() {
		return nofHits;
	}

	public int GetNofRequiredHits() {
		return RequiredHits;
	}

	public bool IsIncludeInGoalCount() {
		return includeInGoalCount;
	}

	private ToonDollHelper currTdh = null;
	private void forceNextPlayer() {
		if (currTdh != null) {
			currTdh.SetOutOfBounds (false);
		}
	}

	void LateUpdate() {
		wasHitThisFrame = false;
	}
	
	public bool WasHitThisFrame() {
		return wasHitThisFrame;
	}
}
