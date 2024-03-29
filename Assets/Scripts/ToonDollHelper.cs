using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToonDollHelper : MonoBehaviour, CollisionListener {

	public class GhostAnimation
	{
		public string animName;
		public bool restoreTorque;
		public HumanBodyBones [] affectedBones;
	}
	private Dictionary<string, GhostAnimation> ghostAnims = new Dictionary<string, GhostAnimation> ();

	private Animator animator;

	Component[] boneRig;
	float mass = .1f;
	public Transform root;
	public GameObject _model;
	public Mesh _bodyMesh;

	public Transform _headBone;

	private bool isRagDoll = false, wasTossed = false, readyForNext = false, isOutOfBounds = false, wasTossedOnce = false, wasReset = false, isActive = true;
	private PhysicMaterial physMat;

	private Vector3 endPos;
	private bool gettingUp = false;

	private bool standUpAtEnd = false;
	private bool progressiveLevel = false;
	public bool turnStandingIntoRagdollOnPlayerHit = false;
	public bool getUpAfterReenabledRagdoll = false;
	private bool allHitsTurnsRagdoll = false;

	public bool isPlayerHelper = false;

	public int prize = 100;
	public string playerDescription = "";

	// SKILLS
	public float extra_tossSpeedModifier = 1;
	public float extra_tossAngleModifier = 1;
	public float extra_tossJumpOverride = 0;
	public int extra_creditMultiplier = 1; // (affects moneymaker objects only)
	public int extra_extraDollsPerLevel = 0;
	public int extra_extraStarsPerLevel = 0;
	public int extra_extraPermanentStars = 0;
	public float extra_timeScale = 1;
	public bool extra_isHeavy = false;
	public float extra_toonPrizeMul = 1;
	public float extra_overrideDrag = -1;
	public float extra_overrideAngularDrag = -1;
	public float extra_overrideMass =  -1;
	public GravityMode extra_gravityModeOverride = GravityMode.UNSPECIFIED;
	public PhysicMaterial extra_overridePhysicsMaterial = null;
	public bool extra_airtimeSteering = false;
	public bool extra_airtimeAcceleration = false;
	public bool extra_airtimeBreak = false;
	public bool extra_airtimeAccChangeDir = false;
	public bool extra_airtimeBrkChangeDir = false;
	public bool extra_elephantFoot = false;
	public bool extra_heavyHead = false;
	public GameObject extra_particleTrail = null;
	public string extrasDescription = "";

	private bool removeRigidsOnFinished = true;

	private Rigidbody rootRb, dollRootRb;

	private Vector3 tossForce = new Vector3(1, -3, 1);
	private float xForceTossMulMod = 1;
	private int tossNumber = 0;
	private int goalHit = -1;

	private bool checkAngularVelocityMove = false;
	private float stillStandTime = 0, stillStandTimeCounter = 0;

	private int timeStepAffectIndex;

	private GravityMode gravityMode = GravityMode.GRAVITY_ON;

	private bool flipperPhysics = false;
	private float originalSleepTreshold;
	private float speedupTimer = 0, speedupForce = 10;

	private Vector3 drag_Angulardrag_Mass = new Vector3 (0.14f, 0.1f, 2.5f);
	private CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

	private ParticleSystem specialFxParticles;
	private float oldSpecialFxY;

	private Animator ghostAnimator = null;
	private bool ghostReadingEnabled = false;
	private float ghostBlendTime = 0.6f;
	private float ghostBlendTimer = float.MaxValue;
	private bool ghostFlying = false;
	private bool ghostUseMoveRotation = true;
	private GhostAnimation currentGhostAnim = null;
	private float ghostAnimDirection = 1;
	private float ghostAnimStopAfterTimeTimer = -1;
	private bool ghostAnimStopOnHitPlaying = false;

	private int nofCollidesThisFrame = 0, nofCollidesLastFrame = 0;

	public bool bGazing = false;
	public Game.FallingPanicType fallingPanic = Game.FallingPanicType.None;
	public float flyingPanicSpeedTreshold = 9f;
	public float flyingBeginPanicTime = 0.25f, flyingPanicLength = 2f;
	public float fallingPanicProbability = 1;
	private bool allowFlyingPanic = true;
	public Game.DropPanicType dropPanic = Game.DropPanicType.None;
	public float dropPanicSpeedTreshold = 2f;
	public float dropBeginPanicTime = 1.5f, dropPanicLength = 4f;
	public float dropPanicProbability = 1;
	private bool allowDropPanic = true;


	public Material materialOnHit = null;
	public bool zombieHitMode = false;
	public string zombieHitAnim = "Running";
	private bool zombieWasHit = false;
	public bool zombieIsPlayerHelperAfterFall = false;
	public bool removeRigidBodyRootOnZombieHit = false;
	public bool allowTriggerCollision = false;

	private bool wasBubbled = false, awaitingBubble = false;

	public bool allowInactivePlayerHit = false;

	private Goal goalComponent = null;

	private SoundEmitter soundEmitter;
	private List<GameObject> objectsHitThisFrame = new List<GameObject> ();
	private List<GameObject> objectsHitLastFrame = new List<GameObject> ();
	private float hitSoundMinTime = 0.8f;
	private float hitVolumeMin = 0.25f;
	private float hitSoundMinTimer = 0;
	private bool addHitObjectsOnStay = true;
	private bool levelFinished = false;

	private bool rigidBodiesRemoved = false;

	public bool playHitMoans = true;
	public float hitMoanInitialWait = 0;

	private bool stoppedPlayingSlideSound = false;

	//public SingleSfx selectedSound = SingleSfx.Selected1;

	public SfxRandomType soundDropPanic = SfxRandomType.DropPanic;
	public SfxRandomType soundFlyingPanic = SfxRandomType.FlyingPanic;
	public SfxRandomType soundHitObject = SfxRandomType.HitObject;
	public SfxRandomType soundHmm = SfxRandomType.Hmm;
	public SfxRandomType soundNextPlayer = SfxRandomType.NextPlayer;
	public SfxRandomType soundOffWeGo = SfxRandomType.OffWeGo;
	public SfxRandomType soundSelected = SfxRandomType.Selected;
	public SfxRandomType soundPlayerHitPlayer = SfxRandomType.PlayerHitPlayer;
	public SfxRandomType soundStandup = SfxRandomType.StandUp;
	public SfxRandomType soundYesWin = SfxRandomType.YesWin;
	public float soundPitch = 1;

	private bool isCelebrating = false;

	private GameObject speechBubble = null;
	private float speechTimeLeft = -1;

	public bool includeChestInAllGhostAnims = false;

	private TrailRenderer [] trails = null;

	public Material faceNeutral = null;
	public Material faceHappy = null;
	public Material faceSad = null;
	public Material faceAngry = null;
	public Material faceWorried = null;
	public Material faceScared = null;
	public Material faceBlink = null;

	public enum FaceType { Neutral, Happy, Sad, Angry, Worried, Scared, Blink };

	private SkinnedMeshRenderer smr;

	public bool partOfZombiePack = false;

	private ParticleSystem trailParticleSystem = null;

	public GameObject sleepBubblePrefab = null;
	public float sleepBubbleDelay = 1f;
	public Vector3 sleepBubbleDelta = new Vector3(0.8f, 1.3f, 0f);

	public float heartBubbleDelay = 0.01f;

	public bool doBlinking = true;
	public float minBlinkTime = 1.2f;
	public float blinkTimeExtraRange = 2f;
	private float blinkTimer = 1f;
	public bool shadedFace = false;
	private FaceType currentFace = FaceType.Neutral;

	private int continueSetRenderQValue = 2999;


	FaceType delayedFace = FaceType.Neutral;
	void DelayFace() {
		ChangeFace(delayedFace);
	}

	public void ChangeFace(FaceType faceType, float delay = -1) {
		
		if (delay > 0) {
			delayedFace = faceType;
			Invoke("DelayFace", delay);
			return;
		}

		Material mat = null;
		switch (faceType) {
			case FaceType.Neutral: mat = faceNeutral; break;
			case FaceType.Happy: mat = faceHappy; break;
			case FaceType.Sad: mat = faceSad; break;
			case FaceType.Angry: mat = faceAngry; break;
			case FaceType.Worried: mat = faceWorried; break;
			case FaceType.Scared: mat = faceScared; break;
			case FaceType.Blink: mat = faceBlink; break;
		}

		if (mat == null)
			return;

		if (smr.materials != null && smr.materials.Length > 1 && smr.materials [1] == mat)
			return;

		if (shadedFace) {
			mat = new Material(mat);
			mat.color = GameUtil.IntColor(40, 40, 40);
			Shader mobParticlesMult = Shader.Find("Mobile/Particles/Multiply");
			if (mobParticlesMult != null)
				mat.shader = mobParticlesMult;
		}

		int oldRenderQ = -666;
		if (smr.materials.Length > 1)
			oldRenderQ = smr.materials[1].renderQueue;
		Material [] newMats = new Material [2];
		newMats [0] = smr.materials [0];
		newMats [1] = mat;
		if (oldRenderQ != -666)
			mat.renderQueue = oldRenderQ;
		smr.materials = newMats;

		currentFace = faceType;
	}

	public void SetRigidBodiesRemoved() {
		rigidBodiesRemoved = true;
	}

	public static void RemoveRigidComponents(GameObject go, bool includeRoot = false, bool removeParticles = false) {

		Transform t = go.transform;
		if (includeRoot == false) {
			t = t.Find ("Root");
			if (t == null)
				return;
		}

		Joint[] js = t.GetComponentsInChildren<Joint> ();
		if (js != null) {
			foreach (Joint c in js) {
				Destroy (c);
			}
		}

		Rigidbody[] rbs = t.GetComponentsInChildren<Rigidbody> ();
		if (rbs != null) {
			foreach (Rigidbody c in rbs) {
				Destroy (c);
			}
		}

		ColliderBridge[] cbs = t.GetComponentsInChildren<ColliderBridge> ();
		if (cbs != null) {
			foreach (ColliderBridge c in cbs) {
				Destroy (c);
			}
		}

		Collider[] cs = t.GetComponentsInChildren<Collider> ();
		if (cs != null) {
			foreach (Collider c in cs) {
				Destroy (c);
			}
		}

		if (removeParticles) {
			ParticleSystem[] pss = t.GetComponentsInChildren<ParticleSystem> ();
			if (pss != null) {
				foreach (ParticleSystem c in pss) {
					c.gameObject.SetActive (false);
				}
			}
		}

		ToonDollHelper tdh = go.transform.GetComponent<ToonDollHelper> ();
		if (tdh != null)
			tdh.SetRigidBodiesRemoved ();

	}


	public void SetOutlineSize(float outlineSize) {
		#if UNITY_EDITOR
		// do nothing here since it is annoying when the materials change all the time and Git shows these changes
		#else
		if (smr != null) smr.material.SetFloat ("_OutlineSize", outlineSize);
		#endif
	}

	public void Awake() {
		if (root == null)
			root = transform.Find ("Root");
		if (_model == null) {
			Transform t = transform.Find ("MicroMale");
			if (t != null)
				_model = t.gameObject;
			else
				_model = gameObject;
		}
		if (_headBone == null) {
			_headBone = GameUtil.FindDeepChild (transform, "Head");
		}
		boneRig = gameObject.GetComponentsInChildren<Rigidbody> (); 
		disableRagdoll ();

		animator = GetComponent<Animator> ();

		rootRb = GetComponent<Rigidbody> ();
		if (root != null)
			dollRootRb = root.GetComponent<Rigidbody> ();

		/* ** Blend stuff */
		Component[] components = GetComponentsInChildren (typeof(Transform));
		foreach (Component c in components) {
			BodyPart bodyPart = new BodyPart ();
			bodyPart.transform = c as Transform;
			bodyParts.Add (bodyPart);
		}

		if (!isPlayerHelper) {
			foreach (Component c in components) {
				ColliderBridge cb = c.gameObject.AddComponent<ColliderBridge> ();
				cb.Initialize (this);
			}
		}

		if (!isPlayerHelper && playHitMoans) {
			soundEmitter = gameObject.AddComponent<SoundEmitter> ();
			soundEmitter.emitterType = SoundEmitter.EmitterType.RemoteControlled;
			soundEmitter.volume = 0.75f;
			soundEmitter.randomPitch = true;
			soundEmitter.pitch = soundPitch;
			soundEmitter.randomSfx = soundHitObject;
		}

		specialFxParticles = gameObject.GetComponentInChildren<ParticleSystem> ();
		if (specialFxParticles != null)
			oldSpecialFxY = specialFxParticles.gameObject.transform.localPosition.y;


		//HumanBodyBones [] allBones = new HumanBodyBones[] { HumanBodyBones.Hips, HumanBodyBones.Chest, HumanBodyBones.Head, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm,  HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg };

		HumanBodyBones extraBone = includeChestInAllGhostAnims? HumanBodyBones.Chest : HumanBodyBones.RightToes;

		GhostAnimation ga1 = new GhostAnimation ();
		ga1.animName = "Unlocked";
		ga1.restoreTorque = true;
		ga1.affectedBones = new HumanBodyBones[] { extraBone, HumanBodyBones.Head, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm  };
		ghostAnims.Add ("Boxing", ga1);

		GhostAnimation ga4 = new GhostAnimation ();
		ga4.animName = "FallAir";
		ga4.restoreTorque = true;
		ga4.affectedBones = new HumanBodyBones[] { extraBone, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg };
		ghostAnims.Add ("Falling", ga4);

		GhostAnimation ga5 = new GhostAnimation ();
		ga5.animName = "Floating";
		ga5.restoreTorque = true;
		//ga5.affectedBones = new HumanBodyBones[] { extraBone, HumanBodyBones.Head, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm  };
		ga5.affectedBones = new HumanBodyBones[] { extraBone, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg };
		ghostAnims.Add ("Floating", ga5);

		goalComponent = GetComponent<Goal> ();

		SetOutlineSize (0.015f);

		trails = GetComponentsInChildren<TrailRenderer>();

		smr = GetComponentInChildren<SkinnedMeshRenderer> ();

		ChangeFace(FaceType.Neutral);
	}

	public void Start() {
		
		GameObject g = GameObject.Find("GHOST");
		if (g != null) {
			ghostAnimator = g.GetComponent<Animator> ();

			if (ghostAnimator != null)
				StopGhostAnim ();
		}
	}


	private float orgAccY = float.MaxValue;
	private int counterSetpos = -1;

	private void RemoveRigidAfterFinished(bool enabledRootCollider = false) {
		RemoveRigidComponents (gameObject);
		rootRb.isKinematic = true;
		GetComponent<Collider>().enabled = enabledRootCollider;
	}

	private void SetTrailColors() {
		if (trails == null || trails.Length == 0)
			return;

		return; // nothing happens even if setting all opacities to 0... return to this later

		float startOpa = Mathf.Clamp (dollRootRb.velocity.magnitude / 100, 0, 0.4f);
		float endOpa = Mathf.Clamp (dollRootRb.velocity.magnitude / 100 - 0.2f, 0, 0.4f);

		foreach (TrailRenderer tr in trails) {
			tr.startColor = new Color(tr.startColor.r, tr.startColor.g, tr.startColor.b, startOpa); // opacity value seeems to have no effect, even set to 0.01f it's quite visible
			tr.endColor = new Color(tr.endColor.r, tr.endColor.g, tr.endColor.b, endOpa);
			GradientAlphaKey [] alphas = tr.colorGradient.alphaKeys;
			for (int i = 0; i < alphas.Length; i++) {
				if (i == 0)
					alphas[i].alpha = startOpa;
				else
					alphas[i].alpha = endOpa;
			}
			tr.colorGradient.alphaKeys = alphas;
		}
		
	}

	private float multi = 0, rotMulti = 0;

	public void Update() {
		//if (inAirTimer != 0) Debug.Log("Flying?!" + inAirTimer);
		//if (dropInAirTimer != 0) Debug.Log("Flyingdos?!" + dropInAirTimer);

		/* if (Input.GetKeyDown (KeyCode.Alpha5) && !isPlayerHelper && isActive) {
			StopGhostAnim ();
			PlayGhostAnim ("Boxing");
		}
		if (Input.GetKeyDown (KeyCode.Alpha6))
			StopGhostAnim ();
		*/

		if (StaticManager.testTrackingDoNotEnable) {
			if (Input.GetKeyDown(KeyCode.W)) multi = 3;
			if (Input.GetKeyUp(KeyCode.W)) multi = 0;
			if (Input.GetKeyDown(KeyCode.S)) multi = -2;
			if (Input.GetKeyUp(KeyCode.S)) multi = 0;
			if (multi > 0 || multi < 0) transform.position += transform.forward * multi * Time.deltaTime;
			if (Input.GetKeyDown(KeyCode.K)) rotMulti = -100;
			if (Input.GetKeyUp(KeyCode.K)) rotMulti = 0;
			if (Input.GetKeyDown(KeyCode.L)) rotMulti = 100;
			if (Input.GetKeyUp(KeyCode.L)) rotMulti = 0;
			if (rotMulti > 0 || rotMulti < 0) transform.Rotate(0, rotMulti * Time.deltaTime, 0);
		}

		if (counterSetpos >= 0) {
			transform.position = endPos;
			counterSetpos--;
		}

		if (wasTossed && gettingUp == false) {

			if (isActive) {
				bool isMoving = IsMoving();
				if (!isMoving && nofCollidesThisFrame > 0) {

					stillStandTimeCounter -= Time.deltaTime;

					if (stillStandTimeCounter <= 0) {
						isActive = false;

						ResetSleepTreshold ();

						if (awaitingBubble)
						{
							CancelInvoke("DelayedBubble");
							Bubble(savedBubblePrefab);
							awaitingBubble = false;
							readyForNext = true;
						}
						else
						{
							if (trailParticleSystem != null)
								trailParticleSystem.Stop();

							if (!standUpAtEnd)
							{
								if (removeRigidsOnFinished)
									RemoveRigidAfterFinished();
								readyForNext = true;
							}
							else
							{
								Standup();
							}
						}
					}
				} else
					stillStandTimeCounter = stillStandTime;


				SetTrailColors ();

				if (extra_airtimeSteering || extra_airtimeAcceleration || extra_airtimeBreak) {
					float xSpeed = 8;
					float zSpeed = 25;

					if (orgAccY == float.MaxValue)
						orgAccY = Input.acceleration.y;

					Vector3 dir = Vector3.zero;
					if (extra_airtimeSteering) dir.x = Input.acceleration.x;
					if (extra_airtimeAcceleration || extra_airtimeBreak) dir.z = Input.acceleration.y - orgAccY;
					if (dir.sqrMagnitude > 1)
						dir.Normalize();

					dir *= Time.deltaTime;
					dir.x *= xSpeed;
					dir.z *= zSpeed;

					dir.x = Mathf.Clamp (dir.x, -0.3f, 0.3f);
					dir.z = Mathf.Clamp (dir.z, -80, 80);

					foreach (Component c in boneRig) {
						Rigidbody rb = (Rigidbody)c;

						Vector3 newVelocity = rb.velocity;
						if (extra_airtimeSteering && (Mathf.Abs(rb.velocity.z) > 3 || extra_airtimeBrkChangeDir || extra_airtimeAccChangeDir))
							newVelocity.x += dir.x;
						
						if (extra_airtimeBreak) {
							if (dir.z < 0) {
								newVelocity.z += dir.z;
								if (rb.velocity.z > -3 && newVelocity.z < 0 && extra_airtimeBrkChangeDir == false)
									newVelocity.z = 0;
							}
						}

						if (extra_airtimeAcceleration) {
							if (dir.z > 0) {
								newVelocity.z += dir.z;
								if (rb.velocity.z < 3 && newVelocity.z > 0 && extra_airtimeAccChangeDir == false)
									newVelocity.z = 0;
							}
						}

						rb.velocity = newVelocity;
					}
				}
			}

			if (extra_isHeavy) {
				foreach (Component c in boneRig) {
					Rigidbody rb = (Rigidbody)c;
					if (rb.velocity.y > 0)
						rb.velocity = GameUtil.SetY (rb.velocity, rb.velocity.y / (1 + Time.deltaTime * 6));
				}
			}

			if (flipperPhysics) {

				if (speedupTimer > 0) {
					foreach (Component c in boneRig) {
						Rigidbody rb = (Rigidbody)c;
						if (rb.velocity.z > 0 && rb.velocity.z < 50) {
							rb.velocity = GameUtil.SetZ (rb.velocity, rb.velocity.z * (1 + Time.deltaTime * speedupForce));
						}
					}
					speedupTimer -= Time.deltaTime;
				}
			}

			CheckAnimationState ();
		}

		if (readyForNext)
			ghostAnimator = null;

		if (speechTimeLeft >= 0 && speechBubble != null) {
			speechBubble.transform.position = animator.GetBoneTransform (HumanBodyBones.Head).position + speechBubblePos;
			speechTimeLeft -= Time.deltaTime;

			if (speechTimeLeft < 0.5f && speechBubbleDisappering == false) {
				LeanTween.scale (speechBubble.gameObject, Vector3.zero, 0.5f).setEaseInBounce ();
				speechBubbleDisappering = true;
			}
			if (speechTimeLeft < 0) {
				Destroy (speechBubble);
				speechBubble = null;
				speechBubbleDisappering = false;
			}
		}

		if (doBlinking && faceBlink) {
			blinkTimer -= Time.deltaTime;
			if (blinkTimer < 0 && currentFace == FaceType.Neutral) {
				ChangeFace(ToonDollHelper.FaceType.Blink);
				ChangeFace(ToonDollHelper.FaceType.Neutral, 0.2f);
				blinkTimer = minBlinkTime + Random.Range(0, blinkTimeExtraRange);
			}
		}

		SetRenderQueue(continueSetRenderQValue);
	}

	private bool forcedGround = false;

	private void Standup() {
		disableRagdoll ();

		rootRb.isKinematic = false;
		rootRb.useGravity = true;

		timeStepAffectIndex = StaticManager.PushFixedTimeStep (0.01f); // needed for smooth standup, better than before atleast (needed 0.004-0.006)

		if (animator.GetBoneTransform (HumanBodyBones.Hips).forward.y > 0)
			PlayAnim ("GetUpBack");
		else
			PlayAnim ("GetUpFront"); // this anim is 7-8 s long! Gets cut after 2s, looks ok anyway

		if (specialFxParticles != null) {
			LeanTween.moveLocalY (specialFxParticles.gameObject, oldSpecialFxY, 1f);
		}

		endPos = root.position;
		gettingUp = true;

		if (progressiveLevel == false) {
			readyForNext = true; // don't wait for standup anim
		} else {
			SoundManager.instance.PlayRandomFromType(soundStandup, -1, 0, -1, soundPitch, true);
		}

		Invoke ("ReadyNext", 2f);
		Invoke ("RestoreTimeStep", ragdollToMecanimBlendTime + 0.1f);

		//Invoke ("RotateToFrontFacing", 0.2f); // disable to wait with rotation until up

		// ** Blend stuff
		ragdollingEndTime = Time.time; //store the state change time
		state = RagdollState.blendToAnim;

		foreach (BodyPart b in bodyParts) {
			b.storedRotation = b.transform.rotation;
			b.storedPosition = b.transform.position;
		}
		ragdolledFeetPosition = 0.5f * (animator.GetBoneTransform (HumanBodyBones.LeftToes).position + animator.GetBoneTransform (HumanBodyBones.RightToes).position);
		ragdolledHeadPosition = animator.GetBoneTransform (HumanBodyBones.Head).position;
		ragdolledHipPosition = animator.GetBoneTransform (HumanBodyBones.Hips).position;

		transform.position = endPos;
		counterSetpos = forcedGround? 1000 : 30; // setting this position again later attempts to remove a strange bug where the player sometimes would return to its start pos while attempting to stand up
		forcedGround = false;

	}

	public bool IsAwaitingBubble() {
		return awaitingBubble;
	}

	private GameObject savedBubblePrefab;
	private void DelayedBubble() {
		Bubble(savedBubblePrefab);
	}

	public void Bubble(GameObject bubblePrefab, float bubbleDelay = 0) {

		if (bubbleDelay > 0) {
			savedBubblePrefab = bubblePrefab;
			CancelInvoke("DelayedBubble");
			Invoke("DelayedBubble", bubbleDelay);
			awaitingBubble = true;
			return;
		}

		RemoveRigidComponents (gameObject, true);

		readyForNext = true;
		wasTossed = false;

		wasBubbled = true;

		if (!gameObject.activeSelf)
			return;

		GameObject bubble = Instantiate(bubblePrefab, animator.GetBoneTransform (HumanBodyBones.Neck), false);

		if (trailParticleSystem != null)
			trailParticleSystem.Stop();

		bubble.transform.localScale = new Vector3 (0,0,0);
		LeanTween.scale (bubble.gameObject, new Vector3 (2, 2, 2), 1.0f).setEaseOutBounce ();

		if (gettingUp)
			gameObject.transform.position = endPos;
		LeanTween.move(gameObject, GameUtil.AddY(gameObject.transform.position, 150), 150f);

		smr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
	}

	public bool WasBubbled() {
		return wasBubbled;
	}

	private float inAirTimer = 0, orgBeginPanicTime = -1;
	private float dropInAirTimer = 0;

	private void CheckAnimationState() {

		if (orgBeginPanicTime == -1)
			orgBeginPanicTime = flyingBeginPanicTime;

		if (fallingPanic != Game.FallingPanicType.None && allowFlyingPanic && !(currentGhostAnim != null && currentGhostAnim.animName == "Floating" && ghostFlying)) {

			if (nofCollidesThisFrame == 0 && ghostFlying == false && inAirTimer > flyingBeginPanicTime && (fallingPanic == Game.FallingPanicType.Normal || ((animator.GetBoneTransform (HumanBodyBones.Hips).forward.z) > 0.2f && (animator.GetBoneTransform (HumanBodyBones.Hips).forward.y) < -0.2f && Mathf.Abs(animator.GetBoneTransform (HumanBodyBones.Hips).forward.x) < 20.8f))) {
				SoundManager.instance.PlayRandomFromType(soundFlyingPanic, -1, 0, -1, soundPitch);
				PlayGhostAnim ("Falling", 0.5f);
				ChangeFace(FaceType.Worried);
				ghostFlying = true;
			}
			if (ghostFlying && inAirTimer > flyingBeginPanicTime + flyingPanicLength) {
				ghostAnimDirection = -1;
				inAirTimer = -1000;
				flyingBeginPanicTime = orgBeginPanicTime + 0.55f;
			}
			if (nofCollidesThisFrame > 0) {
				if (ghostFlying) {
					StopGhostAnim ();
					ghostFlying = false;
					flyingBeginPanicTime = orgBeginPanicTime + 0.55f;
				}
				inAirTimer = 0;
			}
			if (nofCollidesThisFrame == 0 && !ghostFlying && Mathf.Abs(dollRootRb.velocity.x) + Mathf.Abs(dollRootRb.velocity.z) > flyingPanicSpeedTreshold) {
				//print (Mathf.Abs (dollRootRb.velocity.x) + Mathf.Abs (dollRootRb.velocity.z));
				inAirTimer += Time.deltaTime;
			}
			if (nofCollidesThisFrame == 0 && ghostFlying) {
				inAirTimer += Time.deltaTime;
			}
	
		}

		if (dropPanic != Game.DropPanicType.None && allowDropPanic && !(currentGhostAnim != null && currentGhostAnim.animName == "Falling" && ghostFlying)) {
			//print (Mathf.Abs (dollRootRb.velocity.x/2) + Mathf.Abs (dollRootRb.velocity.z));

			if (nofCollidesThisFrame == 0 && ghostFlying == false && dropInAirTimer > dropBeginPanicTime) {
				//SoundManager.instance.PlayRandomFromType (SfxRandomType.DropPanic, -1, 0, -1, 0.45f);
				SoundManager.instance.PlayRandomFromType(soundDropPanic, -1, 0, -1, soundPitch);

				ChangeFace(FaceType.Scared);
				PlayGhostAnim ("Floating", 0.5f);
				ghostFlying = true;
			}
			if (ghostFlying && dropInAirTimer > dropBeginPanicTime + dropPanicLength) {
				ghostAnimDirection = -1;
				dropInAirTimer = -1000;
				//dropBeginPanicTime = 0.8f;
			}
			if (nofCollidesThisFrame > 0) {
				if (ghostFlying) {
					StopGhostAnim ();
					ghostFlying = false;
					//dropBeginPanicTime = 0.8f;
				}
				dropInAirTimer = 0;
			}
			if (nofCollidesThisFrame == 0 && !ghostFlying && Mathf.Abs(dollRootRb.velocity.x/2) + Mathf.Abs(dollRootRb.velocity.z) < dropPanicSpeedTreshold) {
				//print (Mathf.Abs (dollRootRb.velocity.x/2) + Mathf.Abs (dollRootRb.velocity.z));

				if (dropPanic == Game.DropPanicType.GoingUpOrDown || (dropPanic == Game.DropPanicType.Normal && dollRootRb.velocity.y < 0f) || (dropPanic == Game.DropPanicType.GoingUp && dollRootRb.velocity.y > 0f) )
				dropInAirTimer += Time.deltaTime;
			}
			if (nofCollidesThisFrame == 0 && ghostFlying) {
				dropInAirTimer += Time.deltaTime;
			}
		}

		if (ghostAnimStopAfterTimeTimer >= 0)
			ghostAnimStopAfterTimeTimer -= Time.deltaTime;
		if (ghostAnimStopOnHitPlaying && ghostAnimStopAfterTimeTimer < 0 && nofCollidesThisFrame > 0) {
			if (currentGhostAnim != null)
				StopGhostAnim();
			ghostAnimStopOnHitPlaying = false;
		}

	}

	public bool ZombieWasHit() {
		if (zombieHitMode && zombieWasHit)
			return true;
		else
			return false;
	}

	public bool IsActive() {
		return isActive;
	}
	public void SetActive(bool active) {
		isActive = active;
	}

	public void SetRigidValues(Vector3 dam, CollisionDetectionMode cdm, bool removeRigidsOnFinished) {
		this.removeRigidsOnFinished = removeRigidsOnFinished;
		drag_Angulardrag_Mass = dam;
		collisionDetectionMode = cdm;
	}

	public void SetKinematic() {
		counterSetpos = -1;

		if (!wasBubbled) {
			rootRb = GetComponent<Rigidbody> ();
			rootRb.isKinematic = true;
		}

		gettingUp = false;
	}

	private void SetKinematicAfterGetup() {
		counterSetpos = -1;

		if (!wasBubbled) {
			rootRb = GetComponent<Rigidbody> ();
			rootRb.isKinematic = true;
		}

		if (removeRigidsOnFinished)
			RemoveRigidAfterFinished (true);
		
		gettingUp = false;
	}

	private void ReadyNext() {
		readyForNext = true;
		TrigAnim ("Idle"); // I used to use PlayAnim for non-progressive levels.. What was the advantage, I can't remember? The disadvantage is no blending, obviously...

		Invoke ("SetKinematicAfterGetup", 0.6f);
		if (progressiveLevel) {
			LeanTween.rotateY (gameObject, Camera.main.transform.rotation.eulerAngles.y, 0.6f).setEaseInCubic ();
			SoundManager.instance.PlaySingleSfx (SingleSfx.RotateAfterStand, false);
		}
	}

	private void RestoreTimeStep() {
		StaticManager.RestoreTimeStep(timeStepAffectIndex);
	}

	public bool IsGettingUp() {
		return gettingUp;
	}

	public bool IsRagDoll() {
		return isRagDoll;
	}

	public bool IsPlayerHelper() {
		return isPlayerHelper;
	}

	public void SetTossForce(Vector3 tossForce, float xForceMulMod = 1) {
		this.tossForce = tossForce;
		xForceTossMulMod = xForceMulMod;
	}

	private void RotateToFrontFacing () {
		LeanTween.rotateY (gameObject, Camera.main.transform.rotation.eulerAngles.y, 1.7f).setEaseInCubic ();
	}

	public void SetFlipperPhysics(bool state) {
		flipperPhysics = state;
	}
	public void SetSpeedupTimer(float time, float force) {
		speedupTimer = time;
		speedupForce = force;
	}

	private void ResetSleepTreshold() {
		if (flipperPhysics == false)
			return;

		foreach (Component c in boneRig)
			(c as Rigidbody).sleepThreshold = originalSleepTreshold;
	}


	private float [] stopTimes 	=    new float[] { 2, 4, 8 };
	private float [] moveTresholds = new float[] { 2, 3, 7 };
	private float [] stopTimers =    new float[] { 1, -1, -1 };

	public bool IsMoving() {
		bool isMoving = false;

		foreach (Component c in boneRig) {
			Rigidbody rb = (Rigidbody)c;

			if (Mathf.Abs (rb.velocity.x) > 0.2f || Mathf.Abs (rb.velocity.y) > 0.2f || Mathf.Abs (rb.velocity.z) > 0.2f)
				isMoving = true;

			if (checkAngularVelocityMove) {
				if (Mathf.Abs (rb.angularVelocity.x) > 2f || Mathf.Abs (rb.angularVelocity.y) > 2f || Mathf.Abs (rb.angularVelocity.z) > 2f)
					isMoving = true;
			}
		}

		float magnitude = dollRootRb.velocity.magnitude;
		for (int i = 0; i < stopTimes.Length; i++) {

			if (magnitude < moveTresholds[i]) {
				if (stopTimers[i] < 0)
					stopTimers[i] = 0;
				stopTimers[i] += Time.deltaTime;
				if (stopTimers [i] > stopTimes [i]) {
					isMoving = false; // Debug.Log ("Stopping - timer " + i);
				}
			} else {
				stopTimers[i] = -1;
			}
		}

		if (isCelebrating)
			return true;

		return isMoving;
	}


	private float noCollideTimer = -1;
	private float lowSpeedTimer = -1;

	public float nextDollButton_noCollideTime = 3f;
	public float nextDollButton_lowSpeedMagnitude = 10f;
	public float nextDollButton_lowSpeedTime = 2f;

	public void SetShowNextDollButtonLimits(float noCollideTime, float lowSpeedMagnitude, float lowSpeedTime) {
		nextDollButton_noCollideTime = noCollideTime;
		nextDollButton_lowSpeedMagnitude = lowSpeedMagnitude;
		nextDollButton_lowSpeedTime = lowSpeedTime;
	}

	public bool showNextDollButton() {

		if (flipperPhysics)
			return false;

		if (nofCollidesThisFrame == 0) {
			if (noCollideTimer < 0)
				noCollideTimer = Time.deltaTime;
			else
				noCollideTimer += Time.deltaTime;
			if (noCollideTimer > nextDollButton_noCollideTime)
				return true;
		} else
			noCollideTimer = -1;

		float magnitude = dollRootRb.velocity.magnitude;
		if (magnitude < nextDollButton_lowSpeedMagnitude) {
			if (lowSpeedTimer < 0)
				lowSpeedTimer = Time.deltaTime;
			else
				lowSpeedTimer += Time.deltaTime;
			
			if (lowSpeedTimer > nextDollButton_lowSpeedTime)
				return true;
		} else {
			lowSpeedTimer = -1;
		}

		return false;
	}

	public Vector3 GetRootPos() {
		return root.position;
	}


	private List<Vector3> angularVelocities = null;

	public void PlayGhostAnim(string ghostAnimName, float blendTime = 0.6f, bool inheritRootRotation = true, float stopOnHitAfterTime = -1) {
		if (ghostAnimator != null) {

			GhostAnimation ga = ghostAnims[ghostAnimName];
			if (ga != null)
			{
				ghostAnimator.Play(ga.animName);
				currentGhostAnim = ga;
				ghostReadingEnabled = true;

				if (stopOnHitAfterTime >= 0) {
					ghostAnimStopAfterTimeTimer = stopOnHitAfterTime;
					ghostAnimStopOnHitPlaying = true;
				}

				ghostBlendTimer = 0;
				ghostBlendTime = blendTime;
				ghostAnimDirection = 1;

				if (inheritRootRotation)
					ghostAnimator.gameObject.transform.rotation = dollRootRb.transform.rotation;

				GameUtil.SetDeepLayer (transform, LayerMask.NameToLayer ("PlayerBody"));

				if (ga.restoreTorque)
					angularVelocities = new List<Vector3> ();

				foreach (HumanBodyBones bone in currentGhostAnim.affectedBones) {

					Transform t2 = animator.GetBoneTransform (bone);
					Rigidbody rb = t2.gameObject.GetComponent<Rigidbody> ();
					if (rb != null) {
						rb.constraints = RigidbodyConstraints.FreezeRotation;
						if (ga.restoreTorque)
							angularVelocities.Add (rb.angularVelocity);
					}
				}

				// freezing rotation on all rb's removes the artefacts shown for e.g. Undead Yeti (but obviously it looks retarded to freeze all non-ghosted limbs)
				/* foreach (Component c in boneRig) {
					((Rigidbody)c).constraints = RigidbodyConstraints.FreezeRotation; 
					// ((Rigidbody)c).interpolation = RigidbodyInterpolation.None; ((Rigidbody)c).detectCollisions = false; ((Rigidbody)c).maxDepenetrationVelocity = 0; ((Rigidbody)c).solverIterations = 1000; ((Rigidbody)c).solverVelocityIterations = 1000; // no
					// trying to modify joints (swinglimits / preprocessing also failed
				} */


			} else
				Debug.Log ("Could not find ghost anim to play");
		}
	}

	public void StopGhostAnim() {
		if (ghostAnimator != null) {
			ghostAnimator.Play ("DoNothing");
			ghostReadingEnabled = false;

			GameUtil.SetDeepLayer (transform, LayerMask.NameToLayer("PlayerBody"));

			ChangeFace(FaceType.Neutral);

			if (currentGhostAnim != null) {
				int i = 0;
				foreach (HumanBodyBones bone in currentGhostAnim.affectedBones) {

					Transform t2 = animator.GetBoneTransform (bone);
					Rigidbody rb = t2.gameObject.GetComponent<Rigidbody> ();
					if (rb != null) {
						rb.constraints = RigidbodyConstraints.None;
						if (currentGhostAnim.restoreTorque && angularVelocities != null)
							rb.AddRelativeTorque (angularVelocities[i++]);
					}
				}
				if (currentGhostAnim.restoreTorque) {
					if (angularVelocities != null)
						angularVelocities.Clear ();
					angularVelocities = null;
				}

			}

			GameUtil.SetDeepLayer (transform, LayerMask.NameToLayer("Default"));
			ghostAnimDirection = 1;
		}
	}

	public void FadeGhostAnim() {
		if (currentGhostAnim != null)
			ghostAnimDirection = -1;
	}



	public void disableRagdoll(bool blendAnim = false) {
		isRagDoll = false;

		if (boneRig == null)
			return;

		foreach(Component ragdoll in boneRig) {

			Rigidbody rb = ragdoll.GetComponent<Rigidbody> ();

			if(rb != null && ragdoll.GetComponent<Collider>()!=this.GetComponent<Collider>()){
				ragdoll.GetComponent<Collider>().enabled = false;
				rb.isKinematic = true;
				rb.mass = 0.01f;
			}

			if (rb != null) {
				rb.interpolation = RigidbodyInterpolation.None;
				rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
			}
		}
		GetComponent<Collider>().enabled = true;
	}
		
	public enum GravityMode
	{
		GRAVITY_ON,
		GRAVITY_OFF,
		GRAVITY_ROOT,
		UNSPECIFIED
	};

	public void enableRagdoll(GravityMode gMode) {

		isRagDoll = true;

		foreach(Component ragdoll in boneRig) {
			if (ragdoll.GetComponent<Collider> () != null) {
				ragdoll.GetComponent<Collider> ().enabled = true;
				ragdoll.GetComponent<Collider> ().material =  physMat;
			}
			if (ragdoll.GetComponent<CharacterJoint> () != null) {
				ragdoll.GetComponent<CharacterJoint> ().enablePreprocessing = true;
				ragdoll.GetComponent<CharacterJoint> ().enableProjection = true;
			}

			Rigidbody rb = ragdoll.GetComponent<Rigidbody> ();

			if (rb != null) {
				rb.isKinematic = false; 
				rb.mass = mass;
				if (extra_overrideMass >= 0)
					rb.mass = extra_overrideMass;
				if (extra_overrideDrag >= 0)
					rb.drag = extra_overrideDrag;
				if (extra_overrideAngularDrag >= 0)
					rb.angularDrag = extra_overrideAngularDrag;
				rb.useGravity = true;
				if (gMode == GravityMode.GRAVITY_OFF || (gMode == GravityMode.GRAVITY_ROOT && ragdoll.gameObject.name != "Root"))
					rb.useGravity = false;

				rb.interpolation = isPlayerHelper ? RigidbodyInterpolation.None : RigidbodyInterpolation.Interpolate;
				rb.collisionDetectionMode = isPlayerHelper ? CollisionDetectionMode.Discrete : collisionDetectionMode;
			}
		}
		animator.enabled=false;
		GetComponent<Collider>().enabled = false;	
		Destroy(GetComponent<BotControlScript>());
		if (GetComponent<Rigidbody> () != null) {
			GetComponent<Rigidbody> ().isKinematic = true;
			GetComponent<Rigidbody> ().useGravity = false;
		}

		animator.enabled = false;
	}

	public void SetCollisionDetectionMode(CollisionDetectionMode cdm) {
		collisionDetectionMode = cdm;
		foreach(Component ragdoll in boneRig) {
			Rigidbody rb = ragdoll.GetComponent<Rigidbody> ();
			if (rb != null) {
				rb.collisionDetectionMode = isPlayerHelper ? CollisionDetectionMode.Discrete : collisionDetectionMode;
			}
		}
	}

	public int GetTossNumber() {
		return tossNumber;
	}

	private void InitPhysics(GravityMode gMode) {
		gravityMode = gMode;
		if (extra_gravityModeOverride != GravityMode.UNSPECIFIED)
			gravityMode = extra_gravityModeOverride;

		enableRagdoll (gravityMode);

		foreach (Component c in boneRig) {
			Rigidbody rb = c.GetComponent<Rigidbody> ();

			rb.drag = drag_Angulardrag_Mass.x;
			rb.angularDrag = drag_Angulardrag_Mass.y;
			rb.mass = drag_Angulardrag_Mass.z;
			if (extra_elephantFoot) {
				rb.mass = 0.8f;
				rb.drag = 0.05f;
			}
			if (extra_heavyHead)
				rb.mass = 1f;

			if (flipperPhysics)
				originalSleepTreshold = rb.sleepThreshold;
			rb.sleepThreshold = 0;

			if (c.gameObject.name == "Boja") {
				rb.drag = 2;
				rb.angularDrag = 1;
				rb.mass = 50;
			}
		}

		wasTossed = wasTossedOnce = true;
		tossNumber++;

		stillStandTimeCounter = stillStandTime;

		if (extra_elephantFoot) {
			Transform t2 = animator.GetBoneTransform (HumanBodyBones.LeftLowerLeg);
			Rigidbody rb = t2.gameObject.GetComponent<Rigidbody> ();
			rb.mass = 20;
			rb.drag = 0.3f;
		}

		if (extra_heavyHead) {
			Transform t2 = animator.GetBoneTransform (HumanBodyBones.Head);
			Rigidbody rb = t2.gameObject.GetComponent<Rigidbody> ();
			rb.mass = 20;
			rb.drag = 0.25f;
		}
	}


	public void Toss(float forcePos, float anglePos, float skew, bool enableMinimumZForce = false, GravityMode gMode = GravityMode.GRAVITY_ON) {

//		SetRenderQueue(3000);

		InitPhysics (gMode);

		stoppedPlayingSlideSound = false;
		hitSoundMinTimer = hitMoanInitialWait;

		SoundManager.instance.FadeRandomPlayingSfx(soundHmm);
		SoundManager.instance.PlayRandomFromType(soundOffWeGo, -1, 0, -1, soundPitch, true);

		float newForce = 0.5f + forcePos / 1.8f;
		float div = 30;
		float zForce = newForce * 600/div;
		if (enableMinimumZForce && zForce < 200/div)
			zForce = 200/div;

		float xForce = ((anglePos) + skew / 30f) * 220 * Mathf.Clamp (forcePos * 2f, 0.6f, 0.95f) / div * tossForce.x;

		float yForce = tossForce.y;
		if (extra_tossJumpOverride != 0)
			yForce = extra_tossJumpOverride;

		foreach (Component c in boneRig) {

			if (StaticManager.newEndforceCalculation) { // Note: this new calc doesn't use any of the above calculations! Also note that tossForce.x is not used because we don't want x force to lose its relation to z force calculated by SwipeReader
				(c as Rigidbody).velocity = new Vector3 (anglePos * 22 * tossForce.z * xForceTossMulMod * extra_tossSpeedModifier, yForce, forcePos * 22 * extra_tossSpeedModifier * tossForce.z) * StaticManager.GetGlobalGravityMod();
			} else {
				//if (c.gameObject.name != "Head" && c.gameObject.name != "Spine1")
				(c as Rigidbody).velocity = new Vector3 (xForce * extra_tossAngleModifier, yForce, (zForce * tossForce.z) * extra_tossSpeedModifier);
			}
		}

		if (specialFxParticles != null) {
			LeanTween.moveLocalY (specialFxParticles.gameObject, 0, 1f);
		}

		flyingBeginPanicTime = 0.25f;

		allowFlyingPanic = false; if (Random.Range(0f, 1.0f) <= fallingPanicProbability) allowFlyingPanic = true;
		allowDropPanic = false; if (Random.Range(0f, 1.0f) <= dropPanicProbability) allowDropPanic = true;

		if (trailParticleSystem != null)
			trailParticleSystem.Play();
	}

	public void Drop(GravityMode gMode = GravityMode.GRAVITY_ON) {
//		SetRenderQueue(3000);

		InitPhysics (gMode);

		if (specialFxParticles != null) {
			specialFxParticles.gameObject.transform.localPosition = GameUtil.SetY (specialFxParticles.gameObject.transform.localPosition, 0);
		}

		allowFlyingPanic = false; if (Random.Range(0f, 1.0f) <= fallingPanicProbability) allowFlyingPanic = true;
		allowDropPanic = false; if (Random.Range(0f, 1.0f) <= dropPanicProbability) allowDropPanic = true;
	}


	public bool IsReadyForNext() {
		return readyForNext;
	}

	public bool WasTossed() {
		return wasTossed;
	}

	public void SetInactive () {
		readyForNext = true;
		wasTossed = false;
	}

	public void SetOutOfBounds (bool hideDoll = true) {
		readyForNext = true;
		wasTossed = false;
		isOutOfBounds = true;
		if (hideDoll)
			this.gameObject.SetActive (false);
	}

	public bool IsOutOfBounds() {
		return isOutOfBounds;
	}

	public void SetMaterial(PhysicMaterial physMat, bool forceSet = false, bool setImmediately = false) {
		this.physMat = physMat;

		if (extra_overridePhysicsMaterial != null && forceSet == false)
			this.physMat = extra_overridePhysicsMaterial;

		if (setImmediately) {
			foreach (Component ragdoll in boneRig)
				if (ragdoll.GetComponent<Collider>() != null)
					ragdoll.GetComponent<Collider>().material = physMat;
		}
	}


	public void PlayAnim(string animName, float animSpeed = 1) {
		animator.speed = 1 * animSpeed;
		animator.enabled = true;
		animator.Play (animName);
	}

	public void TrigAnim(string triggerName, float animSpeed = 1) {
		animator.speed = 1 * animSpeed;
		animator.enabled = true;
		animator.SetTrigger (triggerName);
	}

	public void Reset() {
		readyForNext = false;
		wasTossed = false;
		// gettingUp = false; // we no longer wait for standing up, so set this elsewhere after standup finished
		isOutOfBounds = false;
		disableRagdoll ();
		wasReset = true;
	}

	public void SetStandup(bool standup, bool progressive, bool turnStandingIntoRagdoll, bool getUpAfterReenabledRagdoll) {
		standUpAtEnd = standup;
		progressiveLevel = progressive;
		if (progressiveLevel)
			standUpAtEnd = true;
		this.turnStandingIntoRagdollOnPlayerHit = turnStandingIntoRagdoll;
		this.getUpAfterReenabledRagdoll = getUpAfterReenabledRagdoll;
	}

	public void HitGoal (int goalIndex) {
		goalHit = goalIndex;
	}

	public int HasHitGoal() {
		int retval = goalHit;
		goalHit = -1;
		return retval;
	}


	GameObject affectedColliderBridgeObject = null;
	bool headHit = false;
	public void SetAffectedColliderBridgeObject(GameObject go) {
		affectedColliderBridgeObject = go;
	}


	private int skinChangeCount = 4; // 8;
	private float skinChangeTime = 0.3f; // 0.4f;
	private bool skinChanged = false;
	private bool actualSkinChange = false;
	private Material orgMat = null;
	void ChangeSkin() {

		if (orgMat == null)
			orgMat = smr.materials[0];

		skinChanged = !skinChanged;

		Material [] mat = new Material[1];
		mat[0] = skinChanged? materialOnHit : orgMat;
		smr.materials = mat;

		//skinChangeTime *= 0.8f;

		if (skinChangeCount-- > 0 || (!skinChanged && actualSkinChange)) {
			Invoke ("ChangeSkin", skinChangeTime);
			//Invoke ("ChangeSkin", skinChanged? 0.4f - skinChangeTime : skinChangeTime);
		} else {
			if (goalComponent != null && goalComponent.GetNofTimesHit () < goalComponent.GetNofRequiredHits ()) {
				zombieWasHit = false;
				// print ("GHit: " + goalComponent.GetNofTimesHit());
			}
		}
	}


	private void AddToHitObjects(GameObject g) {
		if (!objectsHitThisFrame.Contains (g) && g.GetComponent<SoundEmitter> () == null && !(g.transform.parent != null && g.transform.parent.name == "Ground") && g.GetComponent<Goal>() == null && g.GetComponent<Booster>() == null && g.GetComponent<BoosterSimple>() == null && g.GetComponent<Breakable>() == null) {
			NoHitSound nhs = g.GetComponent<NoHitSound> ();
			if (nhs != null) {
				if (nhs.type == NoHitSound.NoHitSoundType.Never)
					return;
				if (nhs.type == NoHitSound.NoHitSoundType.OnlyComingFromAir && nofCollidesLastFrame > 0)
					return;
			}

			objectsHitThisFrame.Add (g);
		}
	}

	private void CreateSleepBubble() {
		GameObject sleepBubble = Instantiate(sleepBubblePrefab);
		FollowObject follow = sleepBubble.GetComponent<FollowObject>();
		follow.SetFollowThis(_headBone.gameObject);
		follow.positionOffset = sleepBubbleDelta;
	}

	private void CheckColliderEnter(Collider myCollider)
	{

		/* if (collision.collider.gameObject.tag == "Sweeper") { // very specific use case for specific object
			foreach (Component c in boneRig) {
				(c as Rigidbody).maxDepenetrationVelocity = float.MaxValue;
			}
		} */

		GameObject findMe = GameUtil.FindParentWithTag(myCollider.gameObject, "Player");

		if (findMe != null && findMe == gameObject)
			return;
		
		if (affectedColliderBridgeObject != null)
		{
			if (affectedColliderBridgeObject.name == "Head")
			{
				headHit = true;
			}
			affectedColliderBridgeObject = null;
		}

		nofCollidesThisFrame++;

		AddToHitObjects(findMe != null ? findMe : myCollider.gameObject);

		bool tdhCheckOk = true;
		ToonDollHelper tdh = null;
		if (findMe != null)
			tdh = findMe.gameObject.GetComponent<ToonDollHelper>();

		if (goalComponent != null)
		{
			// !!! NOTE: assumes that Goal component (if present) is ABOVE ToonDollHelper component in the object!!! (so that onCollision in Goal is called BEFORE this)
			if (!goalComponent.WasHitThisFrame())
				return;

		}
		else
		{
			if (tdh != null)
			{
				if (tdh.IsRagDoll() == true && ((tdh.WasTossed() == false || (tdh.WasTossed() && (!tdh.IsActive()))) && !allowInactivePlayerHit) || tdh.IsGettingUp())
					tdhCheckOk = false;
			}

			if ((rootRb != null && !rootRb.isKinematic) || (!wasReset && !isPlayerHelper) || (!turnStandingIntoRagdollOnPlayerHit && !zombieHitMode) || tdhCheckOk == false)
				return;
		}

		if (findMe != null && zombieHitMode && !zombieWasHit)
		{
			zombieWasHit = true;

			// !!! NOTE: assumes that Goal component (if present) is ABOVE ToonDollHelper component in the object!!! (so that onCollision in Goal is called BEFORE this)
			bool goalFinished = true;
			if (goalComponent != null)
			{
				goalFinished = goalComponent.GetNofTimesHit() >= goalComponent.GetNofRequiredHits();
			}

			if (goalFinished)
			{
				SimpleTransform st = GetComponent<SimpleTransform>();
				if (st != null)
					Destroy(st);

				if (zombieHitAnim.Length < 1) {
					enableRagdoll(GravityMode.GRAVITY_ON);

					if (sleepBubblePrefab != null) {
						Invoke("CreateSleepBubble", sleepBubbleDelay);
					}
				}
				else {
					PlayAnim(zombieHitAnim);
					if (sleepBubblePrefab != null) {
						Invoke("CreateSleepBubble", heartBubbleDelay);
					}
				}

				isPlayerHelper = zombieIsPlayerHelperAfterFall;

				skinChangeCount = 4; skinChangeTime = 0.3f;
				actualSkinChange = true;

				if (removeRigidBodyRootOnZombieHit)
				{
					Rigidbody rb = GetComponent<Rigidbody>();
					if (rb != null)
						Destroy(rb);
				}
			}
			else
			{
				skinChangeCount = 1; skinChangeTime = 0.3f;
				actualSkinChange = false;
			}

			if (materialOnHit != null)
			{
				Invoke("ChangeSkin", 0);
			}

			return;
		}
		if (zombieWasHit)
			return;


		if ((findMe != null || allHitsTurnsRagdoll) && !isRagDoll)
		{
			enableRagdoll(gravityMode);

			foreach (Component c in boneRig)
			{
				(c as Rigidbody).maxDepenetrationVelocity = 1;
			}

			if (isPlayerHelper == false)
			{
				SoundManager.instance.PlayRandomFromType(soundPlayerHitPlayer, -1, 0, -1, soundPitch);
				// print ("Player hit player");
			}

			//if (materialOnHit != null) {
			//	Invoke ("ChangeSkin", 0);
			//}

			if (getUpAfterReenabledRagdoll)
			{
				forcedGround = true; // standup sometimes happens with lower legs in ground. Another approach is to force kinematic=true on root rigidbody, this is the opposite, ie the standup is good but then there is a little "jump" and it ends up hovering slightly above ground...
				Invoke("Standup", 2f);
			}
		}
	}


	public void OnCollisionEnter(Collision collision) {
		CheckColliderEnter(collision.collider);
	}


	public void OnCollisionExit(Collision collision) {}

	private void CheckColliderStay(Collider myCollider) {
		GameObject findMe = GameUtil.FindParentWithTag(myCollider.gameObject, "Player");
		if (findMe != null && findMe == gameObject)
			return;

		if (affectedColliderBridgeObject != null)
		{
			if (affectedColliderBridgeObject.name == "Head")
			{
				headHit = true;
			}
			affectedColliderBridgeObject = null;
		}

		if (addHitObjectsOnStay)
			AddToHitObjects(findMe != null ? findMe : myCollider.gameObject);

		nofCollidesThisFrame++;
	}

	public void OnCollisionStay(Collision collision)
	{
		CheckColliderStay(collision.collider);
	}
	public void OnTriggerEnter(Collider collider)
	{
		GameObject findMe = GameUtil.FindParentWithTag (collider.gameObject, "Player");
		if (findMe != null && findMe == gameObject)
			return;

		if (allowTriggerCollision)
			CheckColliderEnter(collider);

		nofCollidesThisFrame++;
	}
	public void OnTriggerExit(Collider collider) {}
	public void OnTriggerStay(Collider collider)
	{
		GameObject findMe = GameUtil.FindParentWithTag (collider.gameObject, "Player");
		if (findMe != null && findMe == gameObject)
			return;

		if (allowTriggerCollision)
			CheckColliderStay(collider);

		nofCollidesThisFrame++;
	}

	public void EnableAngularVelocityCheck() {
		checkAngularVelocityMove = true;
		Invoke ("DisableAngularVelocityCheck()", 6f);
	}
	public void DisableAngularVelocityCheck() {
		checkAngularVelocityMove = false;
	}



	/* Transition from ragdoll to anim */
	enum RagdollState
	{
		animated,	 //Mecanim is fully in control
		ragdolled,   //Mecanim turned off, physics controls the ragdoll
		blendToAnim  //Mecanim in control, but LateUpdate() is used to partially blend in the last ragdolled pose
	}

	RagdollState state=RagdollState.animated;

	private float ragdollToMecanimBlendTime=0.5f;
	float mecanimToGetUpTransitionTime=0.05f;

	float ragdollingEndTime=-100;

	public class BodyPart
	{
		public Transform transform;
		public Vector3 storedPosition;
		public Quaternion storedRotation;
	}
	List<BodyPart> bodyParts=new List<BodyPart>();

	Vector3 ragdolledHipPosition,ragdolledHeadPosition,ragdolledFeetPosition;


	int headUpState = 0;
	float headUpSlerpTimer = 0, noHeadUpTimer = -1, headDir = 1, yPosFix = 0, forwardLookingTendency = 0;
	int posCounter = 0, cleanCollFrames = 0;
	Vector3 oldPos = Vector3.zero;

	public void ForceHeadDown(float downTime) {
		noHeadUpTimer = downTime;
		headUpState = 0;
		headUpSlerpTimer = 0;
	}


	void LateUpdate()
	{
		//Blending from ragdoll back to animated
		if (state == RagdollState.blendToAnim) {
			if (Time.time <= ragdollingEndTime + mecanimToGetUpTransitionTime) {
				//If we are waiting for Mecanim to start playing the get up animations, update the root of the mecanim character to the best match with the ragdoll
				Vector3 animatedToRagdolled = ragdolledHipPosition - animator.GetBoneTransform (isCelebrating? HumanBodyBones.Hips : HumanBodyBones.RightFoot).position;  // HumanBodyBones.Hips
				Vector3 newRootPosition = transform.position + animatedToRagdolled;

				transform.position = newRootPosition;

				//Get body orientation in ground plane for both the ragdolled pose and the animated get up pose
				Vector3 ragdolledDirection = ragdolledHeadPosition - ragdolledFeetPosition;
				ragdolledDirection.y = 0;

				Vector3 meanFeetPosition = 0.5f * (animator.GetBoneTransform (HumanBodyBones.LeftFoot).position + animator.GetBoneTransform (HumanBodyBones.RightFoot).position);
				Vector3 animatedDirection = animator.GetBoneTransform (HumanBodyBones.Head).position - meanFeetPosition;
				animatedDirection.y = 0;

				//Try to match the rotations. Note that we can only rotate around Y axis, as the animated characted must stay upright, hence setting the y components of the vectors to zero. 
				transform.rotation *= Quaternion.FromToRotation (animatedDirection.normalized, ragdolledDirection.normalized);
			}
			float ragdollBlendAmount = 1.0f - (Time.time - ragdollingEndTime - mecanimToGetUpTransitionTime) / ragdollToMecanimBlendTime;
			ragdollBlendAmount = Mathf.Clamp01 (ragdollBlendAmount);

			//To enable smooth transitioning from a ragdoll to animation, we lerp the position of the hips and slerp all the rotations towards the ones stored when ending the ragdolling
			foreach (BodyPart b in bodyParts) {
				if (b.transform != transform) { //this if is to prevent us from modifying the root of the character, only the actual body parts
					//position is only interpolated for the hips
					if (b.transform == animator.GetBoneTransform (HumanBodyBones.Hips))
						b.transform.position = Vector3.Lerp (b.transform.position, b.storedPosition, ragdollBlendAmount);
					b.transform.rotation = Quaternion.Slerp (b.transform.rotation, b.storedRotation, ragdollBlendAmount);
				}
			}

			if (ragdollBlendAmount == 0) {
				state = RagdollState.animated;
				StaticManager.RestoreTimeStep (timeStepAffectIndex);
				yPosFix = transform.position.y + 0.1f;
//				return;
			}
		} else if (gettingUp && progressiveLevel && tossNumber > 1) { // EXTREMELY ugly "fix" of very weird bug where player sometimes would "rise to the sky" on second or later tosses while getting up
			if (transform.position.y > yPosFix)
				transform.position = GameUtil.SetY (transform.position, yPosFix);
		}


		noHeadUpTimer -= Time.deltaTime;
		if (!ghostReadingEnabled && bGazing && noHeadUpTimer < 0 && !wasBubbled && isActive) {
			Vector3 hipForward = animator.GetBoneTransform (HumanBodyBones.Head).forward; // or Hips?

			float magnitude = dollRootRb.velocity.magnitude;

			if (headUpState == 0 && hipForward.y <= -0.5f && nofCollidesThisFrame > 0 && magnitude > 6) {
				headUpState = 1;
				headDir = 1;
			}
			if (headUpState == 0 && hipForward.y >= 0.5f && nofCollidesThisFrame > 0 && magnitude > 6) {
				headUpState = 2;
				headDir = 1;
			}

			if (headUpState != 0 && hipForward.y < 0 && hipForward.y >= -0.2f) {
				cleanCollFrames = 0;
				if (headUpSlerpTimer > 1)
					headUpSlerpTimer = 1;
				headDir = -2;
			}
			if (headUpState != 0 && hipForward.y > 0 && hipForward.y <= 0.2f) {
				cleanCollFrames = 0;
				if (headUpSlerpTimer > 1)
					headUpSlerpTimer = 1;
				headDir = -2;
			}


			if (magnitude < 5f && headUpState != 0 && headUpSlerpTimer > 1) {
				if (headUpSlerpTimer > 1)
					headUpSlerpTimer = 1;
				headDir = -2;
			}
			if (magnitude < 1f) {
				if (headUpSlerpTimer > 1)
					headUpSlerpTimer = 1;
				headDir = -4;
//				cleanCollFrames = 0;
//				headUpState = 0;
//				headUpSlerpTimer = 0;
			}
			if (nofCollidesThisFrame == 0) {
				cleanCollFrames++;
			}
			if (cleanCollFrames > 3) {
				cleanCollFrames = 0;
				if (headUpSlerpTimer > 1)
					headUpSlerpTimer = 1;
				headDir = -2;
			}

			if (wasTossed && !gettingUp && headUpState != 0) {
				float hUprogress = Mathf.Clamp01 (headUpSlerpTimer);

				Vector3 pos = animator.GetBoneTransform (HumanBodyBones.Head).forward;
				pos = GameUtil.SetY (pos, 0);
				pos = animator.GetBoneTransform (HumanBodyBones.Head).transform.position + pos * 100;
				pos = GameUtil.SetZ (pos, pos.z + forwardLookingTendency); // force a "tendency" to look forwards instead of backwards
				pos = GameUtil.SetY (pos, pos.y + 25);

				if (headUpState == 1)
					pos = GameUtil.SetY (pos, -pos.y * 1.8f);


				Rigidbody headRb = _headBone.GetComponent<Rigidbody> ();
				float speedMod = Mathf.Clamp (1 - Mathf.Clamp01 (headRb.velocity.magnitude / 25), 0.9f, 1f);
				pos = GameUtil.SetY (pos, pos.y * speedMod);

				posCounter++;
				if (posCounter % 4 == 0 || oldPos == Vector3.zero)
					oldPos = pos;

				Vector3 targetDir = oldPos - _headBone.transform.position;
				Vector3 newDir = Vector3.RotateTowards (_headBone.transform.forward, targetDir, 1000, 0);
				Quaternion toRot = Quaternion.LookRotation (newDir);
				Quaternion slerpRot = Quaternion.Slerp (_headBone.transform.rotation, toRot, hUprogress);

				_headBone.transform.rotation = slerpRot;
//				headRb.MoveRotation (slerpRot); // wobbles too much

				headUpSlerpTimer += Time.deltaTime * 1.1f * headDir;
				if (headUpSlerpTimer < 0) {
					headUpState = 0;
					headUpSlerpTimer = 0;
				}
			}
		}


		if (ghostAnimator != null && ghostReadingEnabled) {

			ghostAnimator.gameObject.transform.position = root.position;

			float progress = ghostBlendTimer / ghostBlendTime;
			ghostBlendTimer += Time.deltaTime * ghostAnimDirection;
			if (ghostBlendTimer > ghostBlendTime)
				ghostBlendTimer = ghostBlendTime;
			if (ghostBlendTimer < 0) {
				ghostBlendTimer = 0;
				StopGhostAnim ();
			}

			foreach (HumanBodyBones bone in currentGhostAnim.affectedBones) {

				Transform t = ghostAnimator.GetBoneTransform (bone);
				Transform t2 = animator.GetBoneTransform (bone);

				Quaternion animRotation = t.transform.rotation;

				if (ghostBlendTimer < ghostBlendTime) {
					animRotation = Quaternion.Slerp (t2.transform.rotation, animRotation, progress);
				}

				if (ghostUseMoveRotation == false) {
					t2.transform.rotation = animRotation; // looks smoother, but not good because it affects collisions etc
				} else {
					Rigidbody rb = t2.gameObject.GetComponent<Rigidbody> ();
					if (rb != null) rb.MoveRotation (animRotation); // supposedly this is more correct. Also, it automatically only works when player is ragdolled
				}
			}
		}

		if (playHitMoans && !isPlayerHelper && rigidBodiesRemoved == false) {
			hitSoundMinTimer -= Time.deltaTime;
			bool playHit = false;
			foreach (GameObject g in objectsHitThisFrame) {
				if (!objectsHitLastFrame.Contains (g)) {
					playHit = true;
					// print (g.name);
					// print (g.name + "  " + gameObject.name);
				}
			}

			if (playHit) {

				if (stoppedPlayingSlideSound == false && hitSoundMinTimer <= 0) {
					stoppedPlayingSlideSound = true;
					SoundManager.instance.FadeRandomPlayingSfx (SfxRandomType.Slide);
				}

				if (isActive && !gettingUp && hitSoundMinTimer <= 0 && levelFinished == false) {
					float vol = Mathf.Clamp (dollRootRb.velocity.magnitude / 20.0f, 0, 1f);
					if (vol > hitVolumeMin && Random.Range(0,100) > 33) {
						//print (vol);
						soundEmitter.volume = vol;
						soundEmitter.PlaySound ();
						hitSoundMinTimer = hitSoundMinTime;
					}
				}
			}
			objectsHitLastFrame.Clear ();
			objectsHitLastFrame = objectsHitThisFrame;
			objectsHitThisFrame = new List<GameObject> ();

			nofCollidesLastFrame = nofCollidesThisFrame;
			nofCollidesThisFrame = 0;
		}

		if (isCelebrating && gettingUp && celebrationRiseSpeed != 0) {
			dollRootRb.transform.position = GameUtil.SetY (dollRootRb.transform.position, orgVictoryY);
			endPos = dollRootRb.transform.position;
			orgVictoryY += celebrationRiseSpeed * Time.deltaTime;
		}
	}

	public void SetStillStandTime(float value, bool setOnlyIfHigher = false) {
		if (setOnlyIfHigher == false || value > stillStandTime)
			stillStandTime = value;
		stillStandTimeCounter = stillStandTime;
	}

	public void SetAirtime (bool airtimeSteering, bool airtimeAcceleration, bool airtimeBreak, bool airtimeAccChangeDir, bool airtimeBrkChangeDir) {
		if (airtimeSteering)
			extra_airtimeSteering = true;
		if (airtimeAcceleration)
			extra_airtimeAcceleration = true;
		if (airtimeBreak)
			extra_airtimeBreak = true;
		if (airtimeAccChangeDir)
			extra_airtimeAccChangeDir = true;
		if (airtimeBrkChangeDir)
			extra_airtimeBrkChangeDir = true;
	}

	public bool WasTossedOnce() {
		return wasTossedOnce;
	}

	private float orgVictoryY;
	private float celebrationRiseSpeed;
	public void SetLevelFinished(bool celebrateVictory, float celebrateStartTime, float celebrationRiseSpeed) {
		levelFinished = true;
		this.celebrationRiseSpeed = celebrationRiseSpeed;

		if (celebrateVictory)
			Invoke ("DelayedCelebration", celebrateStartTime);
	}

	public void TestStuff() {
	}

	void DelayedCelebration() {
		Rigidbody[] rs = dollRootRb.GetComponentsInChildren<Rigidbody> ();
		if (rs != null) {
			foreach (Rigidbody c in rs) {
				c.velocity = Vector3.zero;
			}
		}

		isCelebrating = true;

		Invoke ("DelayedCelebration2", 0);
	}

	void DelayedCelebration2() {

		if (celebrationRiseSpeed != 0) {
			Collider[] cs = dollRootRb.GetComponentsInChildren<Collider> ();
			if (cs != null) {
				foreach (Collider c in cs) {
					Destroy (c);
				}
			}
		}

		disableRagdoll ();

		rootRb.useGravity = false;
		rootRb.isKinematic = celebrationRiseSpeed == 0? false : true;

		gettingUp = true;

		timeStepAffectIndex = StaticManager.PushFixedTimeStep (0.01f);
		//Debug.Break ();

		PlayAnim ("Victory");
		mecanimToGetUpTransitionTime=0.2f;

		orgVictoryY = dollRootRb.transform.position.y;
//		endPos = dollRootRb.transform.position; //GameUtil.AddY(root.position, 3);

		// ** Blend stuff
		ragdollingEndTime = Time.time; //store the state change time
		state = RagdollState.blendToAnim;

		foreach (BodyPart b in bodyParts) {
			b.storedRotation = b.transform.rotation;
			b.storedPosition = b.transform.position;
		}
		ragdolledFeetPosition = 0.5f * (animator.GetBoneTransform (HumanBodyBones.LeftToes).position + animator.GetBoneTransform (HumanBodyBones.RightToes).position);
		ragdolledHeadPosition = animator.GetBoneTransform (HumanBodyBones.Head).position;
		ragdolledHipPosition = animator.GetBoneTransform (HumanBodyBones.Hips).position;

//		transform.position = endPos;
		counterSetpos = forcedGround? 1000 : 30; // setting this position again later attempts to remove a strange bug
		forcedGround = false;
		counterSetpos = -1;
	}


	public void Hmm() {
		TrigAnim ("Hmm");
		SoundManager.instance.PlayRandomFromType(soundHmm, -1, 0, -1, soundPitch, true);
	}

	private Vector3 speechBubblePos = new Vector3 (0.7f, 1.2f, 0f);
	private bool speechBubbleDisappering = false;
	public void ShowSpeechBubble(GameObject msgPrefab, float showTime, SingleSfx sfx = SingleSfx.None, SfxRandomType sfxRandom = SfxRandomType.None) {
		if (speechBubble != null) {
			Destroy (speechBubble); speechBubble = null;
		}

		speechBubble = Instantiate (msgPrefab);
		speechBubble.transform.position = animator.GetBoneTransform (HumanBodyBones.Head).position + speechBubblePos;

		speechBubble.transform.localScale = new Vector3 (0,0,0);
		LeanTween.scale (speechBubble.gameObject, new Vector3 (0.15f, 0.1f, 0.1f), 0.5f).setEaseOutBounce ();

		if (sfxRandom != SfxRandomType.None)
			SoundManager.instance.PlayRandomFromType (sfxRandom);
		else if (sfx != SingleSfx.None)
			SoundManager.instance.PlaySingleSfx (sfx);

		speechTimeLeft = showTime;
	}


	private HumanBodyBones [] tempBones;
	private GameObject tempTrailPrefab;

	private void AssignTrails() {

		GameObject trailContainer = Instantiate (tempTrailPrefab);
		TrailRenderer trail = trailContainer.GetComponent<TrailRenderer>();
	
		foreach(HumanBodyBones hbb in tempBones) {
			Transform t = animator.GetBoneTransform (hbb);
			if (t != null) {
				TrailRenderer newTR = t.gameObject.AddComponent<TrailRenderer> ();
				// if we want to hide the trail from ice reflection(visual) add below

				//t.gameObject.layer = trailContainer.layer; 

				//t.gameObject.CopyComponent<TrailRenderer> (trail);    // reflection copying did not work on iOS device, so we must resort to:

				newTR.alignment = trail.alignment;
				newTR.colorGradient = trail.colorGradient;
				newTR.endColor = trail.endColor;
				newTR.startColor = trail.startColor;
				newTR.widthCurve = trail.widthCurve;
				newTR.startWidth = trail.startWidth;
				newTR.endWidth = trail.endWidth;
				newTR.material = trail.material;
				newTR.shadowCastingMode = trail.shadowCastingMode;
				newTR.receiveShadows = trail.receiveShadows;
				newTR.time = trail.time;
				newTR.minVertexDistance = trail.minVertexDistance;
				newTR.motionVectorGenerationMode = trail.motionVectorGenerationMode;
				newTR.autodestruct = trail.autodestruct;
				newTR.numCornerVertices = trail.numCornerVertices;
				newTR.numCapVertices = trail.numCapVertices;
				newTR.alignment = trail.alignment;
				newTR.textureMode = trail.textureMode;
				newTR.generateLightingData = trail.generateLightingData;
				newTR.sortingLayerID = trail.sortingLayerID;
				newTR.sortingLayerName = trail.sortingLayerName;
				newTR.sortingOrder = trail.sortingOrder;
				newTR.lightProbeUsage = trail.lightProbeUsage;
				newTR.lightmapIndex = trail.lightmapIndex;
				newTR.lightmapScaleOffset = trail.lightmapScaleOffset;
				newTR.realtimeLightmapIndex = trail.realtimeLightmapIndex;
				newTR.realtimeLightmapScaleOffset = trail.realtimeLightmapScaleOffset;
				// is this all?? Add more as needed...
			}
		}

		Destroy (trailContainer);

		trails = GetComponentsInChildren<TrailRenderer>();
		/* print (trails.Length);
		if (trails != null && trails.Length > 0) 
			foreach (TrailRenderer tr in trails) {
				tr.startColor = Color.red;
				tr.endColor = Color.blue;
		}*/

	}

	public void SetRenderQueue(int newQ = 3001) {
		if (smr && smr.material)
		{
			smr.material.renderQueue = newQ;
			smr.materials[0].renderQueue = newQ;
			if (smr.materials.Length > 1)
			{
				smr.materials[1].renderQueue = newQ;
			}
		}
	} 

	public void SetTrails(HumanBodyBones [] bones, GameObject trailPrefab) {

		if (trailPrefab == null || bones == null || bones.Length == 0)
			return;

		TrailRenderer [] trs = gameObject.GetComponentsInChildren<TrailRenderer> ();
		if (trs != null && trs.Length > 0)
			foreach (TrailRenderer tr in trs) { // weirdly enough, the trails disappear and are destroyed, but they are visibly there still! Must be a bug... do not have trails on the prefab at all, add here instead (https://answers.unity.com/questions/1246901/line-rendere-lines-still-remains-after-destroying.html)
				Destroy (tr);
			}

		tempBones = bones;
		tempTrailPrefab = trailPrefab;
		Invoke ("AssignTrails", 0); // need to wait 1 frame if there were already trails that we destroyed here
	}

	public void SetParticleTrails(GameObject trailParticlePrefab) {
		if (trailParticlePrefab == null)
			return;

		GameObject trail = Instantiate(trailParticlePrefab, root);
		FollowObject follow = trail.GetComponent<FollowObject>();
		follow.SetFollowThis(root.gameObject);
		trailParticleSystem = trail.GetComponent<ParticleSystem>();
		trailParticleSystem.Stop();
	}

	public void SetContinueSetRenderQValue(int value) {
		continueSetRenderQValue = value;
	}
}
