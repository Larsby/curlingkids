using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;

public class GameMultiTest : NetworkBehaviour {

	public enum LevelScoreType { NofDollsUsed, NofGoalsHit, NofTosses };

	public enum FallingPanicType { None, Normal, Restricted };

	public enum DropPanicType { None, Normal, GoingUp, GoingUpOrDown };

	public enum InitSwoopType { Spinning, Zooming };

	public enum LevelResultType { Pending, Won,  };

	public LevelScoreType levelScoreType = LevelScoreType.NofDollsUsed;

    /*[SerializeField]
    Behaviour Listener;*/
    public GameObject Player;

    public GameObject [] dolls;
	public bool useSingleDoll = false;
	public GameObject singleDoll;
	private int dollCounter = 0;
	public Canvas gameUICanvasPrefab;
	public Event EventSystemPrefab;
	public ScreenFader screenFaderPrefab = null;
	private ScreenFader screenFader = null;
	public Deathbox deathBoxPrefab;
	public GameObject bubblePrefab;
	public int groundPosY = -100, heavenPosY = 100;
	private GameObject nextDollButton;
	public PhysicMaterial playerPhysMaterial;
	public float fixedTimeStep = 0.02f;
	public Vector3 tossForce = new Vector3(1, 0.1f, 1);
	public float tossXForceActualOverride = 1;
	public GameObject speechBubblePrefab;
	public GameObject comboTextPrefab;
	public GameObject trailPrefab;
	public GameObject powerIndicatorPrefab;

	public int maxDolls = 0;
	private int maxDollCounter = 0;
	private int tossCounter = 0;

	public bool progressiveLevel = false;
	public bool standUpPlayer = false;
	public bool hitTurnsStandingIntoRagdoll = false;
	public bool getUpAfterReenabledRagdoll = false;
	public bool removeRigidsOnFinished = false;

	private List<ToonDollHelper> ragdolls = new List<ToonDollHelper>();
	private ToonDollHelper currentDoll;

	private bool init = true;
	private float tossCamTimer = 0;

	private float nextTossZoomTime = 0.7f;

	private float nextDollZoomTime = 0f;

	//private Camera swoopCam;

	private Text goalsHitText;
	private Text dollsTossedText;
	private Text finishText;
	private Text failText;
	private Text creditsText;
	private Text timerText;
	private Text currentTossesText;
	private int requiredGoalHits = -1;
	private EventSystem eventSystem = null;

	private bool levelFinished = false;

	private Goal[] allGoals = null;

	public ToonDollHelper.GravityMode gravityMode = ToonDollHelper.GravityMode.GRAVITY_ON;

	private StaticManager.LevelData levelResults = new StaticManager.LevelData();

	public int star1req = -1;
	public int star3req = -1;

	private Vector3 cameraOrgPos;
	private Quaternion cameraOrgRotation;

	private List<Vector3> startPositions = new List<Vector3>();

	public bool moveCamera = true;
	public float camFollowDelay = 0;
	public Vector3 cameraDistanceAfterToss = new Vector3 (0, 1.7f, -5f);
	public Vector3 cameraRotationAfterToss = new Vector3 (30, 0, 0);

	public Vector3 cameraDistanceBeforeNewToss = new Vector3 (0, 4.7f, -3.2f);
	public Vector3 cameraRotationBeforeNewToss = new Vector3 (33.56f, 0, 0);


	public float stillStandingTime = 0;

	public Vector3 worldGravity = new Vector3 (0, -9.81f, 0);

	public float shadowDistance = 25;
	public ShadowProjection shadowProjection = ShadowProjection.StableFit;
	public float shadowDistanceAfterToss = -1;

	private List<PushButton> pushButtons = new List<PushButton> ();

	public bool allwToonToOverride_DragAngularMass = true;
	public Vector3 DragAngulardragMass = new Vector3 (0.14f, 0.1f, 2.5f);
	public CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.Continuous;

	public bool noToss = false;
	public bool flipperPhysics = false;

	private bool showCheatButtons = false;

	private SoundManager soundManager = null;

	public int forcedRequiredGoalHits = 0;

	private ToonDollHelper[] playerHelpers = null;

	private int temporaryCredits = 0;

	private SwipeReader swipeReader;
	public float pinModZ = 0.35f;
	public float progressivePinModZ = 0.35f;

	public bool showTossIndicators = true;
	private GameObject indicatorContainer = null;
	private RectTransform forceIndicator, angleIndicator;
	private RectTransform forceOldIndicator, angleOldIndicator;

	private bool extrasApplied = false;

	//private bool useGhost = true;

	public float initialLoweredTimeStepTime = 0;
	public float initialLoweredTimeStepValue = 0.004f;
	private int timeStepAffectIndex;
	private float gameTime;
	public bool airtimeSteering = false;
	public bool airtimeAcceleration = false;
	public bool airtimeBreak = false;
	public bool airtimeAccChangeDir = false;
	public bool airtimeBrkChangeDir = false;

	public float timer = -1;
	public bool timerAwaitToss = true;

	public float nextDollButton_noCollideTime = 3f;
	public float nextDollButton_lowSpeedMagnitude = 10f;
	public float nextDollButton_lowSpeedTime = 2f;
	public bool showNextButtonBeforeProgressiveTosses = false;

	public bool startSwoop = false;
	public InitSwoopType initSwoop = InitSwoopType.Zooming;

	private float startSwoopTime = 1.5f, startSwoopTimer = 0;
	private float startSwoopHeight = 115, startSwoopCircleRadius = 0, startSwoopRotationSpeed = 0, startSwoopLastRotModiferTime = 1.5f, startSwoopXRotation = 65;

	public bool keepHeadUp = true;
	public FallingPanicType fallingPanic = FallingPanicType.None;
	public Vector3 fallingPanic_SpeedTreshold_beginLength_panicLength = new Vector3 (9f, 0.25f, 2f);
	public float fallingPanicProbability = 1;
	public DropPanicType dropPanic = DropPanicType.None;
	public Vector3 dropPanic_SpeedTreshold_beginLength_panicLength = new Vector3 (2f, 1.5f, 4f);
	public float dropPanicProbability = 1;

	public float customTimeScale = 1.3f;

	public bool noNextButton = false;

	public bool autoBubble = false;
	public float bubbleButtonBubbleDelay = 0;

	private int maxTosses = -1;

