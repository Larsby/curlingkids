using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
	public enum LevelScoreType { NofDollsUsed, NofGoalsHit, NofTosses };

	public enum FallingPanicType { None, Normal, Restricted };

	public enum DropPanicType { None, Normal, GoingUp, GoingUpOrDown };

	public enum InitSwoopType { Spinning, Zooming };

	public LevelScoreType levelScoreType = LevelScoreType.NofDollsUsed;

	public bool forceBelowDoll = false;

	public GameObject[] dolls;
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
	public GameObject trailParticlePrefab;
	public GameObject fireWorkPrefab;
	public GameObject zombieHeartPrefab;

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

	private Camera swoopCam;

	private Text goalsHitText;
	private Text dollsTossedText;
	private Text finishText;
	private Text failText;
	private Text creditsText;
	private Text timerText;
	private Text currentTossesText;
	private Text outOfPlayersText;
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
	public Vector3 cameraDistanceAfterToss = new Vector3(0, 1.7f, -5f);
	public Vector3 cameraRotationAfterToss = new Vector3(30, 0, 0);

	public Vector3 cameraDistanceBeforeNewToss = new Vector3(0, 4.7f, -3.2f);
	public Vector3 cameraRotationBeforeNewToss = new Vector3(33.56f, 0, 0);


	public float stillStandingTime = 0;

	public Vector3 worldGravity = new Vector3(0, -9.81f, 0);

	public float shadowDistance = 25;
	public ShadowProjection shadowProjection = ShadowProjection.StableFit;
	public float shadowDistanceAfterToss = -1;

	private List<PushButton> pushButtons = new List<PushButton>();

	public bool allwToonToOverride_DragAngularMass = true;
	public Vector3 DragAngulardragMass = new Vector3(0.14f, 0.1f, 2.5f);
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

	private bool useGhost = true;

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
	public Vector3 fallingPanic_SpeedTreshold_beginLength_panicLength = new Vector3(9f, 0.25f, 2f);
	public float fallingPanicProbability = 1;
	public DropPanicType dropPanic = DropPanicType.None;
	public Vector3 dropPanic_SpeedTreshold_beginLength_panicLength = new Vector3(2f, 1.5f, 4f);
	public float dropPanicProbability = 1;

	public float customTimeScale = 1.3f;

	public bool noNextButton = false;

	public bool autoBubble = false;
	public float bubbleButtonBubbleDelayRapidFire = 0;

	private int maxTosses = -1;

	public GameObject[] dollProgressionActivation;
	public GameObject[] awaitStopObjects;

	public bool resetPushButtonsOnNewDoll = true;

	public bool playHitMoans = true;
	public float hitMoanInitialWait = 0;

	public bool playSlidingSound = true;
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

	public Collider[] disableOnFinish;
	private bool showCoolDownPopup = true;
	private float restartButtonCooldownTime = 13f; // unscaledDeltaTime is used because of timescale
	private float restartButtonTimer;
	private Button restartButton = null;
	private Text restartButtonText = null;
	private int beginNofJewelsTaken = 0;
	private Canvas canvas;
	private float timePassed;

	void OnDestroy()
	{
		StaticManager.StoreTemporaryCredits();
		StaticManager.SaveMoneyMakers();
		StaticManager.Save();
		StaticTaskManager.Save();
		StaticManager.nofNewJewelsTaken = StaticManager.GetMoneyMakersTaken() - beginNofJewelsTaken;
		Time.timeScale = 1;
	}


	private GameObject ActivateAndListenToButton(GameObject root, string buttonName, UnityEngine.Events.UnityAction call, bool active = true)
	{
		GameObject find = null;
		Button b = null;

		Transform t = GameUtil.FindDeepChild(root.transform, buttonName);
		if (t != null)
		{
			find = t.gameObject;
			find.SetActive(active);
			b = find.GetComponent<Button>();
			if (b != null)
				b.onClick.AddListener(call);
		}
		return find;
	}

	private Text FindUIText(GameObject root, string name)
	{
		Text text = null;
		Transform t = GameUtil.FindDeepChild(root.transform, name);

		if (t != null)
		{
			text = t.gameObject.GetComponent<Text>();
		}
		return text;
	}

	void Awake()
	{
		timePassed = Time.time;
		AppLovin.SetUnityAdListener("GameController");
		if ((float)Screen.width / (float)Screen.height < 0.5f) { // the iPhone X/tall devices hack (iPhone X: 1125/2436 = 0.46   9/16 = 0.56 )
			Camera.main.fieldOfView += 10.0f;
		}

		levelResults.stars = -1;

		if (initSwoop == InitSwoopType.Spinning)
		{
			startSwoopTime = 3; startSwoopTimer = 0;
			startSwoopHeight = 50; startSwoopCircleRadius = 30; startSwoopRotationSpeed = 3.2f; startSwoopLastRotModiferTime = 1f;
		}

		SoundManager.Create();

		if (CharacterManager.instance != null && forceBelowDoll == false)
		{
			useSingleDoll = true;
			singleDoll = CharacterManager.instance.GetAvatar(StaticManager.GetSelectedAvatarIndex());
		}

		if (screenFaderPrefab != null)
		{
			screenFader = Instantiate(screenFaderPrefab);
		}

		QualitySettings.shadowProjection = shadowProjection;
		QualitySettings.shadowDistance = shadowDistance;

		if (gameUICanvasPrefab)
		{
			GameObject go;
			Button b;
			 canvas = Instantiate(gameUICanvasPrefab);
		
			nextDollButton = ActivateAndListenToButton(canvas.gameObject, "NextDollButton", NextButton, false);

			goalsHitText = FindUIText(canvas.gameObject, "GoalsText");
			dollsTossedText = FindUIText(canvas.gameObject, "DollsText");
			finishText = FindUIText(canvas.gameObject, "FinishText");
			failText = FindUIText(canvas.gameObject, "FailText");
			creditsText = FindUIText(canvas.gameObject, "CreditsText");
			timerText = FindUIText(canvas.gameObject, "TimerText");
			currentTossesText = FindUIText(canvas.gameObject, "TossText");
			outOfPlayersText = FindUIText(canvas.gameObject, "OutOfPlayersText");
			GameObject pop = GameObject.FindWithTag("Popup");
			pop.transform.parent = null;
			StaticManager.ResetTemporaryCredits();

			if (creditsText != null)
			{
				creditsText.gameObject.SetActive(true);
				creditsText.text = "" + StaticManager.GetNumberOfCredits();
			}


			GameObject restartButtonObject = ActivateAndListenToButton(canvas.gameObject, "RestartButton", ActoOnRestartButtonPressed);
			if (restartButtonObject && restartButtonCooldownTime > 0) {
				restartButton = restartButtonObject.GetComponent<Button>();
				if (restartButton) {
			//		restartButton.interactable = false;
					showCoolDownPopup = true;
					restartButtonTimer = restartButtonCooldownTime;
					restartButtonText = restartButton.GetComponentInChildren<Text>();
					//Invoke("EnableRestartButton", restartButtonCooldownTime);
				}else if(restartButtonCooldownTime <= 0) {
					showCoolDownPopup = false;
				}
			}

			ActivateAndListenToButton(canvas.gameObject, "LevelsButton", GotoLevelSelect);

			if (showCheatButtons)
			{
				ActivateAndListenToButton(canvas.gameObject, "Lose", CheatLose);
				ActivateAndListenToButton(canvas.gameObject, "Win1", CheatWin1);
				ActivateAndListenToButton(canvas.gameObject, "Win2", CheatWin2);
				ActivateAndListenToButton(canvas.gameObject, "Win3", CheatWin3);
			}

			go = canvas.gameObject.transform.Find("TossButton").gameObject;
			go.SetActive(true);

			swipeReader = go.GetComponent<SwipeReader>();
			swipeReader.enabled = true;
			swipeReader.pinModZ = pinModZ;

			if (showTossIndicators)
			{
				indicatorContainer = canvas.gameObject.transform.Find("Indicators").gameObject;
				if (indicatorContainer != null)
				{
					indicatorContainer.SetActive(true);
					forceIndicator = GameUtil.FindDeepChild(indicatorContainer.transform, "ForceIndicator").gameObject.GetComponent<RectTransform>();
					angleIndicator = GameUtil.FindDeepChild(indicatorContainer.transform, "AngleIndicator").gameObject.GetComponent<RectTransform>();
					forceOldIndicator = GameUtil.FindDeepChild(indicatorContainer.transform, "ForceOld").gameObject.GetComponent<RectTransform>();
					angleOldIndicator = GameUtil.FindDeepChild(indicatorContainer.transform, "AngleOld").gameObject.GetComponent<RectTransform>();
				}
			}

			Deathbox ground = Instantiate(deathBoxPrefab);
			ground.gameObject.transform.position = GameUtil.SetY(ground.gameObject.transform.position, groundPosY);

			Deathbox heaven = Instantiate(deathBoxPrefab);
			heaven.gameObject.transform.position = GameUtil.SetY(heaven.gameObject.transform.position, heavenPosY);

			eventSystem = FindObjectOfType<EventSystem>();

			Object[] all = FindObjectsOfType<Object>();
			foreach (Object g in all)
			{
				PushButton pb = g as PushButton;
				if (pb != null)
					pushButtons.Add(pb);
			}
			foreach (PushButton pb in pushButtons)
			{
				go = null;
				if (pb.GetPushButtonIndex() >= 0)
					go = canvas.gameObject.transform.Find("PushButton" + pb.GetPushButtonIndex()).gameObject;
				if (go != null)
				{
					go.SetActive(true);
					b = go.GetComponent<Button>();
					pb.AddButtonListener(b);
				}
			}
		}

		playerHelpers = Object.FindObjectsOfType<ToonDollHelper>();
		if (playerHelpers != null && playerHelpers.Length != 0)
		{
			ragdolls.AddRange(playerHelpers);
		}


		Time.fixedDeltaTime = fixedTimeStep;
		StaticManager.originalLevelTimeStep = fixedTimeStep;

		StaticManager.originalCollisionDetectionMode = collisionDetectionMode;

		Application.targetFrameRate = 60;

		Physics.gravity = worldGravity;
		if (!flipperPhysics)
			Physics.gravity = GameUtil.AddY(Physics.gravity, -StaticManager.globalGravityAdd);

		swoopCam = Camera.main;

		cameraOrgPos = swoopCam.transform.position;
		cameraOrgRotation = swoopCam.transform.rotation;

		foreach (GameObject go in GameObject.FindGameObjectsWithTag("StartPos")) {
			startPositions.Add(go.transform.position);
		}

		if (dollProgressionActivation != null)
			foreach (GameObject g in dollProgressionActivation)
				g.SetActive(false);


		// this crap is to put a soundemitter on referees on the curling levels, because I stupidly did not make them prefabs from the beginning and it's very hard to turn them into prefabs due to various added components etc
		if (StaticManager.UGLYFIX_IsLevelType(StaticManager.UGLYFIX_curlingWorldIndex))
		{
			SimpleTransform[] refTransf = GameObject.FindObjectsOfType<SimpleTransform>();
			ToonDollHelper[] refTdh = GameObject.FindObjectsOfType<ToonDollHelper>();
			List<GameObject> referees = new List<GameObject>();
			if (refTransf != null)
			{
				foreach (SimpleTransform st in refTransf)
					if (st.gameObject.name.Length >= 6 && st.gameObject.name.Substring(0, 6) == "Sporty")
						referees.Add(st.gameObject);
			}
			if (refTdh != null)
			{
				foreach (ToonDollHelper tdh in refTdh)
					if (tdh.gameObject.name.Length >= 6 && tdh.gameObject.name.Substring(0, 6) == "Sporty")
						referees.Add(tdh.gameObject);
			}

			if (referees.Count > 0)
			{
				foreach (GameObject g in referees)
				{
					if (g.GetComponent<SoundEmitter>() == null)
					{
						SoundEmitter sg = g.AddComponent<SoundEmitter>();
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
					if (g.GetComponent<AwardTrigger>() == null)
					{
						AwardTrigger awardTrigger = g.AddComponent<AwardTrigger>();
						awardTrigger.taskType = StaticTaskManager.TaskType.Other;
						awardTrigger.description = "Referee";
						awardTrigger.destroyOnHit = false;
					}
				}
			}
		}
		// same as above for same reason, but for zombies for zombie levels
		if (StaticManager.UGLYFIX_IsLevelType(StaticManager.UGLYFIX_zombieWorldIndex) || StaticManager.UGLYFIX_IsLevelType(StaticManager.UGLYFIX_xmasWorldIndex))
		{
			ToonDollHelper[] zomTdh = GameObject.FindObjectsOfType<ToonDollHelper>();
			List<GameObject> zombies = new List<GameObject>();
			if (zomTdh != null)
			{
				foreach (ToonDollHelper tdh in zomTdh)
					if (tdh.zombieHitMode)
						zombies.Add(tdh.gameObject);
			}

			if (zombies.Count > 0)
			{
				foreach (GameObject g in zombies)
				{
					ToonDollHelper tdh = g.GetComponent<ToonDollHelper>();
					if (tdh && tdh.sleepBubblePrefab == null)
						tdh.sleepBubblePrefab = zombieHeartPrefab;

					Goal goal = g.GetComponent<Goal>();
					if (goal != null)
					{
						if (goal.hitSound == SingleSfx.NewWorld) // extremely ugly hack to allow silencing a zombie after all
							goal.hitRandomSound = SfxRandomType.None;
						else
							goal.hitRandomSound = SfxRandomType.ZombieHit;
						goal.hitSound = SingleSfx.None;
					}
					else if (g.GetComponent<SoundEmitter>() == null)
					{
						SoundEmitter sg = g.AddComponent<SoundEmitter>();
						sg.emitterType = SoundEmitter.EmitterType.OnCollideActivePlayer;
						sg.playerType = SoundEmitter.PlayerType.Player;
						sg.randomSfx = SfxRandomType.ZombieHit;
					}
				}
			}
		}

		NewDoll();

		if (useGhost)
			MakeGhost();

		UpdateGoalStatus();

		maxTosses = CalculateMaxTosses();
		UpdateTossNofText();

		UpdateTimerText();

		StaticManager.SetCurrentScene();
	
		GameObject jewelsContainer = GameObject.Find("Stars");
		if (jewelsContainer != null)
		{
			List<MoneyMaker> moneymakers = new List<MoneyMaker>();
			for (int i = 0; i < jewelsContainer.transform.childCount; i++)
				if (jewelsContainer.transform.GetChild(i).GetComponentInChildren<MoneyMaker>() != null)
					moneymakers.Add(jewelsContainer.transform.GetChild(i).GetComponentInChildren<MoneyMaker>());
			if (moneymakers.Count > 0)
				StaticManager.PrepareMoneyMakers(moneymakers.ToArray());
		}
		// StaticManager.PrepareMoneyMakers(FindObjectsOfType<MoneyMaker>()); // was used before without any cumbersome and ugly search for "Stars" etc, but turns out FindObjectsOfType does not in any way guarantee order of objects found! (worked for a long time, then just stopped working)

		if (playSlidingSound)
		{
			slideSoundEmitter = gameObject.AddComponent<SoundEmitter>();
			slideSoundEmitter.emitterType = SoundEmitter.EmitterType.RemoteControlled;
			slideSoundEmitter.randomSfx = SfxRandomType.Slide;
			slideSoundEmitter.volume = 0.5f;
			slideSoundEmitter.randomPitch = false;
			if (repeatSlideSound)
				slideSoundEmitter.repeatSoundNof = -1; // repeat infinite
		}

		soundEmitter = gameObject.AddComponent<SoundEmitter>();
		soundEmitter.emitterType = SoundEmitter.EmitterType.RemoteControlled;

		GameObject[] hideUs = GameObject.FindGameObjectsWithTag("HideMe");
		if (hideUs != null && hideUs.Length > 0)
			foreach (GameObject g in hideUs)
				g.SetActive(false);

		StaticManager.showComboHitTextTreshold = showComboHitTextTreshold;
		StaticManager.comboTossGoalsHit = 0;

		beginNofJewelsTaken = StaticManager.GetMoneyMakersTaken();
	}


	void Start()
	{
		soundManager = SoundManager.instance; // instance is safe to use from Start and onwards
		timePassed = Time.time+restartButtonCooldownTime;
		StaticManager.PlayMusicBasedOnWorld();

		if (beginSound != SingleSfx.None)
		{
			soundEmitter.singleSfx = new SingleSfx[] { beginSound };
			soundEmitter.PlaySound();
		}
		gameTime = Time.time;
		PerformanceManager manager = gameObject.AddComponent<PerformanceManager>();
		manager.Setup("CKPerformance_v1_01" + StaticManager.GetCurrentSceneString());
	}

	private void EnableRestartButton() {
		showCoolDownPopup = false;
	//	restartButton.interactable = true;
	}

	private void UpdateTimerText()
	{
		if (timer >= 0)
		{
			timerText.gameObject.SetActive(true);
			timerText.transform.parent.gameObject.SetActive(true);

			string minutes = Mathf.Floor(timer / 60).ToString("00");
			string seconds = Mathf.Floor(timer % 60).ToString("00");
			string millis = Mathf.Floor((timer * 10) % 10).ToString("0");

			timerText.text = minutes + ":" + seconds + ":" + millis;
		}
	}

	private void ActoOnRestartButtonPressed() {
		
		if(StaticManager.HaveUnlimitedReset()) {
			showCoolDownPopup = false;
		}
		if(Time.time >timePassed){
			showCoolDownPopup = false;
		}
		if (showCoolDownPopup)
		{
			
			GameObject go = canvas.gameObject.transform.Find("TossButton").gameObject;
			go.SetActive(false);
			DoShowCooldownPopup();
		}
		else
			RestartLevel();

	}


	private float prevTimeScale = 0;
	void onAppLovinEventReceived(string ev)
	{
		if (ev.Contains("REWARDAPPROVEDINFO"))
		{
			/*
			// The format would be "REWARDAPPROVEDINFO|AMOUNT|CURRENCY" so "REWARDAPPROVEDINFO|10|Coins" for example
			string delimeter = "|";

			// Split the string based on the delimeter
			string[] split = ev.Split(delimeter);

			// Pull out the currency amount
			double amount = double.Parse(split[1]);

			// Pull out the currency name
			string currencyName = split[2];
			*/
		
			// Do something with the values from above.  For example, grant the coins to the user.
			//updateBalance(amount, currencyName);
		}
		else if (ev.Contains("LOADEDREWARDED"))
		{
			// A rewarded video was successfully loaded.
		}
		else if (ev.Contains("LOADREWARDEDFAILED"))
		{
			if (prevTimeScale > 0)
				Time.timeScale = prevTimeScale;
			soundManager.UnPauseMusic();
			GameObject go = canvas.gameObject.transform.Find("TossButton").gameObject;
			go.SetActive(true);
		}
		else if (ev.Contains("HIDDENREWARDED"))
		{

		if (prevTimeScale > 0)
				Time.timeScale = prevTimeScale;

		soundManager.UnPauseMusic();
		StaticManager.AddTimeForUnlimitedReset(5);
		StaticManager.RestartSameLevel();
		// A rewarded video was closed.  Preload the next rewarded video.
		AppLovin.LoadRewardedInterstitial();
		}
	}

	private void DoShowCooldownPopup()
	{

		Popup popup = GameObject.FindObjectOfType<Popup>();
		if (popup != null)
		{
			popup.ShowYesNo(NotifyCooldownResult, false, "Watch a video for unlimited resets for 5 minutes?");
		}
	}


	public void NotifyCooldownResult(Popup.PopupButtonChoice buttonChoice)
	{
		Popup popup = GameObject.FindObjectOfType<Popup>();

		if (buttonChoice == Popup.PopupButtonChoice.YES)
		{
#if UNITY_EDITOR
			StaticManager.AddTimeForUnlimitedReset(5);
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
#else
			//levelFinished = true;
			AppLovin.ShowRewardedInterstitial();
			soundManager.PauseMusic();
			if (Time.timeScale > 0)
			{
				prevTimeScale = Time.timeScale;
				
				Time.timeScale = 0;
			}
#endif
			return;
		}
		if(buttonChoice == Popup.PopupButtonChoice.NO) {
			showCoolDownPopup = true;
			GameObject go = canvas.gameObject.transform.Find("TossButton").gameObject;
			go.SetActive(true);
		}


	}

	
	private void RestartLevel()
	{
		soundManager.PlaySingleSfx(SingleSfx.Button1, true);

		if (StaticManager.usePlayLevelScreen)
			SceneManager.LoadScene("PlayLevel");
		else
			StaticManager.RestartSameLevel();
	}

	private void GotoLevelSelect()
	{
		soundManager.PlaySingleSfx(SingleSfx.BackButton, true);

		if (StaticManager.playingFreeLevel)
			SceneManager.LoadScene("WorldSelect");
		else
			SceneManager.LoadScene("LevelSelect");
	}

	private int GetNofGoalsHit()
	{
		int nofHitGoals = 0;

		if (allGoals == null)
			allGoals = FindObjectsOfType<Goal>();

		foreach (Goal g in allGoals)
		{
			if (g.IsIncludeInGoalCount())
			{
				nofHitGoals += g.GetNofTimesHit();
			}
		}
		return nofHitGoals;
	}

	private int GetNofRequiredGoalsHit()
	{
		if (forcedRequiredGoalHits > 0)
			return forcedRequiredGoalHits;

		int nofRequiredGoals = 0;

		Goal[] goals = FindObjectsOfType<Goal>();

		foreach (Goal g in goals)
		{
			if (g.IsIncludeInGoalCount())
				nofRequiredGoals += g.GetNofRequiredHits();
		}
		return nofRequiredGoals;
	}

	private bool UpdateGoalStatus()
	{
		if (requiredGoalHits == -1)
			requiredGoalHits = GetNofRequiredGoalsHit();

		if (requiredGoalHits == 0)
			return false;

		int goalsHit = GetNofGoalsHit();
		goalsHitText.text = goalsHit + " / " + requiredGoalHits;
		goalsHitText.gameObject.SetActive(true);
		goalsHitText.transform.parent.gameObject.SetActive(true);

		return goalsHit >= requiredGoalHits;
	}


	private int CalculateMaxTosses()
	{
		if (!progressiveLevel)
			return -1;

		int maxRequired = 0;

		if (allGoals != null)
		{
			foreach (Goal g in allGoals)
			{
				maxRequired = Mathf.Max(maxRequired, g.GetRequiredHitAtTossNumber());
			}
		}
		return maxRequired;
	}

	private void UpdateTossNofText()
	{

		if (maxTosses < 0)
			return;

		currentTossesText.gameObject.SetActive(true);
		currentTossesText.transform.parent.gameObject.SetActive(true);

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

	void MakeGhost()
	{
		GameObject ghost;

		ghost = Instantiate(currentDoll.gameObject);
		ghost.name = "GHOST";

		ToonDollHelper tdh = ghost.GetComponent<ToonDollHelper>();
		tdh.SetKinematic();
		Destroy(tdh);

		ghost.transform.position = new Vector3(400, 0, 5);

		Rigidbody rb = ghost.GetComponent<Rigidbody>();
		Destroy(rb);

		SkinnedMeshRenderer smr = ghost.GetComponentInChildren<SkinnedMeshRenderer>();
		smr.materials = new Material[0]; //smr.enabled = false; smr.updateWhenOffscreen = true;

		ToonDollHelper.RemoveRigidComponents(ghost, true, true);
	}


	private IEnumerator StartSwooper()
	{

		float rotCount = 0;

		swipeReader.enabled = false;

		//yield return new WaitForEndOfFrame();

		while (startSwoopTimer < startSwoopTime + 0.1f)
		{

			float progress = Mathf.Clamp01(1 - (startSwoopTimer / startSwoopTime));

			Camera.main.transform.position = cameraOrgPos + new Vector3(Mathf.Sin(rotCount) * progress * startSwoopCircleRadius, progress * startSwoopHeight, Mathf.Cos(rotCount) * progress * startSwoopCircleRadius);
			Vector3 rotPos = GameUtil.AimTowards(Camera.main.gameObject, currentDoll.transform.position, false);

			if (initSwoop == InitSwoopType.Zooming)
				rotPos = new Vector3(startSwoopXRotation, 0, 0);

			float lastProgress = Mathf.Clamp01((startSwoopTimer - (startSwoopTime - startSwoopLastRotModiferTime)) / startSwoopLastRotModiferTime);

			if (initSwoop == InitSwoopType.Zooming)
				lastProgress = Easing.Cubic.In(lastProgress);
			else
				lastProgress = Easing.Exponential.In(lastProgress);

			Quaternion slerper = Quaternion.Slerp(Quaternion.Euler(rotPos), cameraOrgRotation, lastProgress);

			Camera.main.transform.rotation = slerper;

			rotCount += startSwoopRotationSpeed * Time.deltaTime;

			startSwoopTimer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		swipeReader.SetDollPos(currentDoll.transform.position);
		swipeReader.enabled = true;
		gameTime = Time.time;
		InitHmmDelay();
	}


	void ShowOutOfPlayersText() {
		if (!outOfPlayersText) return;

		outOfPlayersText.gameObject.SetActive(true);

		outOfPlayersText.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
		LeanTween.scale(outOfPlayersText.gameObject, Vector3.one, 0.6f).setEase(LeanTweenType.easeOutBounce);

	}

	void NewDoll(bool playNextDollSound = false)
	{
		GameObject rd;

		Time.timeScale = customTimeScale;

		if (levelFinished) 
			return;
		
		tossCounter = 1;

		QualitySettings.shadowDistance = shadowDistance;

		maxDollCounter++;
		if (maxDolls > 0)
		{
			if (maxDollCounter > maxDolls)
			{
				if (levelScoreType == LevelScoreType.NofGoalsHit)
				{
					if (star1req < 0)
						star1req = 1;

					if (GetNofGoalsHit() >= star1req)
						ShowLevelCleared();
					else
						ShowLevelFailed(true);
				}
				else {
					ShowOutOfPlayersText();
					ShowLevelFailed(true);
				}
				if (currentDoll.IsGettingUp() && noMoreDollsSound == SingleSfx.None)
					SoundManager.instance.PlayRandomFromType(SfxRandomType.StandUp, -1, 0, -1, -1, true);
				return;
			}
			ShowDollsCount();
		}

		if (playNextDollSound)
			soundManager.PlayRandomFromType(currentDoll.soundNextPlayer, -1, 0, -1, currentDoll.soundPitch);

		init = !noToss;

		if (maxDollCounter == 1 || resetPushButtonsOnNewDoll)
			foreach (PushButton pb in pushButtons)
			{
				pb.ResetState();
			}

		if ((progressiveLevel || standUpPlayer) && currentDoll != null && !currentDoll.IsAwaitingBubble())
			currentDoll.Reset();

		if (startSwoop == false || maxDollCounter > 1)
		{
			if (nextDollZoomTime <= 0)
			{
				Camera.main.transform.position = cameraOrgPos;
				Camera.main.transform.rotation = cameraOrgRotation;
			}
			else
			{
				LeanTween.move(Camera.main.gameObject, cameraOrgPos, nextDollZoomTime).setEaseInOutSine();
				LeanTween.rotate(Camera.main.gameObject, cameraOrgRotation.eulerAngles, nextDollZoomTime).setEaseInOutSine();
			}
		}

		if (useSingleDoll)
			rd = Instantiate(singleDoll);
		else
			rd = Instantiate(dolls[dollCounter]);
		dollCounter++;
		if (dollCounter >= dolls.Length)
			dollCounter = 0;
		ToonDollHelper rdh = rd.GetComponent<ToonDollHelper>();
		currentDoll = rdh;
		ragdolls.Add(rdh);
		rdh.SetMaterial(playerPhysMaterial);

		if (startSwoop && maxDollCounter == 1)
			StartCoroutine(StartSwooper());

		Vector3 dam = GameUtil.CloneVector3(DragAngulardragMass);
		if (allwToonToOverride_DragAngularMass)
		{
			if (currentDoll.extra_overrideDrag >= 0)
				dam.x = currentDoll.extra_overrideDrag;
			if (currentDoll.extra_overrideAngularDrag >= 0)
				dam.y = currentDoll.extra_overrideAngularDrag;
			if (currentDoll.extra_overrideMass >= 0)
				dam.z = currentDoll.extra_overrideMass;
		}
		rdh.SetRigidValues(dam, collisionDetectionMode, removeRigidsOnFinished);
		rdh.SetFlipperPhysics(flipperPhysics);

		currentDoll.SetStandup(standUpPlayer, progressiveLevel, hitTurnsStandingIntoRagdoll, getUpAfterReenabledRagdoll);
		currentDoll.SetTossForce(tossForce, tossXForceActualOverride);
		currentDoll.SetStillStandTime(stillStandingTime);

		currentDoll.SetAirtime(airtimeSteering, airtimeAcceleration, airtimeBreak, airtimeAccChangeDir, airtimeBrkChangeDir);

		currentDoll.SetShowNextDollButtonLimits(nextDollButton_noCollideTime, nextDollButton_lowSpeedMagnitude, nextDollButton_lowSpeedTime);

		currentDoll.SetKinematic(); // for flipper stages, so player doesn't start sliding down

		currentDoll.transform.rotation = Quaternion.Euler(currentDoll.transform.rotation.eulerAngles.x, cameraOrgRotation.eulerAngles.y, currentDoll.transform.rotation.eulerAngles.z);

		currentDoll.bGazing = keepHeadUp;

		currentDoll.fallingPanic = fallingPanic;
		currentDoll.flyingPanicSpeedTreshold = fallingPanic_SpeedTreshold_beginLength_panicLength.x;
		currentDoll.flyingBeginPanicTime = fallingPanic_SpeedTreshold_beginLength_panicLength.y;
		currentDoll.flyingPanicLength = fallingPanic_SpeedTreshold_beginLength_panicLength.z;
		currentDoll.fallingPanicProbability = fallingPanicProbability;
		currentDoll.dropPanic = dropPanic;
		currentDoll.dropPanicSpeedTreshold = dropPanic_SpeedTreshold_beginLength_panicLength.x;
		currentDoll.dropBeginPanicTime = dropPanic_SpeedTreshold_beginLength_panicLength.y;
		currentDoll.dropPanicLength = dropPanic_SpeedTreshold_beginLength_panicLength.z;
		currentDoll.dropPanicProbability = dropPanicProbability;

		currentDoll.playHitMoans = playHitMoans;
		currentDoll.hitMoanInitialWait = hitMoanInitialWait;

		currentDoll.transform.position = new Vector3(0, 0.04500008f, -5.17f);

		currentDoll.SetContinueSetRenderQValue(3001);

		if (startPositions.Count > 0)
		{
			int posIndex = Random.Range(0, startPositions.Count);
			Vector3 startPos = startPositions[posIndex];

			currentDoll.transform.position = startPos;
		}

		if (showTrails)
		{
			if (currentDoll.extra_particleTrail != null)
				currentDoll.SetParticleTrails(currentDoll.extra_particleTrail);
			else if (trailParticlePrefab != null)
				currentDoll.SetParticleTrails(trailParticlePrefab);
			else
				currentDoll.SetTrails(new HumanBodyBones[] { HumanBodyBones.LeftHand, HumanBodyBones.RightHand }, trailPrefab);
		}

		swipeReader.Reset();
		swipeReader.SetDollPos(currentDoll.transform.position, cameraOrgPos, cameraOrgRotation); // in case we are moving the camera back with a tween, ensure that camera is temporarily set back to org pos when setting swipe pin pos (by sending in the last two params)


		if (!extrasApplied)
		{
			if (maxDolls != 0)
			{
				maxDolls += currentDoll.extra_extraDollsPerLevel;
				ShowDollsCount();
			}
			extrasApplied = true;
		}

		ShowDollsCount();

		Time.timeScale = customTimeScale;

		if (noToss == true)
		{
			swipeReader.enabled = false;
			Invoke("Drop", 0.01f);
		}

		if (showTossIndicators)
			Indicate(0, 0, 1);

		UpdateTossNofText();

		nextDollButton.SetActive(false);

		if (dollProgressionActivation != null && maxDollCounter - 1 < dollProgressionActivation.Length)
		{
			dollProgressionActivation[maxDollCounter - 1].SetActive(true);
		}

		if (initialPlayerDeltaZ != 0 && maxDollCounter > 1)
		{
			Vector3 targetPos = currentDoll.transform.position;
			currentDoll.transform.position = GameUtil.AddZ(currentDoll.transform.position, initialPlayerDeltaZ);
			LeanTween.moveZ(currentDoll.gameObject, targetPos.z, 0.3f); // .setEase(LeanTweenType.easeOutCirc);
		}

		InitHmmDelay();

		if (dollFollower != null)
		{
			Transform t = currentDoll.transform.Find("Root");
			if (t != null)
				dollFollower.SetFollowThis(t.gameObject);
		}

		if (StaticManager.testTrackingDoNotEnable)
		{
			ObjectTracker[] trackers = FindObjectsOfType<ObjectTracker>();
			Transform rt = currentDoll.transform.Find("Root"); Rigidbody trackRb = rt.gameObject.GetComponent<Rigidbody>();
			if (trackers != null && trackers.Length > 0) foreach (ObjectTracker ot in trackers) { ot.trackTarget = currentDoll.gameObject; ot.followIfPhysicsEnabled = trackRb; }
			currentDoll.PlayAnim("DoNothing");
		}
	}

	void ChangeShadowDistance()
	{
		QualitySettings.shadowDistance = shadowDistanceAfterToss;
	}


	void SwipePrepare()
	{
		swipeReader.pinModZ = progressivePinModZ;
		swipeReader.SetDollPos(currentDoll.transform.position);
		swipeReader.enabled = true;
	}

	void NewToss()
	{
		if (levelFinished) {
			return;
		}

		tossCounter++;

		LeanTween.move(Camera.main.gameObject, new Vector3(currentDoll.GetRootPos().x + cameraDistanceBeforeNewToss.x, currentDoll.GetRootPos().y + cameraDistanceBeforeNewToss.y, currentDoll.GetRootPos().z + cameraDistanceBeforeNewToss.z), nextTossZoomTime).setEaseInOutSine();
		LeanTween.rotate(Camera.main.gameObject, cameraRotationBeforeNewToss, nextTossZoomTime).setEaseInOutSine();

		swipeReader.Reset();
		Invoke("SwipePrepare", nextTossZoomTime + 0.1f);

		init = true;
		currentDoll.Reset();
		currentDoll.SetActive(true);

		currentDoll.StopGhostAnim();

		if (showTossIndicators)
			Indicate(0, 0, 1);

		if (showNextButtonBeforeProgressiveTosses && tossCounter > 1 && noNextButton == false) {
			nextDollButton.SetActive(true);
		}

		UpdateTossNofText();

		InitHmmDelay();
	}

	public void NextButton()
	{
		//currentDoll.SetInactive ();  // this was used before bubbling was invented
		currentDoll.Bubble(bubblePrefab, bubbleButtonBubbleDelayRapidFire);

		SoundManager.instance.PlaySingleSfx(SingleSfx.Bubble, true, false, 0, 0.6f);

		NewDoll();
		if (progressiveLevel)
			resetGoals();
	}

	private void restoreFixedTimeStep()
	{
		StaticManager.RestoreTimeStep(timeStepAffectIndex);
	}

	private float forceTemp, angleTemp;
	private bool enableMinimumZForceTemp;

	void Toss()
	{
		StaticManager.comboTossGoalsHit = 0;
		StaticManager.IncreaseTossCount(1);
		EvaluateTosses();
		if (currentDoll.extra_timeScale < 0.99f || currentDoll.extra_timeScale > 1.01f)
			Time.timeScale = currentDoll.extra_timeScale;

		if (slideSoundEmitter != null)
			slideSoundEmitter.PlaySound();

		currentDoll.Toss(forceTemp, angleTemp, 0, enableMinimumZForceTemp, gravityMode);
		// Debug.Log ("TOSS: " + force.pos + "f, " + angle.pos + "f");

		if (shadowDistanceAfterToss > 0)
		{
			Invoke("ChangeShadowDistance", 0.5f);
		}

		if (initialLoweredTimeStepTime > 0)
		{
			timeStepAffectIndex = StaticManager.PushFixedTimeStep(initialLoweredTimeStepValue);
			Invoke("restoreFixedTimeStep", initialLoweredTimeStepTime);
		}
	}

	void Drop()
	{
		StaticManager.comboTossGoalsHit = 0;

		if (currentDoll.extra_timeScale < 0.99f || currentDoll.extra_timeScale > 1.01f)
			Time.timeScale = currentDoll.extra_timeScale;

		currentDoll.Drop(gravityMode);
		tossCamTimer = 0;

		if (initialLoweredTimeStepTime > 0)
		{
			timeStepAffectIndex = StaticManager.PushFixedTimeStep(initialLoweredTimeStepValue);
			Invoke("restoreFixedTimeStep", initialLoweredTimeStepTime);
		}
	}

	public void ForceInstantLevelDeath()
	{
			ShowLevelFailed();

	}

	void Indicate(float forceval, float angleval, float angleMod, bool setOld = false)
	{
		float anglevalcalc = -(angleval / angleMod) * 1.2f;

		if ((int)Camera.main.transform.rotation.eulerAngles.z == 180)
			anglevalcalc = -anglevalcalc;

		if (forceIndicator != null)
			forceIndicator.localScale = GameUtil.SetY(forceIndicator.localScale, forceval * 0.56f);

		if (angleIndicator != null)
		{
			angleIndicator.localScale = GameUtil.SetY(angleIndicator.localScale, anglevalcalc * 0.56f);
		}
	}

	void Update()
	{
		if (restartButtonTimer > 0)
		{
			restartButtonTimer -= Time.unscaledDeltaTime;
			if (restartButtonTimer <= 0)
			{
				EnableRestartButton();
				restartButtonText.text = "";
			}
			else
			{
				if (StaticManager.HaveUnlimitedReset())
					restartButtonText.text = "";
				else
					restartButtonText.text = (int)(restartButtonTimer + 1) + "";
			}
		}

		if (dollsTossedText.transform.parent.gameObject.activeSelf == false && goalsHitText.transform.parent.gameObject.activeSelf == false && timerText.transform.parent.gameObject.activeSelf == false)
			dollsTossedText.transform.parent.parent.gameObject.SetActive(false);

		if (init)
		{
			if (swipeReader.WasFlicked())
			{
				float animSpeed = (0.5f + swipeReader.GetYForce() / 1.8f) * 1.9f;
				if (animSpeed < 0.5f)
					animSpeed = 0.5f;

				currentDoll.transform.rotation = Quaternion.Euler(0, cameraOrgRotation.eulerAngles.y, 0);

				currentDoll.PlayAnim("Jump", animSpeed);

				forceTemp = swipeReader.GetYForce();
				angleTemp = swipeReader.GetXForce();
				// print ("Toss: " + forceTemp + " " + angleTemp);
				enableMinimumZForceTemp = false;

				if (showTossIndicators)
					Indicate(swipeReader.GetSpeed(), angleTemp, 1, true);

				Invoke("Toss", 0.4f / animSpeed); // look at the anim file to see how long this should be (0.7f original)

				init = false;
				tossCamTimer = 0;

			}
			else if (showTossIndicators)
			{
				if (swipeReader.IsOnGoing())
				{
					forceTemp = swipeReader.GetYForce();
					angleTemp = swipeReader.GetXForce();
					Indicate(swipeReader.GetSpeed(), angleTemp, 1);
				}
				else
				{
					Indicate(0, 0, 1);
				}
			}
		}

		if (temporaryCredits != StaticManager.GetTemporaryCredits() || StaticManager.creditWasAdded) {
			temporaryCredits = StaticManager.GetTemporaryCredits();

			if (creditsText != null)
				creditsText.text = "" + (StaticManager.GetNumberOfCredits() + temporaryCredits);
		}

		if (currentDoll.WasTossed())
		{
			tossCamTimer += Time.deltaTime;

			float camLerp = Mathf.Clamp01((tossCamTimer - camFollowDelay) / 3.0f);

			if (!currentDoll.IsGettingUp() && moveCamera && levelResults.stars != 0)
			{
				swoopCam.transform.position = new Vector3(Mathf.Lerp(swoopCam.transform.position.x, currentDoll.GetRootPos().x + cameraDistanceAfterToss.x, camLerp), Mathf.Lerp(swoopCam.transform.position.y, currentDoll.GetRootPos().y + cameraDistanceAfterToss.y, camLerp), Mathf.Lerp(swoopCam.transform.position.z, currentDoll.GetRootPos().z + cameraDistanceAfterToss.z, camLerp));
				if (tossCounter == 1)
					swoopCam.transform.rotation = Quaternion.Euler(new Vector3(Mathf.Lerp(cameraOrgRotation.eulerAngles.x, cameraRotationAfterToss.x, camLerp), Mathf.Lerp(cameraOrgRotation.eulerAngles.y, cameraRotationAfterToss.y, camLerp), Mathf.Lerp(cameraOrgRotation.eulerAngles.z, cameraRotationAfterToss.z, camLerp)));
				else
					swoopCam.transform.rotation = Quaternion.Euler(new Vector3(Mathf.Lerp(cameraRotationBeforeNewToss.x, cameraRotationAfterToss.x, camLerp), Mathf.Lerp(cameraRotationBeforeNewToss.y, cameraRotationAfterToss.y, camLerp), Mathf.Lerp(cameraRotationBeforeNewToss.z, cameraRotationAfterToss.z, camLerp)));
			}

			if (!noNextButton && !levelFinished)
			{
				nextDollButton.SetActive(true);
				Animator nxA = nextDollButton.GetComponent<Animator>();
				nxA.Play("look_at_me_Doll");
			}
		}
		else
		{
			if (progressiveLevel && tossCounter > 1 && noNextButton == false)
				nextDollButton.SetActive(true);

			if (!levelFinished)
			{
				if (hmmDelay > 0)
				{
					hmmDelay -= Time.deltaTime;
					if (hmmDelay < 0)
					{
						Hmm();
						InitHmmDelay();
					}
				}
			}
		}

		foreach (ToonDollHelper th in ragdolls)
		{
			int goalHit = th.HasHitGoal();
			if (goalHit != -1)
			{
				bool allClear = UpdateGoalStatus();
				if (allClear)
				{
					ShowLevelCleared();
				}
			}
		}

		if (currentDoll.IsReadyForNext())
		{

			if (awaitStopObjects != null)
			{
				foreach (GameObject g in awaitStopObjects)
				{
					Rigidbody rb = g.GetComponent<Rigidbody>();
					if (rb != null && rb.velocity.sqrMagnitude > 2)
						return;
				}
			}

			if (repeatSlideSound)
				slideSoundEmitter.StopPlay();

			if (!progressiveLevel)
				nextDollButton.SetActive(false);

			if (autoBubble && !currentDoll.WasBubbled())
			{
				currentDoll.Bubble(bubblePrefab);  //SoundManager.instance.PlaySingleSfx (SingleSfx.Bubble, true, false, 0, 0.05f);
			}

			if (!progressiveLevel)
			{
				NewDoll(true);
				resetGoals();
			}
			else
			{
				if (currentDoll.IsOutOfBounds())
				{
					resetGoals();
					NewDoll(true);
				}
				else
				{
					bool requirementsOk = true;

					if (allGoals != null)
					{
						foreach (Goal g in allGoals)
						{
							if (g.HasFailedRequiredToss(currentDoll.GetTossNumber()))
							{
								requirementsOk = false;
								resetGoals();
							}
						}
					}

					if (requirementsOk)
						NewToss();
					else
					{
						NewDoll(true);
					}
				}
			}
		}

		if (timer > 0)
		{
			if (timerAwaitToss == false || currentDoll.WasTossed())
			{
				timer -= Time.deltaTime;
				if (timer < 0) {
					timer = 0;
					ShowLevelCleared(3);
				}
				UpdateTimerText();
			}
		}
	}

	private void resetGoals()
	{
		bool didReset = false;

		if (allGoals == null)
			return;

		foreach (Goal g in allGoals)
		{
			if (g.resetOnMissedGoal)
			{
				g.ResetGoal();
				didReset = true;
			}
		}
		if (didReset)
			UpdateGoalStatus();
	}
	private void CalculateCollectedStars() {
		int worldIndex = StaticManager.GetWorldIndex();
		bool bSavePrefs = false;
		StaticManager.ProcessProgress(levelResults);
		int nofMax = StaticLevels.levelsPerWorld[worldIndex] * 3;
		//	int nofMax = StaticManager.GetNofStars(worldIndex);
		int collected = StaticManager.GetNofStars(worldIndex);
		if (collected == nofMax)
		{
			bSavePrefs = true;
			if (PlayerPrefs.HasKey("CollectedAllStars" + worldIndex) == false)
			{
				PlayerPrefs.SetInt("CollectedAllStars" + worldIndex, 1);

				StaticTaskManager.RouteTask(StaticTaskManager.TaskType.Other, 1, "Star" + worldIndex, true);
			}
		}
		if (bSavePrefs)
		{
			PlayerPrefs.Save();
		}
		StaticManager.Save();
	}

	private void ToProgress()
	{
		if (StaticManager.playingFreeLevel)
			SceneManager.LoadScene("WorldSelect");
		else
		{
			CalculateCollectedStars();	
			SceneManager.LoadScene("LevelProgress");
		}
	}

	private int CalculateStars() {
		int stars = 2;

		if (levelScoreType == LevelScoreType.NofDollsUsed)
		{
			if (star1req < 0)
				star1req = maxDolls;
			if (star3req < 0)
				star3req = requiredGoalHits;

			if (!currentDoll.WasTossedOnce())
				maxDollCounter--;
			if (maxDollCounter <= star3req)
				stars = 3;
			else if (maxDollCounter >= star1req)
				stars = 1;
		}
		else if (levelScoreType == LevelScoreType.NofTosses)
		{
			if (tossCounter <= star3req)
				stars = 3;
			else if (tossCounter >= star1req)
				stars = 1;
		}
		else if (levelScoreType == LevelScoreType.NofGoalsHit)
		{
			if (star1req < 0)
				star1req = 1;
			if (star3req < 0)
				star3req = requiredGoalHits;

			int goalsHit = GetNofGoalsHit();

			if (goalsHit >= star3req)
				stars = 3;
			else if (goalsHit <= star1req)
				stars = 1;
		}

		stars = Mathf.Clamp(stars + currentDoll.extra_extraStarsPerLevel, 0, 3);

		return stars;
	}

	private void EvaluateTosses() {
		StaticTaskManager.EvaluateAll(StaticTaskManager.TaskType.CharacterThrow, StaticManager.GetTotalTossCount());
	}
	private void ShowLevelCleared(int forcedStars = -1, int cLevel = -1, int wIndex = -1)
	{
		bool dbg = false;
		if (levelFinished)
			return;
		
		if(cLevel == -1) {
			cLevel = StaticManager.GetLevel();

		} else {
			dbg = true;
		}

		if(wIndex== -1) {
			wIndex = StaticManager.GetWorldIndex();
		}
		float gameDuration = Time.time - gameTime; // Debug.Log("Game took " + gameDuration);
		StaticTaskManager.RouteTask(StaticTaskManager.TaskType.Speedy, (int)gameDuration, null);

		levelResults.score = 0;
		levelResults.time = float.MaxValue;
		if (forcedStars == 1 || forcedStars == 2 || forcedStars == 3)
			levelResults.stars = forcedStars;
		else
			levelResults.stars = CalculateStars();
	
		int worldIndex = wIndex;
	
		if (levelResults.stars == 3)
		{
			int numberOfThreeStars = StaticManager.AddThreeStars(cLevel,worldIndex);
			StaticTaskManager.EvaluateAll(StaticTaskManager.TaskType.ThreeStars, numberOfThreeStars);
			StaticTaskManager.RouteTask(StaticTaskManager.TaskType.Other, numberOfThreeStars, "Star"+worldIndex);

		}
		if (!dbg)
		{
			soundManager.PlayRandomFromType(SfxRandomType.Cheering);
			soundManager.PlayRandomFromType(currentDoll.soundYesWin, -1, 0.2f + (celebrateVictory ? celebrationStartTime : 0), -1, currentDoll.soundPitch);

			currentDoll.SetLevelFinished(celebrateVictory, celebrationStartTime, celebrationRiseSpeed);

			levelFinished = true;
		}

		bool bSavePrefs = false;
		if (levelResults.stars >= 1)
		{
			
			if (PlayerPrefs.HasKey("LevelUnlock_" + worldIndex + "_l_" + cLevel) == false)
			{
				bSavePrefs = true;
				int collectedStarsForWorld = 0;
				if(PlayerPrefs.HasKey("WorldStars_"+worldIndex) == false) {
					PlayerPrefs.SetInt("WorldStars_" + worldIndex,0);
				}
				collectedStarsForWorld = PlayerPrefs.GetInt("WorldStars_" + worldIndex);
				collectedStarsForWorld = collectedStarsForWorld + 1;
			//	Debug.Log("Current collected World stars" + collectedStarsForWorld);
				if (worldIndex == 0)
				{
					StaticTaskManager.RouteTask(StaticTaskManager.TaskType.Other, collectedStarsForWorld, "LevelUnlock1",false,true);
				}
				else if (worldIndex == 1)
				{
					StaticTaskManager.RouteTask(StaticTaskManager.TaskType.Other, collectedStarsForWorld, "LevelUnlock2", false, true);
				}
				PlayerPrefs.SetInt("LevelUnlock_" + worldIndex + "_l_" + cLevel, 1);
				PlayerPrefs.SetInt("WorldStars_" + worldIndex, collectedStarsForWorld);
				PlayerPrefs.Save();
			}

		}
	

		nextDollButton.SetActive(false);

		if (eventSystem != null)
			eventSystem.gameObject.SetActive(false);

		if (fireWorkPrefab != null && !StaticManager.HasfireWorksBeenShown()) {
			CalculateCollectedStars();
			Invoke ("ShowFireWorks", 0.95f + (celebrateVictory ? celebrationStartTime : 0));
		} else
			Invoke ("ToProgress", 0.95f + (celebrateVictory? celebrationStartTime : 0));
		if(bSavePrefs)
			PlayerPrefs.Save();
		DisableColliders();
	}

	private void ShowFireWorks()
	{
		if (StaticManager.UGLYFIX_IsLevelType(StaticManager.UGLYFIX_zombieWorldIndex))
		{
			LeanTween.move(Camera.main.gameObject, new Vector3(0, 21.91f, -35.93f), 3f).setEaseInOutSine();
			LeanTween.rotate(Camera.main.gameObject, new Vector3(36.9f, 0, 0), 3f).setEaseInOutSine();
		}
		else
		{
			LeanTween.move(Camera.main.gameObject, new Vector3(0, 30.8f, 36f), 3f).setEaseInOutSine();
			LeanTween.rotate(Camera.main.gameObject, new Vector3(40.3f, 0, 0), 3f).setEaseInOutSine();
		}

		GameObject firstFire = Instantiate(fireWorkPrefab, new Vector3(-0.6f, 0.53f, 20.79f), Quaternion.identity);
		Instantiate(fireWorkPrefab, new Vector3(3.38f, 0.53f, -9.47f), Quaternion.identity);
		Instantiate(fireWorkPrefab, new Vector3(-2.28f, 0.53f, -27.92f), Quaternion.identity);

		firstFire.transform.Find("HelpDisplayer").gameObject.SetActive(true);

		Invoke("ToProgress", StaticManager.UGLYFIX_IsLevelType(StaticManager.UGLYFIX_zombieWorldIndex)? 18f : 10f);
	}


	private void ShowLevelFailed(bool playSound = false) {
		if (levelFinished)
			return;

		if (noMoreDollsSound != SingleSfx.None && playSound) {
			soundEmitter.SetSingleSfx (noMoreDollsSound);
			soundEmitter.PlaySound ();
		}

		levelResults.score = 0;
		levelResults.time = float.MaxValue;
		levelResults.stars = 0;

		levelFinished = true;
	
		if (eventSystem != null)
			eventSystem.gameObject.SetActive (false);

		Invoke ("ToProgress", 1.0f);

		DisableColliders();
	}


	private void DisableColliders() {
		if (disableOnFinish == null || disableOnFinish.Length < 1)
			return;
		foreach(Collider coll in disableOnFinish) {
			coll.enabled = false;
		}
	}

	private Color[] starIndicateColor = new Color[] { GameUtil.IntColor(255, 255, 255), GameUtil.IntColor(255, 192, 192), GameUtil.IntColor(255, 255, 192), GameUtil.IntColor(192, 255, 192) };

	private void ShowDollsCount()
	{
		dollsTossedText.gameObject.SetActive(true);
		dollsTossedText.transform.parent.gameObject.SetActive(true);
		dollsTossedText.text = maxDollCounter + " / " + maxDolls;

		// This is pretty much exactly the same calulation as CalculateStars() but calling that leads to subtle issues I don't want to deal with...

		int stars = 2;

		if (requiredGoalHits == -1)
			requiredGoalHits = GetNofRequiredGoalsHit();

		if (levelScoreType == LevelScoreType.NofDollsUsed)
		{
			if (star1req < 0)
				star1req = maxDolls;
			if (star3req < 0)
				star3req = requiredGoalHits;

			if (maxDollCounter <= star3req)
				stars = 3;
			else if (maxDollCounter >= star1req)
				stars = 1;
		}
		else if (levelScoreType == LevelScoreType.NofTosses)
		{
			if (tossCounter <= star3req)
				stars = 3;
			else if (tossCounter >= star1req)
				stars = 1;
		}
		else if (levelScoreType == LevelScoreType.NofGoalsHit)
		{
			if (star1req < 0)
				star1req = 1;
			if (star3req < 0)
				star3req = requiredGoalHits;
			
			int goalsHit = GetNofGoalsHit();

			if (goalsHit >= star3req)
				stars = 3;
			else if (goalsHit <= star1req)
				stars = 1;
		}

		if (currentDoll != null)
			stars = Mathf.Clamp(stars + currentDoll.extra_extraStarsPerLevel, 0, 3);

		dollsTossedText.color = starIndicateColor[stars];
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
		if (StaticManager.testTrackingDoNotEnable) hmmDelay = 1000000f;
	}

	private void Hmm() {
		currentDoll.Hmm ();
	}

}
