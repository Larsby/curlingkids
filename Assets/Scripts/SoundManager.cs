using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SfxRandomType
{
	None = -1,
	PlayerHitGoal = 0,
	Cheering,
	HitObject,
	Slide,
	ZombiesWin,
	BowlingPin,
	GlassBreak,
	GlassCrack,
	ConeHit,
	RefereeHit,
	PlayerHitPlayer,
	FlyingPanic,
	SlideStructure,
	Hmm,
	OffWeGo,
	StandUp,
	Collapse,
	YesWin,
	DropPanic,
	MainScreen,
	ZombieHit,
	Selected,
	NextPlayer,
	CHAR_Roberto_DropPanic,
	CHAR_Roberto_FlyingPanic,
	CHAR_Roberto_HitObject,
	CHAR_Roberto_Hmm,
	CHAR_Roberto_NextPlayer,
	CHAR_Roberto_OffWeGo,
	CHAR_Roberto_PlayerHitPlayer,
	CHAR_Roberto_Selected,
	CHAR_Roberto_StandUp,
	CHAR_Roberto_YesWin,
    CHAR_Johan_DropPanic,
    CHAR_Johan_FlyingPanic,
    CHAR_Johan_HitObject,
    CHAR_Johan_Hmm,
    CHAR_Johan_NextPlayer,
    CHAR_Johan_OffWeGo,
    CHAR_Johan_PlayerHitPlayer,
    CHAR_Johan_Selected,
    CHAR_Johan_StandUp,
    CHAR_Johan_YesWin,
    ZombieMoan,


};

public enum SingleSfx
{
	None = -1,
	Button1 = 0,
	Button2,
	GoalHit,
	Baloon1,
	Baloon2,
	Baloon3,
	Booster1,
	Begin,
	Teleport,
	GetMoney,
	Teleport2,
	Teleport3,
	Bubble,
	HitKupol,
	SlideKupol,
	ZombieMoan,
	Football,
	LevelFail,
	BowlingClear,
	FatExplosion,
	ZombieRoar,
	UnlockScreen,
	BackButton,
	BuyButton,
	NewWorld,
	TaskFinished,
	RotateAfterStand,
	ProgressStarWon,
	Popcorn,
};

public enum MusicTune
{
	Undefined = -1,
	Regular = 0,
	Zombie = 1,
	Credits = 2,
    Regular_Alternative = 3,
	XMas = 4
};


public class SoundManager : MonoBehaviour {

	private AudioSource musicPlayer = null;
	private AudioSource[] randomSfx;
	private AudioSource[] singleSfx;
	private int[] randomSfxIndex;

	private int sourceSfxPlayerCnt = 0, singleSfxCnt = 0;
	private const int MAX_SOURCES = 8; // at the moment we have this * 2 + 1 (for music) audiosources. I.e. 17. Too many, affects performance? Decrease?

	private AudioClip[][] randomSfxDatabase;
	private AudioClip[] singleSfxDatabase;
	private AudioClip[] musicSongsDatabase;

	private bool initialized = false;
	public static SoundManager instance = null;

	private MusicTune songPlaying = MusicTune.Undefined;

	public static void Create() {
		if (instance == null) {
			GameObject g = new GameObject ();
			g.AddComponent<SoundManager> ();
			MonoBehaviour.Instantiate (g);
		}
	}