	public GameObject[] dollProgressionActivation;
	public GameObject[] awaitStopObjects;

	public bool resetPushButtonsOnNewDoll = true;

	public bool playHitMoans = true;
	public float hitMoanInitialWait = 0;

	public bool playSlideSound = false;
	private SoundEmitter slideSoundEmitter = null;
	private bool repeatSlideSound = false;

	public SingleSfx beginSound = SingleSfx.None;
	private SoundEmitter soundEmitter = null;
	public SingleSfx noMoreDollsSound = SingleSfx.LevelFail;

	private float hmmDelay = -1;

	public bool celebrateVictory = true;
	public float celebrationStartTime = 0.15f;
	public float celebrationRiseSpeed = 1f;

	public int showComboHitTextTreshold = 2;

	public bool showTrails = true;

	public float initialPlayerDeltaZ = 0;

	public FollowObject dollFollower = null;


	void OnDestroy() {
		StaticManager.StoreTemporaryCredits ();
		StaticManager.SaveMoneyMakers ();
		StaticManager.Save();
		StaticTaskManager.Save();
		Time.timeScale = 1;
	}


	private GameObject ActivateAndListenToButton(GameObject root, string buttonName, UnityEngine.Events.UnityAction call, bool active = true) {
		GameObject find = null;
		Button b = null;

		Transform t = GameUtil.FindDeepChild(root.transform, buttonName);
		if (t != null) {
			find = t.gameObject;
			find.SetActive (active);
			b = find.GetComponent<Button> ();
			if (b != null)
				b.onClick.AddListener (call);
		}
		return find;
	}

	private Text FindUIText(GameObject root, string name) {
		Text text = null;
		Transform t = GameUtil.FindDeepChild(root.transform, name);

		if (t != null) {
			text = t.gameObject.GetComponent<Text> ();
		}
		return text;
	}

