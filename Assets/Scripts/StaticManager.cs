using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;
using Facebook;
public static class StaticManager : System.Object
{

	[Serializable]
	public class LevelData
	{
		public int stars;
		public int score;
		public float time;
	}

	[Serializable]
	public class GameSettings
	{
		public bool bPlayMusic;
		public bool bPlaySounds;
	}

	[Serializable]
	public class TaskCompletion
	{
		public int throws;
		public int spinHits;
		public int portals;
		public int boosts;
		public int brokenGlass;
		public int curbsTaken;
		public int loops;
		public int rings;
		public int zoombieHit;
		public int cones;
		public int balloon;
		public int threeStars;
		public TaskCompletion ()
		{
			throws = 
            	spinHits = 
            	portals = 
            	boosts =
                rings =
            	brokenGlass = 
            	curbsTaken = 
                zoombieHit =
                cones =
                balloon =
            	loops = threeStars= 0;
		}
	}

	[Serializable]
	public class SingleAvatar
	{
		public SingleAvatar (bool unlocked, string name)
		{
			this.unlocked = unlocked;
			this.name = name;
		}

		public SingleAvatar ()
		{
			this.unlocked = false;
			this.name = "";
		}

		public bool unlocked;
		public string name;
	}

	[Serializable]
	public class AvatarData
	{
		public int selectedAvatarIndex;
		public string selectedAvatarName;
		public SingleAvatar[] avatars;
		public int nofAvatars;
	}

	[Serializable]
	public class CreditData
	{
		public int credits;
		public int gems;

	}

	[Serializable]
	public class GameData
	{
		public float version;
		public LevelData[,] levels;
		public GameSettings settings;
		public AvatarData avatarData;
		public CreditData creditData;
		public TaskCompletion taskCompletion;
		public int worldIndex;
		public int levelIndex;
		public int lastWorldIndex;
		public int lastLevelIndex;
		public int starsPlus;
		public float prizeMul;
	}

	private const float version = 1.05f;
	private const string saveName = "/GameInfo1f.dat";
	private static GameData gameData;
	private const int MAXWORLDS = 20;
	private const int MAXLEVELS = 40;
	private const int MAXAVATARS = 50;
	static string VIDEOHASHKEY = "VideoShown";
	private static bool bWasLoaded = false;
	private static int nofWorlds = 0;
	public static bool testTrackingDoNotEnable = false;

	private static LevelData achievedResults = new LevelData ();
	private static LevelData oldResults = new LevelData ();

	private static List<string> sceneStack = new List<string> ();

	public static bool usePlayLevelScreen = false;

	public static bool showCharPurchaseAvailable = false;

	private static int unlockedAvatarIndex = -1;

	private static int temporaryCredits = 0;

	public static bool useMaxTime = false;
	public static bool newEndforceCalculation = true;
	public static bool fixedSwipeStartPoint = true;

	private static int unlockedWorldIndex = 0;

	public static bool showAllLevelsDebug = false;
	public static bool playingFreeLevel = false;
	public static bool unpurchasedWorldsAvailable = false;

	public static bool lockZombieCharacters = true;
	public static int nofNewJewelsTaken = 0;
	private static float coolDownTime = -1;
	//normal main
	//public static string MAIN_SCENE = "Main";
	//christmas main.
	public static string MAIN_SCENE = "Main Xmas";
	private static bool _showzombie = true;
	public static bool SHOW_ZOMBIE_POPUP {
		get {
			Debug.Log("in show zombie val is"+bWasLoaded+ ""+_showzombie);
			if (!bWasLoaded) {
				Debug.Log("in show zombie returning true");
				return true; }
			return _showzombie;
		}
		set
		{
			_showzombie = value;
		}
	}
	public static bool SHOW_CHRISTMAS = false;
	public static void AddTimeForUnlimitedReset(int minutes) {
		coolDownTime = Time.time + (60 * minutes);

	}
	public static bool HaveUnlimitedReset() {
		if (coolDownTime < Time.time)
			return false;
		return true;
	}
	public static void ConvertOldToNewThreeStars(int level, int world) {
		if (PlayerPrefs.HasKey("ThreeStars" + level))
		{
			int oldCount = PlayerPrefs.GetInt("ThreeStars" + level);
			PlayerPrefs.DeleteKey("ThreeStars" + level);
			PlayerPrefs.SetInt("ThreeStars_new" + world + "_" + level, oldCount);
			PlayerPrefs.Save();
		}
	}

	public static int AddThreeStars(int level,int world) {
		if (world == -1)
			world = 0;
		
		int oldCount = 0;
		// convert existing users data to the new bug free format
		ConvertOldToNewThreeStars(level, world);

		if(PlayerPrefs.HasKey("ThreeStars_new" + world + "_" + level)== false) {
			IncreaseThreeStars();
		}
		PlayerPrefs.SetInt("ThreeStars_new" + world + "_" + level, oldCount);
		PlayerPrefs.Save();
		return gameData.taskCompletion.threeStars;
	}

