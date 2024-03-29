using UnityEngine;
using UnityEngine.SceneManagement;

public class MainLoader : MonoBehaviour {

	AsyncOperation loader;

	void Start () {
		loader = SceneManager.LoadSceneAsync("Main");
		loader.allowSceneActivation = true;
	}
}