	void Awake () {

		levelResults.stars = -1;

		if (initSwoop == InitSwoopType.Spinning) {
			startSwoopTime = 3; startSwoopTimer = 0;
			startSwoopHeight = 50; startSwoopCircleRadius = 30; startSwoopRotationSpeed = 3.2f; startSwoopLastRotModiferTime = 1f;
		}

		SoundManager.Create ();

		if (CharacterManager.instance != null) {
			useSingleDoll = true;
			singleDoll = CharacterManager.instance.GetAvatar (StaticManager.GetSelectedAvatarIndex());
		}

		if (screenFaderPrefab != null) {
			screenFader = Instantiate (screenFaderPrefab);
		}

		QualitySettings.shadowProjection = shadowProjection;
		QualitySettings.shadowDistance = shadowDistance;

		if (gameUICanvasPrefab) {
			GameObject go;
			Button b;
			Canvas canvas = Instantiate (gameUICanvasPrefab);

			nextDollButton = ActivateAndListenToButton (canvas.gameObject, "NextDollButton", NextButton, false);

			goalsHitText = FindUIText(canvas.gameObject, "GoalsText");
			dollsTossedText = FindUIText(canvas.gameObject, "DollsText");
			finishText = FindUIText(canvas.gameObject, "FinishText");
			failText = FindUIText(canvas.gameObject, "FailText");
			creditsText = FindUIText(canvas.gameObject, "CreditsText");
			timerText = FindUIText(canvas.gameObject, "TimerText");
			currentTossesText = FindUIText(canvas.gameObject, "TossText");

			StaticManager.ResetTemporaryCredits ();

			if (creditsText != null) {
				creditsText.gameObject.SetActive (true);
				creditsText.text = "" + StaticManager.GetNumberOfCredits ();
			}

			ActivateAndListenToButton (canvas.gameObject, "RestartButton", RestartLevel);
			ActivateAndListenToButton (canvas.gameObject, "LevelsButton", GotoLevelSelect);

//			ActivateAndListenToButton (canvas.gameObject, "TossTypeButton", ChangeTossTypeButton);
//			ActivateAndListenToButton (canvas.gameObject, "TossTypeButton2", SwitchMaxSwipeTimeButton);
//			ActivateAndListenToButton (canvas.gameObject, "TossTypeButton3", SwitchFixedSwipePosButton);
//			ActivateAndListenToButton (canvas.gameObject, "HeadUpButton", SwitchHeadUpButton);

			if (showCheatButtons) {
				ActivateAndListenToButton (canvas.gameObject, "Lose", CheatLose);
				ActivateAndListenToButton (canvas.gameObject, "Win1", CheatWin1);
				ActivateAndListenToButton (canvas.gameObject, "Win2", CheatWin2);
				ActivateAndListenToButton (canvas.gameObject, "Win3", CheatWin3);
			}

			go = canvas.gameObject.transform.Find ("TossButton").gameObject;
			go.SetActive (true);

			swipeReader = go.GetComponent<SwipeReader> ();
			swipeReader.enabled = true;
			swipeReader.pinModZ = pinModZ;

			if (showTossIndicators) {
				indicatorContainer = canvas.gameObject.transform.Find ("Indicators").gameObject;
				if (indicatorContainer != null) {
					indicatorContainer.SetActive (true);
					forceIndicator = GameUtil.FindDeepChild (indicatorContainer.transform, "ForceIndicator").gameObject.GetComponent<RectTransform> ();
					angleIndicator = GameUtil.FindDeepChild (indicatorContainer.transform, "AngleIndicator").gameObject.GetComponent<RectTransform> ();
					forceOldIndicator = GameUtil.FindDeepChild (indicatorContainer.transform, "ForceOld").gameObject.GetComponent<RectTransform> ();
					angleOldIndicator = GameUtil.FindDeepChild (indicatorContainer.transform, "AngleOld").gameObject.GetComponent<RectTransform> ();
				}
			}

			Deathbox ground = Instantiate (deathBoxPrefab);
			ground.gameObject.transform.position = GameUtil.SetY (ground.gameObject.transform.position, groundPosY);

			Deathbox heaven = Instantiate (deathBoxPrefab);
			heaven.gameObject.transform.position = GameUtil.SetY (heaven.gameObject.transform.position, heavenPosY);

			eventSystem = FindObjectOfType<EventSystem> ();

			Object [] all = FindObjectsOfType<Object> ();
			foreach (Object g in all) {
				PushButton pb = g as PushButton;
				if (pb != null)
					pushButtons.Add (pb);
			}
			foreach (PushButton pb in pushButtons) {
				go = null;
				if (pb.GetPushButtonIndex() >= 0)
					go = canvas.gameObject.transform.Find ("PushButton" + pb.GetPushButtonIndex()).gameObject;
				if (go != null) {
					go.SetActive (true);
					b = go.GetComponent<Button> ();
					pb.AddButtonListener (b);
				}
			}
		}

		playerHelpers = Object.FindObjectsOfType<ToonDollHelper> ();
		if (playerHelpers != null && playerHelpers.Length != 0) {
			ragdolls.AddRange (playerHelpers);
		}


		Time.fixedDeltaTime = fixedTimeStep;
		StaticManager.originalLevelTimeStep = fixedTimeStep;

		StaticManager.originalCollisionDetectionMode = collisionDetectionMode;

		Application.targetFrameRate = 60;

		Physics.gravity = worldGravity;
		if (!flipperPhysics)
			Physics.gravity = GameUtil.AddY (Physics.gravity, -StaticManager.globalGravityAdd);

		//swoopCam = Camera.main;

		//cameraOrgPos = swoopCam.transform.position;
		//cameraOrgRotation = swoopCam.transform.rotation;

		foreach ( GameObject go in GameObject.FindGameObjectsWithTag("StartPos")) {
			startPositions.Add (go.transform.position);
		}

		if (dollProgressionActivation != null)
			foreach (GameObject g in dollProgressionActivation)
				g.SetActive (false);



		// this crap is to put a soundemitter on referees on the curling levels, because I stupidly did not make them prefabs from the beginning and it's very hard to turn them into prefabs due to various added components etc
		if (StaticManager.UGLYFIX_IsLevelType(StaticManager.UGLYFIX_curlingWorldIndex)) {
			SimpleTransform[] refTransf = GameObject.FindObjectsOfType<SimpleTransform> ();
			ToonDollHelper[] refTdh = GameObject.FindObjectsOfType<ToonDollHelper> ();
			List<GameObject> referees = new List<GameObject> ();
			if (refTransf != null) {
				foreach (SimpleTransform st in refTransf)
					if (st.gameObject.name.Length >= 6 && st.gameObject.name.Substring (0, 6) == "Sporty")
						referees.Add (st.gameObject);
			}
			if (refTdh != null) {
				foreach (ToonDollHelper tdh in refTdh)
					if (tdh.gameObject.name.Length >= 6 && tdh.gameObject.name.Substring (0, 6) == "Sporty")
						referees.Add (tdh.gameObject);
			}

			if (referees.Count > 0) {
				foreach (GameObject g in referees) {
					if (g.GetComponent<SoundEmitter> () == null) {
						SoundEmitter sg = g.AddComponent<SoundEmitter> ();
						sg.emitterType = SoundEmitter.EmitterType.OnCollideActivePlayer;
						sg.playerType = SoundEmitter.PlayerType.Player;
						sg.randomSfx = SfxRandomType.RefereeHit;
						sg.speechBubblePrefab = speechBubblePrefab;
						sg.speechBubbleDuration = 2f;
						sg.speechShowProbability = 0.66f;
						sg.speechBubbleToCollidingPlayer = true;
						sg.speechBubbleToThisObject = false;
						sg.minTriggerDelay = 2;

					}
					if(g.GetComponent<AwardTrigger>() == null) {
						AwardTrigger awardTrigger = g.AddComponent<AwardTrigger>();
						awardTrigger.taskType = StaticTaskManager.TaskType.Other;
						awardTrigger.description = "Referee";
						awardTrigger.destroyOnHit = false;
					}
				}
			}
		}
		// same as above for same reason, but for zombies for zombie levels
		if (StaticManager.UGLYFIX_IsLevelType(StaticManager.UGLYFIX_zombieWorldIndex)) {
			ToonDollHelper[] zomTdh = GameObject.FindObjectsOfType<ToonDollHelper> ();
			List<GameObject> zombies = new List<GameObject> ();
			if (zomTdh != null) {
				foreach (ToonDollHelper tdh in zomTdh)
					if (tdh.zombieHitMode)
						zombies.Add (tdh.gameObject);
			}

			if (zombies.Count > 0) {
				foreach (GameObject g in zombies) {
					Goal goal = g.GetComponent<Goal> ();
					if (goal != null && goal.hitSound == SingleSfx.GoalHit) {
						goal.hitSound = SingleSfx.None;
						goal.hitRandomSound = SfxRandomType.ZombieHit;
					}
					else if (g.GetComponent<SoundEmitter> () == null) {
						SoundEmitter sg = g.AddComponent<SoundEmitter> ();
						sg.emitterType = SoundEmitter.EmitterType.OnCollideActivePlayer;
						sg.playerType = SoundEmitter.PlayerType.Player;
						sg.randomSfx = SfxRandomType.ZombieHit;
					}
				}
			}
		}



        if (isLocalPlayer)
        {
            Player.GetComponent<AudioListener>().enabled = false;
        }

        NewDoll ();

		/*if (useGhost)
			MakeGhost ();*/

		UpdateGoalStatus ();

		maxTosses = CalculateMaxTosses ();
		UpdateTossNofText ();

		UpdateTimerText ();

		StaticManager.SetCurrentScene ();

		StaticManager.PrepareMoneyMakers ( FindObjectsOfType<MoneyMaker> () );

		if (playSlideSound) {
			slideSoundEmitter = gameObject.AddComponent<SoundEmitter> ();
			slideSoundEmitter.emitterType = SoundEmitter.EmitterType.RemoteControlled;
			slideSoundEmitter.randomSfx = SfxRandomType.Slide;
			if (repeatSlideSound)
				slideSoundEmitter.repeatSoundNof = -1; // repeat infinite
		}

		soundEmitter = gameObject.AddComponent<SoundEmitter> ();
		soundEmitter.emitterType = SoundEmitter.EmitterType.RemoteControlled;

		GameObject [] hideUs = GameObject.FindGameObjectsWithTag ("HideMe");
		if (hideUs != null && hideUs.Length > 0)
			foreach (GameObject g in hideUs)
				g.SetActive (false);

		StaticManager.showComboHitTextTreshold = showComboHitTextTreshold;
	}

