using UnityEngine;
using UnityEngine.UI;

public class DisableIfUsedToday : MonoBehaviour
{
	public Image img;
	public float disabledAlphaValue;

	void Start()
	{
		if(StaticManager.VideoAvailableToday() == false) {
			Disable();
		}
	}

	public void Disable() {
		Button b = GetComponent<UnityEngine.UI.Button>();
		GameUtil.DisableButton(b, img, disabledAlphaValue);
	}

}
