using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClickManager : MonoBehaviour {

	public SingleSfx sfx = SingleSfx.Button1;
	public SingleSfx altSfx = SingleSfx.BackButton;
	public bool randomPitch = true;
	public List<Button> altSoundButtons = new List<Button>();
	public List<Button> noSoundButtons = new List<Button>();

	public List<Button> specificSoundButtons = new List<Button>();
	public List<SingleSfx> specificSoundSfx = new List<SingleSfx>();

	void Start () {
		Invoke ("AssignButtonSounds", 0.1f);
	}

	void AssignButtonSounds () {
		Button[] allButtons = Resources.FindObjectsOfTypeAll<Button> ();

		foreach (Button b in allButtons) {

			GameObject go = b.gameObject;

			if (go.hideFlags != HideFlags.None)
				continue;

			if (noSoundButtons != null && noSoundButtons.Contains (b))
				continue;

			if (specificSoundButtons != null && specificSoundButtons.IndexOf (b) != -1) {
				int index = specificSoundButtons.IndexOf (b);
				b.onClick.AddListener (() =>
					{
						PlaySpecificSound (index);
					});
			} else if (altSoundButtons != null && altSoundButtons.Contains (b))
				b.onClick.AddListener (PlayAltSound);
			else
				b.onClick.AddListener (PlayDefaultSound);
		}
	}
		
	public void PlayDefaultSound() {
		if(SoundManager.instance != null)
		SoundManager.instance.PlaySingleSfx (sfx, randomPitch);
	}

	public void PlayAltSound() {
		if (SoundManager.instance != null)
		SoundManager.instance.PlaySingleSfx (altSfx, randomPitch);
	}

	public void PlaySpecificSound(int index) {
		if (index >= 0 && index < specificSoundSfx.Count)
		{
			if (SoundManager.instance != null)
			SoundManager.instance.PlaySingleSfx(specificSoundSfx[index], false);
		}
	}

}