	public static void IncreaseThreeStars() {
		gameData.taskCompletion.threeStars += 1;

	}
	public static int GetThreeStarsCount() {
		return gameData.taskCompletion.threeStars;
	}
	public static void IncreaseTossCountByOne() {
		IncreaseTossCount(1);
	}
	public static void IncreaseTossCount (int tosses)
	{
		gameData.taskCompletion.throws +=tosses;

	}

	public static void IncreasePortalsCount ()
	{
		gameData.taskCompletion.portals += 1;
	}

	public static int GetTotalPortalsCount ()
	{
		return gameData.taskCompletion.portals;
	}

	public static void IncreaseBoostCount ()
	{
		gameData.taskCompletion.boosts += 1;
	}

	public static int GetTotalBoostCount ()
	{
		return gameData.taskCompletion.boosts;
	}

	public static void IncreaseBrokenGlassCount ()
	{
		gameData.taskCompletion.brokenGlass += 1;
	}

	public static int GetBrokenGlassCount ()
	{
		return gameData.taskCompletion.brokenGlass;
	}

	public static void IncreaseConeHitCount ()
	{
		gameData.taskCompletion.cones += 1;
	}

	public static int GetConeHitCount ()
	{
		return gameData.taskCompletion.cones;
	}

	public static void IncreaseZoombieHitCount ()
	{
		gameData.taskCompletion.zoombieHit += 1;

	}

	public static int GetZoombieHitCount ()
	{
		return gameData.taskCompletion.zoombieHit;
	}

	public static void IncreaseBalloonHitCount ()
	{
		gameData.taskCompletion.balloon += 1;

	}

	public static int GetBalloonHitCount ()
	{
		return gameData.taskCompletion.balloon;
	}

	public static void IncreaseLoopCount ()
	{
		gameData.taskCompletion.loops += 1;
	}

	public static int GetTotalLoopCount ()
	{
		return gameData.taskCompletion.loops;
	}

	public static void IncreaseCurbTake ()
	{
		gameData.taskCompletion.curbsTaken += 1;
	}

	public static void IncreaseRingsCount ()
	{
		gameData.taskCompletion.rings += 1;
	}

	public static int GetTotalRingsCount ()
	{
		return gameData.taskCompletion.rings;
	}

	public static int GetTotalTossCount ()
	{
		return gameData.taskCompletion.throws;
	}

	public static int GetCurbTakeCount ()
	{
		return gameData.taskCompletion.curbsTaken;
	}
	public static bool VideoAvailableToday() {
		if (PlayerPrefs.HasKey(VIDEOHASHKEY))
		{
			string dateshown = PlayerPrefs.GetString(VIDEOHASHKEY);
			string pattern = "MM-dd-yy";
			DateTime parsedDate;


			if (DateTime.TryParseExact(dateshown, pattern, null,
									   System.Globalization.DateTimeStyles.None, out parsedDate))
			{
				int diff = parsedDate.AddDays(1).CompareTo(DateTime.Now.Date);
				if (diff > 0)
				{
					return false;
				}

			}

		
		}
		return true;
	}
	public static void VideoShownToday() {
		PlayerPrefs.SetString(VIDEOHASHKEY, System.DateTime.Now.Date.ToString("MM-dd-yy"));
		PlayerPrefs.Save();
	}
	public static float GetTimeStepMul ()
	{
		return 0.02f / Time.fixedDeltaTime; // default fixed timestep / current timestep
	}

	public static float globalGravityAdd = 0f;

	public static float GetGlobalGravityMod ()
	{
		float gravityMod = 1f;

		if (Mathf.Abs (Physics.gravity.y) > 9.82f) {
			gravityMod = Mathf.Abs (Physics.gravity.y) - 9.81f;
			gravityMod = 1f + gravityMod / 24.1f;
		}

		return gravityMod;
	}

	private static int pushCounter = 0;
	public static float originalLevelTimeStep = 0.02f;

	public static int PushFixedTimeStep (float timeStep)
	{
		Time.fixedDeltaTime = timeStep;
		pushCounter++;
		return pushCounter;
	}

	public static void RestoreTimeStep (int timeStepPushIndex)
	{
		if (timeStepPushIndex == pushCounter)
			Time.fixedDeltaTime = originalLevelTimeStep;
	}

	private static int pushCDMCounter = 0;
	public static CollisionDetectionMode originalCollisionDetectionMode = CollisionDetectionMode.Discrete;
	public static int PushCollisionDetectionMode (CollisionDetectionMode cdm, ToonDollHelper tdh)
	{
		tdh.SetCollisionDetectionMode (cdm);
		pushCDMCounter++;
		return pushCDMCounter;
	}

	public static void RestoreCollisionDetectionMode (int pushIndex, ToonDollHelper tdh)
	{
		if (pushIndex == pushCDMCounter)
			tdh.SetCollisionDetectionMode (originalCollisionDetectionMode);
	}


