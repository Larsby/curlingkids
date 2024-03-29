using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HelpDisplayer : MonoBehaviour {

	public float startDelay = 0;
	public float minimumWait = 1;
	public float automaticEndTimer = 0;

	private float waitTimer = 0;
	private int helpScreenIndex = 0;

	public string prefsKey = "";
	public int nofShowings = -1;

	public Button altButton = null;
	private Button button;

	public string loadSceneOnFinished = string.Empty;

	public bool noClosingButton = true; // NOTE: As a reuseable component, this should be false. However since we want this to be true everywhere in this game and I don't want to find all instances by hand, I created this as a new variable and set it to true

	void Start () {
		if (transform.parent == null) return;

		for (int i = 0; i < transform.childCount; i++) {
			transform.GetChild (i).gameObject.SetActive (false);
		}

		button = altButton;

		if (button == null)
			button = GetComponent<Button> ();
		if (button== null || transform.childCount == 0)
			transform.parent.gameObject.SetActive (false);
		else
			Invoke ("FirstHelp", startDelay);

		if (prefsKey.Length > 0 && nofShowings > 0) {
			if (StaticManager.HelpShownEnough(prefsKey, nofShowings))
			{
				if (transform.parent)
				{
					transform.parent.gameObject.SetActive(false);
				}
			}
		}

		if (noClosingButton) {
			Button [] buttons = transform.parent.gameObject.GetComponentsInChildren<Button>();
			if (buttons != null && buttons.Length > 0) {
				foreach (Button b in buttons)
				{
					Image i = b.gameObject.GetComponent<Image>();
					if (i)
						i.enabled = false;
					b.enabled = false;
				}
			}

		}

	}
	
	void Update () {
		if (waitTimer > 0)
			waitTimer -= Time.deltaTime;
	}

	public void FirstHelp() {

		if (button != null) {
			button.onClick.AddListener (NextHelp);
		}

		NextHelp ();
	}

	public void NextHelp() {
		if (waitTimer > 0)
			return;

		if (helpScreenIndex > 0)
			transform.GetChild (helpScreenIndex - 1).gameObject.SetActive (false);

		if (helpScreenIndex < transform.childCount) {
			transform.GetChild (helpScreenIndex).gameObject.SetActive (true);
			helpScreenIndex++;
			waitTimer = minimumWait;

			if (helpScreenIndex >= transform.childCount && automaticEndTimer > 0) {
				Invoke ("End", automaticEndTimer);
			}
		} else
			End();
	}

	public void End() {
		if (transform.parent == null) return;
		transform.parent.gameObject.SetActive (false);

		if (loadSceneOnFinished != string.Empty)
			SceneManager.LoadScene(loadSceneOnFinished);
	}

}