	void Start() {
		soundManager = SoundManager.instance; // instance is safe to use from Start and onwards

		/*
		// RandomNonRepeating ran = new RandomNonRepeating (new int[] { -35, 66, 9000, 6000, 0, -88, -88, -88 }, true);
		// RandomNonRepeating ran = new RandomNonRepeating (0,0,false); ran.AddNumber(666, 6);
		RandomNonRepeating ran = new RandomNonRepeating (0, 10);
		for (int i = 0; i < 15; i++)
			print (ran.GetRandom());
		*/

		StaticManager.PlayMusicBasedOnWorld();

		if (beginSound != SingleSfx.None) {
			soundEmitter.singleSfx = new SingleSfx[] { beginSound };
			soundEmitter.PlaySound ();
		}
		gameTime = Time.time;
	}

	private void UpdateTimerText() {
		if (timer >= 0) {
			timerText.gameObject.SetActive (true);
			timerText.transform.parent.gameObject.SetActive (true);

			string minutes = Mathf.Floor(timer / 60).ToString("00");
			string seconds = Mathf.Floor(timer % 60).ToString("00");
			string millis = Mathf.Floor((timer * 10) % 10).ToString("0");

			timerText.text = minutes + ":" + seconds + ":" + millis;
		}
	}


	private void RestartLevel() {
		soundManager.PlaySingleSfx (SingleSfx.Button1, true);

		if (StaticManager.usePlayLevelScreen)
			SceneManager.LoadScene ("PlayLevel");
		else
			StaticManager.RestartSameLevel ();
	}

	private void GotoLevelSelect() {
		soundManager.PlaySingleSfx (SingleSfx.BackButton, true);

		SceneManager.LoadScene ("LevelSelect");
	}

	private int GetNofGoalsHit() {
		int nofHitGoals = 0;

		if (allGoals == null)
			allGoals = FindObjectsOfType<Goal> ();

		foreach (Goal g in allGoals) {
			if (g.IsIncludeInGoalCount ()) {
				nofHitGoals += g.GetNofTimesHit ();
			}
		}
		return nofHitGoals;
	}

	private int GetNofRequiredGoalsHit() {
		if (forcedRequiredGoalHits > 0)
			return forcedRequiredGoalHits;

		int nofRequiredGoals = 0;

		Goal[] goals = FindObjectsOfType<Goal> ();

		foreach (Goal g in goals) {
			if (g.IsIncludeInGoalCount())
				nofRequiredGoals += g.GetNofRequiredHits ();
		}
		return nofRequiredGoals;
	}
		
	private bool UpdateGoalStatus() {
		if (requiredGoalHits == -1)
			requiredGoalHits = GetNofRequiredGoalsHit();

		if (requiredGoalHits == 0)
			return false;

		int goalsHit = GetNofGoalsHit ();
		goalsHitText.text = goalsHit + " / " + requiredGoalHits;
		goalsHitText.gameObject.SetActive (true);
		goalsHitText.transform.parent.gameObject.SetActive (true);

		return goalsHit >= requiredGoalHits;
	}
		

	private int CalculateMaxTosses() {
		if (!progressiveLevel)
			return -1;

		int maxRequired = 0;

		if (allGoals != null) {
			foreach (Goal g in allGoals) {
				maxRequired = Mathf.Max (maxRequired, g.GetRequiredHitAtTossNumber ());
			}
		}
		return maxRequired;
	}

	private void UpdateTossNofText() {

		if (maxTosses < 0)
			return;

		currentTossesText.gameObject.SetActive (true);
		currentTossesText.transform.parent.gameObject.SetActive (true);

		if (maxTosses == 0)
			currentTossesText.text = "Inf";
		else
			currentTossesText.text = tossCounter + "/" + maxTosses;
	}


	void ChangeTossTypeButton() {
		StaticManager.newEndforceCalculation = !StaticManager.newEndforceCalculation;
	}

	void SwitchMaxSwipeTimeButton() {
		StaticManager.useMaxTime = !StaticManager.useMaxTime;
	}

	void SwitchFixedSwipePosButton() {
		StaticManager.fixedSwipeStartPoint = !StaticManager.fixedSwipeStartPoint;
	}


	void SwitchHeadUpButton() {
		keepHeadUp = !keepHeadUp;
		if (currentDoll != null)
			currentDoll.bGazing = keepHeadUp;
	}

	/*void MakeGhost () {
		GameObject ghost;

		ghost = Instantiate (currentDoll.gameObject);
		ghost.name = "GHOST";

		ToonDollHelper tdh = ghost.GetComponent<ToonDollHelper> ();
		tdh.SetKinematic ();
		Destroy (tdh);

		ghost.transform.position = new Vector3(400,0,5);

		Rigidbody rb = ghost.GetComponent<Rigidbody> ();
		Destroy (rb);

		SkinnedMeshRenderer smr = ghost.GetComponentInChildren<SkinnedMeshRenderer> ();
		smr.materials = new Material[0]; //smr.enabled = false; smr.updateWhenOffscreen = true;

		ToonDollHelper.RemoveRigidComponents (ghost, true, true);
	}*/


	/*private IEnumerator StartSwooper() {

		float rotCount = 0;

		swipeReader.enabled = false;

		//yield return new WaitForEndOfFrame();

		while (startSwoopTimer < startSwoopTime + 0.1f) {

			float progress = Mathf.Clamp01(1 - (startSwoopTimer / startSwoopTime));

			Camera.main.transform.position = cameraOrgPos + new Vector3(Mathf.Sin(rotCount) * progress * startSwoopCircleRadius, progress * startSwoopHeight, Mathf.Cos(rotCount) * progress * startSwoopCircleRadius);
			Vector3 rotPos = GameUtil.AimCamTowards (Camera.main, currentDoll.transform.position, false);

			if (initSwoop == InitSwoopType.Zooming)
				rotPos = new Vector3 (startSwoopXRotation, 0,0);

			float lastProgress = Mathf.Clamp01((startSwoopTimer - (startSwoopTime - startSwoopLastRotModiferTime)) / startSwoopLastRotModiferTime);

			if (initSwoop == InitSwoopType.Zooming)
				lastProgress = Easing.Cubic.In (lastProgress);
			else
				lastProgress = Easing.Exponential.In (lastProgress);

			Quaternion slerper = Quaternion.Slerp (Quaternion.Euler (rotPos), cameraOrgRotation, lastProgress);

			Camera.main.transform.rotation = slerper;

			rotCount += startSwoopRotationSpeed * Time.deltaTime;

			startSwoopTimer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		swipeReader.SetDollPos (currentDoll.transform.position);
		swipeReader.enabled = true;
		gameTime = Time.time;
		InitHmmDelay ();
	}*/


