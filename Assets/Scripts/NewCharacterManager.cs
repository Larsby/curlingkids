using UnityEngine;
using UnityEngine.UI;

public class NewCharacterManager : MonoBehaviour {

	public Text nameText;

	private bool firstRun = true;

	private CharacterManager characterManager = null;
	private ToonDollHelper player;

	void Awake() {
		SoundManager.Create ();

		characterManager = CharacterManager.instance;
	}

	void Start () {

		SoundManager.instance.PlaySingleSfx (SingleSfx.UnlockScreen);
	}

	void Update () {
		
		if (firstRun) {

			int index = StaticManager.GetUnlockedAvatarIndex ();
			if (index < 0)
				index = 0;

			GameObject character = Instantiate (characterManager.characterPrefabs[index]);
			player = character.GetComponentInChildren<ToonDollHelper> ();

			ToonDollHelper.RemoveRigidComponents (character, true);

			character.transform.localPosition = new Vector3 (0, 0.25f, 0); // 0.37f for *1.0f below
			character.transform.localScale = character.transform.localScale * 1.2f;
			character.transform.localRotation = Quaternion.Euler (0, 0, 0);

			if (player != null) {
				player.disableRagdoll ();
				player.SetKinematic ();
				player.PlayAnim ("Unlocked");

				// player.ChangeFace (ToonDollHelper.FaceType.Happy);

				player.blinkTimeExtraRange = 5f;
				player.minBlinkTime = 2f;
			}

			string name = characterManager.characterPrefabs[index].name;
			nameText.text = name;
			I2.Loc.LocalizedString I2 = name;
			string loc = I2;
			if (loc != null)
				nameText.text = loc;


			firstRun = false;
		}
	}

	public void BackButton() {
		bool newWorld = false;

		SoundManager.instance.PlaySingleSfx (SingleSfx.BackButton);

		if (player.extra_extraPermanentStars > 0) {
			StaticManager.AddPermanentStars (player.extra_extraPermanentStars);
		}

		if (player.extra_toonPrizeMul != 1) {
			StaticManager.SetPrizeMul (player.extra_toonPrizeMul);
		}

		if (newWorld == false)
			StaticManager.PopScene ();
	}
}
