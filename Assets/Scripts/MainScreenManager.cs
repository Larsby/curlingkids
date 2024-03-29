using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainScreenManager : MonoBehaviour
{

	public Button unlockWorldsButton;
	public Button settingsButton;
	public Sprite showSettingsImage;
	public Sprite hideSettingsImage;
	public GameObject settingsPanel;
	public Text creditsText;
	public AdManager adManager;
	private bool showSettings = false;

	public GameObject musicToggleButton;
	public GameObject sfxToggleButton;
	public GameObject buyGameButton;
	private ScreenFader screenFader;
	private ToonDollHelper currentPlayer;

	private bool showCharacter = false, firstRun = true;

	private AsyncOperation aso = null;
	private bool creditsLoaded = false;

	private static MusicTune oldTune;

	public GameObject newCharEffect;
	public Sprite christmas_icn;
	public Sprite zombie_icn;
	public Sprite video_icn;
	void Awake()
	{
		Application.targetFrameRate = 30;
		UniRate.Instance.ShouldUniRateOpenRatePage += ShouldUniRateOpenRatePage;
		UniRate.Instance.ShouldUniRatePromptForRating += ShouldUniRateOpenRatePage;

		SoundManager.Create();
		if (buyGameButton)
			buyGameButton.SetActive(false);

	}


	bool ShouldUniRateOpenRatePage()
	{

		return true;
	}
	void DelayPirateMessage()
	{
		CharacterManager.instance.ShowCoinTaskRewardPopup("HAXOR THE GAME", "Change the playlist for profit", 100);
		StaticManager.AddCredits(100);

		PlayerPrefs.SetInt("nerfz", 1);
		PlayerPrefs.Save();

	}
	public void InitAds()
	{


#if !UNITY_EDITOR
		print("InitADDDS..1");
        AppLovin.SetSdkKey("cQiOh1amPJ0ywVME6E3S8a-vk89aDO0wNwrXM989beVQSsaiqq9EoKn2HJX6-VO3DZgpudHWb2J7RwmNMfTZoA");
	//	AppLovin.SetTestAdsEnabled("YES");
       
		AppLovin.InitializeSdk();
	//	AppLovin.SetTestAdsEnabled("YES");
		AppLovin.LoadRewardedInterstitial();
        //   AppLovin.ShowAd(AppLovin.AD_POSITION_CENTER, AppLovin.AD_POSITION_TOP);
#endif

	}
	void ShowZopmbieDelayed() {
		Popup popup = GameObject.FindObjectOfType<Popup>();
		if (popup != null)
		{
			popup.ShowOk(NotifyZombiePackNotification, false, "zombiepopup", null, null, null, false);
		}
		else
		{
			Debug.Log("Could not find popup");
		}
	}
	void Start()
	{
		if (SoundManager.instance.GetPlayingSong() == MusicTune.Credits)
			SoundManager.instance.PlayRegularSong();
		//			SoundManager.instance.PlayMusic(oldTune);
		InitAds();
		screenFader = FindObjectOfType<ScreenFader>();
		if (creditsText != null)
			creditsText.text = "" + StaticManager.GetNumberOfCredits();

		if (unlockWorldsButton != null)
		{
			if (PlayerPrefs.HasKey("DebugUnlock"))
			{
				unlockWorldsButton.gameObject.SetActive(false);
				StaticManager.showAllLevelsDebug = true;

			}
		}

		Input.multiTouchEnabled = false;

		if (newCharEffect && StaticManager.IsNewCharPossibleToPurchase())
			newCharEffect.SetActive(true);

		if (StaticTaskManager.IsGamePirated() && PlayerPrefs.HasKey("nerfz") == false)
		{
			Invoke("DelayPirateMessage", 2.0f);
		}

		if (StaticManager.WorldPurchased(1) == false && StaticManager.SHOW_ZOMBIE_POPUP)
		{
			
			Invoke("ShowZopmbieDelayed", 2.0f);

		}else if(StaticManager.WorldPurchased(1) == true && StaticManager.SHOW_CHRISTMAS) 
		{
			Invoke("ShowChristmasPopup", 2.0f);
		}


	}


	void Update()
	{

		if (aso != null && aso.progress >= 0.9f && !creditsLoaded)
		{
			oldTune = SoundManager.instance.GetPlayingSong();
			SoundManager.instance.PlayMusic(MusicTune.Credits);
			aso.allowSceneActivation = true;
			creditsLoaded = true;
			EventSystem eventSystem = FindObjectOfType<EventSystem>();
			if (eventSystem != null)
				eventSystem.gameObject.SetActive(false);
		}

		if (firstRun && showCharacter)
		{

			int index = StaticManager.GetSelectedAvatarIndex();
			if (index < 0)
				index = 0;

			GameObject character = Instantiate(CharacterManager.instance.characterPrefabs[index]);
			currentPlayer = character.GetComponentInChildren<ToonDollHelper>();

			ToonDollHelper.RemoveRigidComponents(character, true);

			character.transform.localPosition = new Vector3(10f, 0.6f, -7);
			character.transform.localScale = character.transform.localScale * 1.2f;
			character.transform.localRotation = Quaternion.Euler(0, 150, 0);

			if (currentPlayer != null)
			{
				currentPlayer.disableRagdoll();
				currentPlayer.SetKinematic();
				currentPlayer.PlayAnim("Unlocked");
			}

			firstRun = false;
		}
		creditsText.text = "" + StaticManager.GetNumberOfCredits();
	}


	public void PlayAction()
	{
		if (StaticManager.usePlayLevelScreen)
		{
			StaticManager.SetSceneToLastPlayedScene();
			SceneManager.LoadScene("PlayLevel");
		}
		else
		{
			if (StaticManager.GetLastWorldIndex() >= 0)
				StaticManager.LoadLastPlayedScene();
			else
				SceneManager.LoadScene("WorldSelect");
		}
	}

	private bool fadeOut = false;
	public void PlayButton()
	{
		if (screenFader == null || fadeOut == false)
			PlayAction();
		else
			screenFader.FadeOut(PlayAction);
	}

	public void LevelsButton()
	{
		SceneManager.LoadScene("WorldSelect");
	}


	public void AddCreditsButton()
	{
		StaticManager.PushScene();
		SceneManager.LoadScene("BuyCredits");
	}

	public void CharsButton()
	{
		StaticManager.PushScene();
		StaticManager.LoadCharSelectScene();
	}

	public void ToggleMusicButton()
	{

		StaticManager.ToggleMusic();
		SetButtonState(musicToggleButton, StaticManager.IsMusicOn());
		if (StaticManager.IsMusicOn())
		{
			SoundManager.instance.PlayRegularSong();
		}
		else
			SoundManager.instance.StopMusic();
	}

	public void ToggleMusicForAdExitButton()
	{
		if (StaticManager.IsMusicOn())
		{
			SoundManager.instance.PlayMusic(MusicTune.Regular);
		}
	}

	public void ToggleSfxButton()
	{
		SoundManager.instance.PlaySingleSfx(SingleSfx.Button1, true, true);

		StaticManager.ToggleSfx();
		SetButtonState(sfxToggleButton, StaticManager.IsSfxOn());
	}

	public void SettingsButton()
	{

		showSettings = !showSettings;
		if (showSettings)
		{
			SoundManager.instance.PlaySingleSfx(SingleSfx.Button1, true);
			settingsPanel.SetActive(true);
			settingsButton.image.sprite = hideSettingsImage;
			SetButtonState(sfxToggleButton, StaticManager.IsSfxOn());
			SetButtonState(musicToggleButton, StaticManager.IsMusicOn());

		}
		else
		{
			SoundManager.instance.PlaySingleSfx(SingleSfx.Button2, true);
			settingsPanel.SetActive(false);
			settingsButton.image.sprite = showSettingsImage;
		}
	}

	private void SetButtonState(GameObject obj, bool active)
	{
		int activeIndex = active == true ? 0 : 1;
		Image img = obj.transform.GetChild(1 - activeIndex).gameObject.GetComponent<Image>();
		obj.transform.GetChild(1 - activeIndex).gameObject.SetActive(false);
		img.enabled = false;
		obj.transform.GetChild(activeIndex).gameObject.SetActive(true);
		Image img2 = obj.transform.GetChild(activeIndex).gameObject.GetComponent<Image>();
		img2.enabled = true;
	}

	public void NotifyZombiePackNotification(Popup.PopupButtonChoice buttonChoice)
	{
		StaticManager.SHOW_ZOMBIE_POPUP = false;
		ShowChristmasPopup();


	}
	public void ShowChristmasPopup() {
		Popup popup = GameObject.FindObjectOfType<Popup>();
		if (popup != null)
		{
			popup.ShowOk(null, false, "christmaspopup",null,null,new Sprite[]{christmas_icn},false);
		}
		StaticManager.SHOW_CHRISTMAS = false;
	}

	public void NotifyResetResult(Popup.PopupButtonChoice buttonChoice) {
		if (buttonChoice == Popup.PopupButtonChoice.YES) {
			StaticManager.Load (true);
			StaticTaskManager.Load (true);
			StaticManager.ResetMoneyMakers ();
			PlayerPrefs.DeleteAll();
			Debug.Log ("Reset saved data");
			SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
		}
	}

	public void ResetLevelsButton() {
		Popup popup = GameObject.FindObjectOfType<Popup> ();
		if (popup != null)
			popup.ShowYesNo (NotifyResetResult, false, "Really reset all level progress, characters, and tasks?");
	}

	public void ResetMoneyMakersButton() {
		StaticManager.ResetMoneyMakers ();
		Popup popup = GameObject.FindObjectOfType<Popup> ();
		if (popup != null) {
			popup.ShowOk (NotifyUnlockOk, false, "Moneymaker objects were reset");
		}
	}

	public void NotifySeeVideoResult(Popup.PopupButtonChoice buttonChoice) {
		if (buttonChoice == Popup.PopupButtonChoice.YES) {
			adManager.gameObject.SetActive(true);
			adManager.Setup();

			if (StaticManager.IsMusicOn()) {
				SoundManager.instance.StopMusic();
			}

			creditsText.text = "" + StaticManager.GetNumberOfCredits();
		}
	}

	public void WatchVideoButton() {
		if (StaticManager.VideoAvailableToday())
		{
			Popup popup = GameObject.FindObjectOfType<Popup>();
			if (popup != null)
				popup.ShowYesNo(NotifySeeVideoResult, false, "Watch an ad to receive 10 credits?",null,null,null,new Sprite[]{video_icn});
		}
	}

	public void BuyZombieLevel() {
		Purchaser p = gameObject.GetComponent<Purchaser>();
		if(p) {
			p.BuyNonConsumable();
		}
	}

	void NotifyUnlockOk(Popup.PopupButtonChoice buttonChoice) {
	}

	public void ShowTasksButton() {
		StaticManager.PushScene ();
		SceneManager.LoadScene ("TaskList");
	}
    public void Rate()
    {
        Debug.Log("Rating selected.");
#if UNITY_IOS
		Application.OpenURL("https://itunes.apple.com/app/id1294998064");
#endif

		Application.OpenURL("https://play.google.com/store/apps/details?id=se.pastille.curlingbuddies");
       // UniRate.Instance.ShowPrompt();
#if UNITY_ANDROID

        #endif

    }
	public void VisitPastille()
	{
		Application.OpenURL("http://www.pastille.se");
	}

	public void SeeCredits() {
		Application.targetFrameRate = 60;
		aso = SceneManager.LoadSceneAsync("Credits");
		aso.allowSceneActivation = false;
	}

	public void PlayXMasLevels()
	{
		StaticManager.GotoLevelSelectScreen(2);
	}

}