	private static void Prepare ()
	{
		if (!bWasLoaded)
			Load ();
	}

	public static void SetPrizeMul (float mul)
	{
		Prepare ();
		gameData.prizeMul = mul;
		Save ();
	}

	public static float GetPrizeMul ()
	{
		Prepare ();
		return gameData.prizeMul;
	}

	public static bool GetUseMaxTime ()
	{
		return useMaxTime;
	}

	public static int GetUnlockedWorldIndex ()
	{
		return unlockedWorldIndex;
	}

	public static void SetUnlockedWorldIndex (int index)
	{
		unlockedWorldIndex = index;
	}


	private const string baseMMS = "-------------------------------------------------------------------------------------------------";
	private static string currentMMS;
	private static string currentMMSkey;
	private static MoneyMaker[] currentMMSobjects;

	public static void PrepareMoneyMakers (MoneyMaker[] moneymakers)
	{
		int inlen = 0;
		if (moneymakers != null)
			inlen = moneymakers.Length;

		currentMMSobjects = moneymakers;

		currentMMSkey = "mm" + gameData.lastWorldIndex + "_" + gameData.lastLevelIndex;
		if (PlayerPrefs.HasKey (currentMMSkey)) {
			currentMMS = PlayerPrefs.GetString (currentMMSkey);
		} else {
			currentMMS = baseMMS.Substring (0, inlen + 1);
			PlayerPrefs.SetString (currentMMSkey, currentMMS);
			PlayerPrefs.Save ();
		}

		for (int i = 0; i < inlen; i++) {
			if (i < currentMMS.Length)
			if (currentMMS.Substring (i, 1) != "-" && moneymakers [i].regenerate == false)
				moneymakers [i].gameObject.SetActive (false);
		}
		//Debug.Log (currentMMS);
	}


	public static void PurchaseMade(int worldLevel) {
		PlayerPrefs.SetInt("worldUnlocked"+worldLevel,1);
		SetUnlockedWorldIndex(worldLevel+1	);
		PlayerPrefs.Save();
		if (worldLevel < MAXWORLDS)
		{
			if(unlockedWorldIndex < worldLevel) {
				unlockedWorldIndex = worldLevel;
			}
			Save();
		}
	}
	public static bool WorldPurchased(int worldLevel)
	{
		if (showAllLevelsDebug) return true;
		if (unpurchasedWorldsAvailable) return true;

		bool unlocked =  PlayerPrefs.HasKey("worldUnlocked" + worldLevel);
		if(unlocked == false && worldLevel == 0) {
			PurchaseMade(0);
			unlocked = true;
		}
		return unlocked;
	}
	public static void SaveMoneyMakers ()
	{
		PlayerPrefs.SetString (currentMMSkey, currentMMS);
		PlayerPrefs.Save ();
		currentMMSobjects = null;
		//Debug.Log (currentMMS);
	}

	public static void ResetMoneyMakers ()
	{
		for (int i = 0; i < MAXWORLDS; i++)
			for (int j = 0; j < MAXLEVELS; j++) {
				string key = "mm" + i + "_" + j;
				if (PlayerPrefs.HasKey (key))
					PlayerPrefs.DeleteKey (key);

			}
		PlayerPrefs.Save ();
	}

	public static int GetMoneyMakersTaken() {
		return GetMoneyMakersTaken(gameData.worldIndex, gameData.levelIndex);
	}

	public static int GetMoneyMakersTaken(int levelIndex) {
		return GetMoneyMakersTaken (gameData.worldIndex, levelIndex);
	}

	public static int GetMoneyMakersTakenPerWorld(int worldIndex) {
		int sum = 0;
		for (int j = 0; j < StaticLevels.levelsPerWorld [worldIndex]; j++) {
			sum += GetMoneyMakersTaken (worldIndex, j);
		}
		return sum;
	}

	private static int GetMoneyMakersTaken(int worldIndex, int levelIndex) {
		currentMMSkey = "mm" + worldIndex + "_" + levelIndex;
		if (PlayerPrefs.HasKey (currentMMSkey)) {
			string myMMS = PlayerPrefs.GetString (currentMMSkey);
			int cnt = 0;
			foreach (char c in myMMS) {
				if (c == '2') cnt++;
			}
			return cnt;
		}

		return 0;
	}

	public static int GetNumberOfCredits ()
	{
		Prepare ();
		creditWasAdded = false;
		if(PlayerPrefs.HasKey("CurlingKidsC")) {
			gameData.creditData.credits = PlayerPrefs.GetInt("CurlingKidsC");
		}
			return gameData.creditData.credits;
	}

	public static void AddGems (int gems)
	{
		if (gems < 1)
			return;

		gameData.creditData.gems += gems;
		Save ();
	}

	public static bool creditWasAdded = false;
	public static void AddCredits (int credits)
	{
		Prepare ();
		if (credits < 1)
			return;

		creditWasAdded = true;

		gameData.creditData.credits += credits;
		PlayerPrefs.SetInt("CurlingKidsC", gameData.creditData.credits);
		PlayerPrefs.Save();
		Save ();
	}

