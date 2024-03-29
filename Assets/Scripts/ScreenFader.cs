using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScreenFader : MonoBehaviour {

	public Color fadeInColor = Color.black;
	public Color fadeOutColor = Color.black;

	public float fadeInTime = 1.0f;
	public float fadeOutTime = 1.0f;

	public Image coverImage;
	public bool autoFadeIn = true;

	private System.Action fadeInFinishAction = null;
	private System.Action fadeOutFinishAction = null;

	void Awake () {
		coverImage.color = fadeInColor;
	}
	
	void Start () {
		if (autoFadeIn)
			FadeIn ();
	}

	void Update () {
	}

	private void FadeInFinish() {
		coverImage.gameObject.SetActive (false);
		if (fadeInFinishAction != null)
			fadeInFinishAction ();
	}
	private void FadeOutFinish() {
		if (fadeOutFinishAction != null)
			fadeOutFinishAction ();
	}

	public void FadeIn(System.Action callback = null) {
		coverImage.gameObject.SetActive (true);

		iTween.Stop (gameObject);
		iTween.ValueTo(gameObject, iTween.Hash("from", 1f, "to", 0f, "time", fadeInTime, "easetype", "linear", "onupdate", "setAlpha", "oncomplete", "FadeInFinish"));
		fadeInFinishAction = callback;
	}

	public void FadeOut(System.Action callback = null, bool blockInput = true) { // callback is: public void method(). Use e.g. Action<string> for a callback of the type: public void method(string s). For a method that takes two ints, use Action<int, int> etc. An action cannot, however, return values. For that, we can use System.Func instead. For a Func, last param is the return value, so Func<string, int> is: public int method(string s) etc. Cool.
		coverImage.gameObject.SetActive (true);

		float fromAlpha = coverImage.color.a;
		coverImage.color = fadeOutColor;
		setAlpha (fromAlpha);
		iTween.Stop (gameObject);
		iTween.ValueTo(gameObject, iTween.Hash("from", fromAlpha, "to", 1f, "time", fadeOutTime, "easetype", "linear", "onupdate", "setAlpha", "oncomplete", "FadeOutFinish"));

		if (blockInput) {
			EventSystem eventSystem = FindObjectOfType<EventSystem> ();
			if (eventSystem != null)
				eventSystem.gameObject.SetActive (false);
		}
		fadeOutFinishAction = callback;
	}

	public void setAlpha(float newAlpha) {
		coverImage.color = new Color(coverImage.color.r, coverImage.color.g, coverImage.color.b, newAlpha);
	}

}
