using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;

public class AdManager : MonoBehaviour
{

	public class AdInfo
	{
		public string gameInfo;
		public string appstoreUrl;
		public string weburl;
		public string videourl;
	}

	List<AdInfo> ads;
	public VideoPlayer player;
	private bool ready = false;
	public VideoClip backupClip;
	public string adinfo = "http://www.pastille.se/ads/kicka_ads.txt";
	public Button buy;
	public Button visit;
	public GameObject gameUI;
	public GameObject adUI;
	public GameObject closeButton;
	public GameObject plane;
	public Fade fade;
	public GameObject flash;
	bool watchedAll = false;
	int index = 0;
	bool preped = false;

	public DisableIfUsedToday disable;

	void Start ()
	{
		ads = new List<AdInfo> ();
		if (buy != null) {
			buy.enabled = false;
			buy.gameObject.SetActive(true);
			buy.onClick.AddListener (BuyOnClick);
		} 
		if (visit != null) {
			visit.enabled = false;

			visit.onClick.AddListener (VisitOnClick);
		}
		watchedAll = false;
		StartCoroutine (LoadData (adinfo));

		float mod = ((float)Screen.width / (float)Screen.height) / (3f / 4f);
		plane.transform.localScale = new Vector3(plane.transform.localScale.x * mod, plane.transform.localScale.y, plane.transform.localScale.z * mod);
	}
	public void Setup() {
		plane.SetActive(true);

		gameUI.SetActive(false);
		//	player.Play();
		adUI.SetActive(true);
		fade.Reset();
		watchedAll = false;
		ready = true;

	}
	public void PlayAd ()
	{
		
	//	player.loopPointReached += EndReached;
		if (ads.Count <= 0) {
			ready = false;
			StartCoroutine (LoadData (adinfo));
		} else {
			index = Random.Range (0, ads.Count);

			if (ads [index].videourl.Length <= 0) {
				index = 0;
			}
		
			string url = ads [index].videourl;
			string fileName = Application.persistentDataPath + GetNameFromUrl (url);

			if (FileExistInCache (fileName)) {

			
				player.url = fileName;
				player.loopPointReached += EndReached;
				player.Play ();



			} else {
				StartCoroutine (SaveClip (url));
			}
		}

	}
	public void Close() {
		gameUI.SetActive(true);
		plane.SetActive(false);
		adUI.SetActive(false);
		buy.gameObject.SetActive(false);
	//	fade.enabled = false;
		if (watchedAll)
		{
			ready = true;
		
		}
	}
	void EndReached(UnityEngine.Video.VideoPlayer vp)
	{


		if (watchedAll == false)
		{
			watchedAll = true;
			vp.isLooping = false;
			StaticManager.AddCredits(30);
			StaticManager.VideoShownToday();
			SoundManager.instance.PlaySingleSfx(SingleSfx.GetMoney);
			if (flash) {
				flash.SetActive(true);
				Invoke("HideFlash", 0.1f);
			}
			vp.Pause();

			if (disable) disable.Disable();
		}
	}
	void HideFlash() {
		if (flash)
			flash.SetActive(false);
	}

	string GetNameFromUrl (string url)
	{
		string[] tokens = url.Split ('/');
		if (tokens != null) {
			return "/" + tokens [tokens.Length - 1];
		}
		return null;
	}

	bool FileExistInCache (string clipPath)
	{
		return File.Exists (clipPath);
	}

	IEnumerator SaveClip (string clipURL)
	{
		var www = new WWW (clipURL);
		yield return www;
		string name = GetNameFromUrl (clipURL);
		File.WriteAllBytes (Application.persistentDataPath + name, www.bytes);

		player.url = Application.persistentDataPath + name;
		player.loopPointReached += EndReached;
		player.Play ();
	}

	void BuyOnClick ()
	{
		Application.OpenURL (ads [index].appstoreUrl);
	}

	void VisitOnClick ()
	{
		Application.OpenURL (ads [index].weburl);
	}


	void Update ()
	{
		if (ready) {
			ready = false;
			PlayAd ();
			if (buy != null) {
				buy.gameObject.SetActive(true);
				buy.enabled = true;
			}
			if (visit != null) {
				visit.enabled = true;
			}
			ready = false;
		}
	}


	IEnumerator LoadData (string url)
	{
		WWW www = new WWW (url);

		yield return www;
		string text = www.text;
		if (text == null) {
			
			yield return null;
		}
		if (text != null && text.Length > 1) {
			string[] lines = text.Split ('\n');
			foreach (string line in lines) {
				string[] info = line.Split ('|');
				if (info.Length > 4) {
					AdInfo ad = new AdInfo ();
					ad.gameInfo = info [0];
					ad.appstoreUrl = info [1];
					ad.weburl = info [2];
					ad.videourl = info [3];
					ads.Add (ad);
					#if UNITY_ANDROID
					if (info.Length >= 5) {
					ad.appstoreUrl = info [4];
					}
					#endif
				}
			}
		
			ready = true;

		} else {
			Debug.Log ("Could not load from www");
			if (backupClip != null) {
				player.clip = backupClip;
				player.loopPointReached += EndReached;
				player.Play ();
			}

		}


	}
}
