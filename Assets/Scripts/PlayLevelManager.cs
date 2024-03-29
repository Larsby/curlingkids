using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayLevelManager : MonoBehaviour {

	public Text headerText;
	public Image [] oldStarImages;

	private StaticManager.LevelData oldResults;

	void Awake() {
		Application.targetFrameRate = 60;
		SoundManager.Create ();
	}

	void Start () {
		oldResults = StaticManager.GetCurrentLevelData ();

		string levelString = StaticManager.GetLevelString ();
		headerText.text = "Level " + levelString;

		for (int i = 0; i < 3; i++) {
			oldStarImages [i].color = i < oldResults.stars ? Color.white : Color.black;
		}
	}

	void Update () {}


	public void PlayLevel() {
		StaticManager.RestartSameLevel ();
	}

	public void GotoMainScreen() {
		SceneManager.LoadScene (StaticManager.MAIN_SCENE);
	}

	public void GotoLevelScreen() {
		SceneManager.LoadScene ("LevelSelect");
	}

	public void SelectCharacter() {
		// test   SceneManager.LoadScene ("NewCharacter", LoadSceneMode.Additive);  // if we want to use additive solution with scene on scene
		StaticManager.PushScene();
		StaticManager.LoadCharSelectScene ();

		//SceneManager.LoadScene ("CharSelect");
	}

}