	void NewDoll (bool playNextDollSound = false) {
		GameObject rd;

		Time.timeScale = customTimeScale;

		if (levelFinished)
			return;

		tossCounter = 1;

		QualitySettings.shadowDistance = shadowDistance;

		maxDollCounter++;
		if (maxDolls > 0) {
			if (maxDollCounter > maxDolls) {
				if (levelScoreType == LevelScoreType.NofGoalsHit) {
					if (star1req < 0)
						star1req = 1;
					
					if (GetNofGoalsHit () >= star1req)
						ShowLevelCleared ();
					else
						ShowLevelFailed (true);
				} else
					ShowLevelFailed (true);
				if (currentDoll.IsGettingUp() && noMoreDollsSound == SingleSfx.None)
					SoundManager.instance.PlayRandomFromType (SfxRandomType.StandUp, -1, 0, -1, -1, true);
				return;
			}
			ShowDollsCount ();
		}

		if (playNextDollSound)
			soundManager.PlayRandomFromType (currentDoll.soundNextPlayer, -1, 0, -1, currentDoll.soundPitch);
		
		init = !noToss;

		if (maxDollCounter == 0 || resetPushButtonsOnNewDoll)
			foreach (PushButton pb in pushButtons) {
				pb.ResetState ();
			}

		if ((progressiveLevel || standUpPlayer) && currentDoll != null && !currentDoll.IsAwaitingBubble())
			currentDoll.Reset ();

		/*if (startSwoop == false || maxDollCounter > 1) {
			if (nextDollZoomTime <= 0) {
				Camera.main.transform.position = cameraOrgPos;
				Camera.main.transform.rotation = cameraOrgRotation;
			} else {
				LeanTween.move (Camera.main.gameObject, cameraOrgPos, nextDollZoomTime).setEaseInOutSine ();
				LeanTween.rotate (Camera.main.gameObject, cameraOrgRotation.eulerAngles, nextDollZoomTime).setEaseInOutSine ();
			}
		}*/
        
		if (useSingleDoll)
			rd = Instantiate (singleDoll);
		else
			rd = Instantiate (dolls[dollCounter]);
		dollCounter++;
		if (dollCounter >= dolls.Length)
			dollCounter = 0;
            
		ToonDollHelper rdh = rd.GetComponent<ToonDollHelper> ();
		currentDoll = rdh;
		ragdolls.Add (rdh);
		rdh.SetMaterial(playerPhysMaterial);

		/*if (startSwoop && maxDollCounter == 1)
			StartCoroutine (StartSwooper());*/

		Vector3 dam = GameUtil.CloneVector3(DragAngulardragMass);
		if (allwToonToOverride_DragAngularMass) {
			if (currentDoll.extra_overrideDrag >= 0)
				dam.x = currentDoll.extra_overrideDrag;
			if (currentDoll.extra_overrideAngularDrag >= 0)
				dam.y = currentDoll.extra_overrideAngularDrag;
			if (currentDoll.extra_overrideMass >= 0)
				dam.z = currentDoll.extra_overrideMass;
		}
		rdh.SetRigidValues (dam, collisionDetectionMode, removeRigidsOnFinished);
		rdh.SetFlipperPhysics (flipperPhysics);

		currentDoll.SetStandup (standUpPlayer, progressiveLevel, hitTurnsStandingIntoRagdoll, getUpAfterReenabledRagdoll);
		currentDoll.SetTossForce (tossForce, tossXForceActualOverride);
		currentDoll.SetStillStandTime(stillStandingTime);

		currentDoll.SetAirtime (airtimeSteering, airtimeAcceleration, airtimeBreak, airtimeAccChangeDir, airtimeBrkChangeDir);

		currentDoll.SetShowNextDollButtonLimits(nextDollButton_noCollideTime, nextDollButton_lowSpeedMagnitude, nextDollButton_lowSpeedTime);

		currentDoll.SetKinematic (); // for flipper stages, so player doesn't start sliding down

		currentDoll.transform.rotation = Quaternion.Euler (currentDoll.transform.rotation.eulerAngles.x, cameraOrgRotation.eulerAngles.y, currentDoll.transform.rotation.eulerAngles.z);

		currentDoll.bGazing = keepHeadUp;

		//currentDoll.fallingPanic = fallingPanic;
		currentDoll.flyingPanicSpeedTreshold = fallingPanic_SpeedTreshold_beginLength_panicLength.x;
		currentDoll.flyingBeginPanicTime = fallingPanic_SpeedTreshold_beginLength_panicLength.y;
		currentDoll.flyingPanicLength = fallingPanic_SpeedTreshold_beginLength_panicLength.z;
		currentDoll.fallingPanicProbability = fallingPanicProbability;
		//currentDoll.dropPanic = dropPanic;
		currentDoll.dropPanicSpeedTreshold = dropPanic_SpeedTreshold_beginLength_panicLength.x;
		currentDoll.dropBeginPanicTime = dropPanic_SpeedTreshold_beginLength_panicLength.y;
		currentDoll.dropPanicLength = dropPanic_SpeedTreshold_beginLength_panicLength.z;
		currentDoll.dropPanicProbability = dropPanicProbability;

		currentDoll.playHitMoans = playHitMoans;
		currentDoll.hitMoanInitialWait = hitMoanInitialWait;
			
		currentDoll.transform.position = new Vector3 (0, 0.04500008f, -5.17f);

		if (startPositions.Count > 0) {
			int posIndex = Random.Range (0, startPositions.Count);
			Vector3 startPos = startPositions[posIndex];

			currentDoll.transform.position = startPos;
		}

		if (showTrails)
			currentDoll.SetTrails (new HumanBodyBones[] { HumanBodyBones.LeftHand, HumanBodyBones.RightHand }, trailPrefab);

		swipeReader.Reset ();
		swipeReader.SetDollPos (currentDoll.transform.position, cameraOrgPos, cameraOrgRotation); // in case we are moving the camera back with a tween, ensure that camera is temporarily set back to org pos when setting swipe pin pos (by sending in the last two params)


		if (!extrasApplied) {
			if (maxDolls != 0) { 
				maxDolls += currentDoll.extra_extraDollsPerLevel;
				ShowDollsCount ();
			}
			extrasApplied = true;
		}

		Time.timeScale = customTimeScale;

		if (noToss == true) {
			swipeReader.enabled = false;
			Invoke ("Drop", 0.01f);
		}

		if (showTossIndicators)
			Indicate (0, 0, 1);

		UpdateTossNofText ();

		nextDollButton.SetActive (false);

		if (dollProgressionActivation != null && maxDollCounter - 1 < dollProgressionActivation.Length) {
			dollProgressionActivation [maxDollCounter - 1].SetActive (true);
		}

		if (initialPlayerDeltaZ != 0 && maxDollCounter > 1)
		{
			Vector3 targetPos = currentDoll.transform.position;
			currentDoll.transform.position = GameUtil.AddZ(currentDoll.transform.position, initialPlayerDeltaZ);
			LeanTween.moveZ(currentDoll.gameObject, targetPos.z, 0.3f); // .setEase(LeanTweenType.easeOutCirc);
		}

		InitHmmDelay ();

		if (dollFollower != null)
		{
			Transform t = currentDoll.transform.Find("Root");
			if (t != null)
				dollFollower.SetFollowThis(t.gameObject);
		}
	}

