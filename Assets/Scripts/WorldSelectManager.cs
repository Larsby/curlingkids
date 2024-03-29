using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldSelectManager : MonoBehaviour {

	public GameObject worldItemPrefab;
	public GameObject worldContainer;
	public Text acquiredStarsText;
	public GameObject[] worldTemplates;
	void Awake() {
		Application.targetFrameRate = 60;
		SoundManager.Create ();
	}

	void Start()
	{
		StaticManager.playingFreeLevel = false;

		int worldsLength = worldTemplates.Length;
		bool showFreeLevel = true;
		for (int i = 0; i < worldsLength; i++)
		{
			GameObject worldItem = null;
			if (worldTemplates != null && i < worldTemplates.Length)
			{
				worldItem = worldTemplates[i];
				worldItem.transform.parent = worldContainer.transform;
			}
			else
			{
				worldItem = Instantiate(worldItemPrefab, worldContainer.transform, false);

				Transform levelTextTransform = worldItem.transform.Find("WorldText");
				if (levelTextTransform != null)
				{
					Text worldText = levelTextTransform.gameObject.GetComponent<Text>();
					if (worldText != null)
					{
						worldText.text = "" + (i + 1);
					}
				}
			}
			int worldIndex = worldItem.GetComponent<WorldIndex>().index;
			Transform requiredStarsTranform = worldItem.transform.Find("RequiredStars");

			Transform nofStarsTransform = worldItem.transform.Find("NofStars");
			Transform nofJewelsTransform = worldItem.transform.Find("NofJewels");
			bool worldPurchased = StaticManager.WorldPurchased(i);



			int nofMax = StaticLevels.levelsPerWorld[worldIndex] * 3;
			int noStars = StaticManager.GetNofStars(worldIndex);
			int noJewel = StaticManager.GetMoneyMakersTakenPerWorld(worldIndex);

			/*
			Button button = worldItem.gameObject.GetComponent<Button>();

			if (button != null)
			{
				int index = i;
				if (showFreeLevel && index == 2)
					index = 1;
				if (StaticManager.WorldPurchased(index))
				{
					//StaticManager.WorldPurchased(i);
					button.onClick.AddListener(() =>
					   {
						 
						   GotoLevelSelect(index);
					   });
				} else {
					button.onClick.AddListener(() =>
					{

						GoToBuyCredits();
					});

				}
			}*/

			if (nofStarsTransform != null)
			{
				Transform t = GameUtil.FindDeepChild(nofStarsTransform, "NofStars");
				if (t != null)
				{
					Text nofStarsText = t.gameObject.GetComponent<Text>();
					if (nofStarsText != null)
						nofStarsText.text = noStars + "/" + nofMax;
				}
			}

			Transform freeT = GameUtil.FindDeepChild(worldItem.transform, "FREELEVEL");
			if (freeT != null) {
				if (StaticManager.WorldPurchased(worldIndex)) {
					freeT.parent.gameObject.SetActive(false);
					//freeT.gameObject.SetActive(false);
					showFreeLevel = false;
				}

			}

			if (nofJewelsTransform != null) {
				Transform t = GameUtil.FindDeepChild(nofJewelsTransform, "NofStars");
				if (t != null) {
					Text nofJewelsText = t.gameObject.GetComponent<Text>();
					if (nofJewelsText != null)
						nofJewelsText.text = noJewel + "/" + nofMax;
				}
			}
		}

		int NumberOfColumns = 1;
		GridLayoutGroup glg = worldContainer.GetComponent<GridLayoutGroup>();
		float height = glg.cellSize.y + glg.spacing.y;
		if(glg.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
		{
			NumberOfColumns = glg.constraintCount;
		}
		RectTransform contentContainer = worldContainer.GetComponent<RectTransform>();
		if (!showFreeLevel)
		{
			worldsLength = worldsLength - 1;
		}
		float contentHeight = (worldsLength / NumberOfColumns) * height;

		if (worldsLength % NumberOfColumns != 0)
		{
			contentHeight += height;
		}
	
		contentContainer.sizeDelta = new Vector2(contentContainer.sizeDelta.x, contentHeight);

	}
	void GoToBuyCredits() {
		StaticManager.PushScene();
		SceneManager.LoadScene("BuyCredits");
	}
	void Update () {
	}

	public void GotoMainScreen() {
		SceneManager.LoadScene (StaticManager.MAIN_SCENE);
	}
	public void ChristmasSelect() {
		GotoLevelSelect(2);
	}
	public void ZombieSelect() {
		if (StaticManager.WorldPurchased(1)) {
			GotoLevelSelect(1);
		}else {
			GoToBuyCredits();
		}
	}
	public void CurlingSelect() {
		GotoLevelSelect(0);
	}
	public void GotoLevelSelect(int index) {
		StaticManager.GotoLevelSelectScreen (index);
	}

	public void PlayFreeZombieLevel() {
		StaticManager.PlayFreeLevel(1, 4);
	}

}
