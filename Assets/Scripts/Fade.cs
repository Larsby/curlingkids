using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
	public float fadeRate;

	public float targetAlpha;
	public float targetStartAlpha;
	public float delayTime; 
	 Image img;
	bool reset;
	private bool startFade = false;
	void Start() {


		Reset();


	
	}
	public IEnumerator StartWithDelay(float delay) {
		yield return new WaitForSeconds(delay);
		startFade = true;

	}
	public void Reset() {
		startFade = false;
		StartCoroutine(StartWithDelay(delayTime));
		 img = this.GetComponent<Image>();
		if (img == null)
		{
			Debug.LogError("Error: No image component on " + this.name);
		}
		Color curColor = img.color;
		curColor.a = targetStartAlpha;
		img.color = curColor;
		reset = true;
		img.enabled = false;

	}

	// Update is called once per frame
	void Update()
	{
		if (startFade == false) return;
		if(img.enabled == false && reset == false)
		{
			img.enabled = true;
		}
		if(reset) {
			img.enabled = false;
			reset = false;
		}
		if (img.color.a != targetAlpha)
		{
			Color curColor = img.color;
			float alphaDiff = Mathf.Abs(curColor.a - this.targetAlpha);
			if (alphaDiff > 0.0001f)
			{
				curColor.a = Mathf.Lerp(curColor.a, targetAlpha, fadeRate * Time.deltaTime);
				img.color = curColor;
			}
		}

	}

}