	void ChangeShadowDistance() {
		QualitySettings.shadowDistance = shadowDistanceAfterToss;
	}


	void SwipePrepare() {
		swipeReader.pinModZ = progressivePinModZ;
		swipeReader.SetDollPos (currentDoll.transform.position);
		swipeReader.enabled = true;
	}

	void NewToss() {
		
		if (levelFinished) {
			return;
		}
			

		tossCounter++;

		//LeanTween.move (Camera.main.gameObject, new Vector3 (currentDoll.GetRootPos ().x +  cameraDistanceBeforeNewToss.x, currentDoll.GetRootPos ().y + cameraDistanceBeforeNewToss.y, currentDoll.GetRootPos ().z + cameraDistanceBeforeNewToss.z), nextTossZoomTime).setEaseInOutSine();
		//LeanTween.rotate (Camera.main.gameObject, cameraRotationBeforeNewToss, nextTossZoomTime).setEaseInOutSine();

		swipeReader.Reset ();
		Invoke ("SwipePrepare", nextTossZoomTime + 0.1f);

		init = true;
		currentDoll.Reset ();
		currentDoll.SetActive (true);

		currentDoll.StopGhostAnim ();

		if (showTossIndicators)
			Indicate (0, 0, 1);

		if(showNextButtonBeforeProgressiveTosses && tossCounter > 1)
			nextDollButton.SetActive (true);

		UpdateTossNofText ();

		InitHmmDelay ();

	}

	public void NextButton() {
		// if (currentDoll.WasTossed() || showNextButtonBeforeProgressiveTosses) {
		{
            /*if (isLocalPlayer)
            {
                Listener.enabled = false;  //-------- Disables listener on character
            }*/

            //currentDoll.SetInactive ();
            currentDoll.Bubble(bubblePrefab, bubbleButtonBubbleDelay);

            

            SoundManager.instance.PlaySingleSfx (SingleSfx.Bubble, true, false, 0, 0.6f);

			NewDoll ();
			if (progressiveLevel)
				resetGoals ();
		}
	}

	private void restoreFixedTimeStep() {
		StaticManager.RestoreTimeStep(timeStepAffectIndex);
	}
		
	private float forceTemp, angleTemp;
	private bool enableMinimumZForceTemp;

	void Toss() {

		StaticManager.comboTossGoalsHit = 0;
		StaticManager.IncreaseTossCount(1);
		EvaluateTosses(); // massive FPS loss!
		if (currentDoll.extra_timeScale < 0.99f || currentDoll.extra_timeScale > 1.01f)
			Time.timeScale = currentDoll.extra_timeScale;
		
		if (slideSoundEmitter != null)
			slideSoundEmitter.PlaySound ();

		currentDoll.Toss (forceTemp, angleTemp, 0, enableMinimumZForceTemp, gravityMode);
		// Debug.Log ("TOSS: " + force.pos + "f, " + angle.pos + "f");

		if (shadowDistanceAfterToss > 0) {
			Invoke ("ChangeShadowDistance", 0.5f);
		}

		if (initialLoweredTimeStepTime > 0) {
			timeStepAffectIndex = StaticManager.PushFixedTimeStep (initialLoweredTimeStepValue);
			Invoke ("restoreFixedTimeStep", initialLoweredTimeStepTime);
		}
	}

	void Drop() {

		StaticManager.comboTossGoalsHit = 0;

		if (currentDoll.extra_timeScale < 0.99f || currentDoll.extra_timeScale > 1.01f)
			Time.timeScale = currentDoll.extra_timeScale;
		
		currentDoll.Drop (gravityMode);
		tossCamTimer = 0;

		if (initialLoweredTimeStepTime > 0) {
			timeStepAffectIndex = StaticManager.PushFixedTimeStep (initialLoweredTimeStepValue);
			Invoke ("restoreFixedTimeStep", initialLoweredTimeStepTime);
		}
	}

	public void ForceInstantLevelDeath() {
		ShowLevelFailed ();
	}

	private float oldForce = 0;
	private float oldAngle = 0;

	void Indicate(float forceval, float angleval, float angleMod, bool setOld = false) {

		float anglevalcalc = -(angleval / angleMod) * 1.2f;

		/*if ((int)Camera.main.transform.rotation.eulerAngles.z == 180)
			anglevalcalc = -anglevalcalc;*/
		           
		if (forceIndicator != null) {
			forceIndicator.localScale = GameUtil.SetY (forceIndicator.localScale, forceval * 0.56f);
		}
		if (angleIndicator != null) {
			angleIndicator.localScale = GameUtil.SetY (angleIndicator.localScale, anglevalcalc * 0.56f);
		}

		/*
		if (forceOldIndicator != null) {
			forceOldIndicator.localScale = GameUtil.SetY (forceOldIndicator.localScale, oldForce);
		}
		if (angleOldIndicator != null) {
			angleOldIndicator.localScale = GameUtil.SetY (angleOldIndicator.localScale, oldAngle);
		}

		if (setOld) {
			oldForce = forceval;
			oldAngle = anglevalcalc;
			if (forceOldIndicator != null) {
				forceOldIndicator.localScale = GameUtil.SetY (forceOldIndicator.localScale, 0);
			}
			if (angleOldIndicator != null) {
				angleOldIndicator.localScale = GameUtil.SetY (angleOldIndicator.localScale, 0);
			}
		} */

	}


