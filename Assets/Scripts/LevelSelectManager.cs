using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectManager : InvokeWithObject {

	public GameObject levelItemPrefab;
	public GameObject levelContainer;
	public GameObject newCharEffect = null;

	void Awake() {
		Application.targetFrameRate = 60;
		SoundManager.Create ();
	}

	void RestoreElasticity(System.Object o) {
		GameObject.FindObjectOfType<ScrollRect> ().elasticity = (float)o;
	}

	void Start () {
		StaticManager.LevelData [] levels = StaticManager.GetAllLevelsForWorld ();

		int nofStars = StaticManager.GetNofStars();
		int currentLevel = StaticManager.GetCurrentLevelForWorld ();

		for (int i = 0; i < levels.Length; i++) {
			GameObject levelItem = Instantiate (levelItemPrefab, levelContainer.transform, false);

			if (i == currentLevel) {
				Image img = levelItem.GetComponent<Image> ();
				if (img != null)
					img.color = GameUtil.IntColor (76, 81, 0);
			}

			Transform levelTextTransform = levelItem.transform.Find ("LevelText");
			if (levelTextTransform != null) {
				Text levelText = levelTextTransform.gameObject.GetComponent<Text> ();
				if (levelText != null) {
					levelText.text = "" + (i + 1);
				}
			}

			for (int j = 0; j < 3; j++) {
				Transform star = GameUtil.FindDeepChild(levelItem.transform, "Star" + (j + 1));
				if (star != null) {
					Image starImage = star.gameObject.GetComponent<Image> ();
					if (starImage != null) {
						starImage.color = (j < levels[i].stars)? Color.white : Color.black;
					}
				}
			}

			int jewelsPicked = StaticManager.GetMoneyMakersTaken (i);
			for (int j = 0; j < 3; j++) {
				Transform jewel = GameUtil.FindDeepChild(levelItem.transform, "Jewel" + (j + 1));
				if (jewel != null) {
					Image jewelImage = jewel.gameObject.GetComponent<Image> ();
					if (jewelImage != null) {
						jewelImage.color = (j < jewelsPicked)? Color.white : Color.black;
					}
				}
			}

			Transform myLock = GameUtil.FindDeepChild(levelItem.transform, "Lock");

			Button button = levelItem.gameObject.GetComponent<Button> ();
			if (StaticManager.showAllLevelsDebug || (i == 0 || nofStars >= StaticManager.GetNofRequiredStars(i))) {
				if (button != null) {
					int index = i;
					button.onClick.AddListener (() =>
						{
							StartLevel (index);
						}); // Have to create index from i, otherwise it always thinks "i" is 3 (i.e. at end of loop), because the listener gets the int Object, not the value
				}

				if (myLock != null)
					myLock.gameObject.SetActive(false);

			} else {
				if (button != null) {
					button.interactable = false;
				}

				if (myLock != null) {
					Text text = myLock.GetComponentInChildren<Text>();
					if (text != null)
						text.text = "" + StaticManager.GetNofRequiredStars(i);
				}

			}
		}

		GameObject starsCount = GameObject.Find("NofWorldStars");
		if (starsCount != null) {
			Text text = starsCount.GetComponentInChildren<Text>();
			if (text != null)
				text.text = "" + nofStars;
		}


		int NumberOfColumns = 1;
		GridLayoutGroup glg = levelContainer.GetComponent<GridLayoutGroup>();
		float height = glg.cellSize.y + glg.spacing.y;
		if (glg.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
		{
			NumberOfColumns = glg.constraintCount;
		}
		RectTransform contentContainer = levelContainer.GetComponent<RectTransform>();
		float contentHeight = (levels.Length / NumberOfColumns) * height;
	

		if( levels.Length % NumberOfColumns !=0)
		{
			contentHeight += height;
		}
		contentContainer.sizeDelta = new Vector2(contentContainer.sizeDelta.x, contentHeight);

		if (currentLevel >= 0) {

			ScrollRect sr = GameObject.FindObjectOfType<ScrollRect> ();
			if (sr != null) {
				InvokeWO(RestoreElasticity, 0.1f, sr.elasticity);
				sr.elasticity = 0;
			}

			int beginRow = currentLevel / NumberOfColumns;

			// beginRow -= 3; if (beginRow < 0) beginRow = 0;

			Canvas.ForceUpdateCanvases();
			contentContainer.anchoredPosition = new Vector2 (contentContainer.anchoredPosition.x, height * beginRow);
		}

		if (newCharEffect && StaticManager.IsNewCharPossibleToPurchase())
			newCharEffect.SetActive(true);
	}
	
	void Update () {
	}

	public void GotoMainScreen() {
		SceneManager.LoadScene (StaticManager.MAIN_SCENE);
	}
	public void GotoCharacterSelectScreen()
	{
		StaticManager.PushScene();
		SceneManager.LoadScene("CharSelect");
	}
	public void GotoWorldScreen() {
		SceneManager.LoadScene ("WorldSelect");
	}

	public void StartLevel(int index) {
		if (StaticManager.usePlayLevelScreen) {
			StaticManager.SetLevel (index);
			SceneManager.LoadScene ("PlayLevel");
		} else
			StaticManager.StartLevel (index);
	}

}
