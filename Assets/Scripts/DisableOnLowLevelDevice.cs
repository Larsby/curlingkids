using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class DisableOnLowLevelDevice : MonoBehaviour {
	public bool hideOnAndroid = false;
	public bool hideOnIOS = true;
	public GameObject enableAlternativeObject = null;
	private PerformanceManager manager;
	void Awake()
	{
#if UNITY_IOS
		if(hideOnIOS) {
		DeviceGeneration gen = Device.generation;
		if (gen < DeviceGeneration.iPhone7){
			gameObject.SetActive(false);
			if (enableAlternativeObject)
				enableAlternativeObject.SetActive(true);
		}
		}
#endif
#if UNITY_ANDROID
		if(hideOnAndroid) {
			gameObject.SetActive(false);
			if (enableAlternativeObject)
				enableAlternativeObject.SetActive(true);
		}

#endif
	}
	void Start()
	{
		manager = PerformanceManager.GetInstance();
	}
	
	void Update () {
		#if UNITY_ANDROID
		if(manager == null)
		{
			manager = PerformanceManager.GetInstance();
		}
		if(manager.IsReady() && manager.IsLowEnd()) {
			gameObject.SetActive(false);
			if (enableAlternativeObject)
				enableAlternativeObject.SetActive(true);
			Destroy(this);
		}
		#endif
	}
}
