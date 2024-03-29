using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour, ScrollSnapItemChanged {

	public Text nameText;
	public GameObject charItemPrefab;
	public GameObject scrollView;
	public Button selectButton;
	public Text creditsText;
	public Text indexText;
	public Button previousButton;
	public Button nextButton;
	public Button backButton;

	private EventSystem eventSystem = null;

	private CharacterManager characterManager = null;
	private int avatarIndex = -1;
	private List<GameObject> charItems = new List<GameObject> ();
	private List<ToonDollHelper> characters = new List<ToonDollHelper> ();

	private bool showSelectAnimation = true;
	private bool firstNotify = true;

	public GameObject firstBuy;
	public GameObject firstBuyBuy;
	public GameObject firstBuySelect;

	private bool buyGoesToBuyCredits = true;
	private bool forceNoLeftRight = false;

	private bool ShowForcedBuy() {
		if (StaticManager.awaitingFirstBuyStepsLeft > 0) return true;
		return false;
	}

	void Awake() {
		Application.targetFrameRate = 60;

		SoundManager.Create ();

		characterManager = CharacterManager.instance;

		for (int i = 0; i < characterManager.characterPrefabs.Count; i++) {
			GameObject charItem = Instantiate (charItemPrefab, scrollView.transform, false);

			GameObject character = Instantiate (characterManager.characterPrefabs[i], charItem.transform, false);
			ToonDollHelper player = character.GetComponentInChildren<ToonDollHelper> ();

			bool unlocked = StaticManager.IsAvatarUnlocked(i);

			character.transform.localPosition = new Vector3 (0, -153, -252.6f);
			character.transform.localScale = new Vector3 (200, 200, 200);
			character.transform.localRotation = Quaternion.Euler (0, 180, 0);

			if (player != null) {
				player.disableRagdoll ();
				player.SetKinematic ();
				player.TrigAnim ("Idle");

	//			player.doBlinking = false;
				if (!unlocked) player.shadedFace = true;
				player.blinkTimeExtraRange = 5f;
				player.minBlinkTime = 2f;
			}

			SkinnedMeshRenderer smr = character.GetComponentInChildren<SkinnedMeshRenderer> ();
			Button buyButton = charItem.GetComponentInChildren<Button> ();
			Transform t = charItem.transform.Find ("PrizeText");

			TrailRenderer [] trs = character.GetComponentsInChildren<TrailRenderer> ();
			if (trs != null && trs.Length > 0)
				foreach (TrailRenderer tr in trs)
					Destroy (tr);

			if (!unlocked) {

				int prize = (int) ((float)player.prize * StaticManager.GetPrizeMul());

				if (smr != null) {
					smr.material.color = GameUtil.IntColor (40, 40, 40);
					if (smr.materials != null && smr.materials.Length > 1) {
						smr.materials [1].color = GameUtil.IntColor (40, 40, 40);

						Shader mobParticlesMult = Shader.Find ("Mobile/Particles/Multiply"); 
						if (mobParticlesMult != null) {
							smr.materials [1].shader = mobParticlesMult;
						}
					}
				}
				if (buyButton != null) {
					buyButton.onClick.AddListener (BuyButton);

					bool allowInteract = true;
					if (StaticManager.lockZombieCharacters && !StaticManager.WorldPurchased(1) && player.partOfZombiePack) {
						allowInteract = false;
						buyButton.transform.Find("BuyText").gameObject.SetActive(false);
						buyButton.transform.Find("Lock").gameObject.SetActive(true);
						buyButton.transform.Find("LockText").gameObject.SetActive(true);
					}

					if (!buyGoesToBuyCredits)
						if (StaticManager.GetNumberOfCredits () < prize || allowInteract == false)
							buyButton.interactable = false;
				}
				if (t != null) {
					Text prizeText = t.gameObject.GetComponent<Text> ();
					if (prizeText != null)
						prizeText.text = prize + " " + I2.Loc.LocalizationManager.GetTranslation("coins");
				}

			} else {
				if (t != null)
					t.gameObject.SetActive (false);
				if (buyButton != null)
					buyButton.gameObject.SetActive (false);
			}

			Transform t2 = charItem.transform.Find ("SpecialText");
			if (t2 != null) {
				Text specialText = t2.gameObject.GetComponent<Text> ();
				if (specialText != null)
				{
                    I2.Loc.LocalizedString loc = player.extrasDescription;
                
					if (player.extrasDescription.Length > 0)
						specialText.text = "+ " + loc;
					else
						specialText.text = "";
				}
			}

			Transform t3 = charItem.transform.Find("Description");
			if (t3 != null)
			{
				Text text = t3.gameObject.GetComponent<Text>();
				if (text != null)
                {
                     I2.Loc.LocalizedString loc = player.playerDescription;
                     text.text = loc;

                }
			}
           
			player.SetOutlineSize (0.45f);

			ToonDollHelper.RemoveRigidComponents (character, true);
				

			if (characterManager.characterPrefabs[i].name == "Ninja Go Vanish" && smr != null) // hack to make this character appear behind the buy button and prize text, because the default settings for shader Toon->BasicTransparent renders it above
				smr.material.renderQueue = 2000; // "Geometry"

			Transform tGround = charItem.transform.Find ("Ground"); // position the ground where doll's toes are. The Animator can be used to find all bodyparts, e.g. the toes, without using names
			Animator animator = player.gameObject.GetComponent<Animator> ();
			if (tGround != null && animator != null) {
				Transform tFoot = animator.GetBoneTransform (HumanBodyBones.LeftToes);
				if (tFoot != null)
					tGround.position = GameUtil.SetY (tGround.position, tFoot.position.y - 1);
			}

			creditsText.text = "" + StaticManager.GetNumberOfCredits();

			ParticleSystem ps = character.GetComponentInChildren<ParticleSystem> ();
			if (ps != null) {
				var main = ps.main;
				main.startDelay = 0.06f;
				main.gravityModifier = 1; // Disco Roboto
				if (character.name.Substring (0, 2) == "Sk") // Skelly Mel
					main.gravityModifier = -8;
				main.scalingMode = ParticleSystemScalingMode.Hierarchy;
			}

			charItems.Add (charItem);
			characters.Add (player);
		}

		eventSystem = FindObjectOfType<EventSystem> ();

		SetOrRestoreShadowSettings (false);

		if (ShowForcedBuy()) {
			firstBuy.SetActive(true);

			if (backButton) backButton.gameObject.SetActive(false);
			if (selectButton) selectButton.gameObject.SetActive(false);
			forceNoLeftRight = true;

			if (StaticManager.GetUnlockedAvatarIndex() >= 0) {
				firstBuyBuy.SetActive(false);
				firstBuySelect.SetActive(true);
			}
		}

		Invoke("DelayedStart", 0.2f);
	}

	void DelayedStart() {
		StaticManager.awaitingFirstBuyStepsLeft--;
	}

	private ShadowProjection shadow_oldProjection;
	private int shadow_oldCascades;
	private float shadow_oldDistance;

	private void SetOrRestoreShadowSettings(bool restore) {
		if (!restore) {
			shadow_oldProjection = QualitySettings.shadowProjection;
			shadow_oldDistance = QualitySettings.shadowDistance;
			shadow_oldCascades = QualitySettings.shadowCascades;
			QualitySettings.shadowProjection = ShadowProjection.StableFit;
			QualitySettings.shadowDistance = 75;
			QualitySettings.shadowCascades = 1;
		} else {
			QualitySettings.shadowProjection = shadow_oldProjection;
			QualitySettings.shadowDistance = shadow_oldDistance;
			QualitySettings.shadowCascades = shadow_oldCascades;
		}
	}

	void Start () {
	}

	void Update () {
	}

	private void LateUpdate()
	{
		if (forceNoLeftRight) {
			if (nextButton) nextButton.gameObject.SetActive(false);
			if (previousButton) previousButton.gameObject.SetActive(false);
		}
	}

	private void DelayedLoad() {
		SetOrRestoreShadowSettings (true);
		StaticManager.PopScene ();
	}

	public void PlayButton() {

		StaticManager.SetSelectedAvatarIndex (avatarIndex);

		SoundManager.instance.PlayRandomFromType(characters [avatarIndex].soundSelected, -1, 0, -1, characters [avatarIndex].soundPitch);

		characters [avatarIndex].ChangeFace (ToonDollHelper.FaceType.Happy);

		if (showSelectAnimation) {
			characters [avatarIndex].TrigAnim ("Selected");
			if (eventSystem != null)
				eventSystem.gameObject.SetActive (false); // turn off all interaction with UI
			Invoke ("DelayedLoad", 0.5f);
		} else {
			SetOrRestoreShadowSettings (true);
			StaticManager.PopScene ();
		}
	}

	public void PlayButtonForcedBuy()
	{
		StaticManager.SetForceBuyNewCharacterDone();
		PlayButton();
	}

	public void BackButton() {
		SetOrRestoreShadowSettings (true);
		StaticManager.PopScene ();

		SoundManager.instance.PlaySingleSfx (SingleSfx.BackButton);
	}

	public void BuyButton()
	{
		if (buyGoesToBuyCredits)
		{
			bool isLockedZombie = StaticManager.lockZombieCharacters && !StaticManager.WorldPurchased(1) && characters[avatarIndex].partOfZombiePack;
			if ((StaticManager.GetNumberOfCredits() < characters[avatarIndex].prize * StaticManager.GetPrizeMul()) || isLockedZombie)
			{
				StaticManager.SetUnlockedAvatarIndex(avatarIndex);
				StaticManager.PushScene();
				SceneManager.LoadScene("BuyCredits");
				return;
			}
		}

		StaticManager.SetUnlockedAvatarIndex (avatarIndex);
		StaticManager.UnlockAvatar(avatarIndex, (int)((float)characters[avatarIndex].prize * StaticManager.GetPrizeMul()));
		StaticManager.PushScene ();
		SceneManager.LoadScene ("NewCharacter");

		SoundManager.instance.PlaySingleSfx (SingleSfx.BuyButton);
	}

	public void NotifyItemChanged(int index) {
		string name = characterManager.characterPrefabs[index].name;
		nameText.text = name;
		I2.Loc.LocalizedString I2 = name;
		string loc = I2;
		if (loc != null)
			nameText.text = loc;

		avatarIndex = index;
		selectButton.interactable = StaticManager.IsAvatarUnlocked (index);
		previousButton.gameObject.SetActive (index > 0);
		nextButton.gameObject.SetActive (index < characterManager.characterPrefabs.Count - 1);

		indexText.text = (index + 1) + "/" + CharacterManager.instance.characterPrefabs.Count;

		if (firstNotify == false)
			SoundManager.instance.PlaySingleSfx (SingleSfx.Button1);
		firstNotify = false;
	}

	public int GetStartPage() {
		if (StaticManager.GetUnlockedAvatarIndex () >= 0) {
			int index = StaticManager.GetUnlockedAvatarIndex ();
			StaticManager.SetUnlockedAvatarIndex (-1);
			return index;
		} else {
			if (ShowForcedBuy())
				return StaticManager.GetFirstCharacterBuyIndex();

			return StaticManager.GetSelectedAvatarIndex ();
		}
	}

}