	public static void AddTemporaryCredits (int credits, MoneyMaker mm, bool startFromZero = false)
	{
		if (startFromZero)
			temporaryCredits = credits;
		else
			temporaryCredits += credits;

		if (mm != null) {
			int i = 0;
			foreach (MoneyMaker l_mm in currentMMSobjects) {
				if (mm == l_mm) {
					if (i < currentMMS.Length) {
						currentMMS = currentMMS.Substring (0, i) + (mm.isJewel? "2" : "1") + currentMMS.Substring (i + 1);
					}
				}
				i++;
			}
			//Debug.Log (currentMMS);
		}
	}

	public static void ResetTemporaryCredits ()
	{
		temporaryCredits = 0;
	}

	public static int GetTemporaryCredits ()
	{
		return temporaryCredits;
	}

	public static void StoreTemporaryCredits()
	{
		if (temporaryCredits > 0)
		{
			AddCredits(temporaryCredits);
		}
	
		temporaryCredits = 0;
	}

	public static bool IsAvatarUnlocked (int index)
	{
		Prepare ();
		// Debug.Log (index);
		if (PlayerPrefs.HasKey("AvatarUnlocked_" + index))
		{
			return gameData.avatarData.avatars[index].unlocked = true;
		}
		return gameData.avatarData.avatars [index].unlocked;
	}

	public static void UnlockAvatar (int index, int creditPrize)
	{
		Prepare ();

		gameData.avatarData.avatars [index].unlocked = true;
		PlayerPrefs.SetInt("AvatarUnlocked_" + index, 1);

		gameData.creditData.credits -= creditPrize;
		if (gameData.creditData.credits < 0) // sanity check, not supposed to happen
 			gameData.creditData.credits = 0;
		if (PlayerPrefs.HasKey("CurlingKidsC"))
		{
			PlayerPrefs.SetInt("CurlingKidsC",gameData.creditData.credits);
	
		}
		Save ();
		PlayerPrefs.Save();
	}

	// generate initial (alphabetically sorted), run in Prefabs/Characters:  echo { ; for f in *.prefab; do echo new SingleAvatar\(false, \"${f:0:${#f}-7}\"\), ; done ; echo }\;
	private static bool UpdateAvatars (AvatarData ad)
	{
		bool hasChanged = false;

		SingleAvatar[] avatars = new SingleAvatar[] {
			new SingleAvatar(true, "Arasinya"),
			new SingleAvatar(false, "Arkon"),
			new SingleAvatar(false, "Brian"),
			new SingleAvatar(false, "Chef Superb"),
			new SingleAvatar(false, "Chip Woodley"),
			new SingleAvatar(false, "Disco Roboto"),
			new SingleAvatar(false, "Doctor Spaceman"),
			new SingleAvatar(false, "Erkon the Magnificient"),
			new SingleAvatar(false, "Franky Fire"),
			new SingleAvatar(false, "Johan"),
			new SingleAvatar(false, "Knight Kato"),
			new SingleAvatar(false, "Kyle"),
			new SingleAvatar(false, "Maria"),
			new SingleAvatar(false, "Mike"),
			new SingleAvatar(false, "Morty the Mummy"),
			new SingleAvatar(false, "Name Unknown"),
			new SingleAvatar(false, "Ninja Go Camo"),
			new SingleAvatar(false, "Patrick"),
			new SingleAvatar(false, "Pirate Pilt"),
			new SingleAvatar(false, "Pumki"),
			new SingleAvatar(false, "Rumbo"),
			new SingleAvatar(false, "Skelly Mel"),
			new SingleAvatar(false, "Snowy the Man"),
			new SingleAvatar(false, "Timmy"),
			new SingleAvatar(false, "Undead Yeti Strong"),
			new SingleAvatar(false, "Wanda"),
			new SingleAvatar(false, "Zombie Braains"),
			new SingleAvatar(false, "Zombie Zue"),
		};

		for (int i = 0; i < avatars.Length; i++) {
			if (avatars [i].name != ad.avatars [i].name) {
				hasChanged = true;
				break;
			}
		}

		ad.selectedAvatarIndex = 0;
		for (int i = 0; i < avatars.Length; i++) {
			if (avatars [i].name == ad.selectedAvatarName) {
				ad.selectedAvatarIndex = i;
				break;
			}
		}
		int indx=0;
		foreach (SingleAvatar sa in avatars) {
			foreach (SingleAvatar sa_saved in ad.avatars) {
				if (sa_saved.name == sa.name) {
					if (sa_saved.unlocked)
					{
						sa.unlocked = sa_saved.unlocked;
						if(PlayerPrefs.HasKey("AvatarUnlocked_"+indx)) {
							sa.unlocked = true;
						}
					}
				}
			}
			indx++;
		}

		for (int i = 0; i < MAXAVATARS; i++) {
			if (i < avatars.Length) {
				ad.avatars [i].name = avatars [i].name;
				ad.avatars [i].unlocked = avatars [i].unlocked;
			} else {
				ad.avatars [i].name = "";
				ad.avatars [i].unlocked = false;
			}
		}

		if (ad.nofAvatars != avatars.Length)
			hasChanged = true;

		ad.nofAvatars = avatars.Length;

		return hasChanged;
	}

