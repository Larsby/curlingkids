using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterManager : MonoBehaviour {

	public GameObject [] unorderedCharacterPrefabs = null;
	public List<GameObject> characterPrefabs = null;

	private bool initialized = false;
	public static CharacterManager instance = null;
	private List<string> notifications;
	private bool doingNotification = false;
	void Awake ()
	{
		if (instance == null) {
			instance = this;
			Initialize ();
			DontDestroyOnLoad (gameObject);

		} else if (instance != this)
			Destroy (gameObject);
	}

	public void Initialize ()
	{
		if (initialized)
			return;

		bool isOk = StaticManager.ReorderPrefabs ();
		if (!isOk) {
			Debug.Assert (false, "NAME MISMATCH PANIC ASSERT!");
		}

		initialized = true;
	}

	public GameObject GetAvatar(int index) {
		if (index < 0 || index >= characterPrefabs.Count) {
			Debug.Log ("Invalid avatar index, returning default avatar");
			return characterPrefabs[0];
		}

		return characterPrefabs[index];
	}


	// Ugly to have this here (this is not related to the characters at all), but I can't be bothered with yet another Singleton keepalive object

	public GameObject taskRewardPopupPrefab;

	public void ShowCoinTaskRewardPopup(string name, string description, int credits) {
		ShowTaskRewardPopup (name, description, credits, " " + I2.Loc.LocalizationManager.GetTranslation("credits"));
	}
	public void ShowGemTaskRewardPopup(string name, string description, int credits) {
		ShowTaskRewardPopup (name, description, credits, " gems");
	}
	private void ShowTaskRewardPopup(string name, string description, int val, string desc) {


		string text = name + " - " + val + desc;
		ShowNotification(text);
	}
	public void ShowNotificationWithDelay(string text,float delay) {
		StartCoroutine(ShowNotificationWithDelayCO(text,delay));
	}
	public IEnumerator ShowNotificationWithDelayCO(string text, float delay) {
		yield return new WaitForSeconds(delay);
		ShowNotification(text);
	}
	public void ShowNotificationDone() {
		
		notifications.RemoveAt(0);
		if (notifications.Count > 0)
		{
			string notification = notifications[0];
			if (notification != null)
			{
				DoNotification(notification);
			}
			else
			{
				doingNotification = false;
			}
		} else {
			doingNotification = false;
		}
	}
	private void DoNotification(string text) {
		SoundManager.instance.PlaySingleSfx(SingleSfx.TaskFinished);

		GameObject popup = Instantiate(taskRewardPopupPrefab);

		if ((float)Screen.width / (float)Screen.height > 0.5f) { // Not Iphone X
			RectTransform img = popup.transform.GetChild(0).GetComponent<RectTransform>();
			img.sizeDelta = new Vector2(img.sizeDelta.x, 140);
		}

		Text[] texts = popup.GetComponentsInChildren<Text>();
		texts[0].text = text;
	
	}
	public void ShowNotification(string text) {
		if(notifications == null) {
			notifications = new List<string>();
		}
		notifications.Add(text);
		if(doingNotification == false){
			doingNotification = true;
			DoNotification(text);
		}

	}
	// Likewise ugly, same reason (for level-loading spinner object)

	public GameObject loadSpinnerPrefab;

	public void ShowLoadSpinner() {
		/*GameObject popup =*/ Instantiate (loadSpinnerPrefab);
		EventSystem eventSystem = FindObjectOfType<EventSystem> ();
		if (eventSystem != null)
			eventSystem.gameObject.SetActive (false);
	}

}