	void Awake ()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);

		Initialize ();

		StaticManager.lastMusicTune = MusicTune.Regular;
		// PlayMusic (StaticManager.lastMusicTune);

		PlayRegularSong();
	}

	public void PlayRegularSong() {
//		PlayMusicSequence(new MusicTune[] { MusicTune.Regular_Alternative, MusicTune.Regular }, 0.3f, true, true); 
		PlayMusicSequence(new MusicTune[] { MusicTune.Regular_Alternative, MusicTune.Regular }, 0.3f, true, true, 1,1); 
	}


	void Start ()
	{
	}

	// quick and dirty if we want to replace all char sounds for a character
	void AddRandomForCharacter (List<string[]> randomSoundClipPaths, string sfxPathName) {
		randomSoundClipPaths.Add(new string[] { "Characters/" + sfxPathName + "/DropPanic/*" });
		randomSoundClipPaths.Add(new string[] { "Characters/" + sfxPathName + "/FlyingPanic/*" });
		randomSoundClipPaths.Add(new string[] { "Characters/" + sfxPathName + "/Hit/*" });
		randomSoundClipPaths.Add(new string[] { "Characters/" + sfxPathName + "/Hmm/*" });
		randomSoundClipPaths.Add(new string[] { "Characters/" + sfxPathName + "/NextPlayer/*" });
		randomSoundClipPaths.Add(new string[] { "Characters/" + sfxPathName + "/OffWeGo/*" });
		randomSoundClipPaths.Add(new string[] { "Characters/" + sfxPathName + "/PlayerHitPlayer/*" });
		randomSoundClipPaths.Add(new string[] { "Characters/" + sfxPathName + "/Selected/*" });
		randomSoundClipPaths.Add(new string[] { "Characters/" + sfxPathName + "/StandUp/*" });
		randomSoundClipPaths.Add(new string[] { "Characters/" + sfxPathName + "/YesWin/*" });
	}

	private void Initialize ()
	{
		int i, j;

		if (initialized)
			return;
		initialized = true;

		List<string[]> randomSoundClipPaths = new List<string[]> ();


		// Generate arrays by, in parent folder, write:
		// echo -n { ; for f in _matad/*.wav; do echo -n \"${f:0:${#f}-4}\", ; done ; echo }\;
		// alt:
		// echo } ; for f in _matad/*.wav; do echo \"${f:0:${#f}-4}\", ; done ; echo }\;

		randomSoundClipPaths.Add(new string[] { // 0
			"GoalHit/*",
		});
		randomSoundClipPaths.Add(new string[] { // 1
			"Win/*"
		});
		randomSoundClipPaths.Add(new string[] { // 2
			"Hit/*"
		});
		randomSoundClipPaths.Add(new string[] { // 3
			"Slide/*"
		});
		randomSoundClipPaths.Add(new string[] { // 4
			"ZombiesWin/*"
		});
		randomSoundClipPaths.Add(new string[] { // 5
			"Bowlingpin/*",
		});
		randomSoundClipPaths.Add(new string[] { // 6
			"Glass/*"
		});
		randomSoundClipPaths.Add(new string[] { // 7
			"GlassThump/*",
		});
		randomSoundClipPaths.Add(new string[] { // 8
			"ConeHit/*"
		});
		randomSoundClipPaths.Add(new string[] { // 9
			"RefereeHit/*"
		});
		randomSoundClipPaths.Add(new string[] { // 10
			"PlayerHitPlayer/*"
		});
		randomSoundClipPaths.Add(new string[] { // 11
			"FlyingPanic/*"
		});
		randomSoundClipPaths.Add(new string[] { // 12
			"SlideStructure/*"
		});
		randomSoundClipPaths.Add(new string[] { // 13
			"Hmm/*"
		});
		randomSoundClipPaths.Add(new string[] { // 14
			"OffWeGo/*"
		});
		randomSoundClipPaths.Add(new string[] { // 15
			"StandUp/*"
		});
		randomSoundClipPaths.Add(new string[] { // 16
			"Collapse/*"
		});
		randomSoundClipPaths.Add(new string[] { // 17
			"YesWin/*"
		});
		randomSoundClipPaths.Add(new string[] { // 18
			"DropPanic/*"
		});
		randomSoundClipPaths.Add(new string[] { // 19
			"MainScreen/*"
		});
		randomSoundClipPaths.Add(new string[] { // 20
			"ZombieHit/*"
		});
		randomSoundClipPaths.Add(new string[] { // 21
			"Selected/*"
		});
		randomSoundClipPaths.Add(new string[] { // 22
			"NextPlayer/*"
		});
		AddRandomForCharacter (randomSoundClipPaths, "RobertoRoboto"); // 23-31

		AddRandomForCharacter (randomSoundClipPaths, "Johan"); // 32-40

		string[] singleSoundPaths = new string[] {
			"button01",
			"button02",
			"goalHit1",
			"balloon_pop01",
			"balloon_pop02",
			"balloon_pop03",
			"powerup01",
			"begin",
			"teleport",
			"money1",
			"teleport2",
			"teleport3",
			"bubble",
			"hitkupol",
			"slidekupol",
			"ZombieMoan",
		    "football",
			"levelfail",
			"bowlingClear",
			"fatExplosion",
			"zombieSpawn",
			"unlockscreen",
			"backbutton",
			"buybutton",
			"newWorld",
			"taskFinished",
			"rotateAfterStand",
			"progressStarWon",
			"popcorns",
		};

        randomSoundClipPaths.Add(new string[] {  
            "ZombiesMoan/*"
        });
		string[] musicSoundPaths = new string[] {
			"regularbana",
			"zombie1",
			"hiddemenugem",
            "song2",
			"zombie-christmas"
		};

		randomSfxDatabase = new AudioClip[randomSoundClipPaths.Count][];

		musicPlayer = gameObject.AddComponent<AudioSource> ();

		randomSfx = new AudioSource[MAX_SOURCES];
		singleSfx = new AudioSource[MAX_SOURCES];

		randomSfxIndex = new int[MAX_SOURCES];

		for (i = 0; i < MAX_SOURCES; i++) {
			randomSfx [i] = gameObject.AddComponent<AudioSource> ();
			randomSfx [i].volume = 1;
			singleSfx [i] = gameObject.AddComponent<AudioSource> ();
			singleSfx [i].volume = 1;
			randomSfxIndex [i] = -1;
		}


		for (j = 0; j < randomSoundClipPaths.Count; j++) {
			if (randomSoundClipPaths [j].Length == 1 && randomSoundClipPaths [j] [0].EndsWith ("/*")) {
				string allPath = "Audio/RandomSfx/" + randomSoundClipPaths [j] [0].Substring(0, randomSoundClipPaths [j] [0].Length - 1);

				Object [] allSounds = Resources.LoadAll(allPath, typeof(AudioClip));
				//print(allPath + " : " + allSounds.Length);

				randomSfxDatabase [j] = new AudioClip[allSounds.Length];
				for (i = 0; i < allSounds.Length; i++) {
					randomSfxDatabase [j] [i] = (AudioClip)allSounds[i];	
				}

			} else {
				randomSfxDatabase [j] = new AudioClip[randomSoundClipPaths [j].Length];
				for (i = 0; i < randomSoundClipPaths [j].Length; i++) {

					string fullPath = "Audio/RandomSfx/" + randomSoundClipPaths [j] [i];

					// I think this should only be done for iOS (if string may contain special characters, like Swedish). To be tested, if necessary.
					/*	#if UNITY_IOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
					fullPath = fullPath.Normalize (System.Text.NormalizationForm.FormD);
					#endif */

					randomSfxDatabase [j] [i] = (AudioClip)Resources.Load (fullPath);
					if (randomSfxDatabase [j] [i] == null)
						Debug.Log ("Could not load audio clip: " + randomSoundClipPaths [j] [i] + " , index " + j + " " + i);
				}
			}
		}

		singleSfxDatabase = new AudioClip[singleSoundPaths.Length];
		for (i = 0; i < singleSoundPaths.Length; i++) {
			string fullPath = "Audio/Sfx/" + singleSoundPaths [i];

			singleSfxDatabase [i] = (AudioClip)Resources.Load (fullPath);
			if (singleSfxDatabase [i] == null)
				Debug.Log ("Could not load single audio clip: " + singleSoundPaths [i] + " , index " + i);
		}

		musicSongsDatabase = new AudioClip[musicSoundPaths.Length];
		for (i = 0; i < musicSoundPaths.Length; i++) {
			string fullPath = "Audio/Music/" + musicSoundPaths [i];

			musicSongsDatabase [i] = (AudioClip)Resources.Load (fullPath);
			if (musicSongsDatabase [i] == null)
				Debug.Log ("Could not load music clip: " + musicSoundPaths [i] + " , index " + i);
		}	
	}

	public int lastRandomIndex = -1;
	public float PlayRandomFromType (SfxRandomType sfxType, int forcedIndex = -1, float startDelay = 0, float volume=-1, float pitch=-1, bool randomPitch = false)
	{
		float len = 0;
		float randMax = 0.2f;
		float defaultPitch = 1;

			if (!StaticManager.IsSoundEffectsEnabled())
				return len;

			int sfxTypeIndex = (int)sfxType;

			if (sfxTypeIndex < 0)
				return 0;

			int index = UnityEngine.Random.Range(0, randomSfxDatabase[sfxTypeIndex].Length);
			if (forcedIndex >= 0)
				index = forcedIndex;

			randomSfx[sourceSfxPlayerCnt].clip = randomSfxDatabase[sfxTypeIndex][index];
			lastRandomIndex = index;
			len = randomSfx[sourceSfxPlayerCnt].clip.length;
			if (startDelay > 0)
				randomSfx[sourceSfxPlayerCnt].PlayDelayed(startDelay);
			else
				randomSfx[sourceSfxPlayerCnt].Play();

			if (pitch >= 0)
				defaultPitch = pitch;

			if (randomPitch)
				randomSfx[sourceSfxPlayerCnt].pitch = Random.Range(defaultPitch - randMax, defaultPitch + randMax);
			else
				randomSfx[sourceSfxPlayerCnt].pitch = defaultPitch;

			randomSfx[sourceSfxPlayerCnt].volume = volume >= 0 ? volume : 1;

			randomSfxIndex[sourceSfxPlayerCnt] = sfxTypeIndex;

			sourceSfxPlayerCnt++;
			if (sourceSfxPlayerCnt >= MAX_SOURCES)
				sourceSfxPlayerCnt = 0;
		

		return len;
	}

	public int GetNofSoundsForRandomType(SfxRandomType sfxType) {
		if ((int)sfxType < 0 || (int)sfxType >= randomSfxDatabase.Length)
			return -1;

		return randomSfxDatabase [(int)sfxType].Length;
	}

	public bool IsPlayingRandomSfx (SfxRandomType sfxType = SfxRandomType.None)
	{
		for (int i = 0; i < MAX_SOURCES; i++)
			if (randomSfx [i].isPlaying && (sfxType == SfxRandomType.None || (int)sfxType == randomSfxIndex[i]))
				return true;
		
		return false;
	}

	public void StopPlayingRandomSfx (SfxRandomType sfxType = SfxRandomType.None)
	{
		for (int i = 0; i < MAX_SOURCES; i++)
			if (randomSfx [i].isPlaying) {
				if (sfxType == SfxRandomType.None)
					randomSfx [i].Stop ();
				else {
					if (randomSfxIndex[i] == (int)sfxType)
						randomSfx [i].Stop ();
				}
			}
	}


	public void StopSingleSfx (SingleSfx sfxIndex = SingleSfx.None) {

		int index = (int)sfxIndex;

		if (index < -1)
			return;

		for (int i = 0; i < MAX_SOURCES; i++) {
			if ((sfxIndex == SingleSfx.None || singleSfx [i].clip == singleSfxDatabase [index]) && singleSfx [i].isPlaying)
				singleSfx [i].Stop ();
		}
	}

	public float PlaySingleSfx (SingleSfx sfxIndex, bool randomPitch = false, bool forcePlay = false, float startDelay = 0, float volume=-1, float pitch=-1)
	{
		float randMax = 0.3f;
		float len = 0;
		try
		{

			if (!StaticManager.IsSoundEffectsEnabled() && forcePlay == false)
				return len;

			int index = (int)sfxIndex;

			if (index < 0)
				return 0;
			if (singleSfx == null)
			{
				
				Debug.Log("SingleSFX is null");
				return 0;
			}

			var sing = singleSfx[singleSfxCnt];

			if (sing == null)
				return 0;
			

			sing.clip = singleSfxDatabase[index];

			if (singleSfx.Length < singleSfxCnt || singleSfx[singleSfxCnt] == null || singleSfx[singleSfxCnt].clip == null)
			{
				
				return 0;
			}
			len = singleSfx[singleSfxCnt].clip.length;

			if (randomPitch)
				sing.pitch = Random.Range(1.0f - randMax, 1.0f + randMax);
			else
				sing.pitch = 1;
			
			singleSfx[singleSfxCnt].volume = volume >= 0 ? volume : 1;
			if (pitch >= 0)
				sing.pitch = pitch;
			
			if (startDelay > 0)
				sing.PlayDelayed(startDelay);
			else
				singleSfx[singleSfxCnt].Play();
			
			singleSfxCnt++;
			if (singleSfxCnt >= MAX_SOURCES)
				singleSfxCnt = 0;
			
		} catch(System.Exception e) {
			Debug.Log("" + e);
		}
		return len;
	}

	public float PlayMusic (MusicTune tuneIndex, float volume = 0.3f)
	{
		float len = 0;

		if (!StaticManager.IsMusicEnabled ())
			return len;

		int index = (int)tuneIndex;

		musicPlayer.Stop ();
		musicPlayer.clip = musicSongsDatabase [index];
		musicPlayer.loop = true;
		musicPlayer.volume = volume;
		len = musicPlayer.clip.length;
		musicPlayer.Play ();
		songPlaying = tuneIndex;

		musicSequenceId++;

		return len;
	}


	private int oldMusicSequenceId;
	private void UpdateMusicPlayerVolume(float val) {
		if (oldMusicSequenceId == musicSequenceId && StaticManager.IsMusicEnabled())
			musicPlayer.volume = val;
	}

	private int musicSequenceId = 0;
	private RandomNonRepeating musicSequenceOrderGenerator;

	private IEnumerator RunMusicSequence(int sequenceId, float fadeInTime = 0, float fadeOutTime = 0, bool fadeOnlyFirstAndLast = false)
	{
		bool firstFaded = false;
		float orgVolume = musicPlayer.volume;

		do
		{
			int index = musicSequenceOrderGenerator.GetRandom();

			musicPlayer.Stop();
			musicPlayer.clip = musicSongsDatabase[index];
			float len = musicPlayer.clip.length;
			musicPlayer.Play();
			songPlaying = (MusicTune)index;

			if (fadeInTime > 0)
			{
				if (!(fadeOnlyFirstAndLast && firstFaded == true)) {
					oldMusicSequenceId = sequenceId;
					LeanTween.value(gameObject, UpdateMusicPlayerVolume, 0f, orgVolume, fadeInTime).setEase(LeanTweenType.linear);
					musicPlayer.volume = 0;
					firstFaded = true;
				}
			}

			if (fadeOutTime > 0 && !(fadeOnlyFirstAndLast && musicSequenceOrderGenerator.GetNofRemainingNumbers() == 0) ) {
				yield return new WaitForSeconds(len - fadeOutTime);
				oldMusicSequenceId = sequenceId;
				LeanTween.value(gameObject, UpdateMusicPlayerVolume, orgVolume, 0, fadeOutTime).setEase(LeanTweenType.linear);
				yield return new WaitForSeconds(fadeOutTime);
			}	else
				yield return new WaitForSeconds(len);
			
		} while (sequenceId == musicSequenceId && musicSequenceOrderGenerator.GetNofRemainingNumbers() > 0 && StaticManager.IsMusicEnabled());
	}

	public float PlayMusicSequence(MusicTune [] tuneIndices, float volume = 0.3f, bool playRandomOrder = false, bool loop = true, float fadeInTime = 0, float fadeOutTime = 0, bool fadeOnlyFirstAndLast = false)
	{
		float len = 0;

		if (!StaticManager.IsMusicEnabled())
			return len;
		if (tuneIndices.Length < 1)
			return len;

		int[] indices = new int[tuneIndices.Length];
		for (int i = 0; i < tuneIndices.Length; i++) {
			indices[i] = (int)tuneIndices[i];
			len += musicSongsDatabase[indices[i]].length;
		}

		musicSequenceOrderGenerator = new RandomNonRepeating(indices, loop? RandomRegenerationLoop.LoopNonRepeating : RandomRegenerationLoop.None);
		if (playRandomOrder == false) musicSequenceOrderGenerator.linearNotRandom = true;

		musicPlayer.loop = false;
		musicPlayer.volume = volume;

		musicSequenceId++;
		StartCoroutine(RunMusicSequence(musicSequenceId, fadeInTime, fadeOutTime, fadeOnlyFirstAndLast));

		return len;
	}


	public MusicTune GetPlayingSong() {
		return songPlaying;
	}


	public void StopMusic ()
	{
		musicPlayer.Stop ();
	}

	public void PauseMusic()
	{
		if (!StaticManager.IsMusicEnabled())
			return;
		
		musicPlayer.Pause();
	}

	public void UnPauseMusic()
	{
		if (!StaticManager.IsMusicEnabled())
			return;

		if (!musicPlayer.isPlaying)
			musicPlayer.UnPause();
	}


	public void FadeRandomPlayingSfx ()
	{

		for (int i = 0; i < MAX_SOURCES; i++)
			if (randomSfx [i].isPlaying)
				randomSfx [i].volume = 0.98f;

		Invoke ("ContinousFadeOfRandom", 0.05f);
	}

	private void ContinousFadeOfRandom ()
	{
		bool keepFading = false;
		int i;

		for (i = 0; i < MAX_SOURCES; i++) {
			if (randomSfx [i].isPlaying && randomSfx [i].volume < 1) {
				keepFading = true;
				randomSfx [i].volume -= 0.2f;
				if (randomSfx [i].volume <= 0) {
					randomSfx [i].Stop ();
					randomSfx [i].volume = 1;
				}
			}
		}
		
		if (keepFading)
			Invoke ("ContinousFadeOfRandom", 0.05f);
	}


	private IEnumerator FadeRandom(SfxRandomType sfxRandomType) {
		bool keepGoing;
		do {
			keepGoing = false;
			for (int i = 0; i < MAX_SOURCES; i++) {
				if ((sfxRandomType == SfxRandomType.None || randomSfxIndex [i] == (int)sfxRandomType) && randomSfx[i].isPlaying && randomSfx[i].volume > 0) {
					randomSfx [i].volume = Mathf.Clamp01(randomSfx [i].volume - 0.2f);
					keepGoing = true;
				}
			}
			yield return new WaitForSeconds(0.05f);
		} while (keepGoing);
	}

	public void FadeRandomPlayingSfx (SfxRandomType sfxRandomType = SfxRandomType.None) {

		for (int i = 0; i < MAX_SOURCES; i++) {
			if ((sfxRandomType == SfxRandomType.None || randomSfxIndex [i] == (int)sfxRandomType) && randomSfx[i].isPlaying) {
				StartCoroutine (FadeRandom(sfxRandomType));
				break;
			}
		}
	}
		
	private IEnumerator FadeSingle(SingleSfx sfx) {
		bool keepGoing;
		do {
			keepGoing = false;
			for (int i = 0; i < MAX_SOURCES; i++) {
				if ((sfx == SingleSfx.None || singleSfx [i].clip == singleSfxDatabase [(int)sfx]) && singleSfx[i].isPlaying && singleSfx[i].volume > 0) {
					randomSfx [i].volume = Mathf.Clamp01(randomSfx [i].volume - 0.2f);
					keepGoing = true;
				}
			}
			yield return new WaitForSeconds(0.05f);
		} while (keepGoing);
	}

	public void FadeSingleSfx (SingleSfx sfx = SingleSfx.None) {
		for (int i = 0; i < MAX_SOURCES; i++) {
			if ((sfx == SingleSfx.None || singleSfx [i].clip == singleSfxDatabase [(int)sfx]) && singleSfx[i].isPlaying) {
				StartCoroutine (FadeSingle(sfx));
				break;
			}
		}
	}

	public void StopAll() {
		StopPlayingRandomSfx ();
		StopSingleSfx ();
	}
	public void FadeAll() {
		FadeRandomPlayingSfx ();
		FadeSingleSfx ();
	}

}