	public static bool IsNewCharPossibleToPurchase() {
		bool isPossibleToPurchase = false;

		if (!StaticManager.showCharPurchaseAvailable)
			return false;

		int credits = StaticManager.GetNumberOfCredits();

		Prepare();

		CharacterManager characterManager = CharacterManager.instance;

		for (int i = 0; i < characterManager.characterPrefabs.Count; i++)
		{
			if (!StaticManager.IsAvatarUnlocked(i))
			{
				ToonDollHelper player = characterManager.characterPrefabs[i].GetComponentInChildren<ToonDollHelper>();
				int prize = (int)((float)player.prize * StaticManager.GetPrizeMul());

				if (StaticManager.lockZombieCharacters && !StaticManager.WorldPurchased(1) && player.partOfZombiePack) { } else if (prize <= credits) { isPossibleToPurchase = true; break; }
			}
		}

		return isPossibleToPurchase;
	}


	public static bool ReorderPrefabs ()
	{
		Prepare ();

		CharacterManager cm = CharacterManager.instance;

		for (int i = 0; i < gameData.avatarData.nofAvatars; i++) {
            			
			SingleAvatar sa = gameData.avatarData.avatars [i];

			bool found = false;
			foreach (GameObject g in cm.unorderedCharacterPrefabs) {
				if (g.name == sa.name) {
					found = true;
					cm.characterPrefabs.Add (g);
				}
			}

			if (!found) {
				Debug.Log ("PANIC!! All character names in StaticManager data must be possible to find among CharacterManager prefab names!");
				return false;
			}

		}

		return true;
	}
	public static int GetWorldIndex() {
		Prepare();
		return gameData.worldIndex;
	}

	public static void Load (bool erase = false)
	{
		int i, j;
		_showzombie = true;

		if (erase && File.Exists (Application.persistentDataPath + saveName)) {
			File.Delete (Application.persistentDataPath + saveName);
		}
		nofWorlds = 0;

		// Initial values
		gameData = new GameData ();
		gameData.version = version;
		gameData.worldIndex = gameData.levelIndex = gameData.starsPlus = 0;
		gameData.lastWorldIndex = gameData.lastLevelIndex = -1;
		gameData.prizeMul = 1;
		gameData.settings = new GameSettings ();
		gameData.settings.bPlayMusic = true;
		gameData.settings.bPlaySounds = true;
		gameData.creditData = new CreditData ();
		gameData.creditData.credits = 0;
		gameData.avatarData = new AvatarData ();
		gameData.avatarData.selectedAvatarIndex = 0;
		gameData.avatarData.selectedAvatarName = "";
		gameData.avatarData.avatars = new SingleAvatar[MAXAVATARS];

		gameData.taskCompletion = new TaskCompletion ();

		for (i = 0; i < MAXAVATARS; i++) {
			gameData.avatarData.avatars [i] = new SingleAvatar ();
		}
		gameData.avatarData.nofAvatars = 0;

		gameData.levels = new LevelData[MAXWORLDS, MAXLEVELS];
		for (i = 0; i < MAXWORLDS; i++) {
			for (j = 0; j < MAXLEVELS; j++) {
				gameData.levels [i, j] = new LevelData ();
				gameData.levels [i, j].score = 0;
				gameData.levels [i, j].time = float.MaxValue;
				gameData.levels [i, j].stars = 0;
			}
			if (StaticLevels.levelsPerWorld[i] > 0)
				nofWorlds++;
		}

		if (File.Exists (Application.persistentDataPath + saveName)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + saveName, FileMode.Open);
			gameData = (GameData)bf.Deserialize (file);
			file.Close ();
		}
		if (PlayerPrefs.HasKey("CurlingKidsC"))
		{
			gameData.creditData.credits = PlayerPrefs.GetInt("CurlingKidsC");
		}
		bool wasChanged = UpdateAvatars (gameData.avatarData);
		if (wasChanged)
			Save ();
		
