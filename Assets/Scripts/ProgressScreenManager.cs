using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProgressScreenManager : MonoBehaviour {

	public Text headerText;
	public Text messageText;
	public Text levelText;
	public Animator starAnimator;
	public Animator jewelAnimator;
	public GameObject betterContainer;
	public GameObject worseContainer;
	public GameObject neverWonContainer;
	public Image [] worseStarImages;
	public Image [] jewelImages;
	public Text nofStarsText;

	public Image [] starImages;
	public Button nextLevelButton;

	private StaticManager.LevelData results, oldResults;
	private ToonDollHelper currentPlayer;
	private bool firstRun = true;

	private CharacterManager characterManager = null;

	public GameObject dummyContainer;

	private bool [] starTriggered;
	private bool[] jewelTriggered;

	private SingleSfx starSfx = SingleSfx.ProgressStarWon;
	private SingleSfx jewelSfx = SingleSfx.GetMoney;

	private int remainingStars;

	public GameObject firstBuy;

	public GameObject [] zombies;
	private bool showZombie = false, gotJewels = false;

	public GameObject newCharEffect;
	public GameObject popup;
	public GameObject zombiePopup;

	void Awake() {
		Application.targetFrameRate = 60;
		SoundManager.Create ();
	}
	void HideProgressContainers() {
		betterContainer.SetActive(false);
		worseContainer.SetActive(false);
		neverWonContainer.SetActive(false);
	}
	public void ShowZombiePopup() 
	{
		Popup p = zombiePopup.GetComponent<Popup>();
		if (p != null && !StaticManager.showAllLevelsDebug)
		{

			string str = I2.Loc.LocalizationManager.GetTranslation("Christmas promo Buy zombiepack");
			p.localizeStrings = false;
			p.ShowYesNo(NotifyZombiePurchase, false, str);
			HideProgressContainers();

		}
	}

	void Start () {
		characterManager = CharacterManager.instance;

		results = StaticManager.GetPlayResults ();
		oldResults = StaticManager.GetOldPlayResults ();

		string levelString = StaticManager.GetLevelString ();

		levelText.text = levelString;

		if (nofStarsText)
			nofStarsText.text = "" + StaticManager.GetNofStars();

		starAnimator.SetInteger ("Stars", results.stars);

		int newJewelsTaken = Mathf.Clamp(StaticManager.nofNewJewelsTaken, 0, 3);
		if (newJewelsTaken > 0) gotJewels = true;
		//print("New jewels: " + newJewelsTaken);

		starTriggered = new bool[starImages.Length];
		for (int i = 0; i < starImages.Length; i++) starTriggered [i] = false;
			
		jewelTriggered = new bool[jewelImages.Length];
		for (int i = 0; i < jewelImages.Length; i++) jewelTriggered[i] = false;

		StaticManager.LevelData savedLevelData = StaticManager.GetCurrentLevelData ();

		bool isNextLevelAvailable = StaticManager.IsNextLevelAvailable(out remainingStars);


		bool endOfChristmasLevels = StaticManager.AtWorldsLastLevel();
		HideProgressContainers();
		if(endOfChristmasLevels && StaticManager.GetWorldIndex() == 2 && StaticManager.WorldPurchased(1) == false) {
			Invoke("ShowZombiePopup", 1.0f);
			return;

		}
		if (!isNextLevelAvailable && StaticManager.showAllLevelsDebug == false) {
			string collectmessage = GetCollectMessage();
			popup.GetComponent<Popup>().SetText(collectmessage);
			if (remainingStars < 1)
				nextLevelButton.gameObject.SetActive(false);
			else {
				var buttColors = nextLevelButton.colors;
				buttColors.normalColor = GameUtil.IntColor(197, 197, 197);
				buttColors.highlightedColor = GameUtil.IntColor(197, 197, 197);
				buttColors.pressedColor = GameUtil.IntColor(220, 220, 220);
				nextLevelButton.colors = buttColors;
			}

		}
		//nextLevelButton.gameObject.SetActive(StaticManager.IsNextLevelAvailable() || StaticManager.showAllLevelsDebug);



		if (results.stars > oldResults.stars) {
			betterContainer.SetActive (true);
		} else {
			if (results.stars > 0)
				worseContainer.SetActive (true);
			else
				neverWonContainer.SetActive (true);
		}

		for (int i = 0; i < 3; i++) {
			worseStarImages [i].color = i < oldResults.stars ? Color.white : Color.black;
		}

		int nofJewels = StaticManager.GetMoneyMakersTaken (StaticManager.GetLevel ());

		for (int i = 0; i < 3; i++)
		{
			jewelImages[i].color = i < nofJewels ? Color.white : Color.black;
		}
		if (newJewelsTaken > 0) {
			if (jewelAnimator) { jewelAnimator.SetInteger("Jewels", nofJewels); }
		}

		if (StaticManager.ForceBuyNewCharacter()) {
			StaticManager.awaitingFirstBuyStepsLeft = 2;
			firstBuy.SetActive(true);
		}

		if (newCharEffect && StaticManager.IsNewCharPossibleToPurchase())
			newCharEffect.SetActive(true);

	}
	public void NotifyZombiePurchase(Popup.PopupButtonChoice buttonChoice)
	{
		

		if (buttonChoice == Popup.PopupButtonChoice.YES)
		{
			StaticManager.PushScene();
			//AppLovin.ShowRewardedInterstitial();
			SceneManager.LoadScene("BuyCredits");
			return;
		}

	}
	private GameObject character;
	private float aspect;

	void Update () {

		for (int i = 0; i < starImages.Length; i++) {
			if (starImages[i].color.a > 0 && !starTriggered[i]) {
				starTriggered [i] = true;
				SoundManager.instance.PlaySingleSfx (starSfx);
			}
		}

		/*
		for (int i = 0; i < jewelImages.Length; i++)
		{
			if (jewelImages[i].color.a > 0 && !jewelTriggered[i])
			{
				jewelTriggered[i] = true;
				SoundManager.instance.PlaySingleSfx(jewelSfx);
			}
		} */

		if (firstRun) {
			int index = StaticManager.GetSelectedAvatarIndex ();
			if (index < 0)
				index = 0;

			if ((StaticManager.GetWorldIndex() == 1 || StaticManager.GetWorldIndex() == 2) && results.stars == 0)
				showZombie = true;

			character = Instantiate(showZombie? zombies[Random.Range(0, zombies.Length)] : characterManager.characterPrefabs[index]);
			currentPlayer = character.GetComponentInChildren<ToonDollHelper> ();

			ToonDollHelper.RemoveRigidComponents (character, true);

			aspect = 1f; // aspect = (float)Screen.width / (float)Screen.height; aspect /= (9f/16f); if (aspect > 1f) aspect = 1f + (aspect - 1f) / 5f; // (old size change for non-9/16 devices, now fixed in scene somehow)
			if ((float)Screen.width / (float)Screen.height < 0.5f) aspect = 0.9f; // single Iphone X hack instead
				
			character.transform.position = new Vector3 (0f, 0.05f, -5.17f * aspect);
			character.transform.rotation = Quaternion.Euler (0, 180, 0);

			if (currentPlayer != null) {
				currentPlayer.disableRagdoll ();
				currentPlayer.SetKinematic ();

				if (results.stars > 0) {
					currentPlayer.PlayAnim ("ProgressLevelWon");
					currentPlayer.ChangeFace (ToonDollHelper.FaceType.Happy);
				} else {
					currentPlayer.PlayAnim (showZombie? "Gorilla" : gotJewels? "Idle" : "ProgressLevelLost");
					if (!gotJewels) currentPlayer.ChangeFace (ToonDollHelper.FaceType.Sad);
				}

				currentPlayer.blinkTimeExtraRange = 5f;
				currentPlayer.minBlinkTime = 2f;
			}

			firstRun = false;
		}

		if (showZombie)
			character.transform.position = new Vector3(0f, 0.15f, -5.17f * aspect);
	}



	public string GetCollectMessage(){
		string str;
		if (remainingStars > 1)
		{
			str = I2.Loc.LocalizationManager.GetTranslation("You need to collect more stars to play the next level");
			str = str.Replace("0", "" + remainingStars);
		}
		else
		{
			str = I2.Loc.LocalizationManager.GetTranslation("You need to collect 1 more star to play the next level");
		}
		return str;
	}
	public void StartNextLevel() {
		if (remainingStars > 0) {
			string str = GetCollectMessage();

			Popup p = popup.GetComponent<Popup>();
			if (p != null && !StaticManager.showAllLevelsDebug)
			{
				p.localizeStrings = false;

				p.ShowOk(null, false, str);
				HideProgressContainers();

			}
				return;

		}

		if (starSfx != SingleSfx.None) SoundManager.instance.FadeSingleSfx (starSfx);
		StaticManager.StartNextLevel ();
	}

	public void RestartSameLevel() {
		if (starSfx != SingleSfx.None) SoundManager.instance.FadeSingleSfx (starSfx);
		StaticManager.RestartSameLevel ();
	}

	public void GotoMainScreen() {
		if (starSfx != SingleSfx.None) SoundManager.instance.FadeSingleSfx (starSfx);
		SceneManager.LoadScene (StaticManager.MAIN_SCENE);
	}

	public void GotoLevelScreen() {
		if (starSfx != SingleSfx.None) SoundManager.instance.FadeSingleSfx (starSfx);
		SceneManager.LoadScene ("LevelSelect");
	}

	public void SelectCharacter() {
		if (starSfx != SingleSfx.None) SoundManager.instance.FadeSingleSfx (starSfx);

		StaticManager.PushScene();
		StaticManager.LoadCharSelectScene ();
	}

}
