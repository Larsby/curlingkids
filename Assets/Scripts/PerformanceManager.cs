using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bit2Good.DynaRes2017;
public class PerformanceManager : MonoBehaviour
{
	double totalfps;
	int count;
	float scale;
	float deltaTime;
	bool doOptimization = false;
	bool done = false;
	bool init = false;
	public int minFPS = 30;
	public int framesToMessure = 0;
	public float minScale = 0.8f;
	public float scaleStep = 0.1f;
	private string levelHash;
	DynaRes dynamicResolution;
	private bool showFPS = false;
	float height;
	float width;
	int averageFPS;
	private bool readyToAnswer = false;
	private static PerformanceManager instance;
	private bool optimize = false;
	void Start()
	{
		totalfps = 1;
		deltaTime = 0.0f;
		count = 1;
		scale = 1.0f;
		done = false;
		// remove should be called by the game instance.
		height = Screen.height * scale;
		width = Screen.width * scale;
		instance = this;
		optimize = false;
	}
	void Save(bool bedone)
	{
        if (PlayerPrefs.HasKey(levelHash) == false)
        {
            Debug.Log("Saved a save");
            PlayerPrefs.SetFloat(levelHash, scale);
            if (bedone)
            {
                PlayerPrefs.SetInt(levelHash + "DONE", 1);
                done = true;
            }
            PlayerPrefs.Save();
        }
        else
        {
            PlayerPrefs.GetFloat(levelHash, scale);
            Debug.Log("loaded a save");
        }
	}

	public bool IsReady() {
		return readyToAnswer;
	}
	public bool IsLowEnd() {
		// hard coded to return false
		// in order to use it properly we need to change every level
		return false;
		if (scale < 1.0f || averageFPS < 25 )
			return true;
		return false;
	}
	void Optimize()
	{

		if (scale < minScale)
		{
			scale = minScale;
			done = true;
			Save(done);
			Debug.Log("mkay scale is less");
		}
		 height = Screen.height * scale;
		 width = Screen.width * scale;
		if (height < 1024*0.8f) height = Screen.height*0.8f;
		if (width < 600*0.8f) width = Screen.width*0.8f;
		dynamicResolution.Config.ForceResolution((int)height, (int)width);
		Debug.Log("mkay setting resolution to " + (int)height + "x" + (int)width + "with scale " + scale + "final save " + done);
		readyToAnswer = true;

	}
	public static PerformanceManager GetInstance() {
		return instance;
	}
	//called from Game
	public void Setup(string hash)
	{
#if UNITY_ANDROID
		dynamicResolution = Camera.main.GetComponent<DynaRes>();
		levelHash = hash + SystemInfo.deviceUniqueIdentifier;
		done = false;
		if (dynamicResolution == null)
		{
			dynamicResolution = Camera.main.gameObject.AddComponent<DynaRes>();
		}
       

            if (PlayerPrefs.HasKey(levelHash)) {
                scale = PlayerPrefs.GetFloat(levelHash);
                if (PlayerPrefs.GetInt(levelHash + "DONE") == 1)
                {
                   


                } 
			if (scale < 1.0f)
			{

				Optimize();

			}
            

        }
	
		init = true;
#else
		init = false;
#endif
	}
	// Update is called once per frame
	void Update () {
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        if (init == false)
			return;

		if (done)
			return;
		
        float fps = 1.0f / deltaTime;
        totalfps += fps;
        averageFPS = (int)(totalfps / count);

//		if (Time.timeSinceLevelLoad > 0)
		{
			
			if (count > framesToMessure && !done && Time.timeSinceLevelLoad > 5)
			{
					
				Debug.Log("averageFPS: " + averageFPS);

				if (averageFPS >= minFPS)
				{
					done = true;
					Save(true);
					readyToAnswer = true;
				}
				if (averageFPS < minFPS)
				{
					Debug.Log("Average FPS was lower than Minimum FPS");
					scale = scale - scaleStep;
					Optimize();
					Save(false);
					totalfps = 0;
					count = 1;
					return;
				}


				count = 1;

			}
		}
		count++;

	}
	void OnGUI()
	{
		if (showFPS)
		{
			int w = Screen.width, h = Screen.height;

			GUIStyle style = new GUIStyle();

			Rect rect = new Rect(0, 0, w, h * 2 / 100);
			style.alignment = TextAnchor.UpperLeft;
			style.fontSize = 18;
			style.normal.textColor = Color.black;
			float msec = deltaTime * 1000.0f;
			float fps = 1.0f / deltaTime;

			string text = string.Format("{0:0.0} ms ({1:0.} fps width{2} height {3})", msec, fps,width,height);
			GUI.Label(rect, text, style);
		}
	}
}