	void Update () {

		//if (Time.timeScale > currentDoll.extra_timeScale)
		//	Time.timeScale = currentDoll.extra_timeScale;

		if (dollsTossedText.transform.parent.gameObject.activeSelf == false && goalsHitText.transform.parent.gameObject.activeSelf == false && timerText.transform.parent.gameObject.activeSelf == false)
			dollsTossedText.transform.parent.parent.gameObject.SetActive (false);

		if (init) {

			if (swipeReader.WasFlicked ()) {

				float animSpeed = (0.5f + swipeReader.GetYForce () / 1.8f) * 1.9f;
				if (animSpeed < 0.5f)
					animSpeed = 0.5f;

				currentDoll.transform.rotation = Quaternion.Euler (0, cameraOrgRotation.eulerAngles.y, 0);

				currentDoll.PlayAnim ("Jump", animSpeed);

				forceTemp = swipeReader.GetYForce ();
				angleTemp = swipeReader.GetXForce ();
				// print ("Toss: " + forceTemp + " " + angleTemp);
				enableMinimumZForceTemp = false;

				if (showTossIndicators)
					Indicate (swipeReader.GetSpeed (), angleTemp, 1, true);

				Invoke ("Toss", 0.4f / animSpeed); // look at the anim file to see how long this should be (0.7f original)

				init = false;
				tossCamTimer = 0;


			} else if (showTossIndicators) {
				if (swipeReader.IsOnGoing ()) {
					forceTemp = swipeReader.GetYForce ();
					angleTemp = swipeReader.GetXForce ();
					Indicate (swipeReader.GetSpeed (), angleTemp, 1);
				} else {
					Indicate (0, 0, 1);
				}
			}
		}

		if (temporaryCredits != StaticManager.GetTemporaryCredits ()) {
			temporaryCredits = StaticManager.GetTemporaryCredits ();

			if (creditsText != null) {
				creditsText.text = "" + (StaticManager.GetNumberOfCredits () + temporaryCredits);
			}
		}
		 
		//if (Input.GetKeyDown (KeyCode.V))
		//	currentDoll.PlayAnim ("Victory");

		//if (Input.GetKeyDown (KeyCode.T))
		//	currentDoll.TestStuff ();

		if (Input.GetKeyDown (KeyCode.S))
			currentDoll.ShowSpeechBubble (speechBubblePrefab, 2f);

		if (Input.GetKeyDown (KeyCode.C)) {
			float fxForce = 0, fyForce = 1;

			float animSpeed = (0.5f + fyForce / 1.8f) * 1.9f;
			if (animSpeed < 0.5f)
				animSpeed = 0.5f;
			
			forceTemp = fyForce;
			angleTemp = fxForce;
			enableMinimumZForceTemp = false;

			if (showTossIndicators)
				Indicate (swipeReader.GetSpeed (), angleTemp, 1, true);

			Invoke ("Toss", 0.4f / animSpeed); // look at the anim file to see how long this should be (0.7f original)

			init = false;
			tossCamTimer = 0;
		}
		
		/*
		if (Input.GetKeyDown (KeyCode.G))
			currentDoll.PlayAnim ("GetUpBack");
		if (Input.GetKeyDown (KeyCode.F))
			currentDoll.PlayAnim ("GetUpFront");
		if (Input.GetKeyDown (KeyCode.I))
			currentDoll.PlayAnim ("Idle");
		*/

		/*if (currentDoll.WasTossed ()) {
			tossCamTimer += Time.deltaTime;

			float camLerp = Mathf.Clamp01 ((tossCamTimer - camFollowDelay) / 3.0f);

			if (!currentDoll.IsGettingUp () && moveCamera && levelResults.stars != 0) {
				swoopCam.transform.position = new Vector3 (Mathf.Lerp (swoopCam.transform.position.x, currentDoll.GetRootPos ().x + cameraDistanceAfterToss.x, camLerp), Mathf.Lerp (swoopCam.transform.position.y, currentDoll.GetRootPos ().y + cameraDistanceAfterToss.y, camLerp), Mathf.Lerp (swoopCam.transform.position.z, currentDoll.GetRootPos ().z + cameraDistanceAfterToss.z, camLerp));
				if (tossCounter == 1)
					swoopCam.transform.rotation = Quaternion.Euler (new Vector3 (Mathf.Lerp (cameraOrgRotation.eulerAngles.x, cameraRotationAfterToss.x, camLerp), Mathf.Lerp (cameraOrgRotation.eulerAngles.y, cameraRotationAfterToss.y, camLerp), Mathf.Lerp (cameraOrgRotation.eulerAngles.z, cameraRotationAfterToss.z, camLerp)));
				else
					swoopCam.transform.rotation = Quaternion.Euler (new Vector3 (Mathf.Lerp (cameraRotationBeforeNewToss.x, cameraRotationAfterToss.x, camLerp), Mathf.Lerp (cameraRotationBeforeNewToss.y, cameraRotationAfterToss.y, camLerp), Mathf.Lerp (cameraRotationBeforeNewToss.z, cameraRotationAfterToss.z, camLerp)));
			}

			// Needs later cleanup. Basically has to do with when the "Next" button should be shown. Right now it is shown all the time, except before the first toss of a new doll

//			if (!progressiveLevel && !standUpPlayer)
//				nextDollButton.SetActive (currentDoll.showNextDollButton());

/*			if (currentDoll.IsGettingUp())
				nextDollButton.SetActive (false);
			else 
			if (!noNextButton && !levelFinished)
				nextDollButton.SetActive (true);

			//if (progressiveLevel && showNextButtonBeforeProgressiveTosses)
			//	nextDollButton.SetActive (false);


			// enable to reset indicators as soon as dolls starts to stand up
			//if (currentDoll.IsGettingUp() && showTossIndicators)
			//	Indicate (0, 0, 1);
			

		} else {
			if (progressiveLevel && tossCounter > 1)
				nextDollButton.SetActive (true);

			if (!levelFinished) {
				if (hmmDelay > 0) {
					hmmDelay -= Time.deltaTime;
					if (hmmDelay < 0) {
						Hmm ();
						InitHmmDelay ();
					}
				}
			}
		}*/

		foreach (ToonDollHelper th in ragdolls) {
			int goalHit = th.HasHitGoal ();
			if (goalHit != -1) {
				bool allClear = UpdateGoalStatus ();
				if (allClear) {
					ShowLevelCleared ();
				}
			}
		}
			
		if (currentDoll.IsReadyForNext()) {

			if (awaitStopObjects != null) {
				foreach (GameObject g in awaitStopObjects) {
					Rigidbody rb = g.GetComponent<Rigidbody> ();
					if (rb != null && rb.velocity.sqrMagnitude > 2)
						return;
				}
			}

			if (repeatSlideSound)
				slideSoundEmitter.StopPlay ();
				
			if (!progressiveLevel)
				nextDollButton.SetActive (false);
			
			if (autoBubble && !currentDoll.WasBubbled ())
            {
                /*if (isLocalPlayer)
                {
                    Listener.enabled = false;  //-------- Disables listener on character
                }*/
                currentDoll.Bubble (bubblePrefab);
                //SoundManager.instance.PlaySingleSfx (SingleSfx.Bubble, true, false, 0, 0.05f);
            }

			if (!progressiveLevel) {
				NewDoll (true);
				resetGoals ();
			} else {
				if (currentDoll.IsOutOfBounds ()) {
					resetGoals ();
					NewDoll (true);
				} else {
					bool requirementsOk = true;

					if (allGoals != null) {
						foreach (Goal g in allGoals) {
							if (g.HasFailedRequiredToss (currentDoll.GetTossNumber ())) {
								requirementsOk = false;
								resetGoals ();
							}
						}
					}

					if (requirementsOk)
						NewToss ();
					else {
						NewDoll (true);
					}
				}
			}
		}

		if (timer > 0) {
			if (timerAwaitToss == false || currentDoll.WasTossed()) {
				timer -= Time.deltaTime;
				if (timer < 0) {
					timer = 0;
					ShowLevelCleared (3);
				}
				UpdateTimerText ();
			}
		}
	}