		bWasLoaded = true;
	}


	public static void Save ()
	{
		
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.persistentDataPath + saveName, FileMode.OpenOrCreate);

		bf.Serialize (file, gameData);
		file.Close ();
	}


	public static int GetNofStars (int worldIndex = -1)
	{
		Prepare ();

		int i, j, stars = 0;

		if (worldIndex == -1)
			worldIndex = gameData.worldIndex;

		for (i = 0; i < MAXWORLDS; i++) {
			if (i == worldIndex || worldIndex == -2) { // hack: can use -2 if want to count all stars on all worlds. Not used now though
				for (j = 0; j < MAXLEVELS; j++) {
					stars += gameData.levels [i, j].stars;
				}
			}
		}
		return stars + gameData.starsPlus;
	}

	public static void AddPermanentStars (int stars)
	{
		Prepare ();
		gameData.starsPlus += stars;
		Save ();
	}


	public static int GetNofRequiredStars(int levelIndex) {
		Prepare();
		if (gameData.worldIndex == 0)
			return (levelIndex * 2 + levelIndex / 3);
		else
			return (levelIndex * 2);
	}
	public static bool AtWorldsLastLevel() {
		int levelsPerWorld = StaticLevels.levelsPerWorld[gameData.worldIndex]-1;
		if (gameData.levelIndex >=levelsPerWorld)
			return true;
		return false;
	}

	public static bool IsNextLevelAvailable(out int remainingStars) {
		Prepare();

		remainingStars = 0;

		int nofStars = GetNofStars();
		if (gameData.levelIndex >= StaticLevels.levelsPerWorld[gameData.worldIndex] - 1)
			return false;

		int requiredStars = GetNofRequiredStars(gameData.levelIndex + 1);
		if (nofStars < requiredStars)
			remainingStars = requiredStars - nofStars;

		return nofStars >= requiredStars;
	}


	public static void ProcessProgress (LevelData results)
	{
		Prepare ();

		achievedResults = results;

		oldResults.score = gameData.levels [gameData.worldIndex, gameData.levelIndex].score;
		oldResults.stars = gameData.levels [gameData.worldIndex, gameData.levelIndex].stars;
		oldResults.time = gameData.levels [gameData.worldIndex, gameData.levelIndex].time;

		if (results.score > gameData.levels [gameData.worldIndex, gameData.levelIndex].score)
			gameData.levels [gameData.worldIndex, gameData.levelIndex].score = results.score;

		if (results.stars > gameData.levels [gameData.worldIndex, gameData.levelIndex].stars)
			gameData.levels [gameData.worldIndex, gameData.levelIndex].stars = results.stars;

		if (results.time < gameData.levels [gameData.worldIndex, gameData.levelIndex].time)
			gameData.levels [gameData.worldIndex, gameData.levelIndex].time = results.time;



		//SceneManager.LoadScene ("LevelProgress");
	}

	public static void LoadCharSelectScene()
	{
		if (LoadAsynch())
		{
			AsyncOperation async = SceneManager.LoadSceneAsync("CharSelect");
			async.allowSceneActivation = true;
		} else {
			SceneManager.LoadScene("CharSelect");
		}
	
		if (CharacterManager.instance != null)
			CharacterManager.instance.ShowLoadSpinner ();
	}


	public static LevelData GetPlayResults ()
	{
		return achievedResults;
	}

	public static LevelData GetOldPlayResults ()
	{

		return oldResults;
	}

	public static LevelData GetCurrentLevelData ()
	{
		Prepare ();	

		return gameData.levels [gameData.worldIndex, gameData.levelIndex];
	}

	public static void SetCurrentScene ()
	{ // the use of this is to set the current level based on our current level. This is useful if we want to start not from main screen but from a level scene
		Prepare ();

		string currentSceneName = SceneManager.GetActiveScene ().name;

		string[] split = currentSceneName.Split ('_');
		if (split.Length == 3) {
			gameData.worldIndex = gameData.lastWorldIndex = int.Parse (split [1]);
			gameData.levelIndex = gameData.lastLevelIndex = int.Parse (split [2]);

			//Debug.Log ("LVL: " + currentSceneName + "   " + gameData.worldIndex + " : " + gameData.levelIndex);
		}
		Save ();

		/* overkill, save for later use. w is letter, d is number. @ in front of string to not have to use double-backslash
           var r = new Regex(@"(\w+)_(\d+)_(\d+)", RegexOptions.IgnoreCase); var match = r.Match(currentSceneName); if(match.Success) { worldIndex = int.Parse(match.Groups[2].Value); levelIndex = int.Parse(match.Groups[3].Value);} */
	}

	private static bool LoadAsynch()
	{
		bool asynch = true;
#if UNITY_ANDROID
		asynch = false;
#endif
		return asynch;
	}
	private static void LoadCurrentScene ()
	{
		LogLevelProgress();
		if (LoadAsynch())
		{
			AsyncOperation async = SceneManager.LoadSceneAsync(GetCurrentSceneString());
			async.allowSceneActivation = true;
			if (CharacterManager.instance != null)
				CharacterManager.instance.ShowLoadSpinner();
		} else {
			SceneManager.LoadScene(GetCurrentSceneString());
		}
	}

	public static void LoadLastPlayedScene ()
	{
		Prepare ();

		if (!StaticManager.WorldPurchased(gameData.lastWorldIndex))
			playingFreeLevel = true;
		if (LoadAsynch())
		{
			AsyncOperation async = SceneManager.LoadSceneAsync("level_" + gameData.lastWorldIndex + "_" + gameData.lastLevelIndex);
			async.allowSceneActivation = true;
			if (CharacterManager.instance != null)
				CharacterManager.instance.ShowLoadSpinner();
		} else {
			SceneManager.LoadScene("level_" + gameData.lastWorldIndex + "_" + gameData.lastLevelIndex);
		}

	}


	public static int GetLastWorldIndex() {
		Prepare();
		return gameData.lastWorldIndex;
	}


	public static void SetLastLevelIndex(int level,int world) {
		gameData.lastLevelIndex = level;
		SetLevel(level,world);

	}
	public static void SetSceneToLastPlayedScene ()
	{
		Prepare ();
		gameData.worldIndex = gameData.lastWorldIndex;
		gameData.levelIndex = gameData.lastLevelIndex;
	}


	public static int GetCurrentLevelForWorld ()
	{
		Prepare ();

		if (gameData.worldIndex == gameData.lastWorldIndex)
			return gameData.lastLevelIndex;

		return -1;
	}

	public static string GetCurrentSceneString ()
	{
		Prepare ();
		return "level_" + gameData.worldIndex + "_" + gameData.levelIndex;
	}

	private static void LogLevelProgress() {
		try
		{
			Facebook.Unity.FB.LogAppEvent(
				"NextLevel",
				null,
				new Dictionary<string, object>()
				{
			{ "Starting Level", ""+gameData.levelIndex }
				});
		} catch(Exception e){}

	}

	public static void StartNextLevel ()
	{
		Prepare ();

		gameData.levelIndex++;

		Save ();

		LoadCurrentScene ();
	}


	public static void SetLevel (int levelIndex, int worldIndex = -1)
	{
		Prepare ();

		gameData.levelIndex = levelIndex;
		if (worldIndex >= 0)
			gameData.worldIndex = worldIndex;
		Save ();
	}

	public static void StartLevel (int levelIndex, int worldIndex = -1)
	{
		SetLevel (levelIndex, worldIndex);
		LoadCurrentScene ();
	}

	public static void PlayFreeLevel(int worldIndex, int levelIndex)
	{
		playingFreeLevel = true;
		SetLevel(levelIndex, worldIndex);
		LoadCurrentScene();
	
	}

	public static void GotoLevelSelectScreen (int worldIndex)
	{
		Prepare ();

		gameData.worldIndex = worldIndex;
		gameData.levelIndex = 0;
		Save ();

		SceneManager.LoadScene ("LevelSelect");
	}

	public static void RestartSameLevel ()
	{
		Prepare ();
		LoadCurrentScene ();
	}

	public static void GetLevel (out int worldIndex, out int levelIndex)
	{
		Prepare ();
		worldIndex = gameData.worldIndex;
		levelIndex = gameData.levelIndex;
	}
	public static int GetLevel() {
		Prepare();
		return gameData.levelIndex;
	}
	public static string GetLevelString ()
	{
		Prepare ();
		string s = "";
		if(gameData.worldIndex == 2) {
			s = I2.Loc.LocalizationManager.GetTranslation("dayofchristmas"+ (gameData.levelIndex + 1));
		}else {
			s = I2.Loc.LocalizationManager.GetTranslation("level") + " " + (gameData.worldIndex + 1) + ":" + (gameData.levelIndex + 1);
		}
		return s; 

}

	public static bool HaveFinishedAllLevelsForWorld(int worldIndex = -1) {
		Prepare();
		if (worldIndex< 0)
			worldIndex = gameData.worldIndex;
            		
		LevelData[] levels = new LevelData[StaticLevels.levelsPerWorld[worldIndex]];

		for (int i = 0; i<StaticLevels.levelsPerWorld[worldIndex]; i++) {
			if(gameData.levels[worldIndex, i].stars <=0) {
				return false;
			}
		}
		return true;
	}
	public static LevelData [] GetAllLevelsForWorld (int worldIndex = -1)
	{
		Prepare ();

		if (worldIndex < 0)
			worldIndex = gameData.worldIndex;
            		
		LevelData[] levels = new LevelData[StaticLevels.levelsPerWorld [worldIndex]];

		for (int i = 0; i < StaticLevels.levelsPerWorld [worldIndex]; i++)
			levels [i] = gameData.levels [worldIndex, i];
            			
		return levels;
	}

	public static int GetNofWorlds ()
	{
		Prepare ();
		return nofWorlds;
	}

	public static bool IsMusicOn ()
	{
		Prepare ();
		return gameData.settings.bPlayMusic;
	}

	public static bool IsSfxOn ()
	{
		Prepare ();
		return gameData.settings.bPlaySounds;
	}

	public static void ToggleMusic ()
	{
		Prepare ();
		gameData.settings.bPlayMusic = !gameData.settings.bPlayMusic;
		Save ();
	}

	public static void ToggleSfx ()
	{
		Prepare ();
		gameData.settings.bPlaySounds = !gameData.settings.bPlaySounds;
		Save ();
	}

	public static void PushScene (string scene = null)
	{
		if (scene == null)
			sceneStack.Add (SceneManager.GetActiveScene ().name);
		else
			sceneStack.Add (scene);
	}

	public static void PopScene ()
	{
		if (sceneStack.Count <= 0)
			return;
            		
		string loadScene = sceneStack [sceneStack.Count - 1];
		sceneStack.RemoveAt (sceneStack.Count - 1);
		// Debug.Log (sceneStack.Count);
		SceneManager.LoadScene (loadScene);
	}


	public static bool IsSoundEffectsEnabled ()
	{
		Prepare ();
		return gameData.settings.bPlaySounds;
	}

	public static bool IsMusicEnabled ()
	{
		Prepare ();
		return gameData.settings.bPlayMusic;
	}

	public static int GetSelectedAvatarIndex ()
	{
		Prepare ();
		return gameData.avatarData.selectedAvatarIndex;
	}

	public static void SetSelectedAvatarIndex (int index)
	{
		Prepare ();
		if (index >= 0) {
			gameData.avatarData.selectedAvatarIndex = index;
			gameData.avatarData.selectedAvatarName = gameData.avatarData.avatars [index].name;
			Save ();
		}
	}

	public static int GetUnlockedAvatarIndex ()
	{
		return unlockedAvatarIndex;
	}

	public static void SetUnlockedAvatarIndex (int index)
	{
		unlockedAvatarIndex = index;
	}

	public static MusicTune lastMusicTune = MusicTune.Undefined;
	public static int UGLYFIX_curlingWorldIndex = 0;
	public static int UGLYFIX_zombieWorldIndex = 1;
	public static int UGLYFIX_xmasWorldIndex = 2;

	public static bool UGLYFIX_IsLevelType(int targetWorldIndex)
	{

		string[] split = SceneManager.GetActiveScene().name.Split('_');
		if (split.Length == 3)
		{
			int worldIndex = gameData.lastWorldIndex = int.Parse(split[1]);
			if (worldIndex == targetWorldIndex)
				return true;
		}
		return false;
	}

	public static void PlayMusicBasedOnWorld()
	{
		bool isZombie = UGLYFIX_IsLevelType(UGLYFIX_zombieWorldIndex);
		bool isXmas = UGLYFIX_IsLevelType(UGLYFIX_xmasWorldIndex);
		SoundManager sm = SoundManager.instance;
		MusicTune mt = MusicTune.Regular;

		if (isXmas)
		{
			mt = MusicTune.XMas;
			if (lastMusicTune != mt)
			{
				sm.PlayMusic(mt);
				lastMusicTune = mt;
			}
		}
		else if (isZombie)
		{
			mt = MusicTune.Zombie;
			if (lastMusicTune != mt)
			{
				sm.PlayMusic(mt);
				lastMusicTune = mt;
			}
		}
		else
		{
			mt = MusicTune.Regular;

			if (lastMusicTune != mt)
			{
				// sm.PlayMusic(mt);
				SoundManager.instance.PlayRegularSong();
				lastMusicTune = mt;
			}
		}
	}

	public static int showComboHitTextTreshold = 2;
	public static int comboTossGoalsHit = 0;


	/* Help related methods */
	public static int awaitingFirstBuyStepsLeft = 0;
	public static bool ForceBuyNewCharacter()
	{
		if (IsAvatarUnlocked(StaticManager.GetFirstCharacterBuyIndex()))
			return false;

		if (gameData.creditData.credits >= 50 && !PlayerPrefs.HasKey("BoughtFirstCharacter"))
			return true;
		if (awaitingFirstBuyStepsLeft > 0)
			return true;
		return false;
	}
	public static void SetForceBuyNewCharacterDone()
	{
		PlayerPrefs.SetInt("BoughtFirstCharacter", 1);
		PlayerPrefs.Save();
	}

	public static bool HelpShownEnough(string prefsKey, int nofShowings)
	{
		int nof = 0;
		if (PlayerPrefs.HasKey(prefsKey))
			nof = PlayerPrefs.GetInt(prefsKey);
		nof++;
		PlayerPrefs.SetInt(prefsKey, nof);
		PlayerPrefs.Save();

		return nof > nofShowings;
	}

	public static bool HasfireWorksBeenShown() {
		Prepare();

		bool retval = PlayerPrefs.HasKey("FireWorks_" + gameData.worldIndex + "_" + gameData.levelIndex);
		if (retval == false)
		{
			PlayerPrefs.SetInt("FireWorks_" + gameData.worldIndex + "_" + gameData.levelIndex, 1);
			PlayerPrefs.Save();
		}
		return retval;
	}

	public static int GetFirstCharacterBuyIndex() {
		Prepare();

		for (int i = 0; i < gameData.avatarData.nofAvatars; i++) {
			if (gameData.avatarData.avatars[i].name == "Pirate Pilt")
				return i;
		}
		return - 1;
	}

}
