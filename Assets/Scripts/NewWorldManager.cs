using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewWorldManager : MonoBehaviour {

	public Text nameText;
	// private AsyncOperation closeOperation = null;

	void Awake() {
		SoundManager.Create ();
	}

	void Start () {
		int world = StaticManager.GetUnlockedWorldIndex () + 1; // 0-based index
		nameText.text = "Unlocked world " + world + "!"; 

		SoundManager.instance.PlaySingleSfx (SingleSfx.NewWorld);

		StaticTaskManager.EvaluateAll (StaticTaskManager.TaskType.UnlockWorld, world);
	}

	void Update () { 
		// if (closeOperation != null) { /* supposed to do sth? // closeOperation.isDone ? */ }
	}

	public void BackButton() {
		/*
		if (closeOperation != null)
			return;

		closeOperation = SceneManager.UnloadSceneAsync (SceneManager.GetActiveScene ().name);
		if (closeOperation != null)
			closeOperation.allowSceneActivation = true;
		*/

		SoundManager.instance.PlaySingleSfx (SingleSfx.BackButton);

		StaticManager.PopScene ();
	}
}