	private void resetGoals() {
		bool didReset = false;

		if (allGoals == null)
			return;

		foreach (Goal g in allGoals) {
			if (g.resetOnMissedGoal) {
				g.ResetGoal ();
				didReset = true;
			}
		}
		if (didReset)
			UpdateGoalStatus ();
	}


	private void ToProgress() {
	//	StaticManager.GotoProgressScreen (levelResults);
	}


	private int CalculateStars() {
		int stars = 2;

		if (levelScoreType == LevelScoreType.NofDollsUsed) {
			if (star1req < 0)
				star1req = maxDolls;
			if (star3req < 0)
				star3req = requiredGoalHits;

			if (!currentDoll.WasTossedOnce ())
				maxDollCounter--;
			if (maxDollCounter <= star3req)
				stars = 3;
			else if (maxDollCounter >= star1req)
				stars = 1;
		} 
		else if (levelScoreType == LevelScoreType.NofTosses) {
			if (tossCounter <= star3req)
				stars = 3;
			else if (tossCounter >= star1req)
				stars = 1;
		}
		else if (levelScoreType == LevelScoreType.NofGoalsHit) {
			if (star1req < 0)
				star1req = 1;
			if (star3req < 0)
				star3req = requiredGoalHits;

			int goalsHit = GetNofGoalsHit ();

			if (goalsHit >= star3req)
				stars = 3;
			else if (goalsHit <= star1req)
				stars = 1;
		}

		stars = Mathf.Clamp (stars + currentDoll.extra_extraStarsPerLevel, 0, 3);

		return stars;
	}
	private void EvaluateTosses() {

		StaticTaskManager.EvaluateAll (StaticTaskManager.TaskType.CharacterThrow, StaticManager.GetTotalTossCount());
		//StaticManager.Save ();
	}
	private void ShowLevelCleared(int forcedStars = -1) {
		if (levelFinished)
			return;
		float gameDuration = Time.time - gameTime;
		StaticTaskManager.RouteTask(StaticTaskManager.TaskType.Speedy, (int)gameDuration,null);
	//	Debug.Log("Game took " + gameDuration);
		//finishText.gameObject.SetActive (true);

		levelResults.score = 0;
		levelResults.time = float.MaxValue;
		if (forcedStars == 1 || forcedStars == 2 || forcedStars == 3)
			levelResults.stars = forcedStars;
		else
			levelResults.stars = CalculateStars();

		if(levelResults.stars == 3) {
			StaticManager.AddThreeStars(StaticManager.GetLevel(),StaticManager.GetWorldIndex());
		}
		soundManager.PlayRandomFromType (SfxRandomType.Cheering);
		soundManager.PlayRandomFromType (currentDoll.soundYesWin, -1, 0.2f + (celebrateVictory? celebrationStartTime : 0), -1, currentDoll.soundPitch);

		currentDoll.SetLevelFinished (celebrateVictory, celebrationStartTime, celebrationRiseSpeed);

		levelFinished = true;

		nextDollButton.SetActive (false);

		if (eventSystem != null)
			eventSystem.gameObject.SetActive (false);

		Invoke ("ToProgress", 0.95f + (celebrateVictory? celebrationStartTime : 0));
	}

	private void ShowLevelFailed(bool playSound = false) {
		if (levelFinished)
			return;

		if (noMoreDollsSound != SingleSfx.None && playSound) {
			soundEmitter.SetSingleSfx (noMoreDollsSound);
			soundEmitter.PlaySound ();
		}

		//failText.gameObject.SetActive (true);

		levelResults.score = 0;
		levelResults.time = float.MaxValue;
		levelResults.stars = 0;

		levelFinished = true;
	
		if (eventSystem != null)
			eventSystem.gameObject.SetActive (false);

		Invoke ("ToProgress", 1.0f);
	}


	private void ShowDollsCount() {
		dollsTossedText.gameObject.SetActive (true);
		dollsTossedText.transform.parent.gameObject.SetActive (true);
		dollsTossedText.text = maxDollCounter + " / " + maxDolls;
	}

	/* CHEATING */

	public void CheatLose() {
		ShowLevelFailed ();
	}
	public void CheatWin1() {
		ShowLevelCleared (1);
	}
	public void CheatWin2() {
		ShowLevelCleared (2);
	}
	public void CheatWin3() {
		ShowLevelCleared (3);
	}

	private void InitHmmDelay() {
		hmmDelay = 4 + Random.Range (0f, 4f);
	}

	private void Hmm() {
		currentDoll.Hmm ();
	}

}
