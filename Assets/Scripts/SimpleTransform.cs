using UnityEngine;

public class SimpleTransform : MonoBehaviour
{
	// format of OnBrokenContainer is: StartDelay_Animation_translationX_tY_tZ_translationSwitchTime_rotationX_rY_rZ  (don't need to provide more than the first two)

	public Vector3 rotationPerFrame = Vector3.zero;

	public Vector3 translationPerFrame = Vector3.zero;

	public Vector3 scalingPerFrame = Vector3.zero;

	public enum SwitchEase { Linear, InOut, In, Out, BounceOut, BounceIn };

	public float translationSwitchTime = -1;
	public SwitchEase switchEase = SwitchEase.Linear;
	public bool translationSwitch = true;
	public bool rotationSwitch = false;
	public bool scalingSwitch = false;
	private float translationSwitchTimer = float.MaxValue;
	public Vector3 switchRotation = Vector3.zero;
	public float switchRotationTime = 0;
	public iTween.EaseType switchRotationEase = iTween.EaseType.easeOutExpo;
	public bool restoreYOnSwitch = false;
	private bool switched = false;

	public float translationResetTime = -1;
	private float translationResetTimer = float.MaxValue;

	public string animationState = null;
	public bool randomAnimStart = false;

	public bool stopOnAnyHit = false;
	public bool stopOnPlayerHit = false;

	private bool stopMe = false;
	private Vector3 orgPos, orgLocalPos, orgScale;
	private Quaternion orgRot, orgLocalRot;

	public float startWaitTime = 0;
	private bool initialized = false;

	public string singleTranslationAnim = null;
	public bool singleTranslationKill = false;
	public bool singleTranslationRepeat = false;
	public bool singleTranslationDoNothing = false;

	private Animator animator;

	private Goal goalComponent = null;

	public bool setRigidBodyInterpolationAndCollisionDetectionToNoneDiscrete = true;
	public bool freezeRootRigidRotation = true;

	public bool destroyRidigBodiesOnStart = false;

	public string[] stateTriggers = null;
	public float triggerWaitMin = 1, triggerWaitMax = 2;

	public bool localRotation = true;

	public enum TranslateHeading { PerFrameSet, PerFrameSetLocal, Forward, Up, Strafe };
	public TranslateHeading translateHeading = TranslateHeading.PerFrameSet;
	public float translateHeadingMagnitude = 1;
	public bool inverseMagnitudeOnSwitch = true;

	public Vector3 secondTranslationMul = Vector3.one;

	public Vector3[] sequenceTranslationOrMagnitudeInX = null;
	public float[] sequenceTime = null;
	public Vector3[] sequenceSwitchRotation = null;
	public float[] sequenceSwitchRotationTime = null;
	public string[] sequenceAnim = null;
	public Vector3[] sequenceRotation = null;
	public Vector3[] sequenceScaling = null;
	public bool sequenceAnimTrig = false;
	private int sequenceIndex = 0;
	private bool usingSequence = false;

	public bool forceCrossfadePlay = false;
	public float crossFadeTime = 0.8f;

	public bool translationPerFrameIsAbsolutePosition = false;
	private Vector3 absolutePosFullDelta, absolutePosLast;
	public bool isRectTransform = false;
	private RectTransform rectTransform = null;

	private static double randomIndex = 0;
	void PlayAnim(string stateName, bool randomStart = false) {
		if (animator != null) {
			if (forceCrossfadePlay) {
				if (randomStart)
					animator.CrossFadeInFixedTime(stateName, crossFadeTime, -1, Random.value); // for some reason normal Crossfade method leads to completely weird behavior
				else
					animator.CrossFadeInFixedTime(stateName, crossFadeTime);
			} else
			{
				if (randomStart)
					animator.Play(stateName, -1, Random.value);
				else
					animator.Play(stateName);
			}
		}		
	}

	void Start () {
		orgPos = transform.position;
		orgRot = transform.rotation;
		orgLocalPos = transform.localPosition;
		orgLocalRot = transform.localRotation;
		orgScale = transform.localScale;

		if (isRectTransform)
			rectTransform = GetComponent<RectTransform>();

		goalComponent = GetComponent<Goal> ();

		if (setRigidBodyInterpolationAndCollisionDetectionToNoneDiscrete) {
			SetRigidValues (transform);
		}

		if (freezeRootRigidRotation) {
			Rigidbody rb = transform.GetComponent<Rigidbody> ();
			if (rb != null)
				rb.constraints = RigidbodyConstraints.FreezeRotation;
		}

		if (destroyRidigBodiesOnStart)
			ToonDollHelper.RemoveRigidComponents (gameObject);

		if (stateTriggers != null && stateTriggers.Length > 0 && triggerWaitMax >= triggerWaitMin) {
			Invoke("TriggerState", Random.Range(triggerWaitMin, triggerWaitMax));
		}

		if (sequenceTranslationOrMagnitudeInX != null && sequenceTranslationOrMagnitudeInX.Length > 0 && sequenceTime != null && sequenceTime.Length > 0 && sequenceTranslationOrMagnitudeInX.Length == sequenceTime.Length) {
			if ((sequenceSwitchRotation == null || sequenceSwitchRotation.Length == 0) && (sequenceSwitchRotationTime == null || sequenceSwitchRotationTime.Length == 0)) {
				sequenceSwitchRotation = new Vector3[sequenceTime.Length];
				sequenceSwitchRotationTime = new float[sequenceTime.Length];
				for (int i = 0; i < sequenceTime.Length; i++) {
					sequenceSwitchRotation[i] = Vector3.zero;
					sequenceSwitchRotationTime[i] = -1;
				}
			}
		}
		if (sequenceTranslationOrMagnitudeInX != null && sequenceTranslationOrMagnitudeInX.Length > 0 && sequenceTime != null && sequenceTime.Length > 0 && sequenceSwitchRotation != null && sequenceSwitchRotation.Length > 0 && sequenceSwitchRotationTime != null && sequenceSwitchRotationTime.Length > 0) {
			if (sequenceSwitchRotation.Length == sequenceTranslationOrMagnitudeInX.Length && sequenceSwitchRotation.Length == sequenceTime.Length && sequenceSwitchRotation.Length == sequenceSwitchRotationTime.Length)
			{
				usingSequence = true;
				if (translateHeading == TranslateHeading.PerFrameSet || translateHeading == TranslateHeading.PerFrameSetLocal)
					translationPerFrame = sequenceTranslationOrMagnitudeInX[sequenceIndex];
				else
					translateHeadingMagnitude = sequenceTranslationOrMagnitudeInX[sequenceIndex].x;
				translationSwitchTime = sequenceTime[sequenceIndex];
				switchRotation = sequenceSwitchRotation[sequenceIndex];
				switchRotationTime = sequenceSwitchRotationTime[sequenceIndex];

				if (sequenceRotation != null && sequenceRotation.Length > sequenceIndex)
					rotationPerFrame = sequenceRotation[sequenceIndex];
				if (sequenceScaling != null && sequenceScaling.Length > sequenceIndex)
					scalingPerFrame = sequenceScaling[sequenceIndex];

				translationSwitch = true;
				inverseMagnitudeOnSwitch = false;

			} else {
				print("Sequence ignored, all four sequence arrays need to be of same length");				
			}
		}

		if (translationPerFrameIsAbsolutePosition) {
			if (translationSwitchTime < 0)
				print("translationPerFrameIsAbsolutePosition needs a switchTime set. Ignoring setting.");
			else
			{
				absolutePosFullDelta = translationPerFrame - transform.position;
				absolutePosLast = transform.position;
				if (rectTransform) {
					absolutePosFullDelta = translationPerFrame - rectTransform.anchoredPosition3D;
					absolutePosLast = rectTransform.anchoredPosition3D;
				}
			}
		}

	}

	private Vector3 GetTranslationVector() {
		Vector3 translation = translationPerFrame;

		switch(translateHeading) {
			case TranslateHeading.Forward: translation = transform.forward * translateHeadingMagnitude; break;
			case TranslateHeading.Up: translation = transform.up * translateHeadingMagnitude; break;
			case TranslateHeading.Strafe: translation = transform.right * translateHeadingMagnitude; break;
		}
		return new Vector3(translation.x * secondTranslationMul.x, translation.y * secondTranslationMul.y, translation.z * secondTranslationMul.z);
	}


	void TriggerState() {
		if (animator == null)
			return;

		animator.SetTrigger(stateTriggers[Random.Range(0, stateTriggers.Length)]);

		Invoke("TriggerState", Random.Range(triggerWaitMin, triggerWaitMax));
	}

	void SetRigidValues(Transform t) {

		Rigidbody rb = t.GetComponent<Rigidbody> ();

		if (rb != null) {
			rb.interpolation = RigidbodyInterpolation.None;
			rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
		}

		foreach (Transform tc in t)
			SetRigidValues (tc);
	}


	void ResetRot() {
		if (localRotation)
			transform.localRotation = orgLocalRot;
		else
			transform.rotation = orgRot;
	}

	void ResetPos()
	{
		if (translateHeading == TranslateHeading.PerFrameSetLocal)
			transform.localPosition = orgLocalPos;
		else
			transform.position = orgPos;
	}

	void GetEaseValues(out float tt, out float mulMod) {
		tt = translationSwitchTimer / translationSwitchTime;
		mulMod = 1.52f;
		if (switchEase == SwitchEase.InOut)
		{
			if (tt > 0.5f)
				tt = 1 - tt;
			mulMod = 2.6f;
		}
		else if (switchEase == SwitchEase.In)
			tt = 1 - tt;
		else if (switchEase == SwitchEase.BounceOut)
		{
			if (!switched)
				tt = 1 - tt;
		}
		else if (switchEase == SwitchEase.BounceIn)
		{
			if (switched)
				tt = 1 - tt;
		}
	}

	void Update () {

		if (stopMe)
			return;

		if (startWaitTime > 0) {
			startWaitTime -= Time.deltaTime;
			return;
		}

		if (!initialized) {
			if (translationSwitchTime > 0)
				translationSwitchTimer = 0;

			if (translationResetTime > 0)
				translationResetTimer = 0;

			animator = GetComponent<Animator> ();

			if (usingSequence == false || sequenceAnim.Length < 1) {
				if (animator != null && animationState != null) {
					if (GetComponent<RandomAnimatorStart>() != null || randomAnimStart)
						PlayAnim(animationState, true);
					else
						PlayAnim(animationState);
				}
			} else {
				if (usingSequence && sequenceAnim != null && sequenceAnim.Length > sequenceIndex && sequenceAnim[sequenceIndex].Length > 0 && animator != null) {
					if (sequenceAnimTrig)
						animator.SetTrigger(sequenceAnim[sequenceIndex]);
					else
						PlayAnim(sequenceAnim[sequenceIndex]);
				}
			}

			initialized = true;
		}

		if (rotationPerFrame != Vector3.zero)
		{
			if (switchEase != SwitchEase.Linear && translationSwitchTime > 0) {
				float tt, mulMod;
				GetEaseValues(out tt, out mulMod);
				transform.Rotate(rotationPerFrame * 50 * Time.deltaTime * Easing.Sinusoidal.Out(tt) * mulMod, localRotation? Space.Self : Space.World);
			} else
				transform.Rotate(rotationPerFrame * 50 * Time.deltaTime, localRotation ? Space.Self : Space.World);
		}

		if (scalingPerFrame != Vector3.zero)
		{
			if (switchEase != SwitchEase.Linear && translationSwitchTime > 0)
			{
				float tt, mulMod;
				GetEaseValues(out tt, out mulMod);
				transform.localScale += scalingPerFrame * 10 * Time.deltaTime * Easing.Sinusoidal.Out(tt) * mulMod;
			}
			else
				transform.localScale += scalingPerFrame * 10 * Time.deltaTime;
		}

		if (translationPerFrame != Vector3.zero || rotationSwitch || scalingSwitch || translateHeading != TranslateHeading.PerFrameSet || (usingSequence && sequenceAnim.Length > 0 )) {

			if (translationPerFrameIsAbsolutePosition && translationSwitchTime >= 0)
			{
				float tt = translationSwitchTimer / translationSwitchTime;
				if (switchEase != SwitchEase.Linear)
				{
					switch(switchEase) {
						case SwitchEase.InOut: tt = Easing.Sinusoidal.InOut(tt); break;
						case SwitchEase.In: tt = Easing.Sinusoidal.In(tt); break;
						case SwitchEase.Out: tt = Easing.Sinusoidal.Out(tt); break;
						case SwitchEase.BounceIn: tt = Easing.Bounce.In(tt); break;
						case SwitchEase.BounceOut: tt = Easing.Bounce.Out(tt); break;
					}
				}
				if (!rectTransform)
					transform.position = absolutePosLast + absolutePosFullDelta * tt;
				else
				{
					rectTransform.anchoredPosition3D = absolutePosLast + absolutePosFullDelta * tt;
				}
			}
			else if (switchEase != SwitchEase.Linear && translationSwitchTime > 0)
			{
				float tt, mulMod;
				GetEaseValues(out tt, out mulMod);
				Vector3 translation = GetTranslationVector();
				if (translateHeading == TranslateHeading.PerFrameSetLocal)
					transform.localPosition += (translation * Time.deltaTime * Easing.Sinusoidal.Out(tt) * mulMod);
				else
					transform.position += (translation * Time.deltaTime * Easing.Sinusoidal.Out(tt) * mulMod);
			}
			else
			{
				Vector3 translation = GetTranslationVector();
				if (translateHeading == TranslateHeading.PerFrameSetLocal)
					transform.localPosition += (translation * Time.deltaTime);
				else
					transform.position += (translation * Time.deltaTime);
			}

			if (translationSwitchTimer < translationSwitchTime) {
				translationSwitchTimer += Time.deltaTime;
				if (translationSwitchTimer >= translationSwitchTime) {
					if (singleTranslationKill && !(usingSequence && sequenceIndex < sequenceTime.Length - 1)) {
						Destroy(gameObject);
					}
					else if (singleTranslationRepeat && !(usingSequence && sequenceIndex < sequenceTime.Length - 1))
					{
						translationSwitchTimer = 0;
						transform.localPosition = orgLocalPos;
						transform.position = orgPos;
						transform.localRotation = orgLocalRot;
						transform.rotation = orgRot;
						transform.localScale = orgScale;
						sequenceIndex = -1;
					}
					else if (singleTranslationDoNothing && !(usingSequence && sequenceIndex < sequenceTime.Length - 1))
					{
						if (usingSequence) {
							sequenceIndex = -1;
							translationSwitchTimer = translationResetTime - 0.001f;
						} else
							translationSwitchTimer = 0;
					}
					else if (singleTranslationAnim != null && singleTranslationAnim.Length > 0 && !(usingSequence && sequenceIndex < sequenceTime.Length - 1)) {
						usingSequence = false;
						translationPerFrame = Vector3.zero;
						rotationPerFrame = Vector3.zero;
						scalingPerFrame = Vector3.zero;
						if (animator != null)
							PlayAnim (singleTranslationAnim);
					} else {
						if (translationSwitch)
						{
							translationPerFrame = -translationPerFrame;
							if (inverseMagnitudeOnSwitch)
								translateHeadingMagnitude = -translateHeadingMagnitude;
						}
 						if (rotationSwitch)
							rotationPerFrame = -rotationPerFrame;
						if (scalingSwitch)
							scalingPerFrame = -scalingPerFrame;
						
						translationSwitchTimer = 0;
						if (switchRotation != Vector3.zero) {
							if (switchRotationTime <= 0)
								transform.Rotate(switchRotation, localRotation ? Space.Self : Space.World);
							else
							{
								
								iTween.EaseType oldET = iTween.Defaults.easeType;
								iTween.Defaults.easeType = switchRotationEase;
								//iTween.RotateAdd(gameObject, switchRotation, switchRotationTime); // doesn't seem like iTween has any local rotation functions... using this now in both cases (local and world), we don't have any locally rotating objects in the game anyway
								//not providing an id is costly so create "unique" id.
								iTween.RotateAdd(gameObject,iTween.Hash("amount", switchRotation, "time", switchRotationTime,"id",""+gameObject.name+""+randomIndex++));
								iTween.Defaults.easeType = oldET;
							}
						}

						if (usingSequence) {
							sequenceIndex++; if (sequenceIndex >= sequenceTime.Length) sequenceIndex = 0;
							if (translateHeading == TranslateHeading.PerFrameSet || translateHeading == TranslateHeading.PerFrameSetLocal)
								translationPerFrame = sequenceTranslationOrMagnitudeInX[sequenceIndex];
							else
								translateHeadingMagnitude = sequenceTranslationOrMagnitudeInX[sequenceIndex].x;
							translationSwitchTime = sequenceTime[sequenceIndex];
							switchRotation = sequenceSwitchRotation[sequenceIndex];
							switchRotationTime = sequenceSwitchRotationTime[sequenceIndex];

							if (sequenceRotation != null && sequenceRotation.Length > sequenceIndex)
								rotationPerFrame = sequenceRotation[sequenceIndex];
							if (sequenceScaling != null && sequenceScaling.Length > sequenceIndex)
								scalingPerFrame = sequenceScaling[sequenceIndex];

							if (sequenceAnim != null && sequenceAnim.Length > sequenceIndex && sequenceAnim[sequenceIndex].Length > 0 && animator != null) {
								if (sequenceAnimTrig)
									animator.SetTrigger(sequenceAnim[sequenceIndex]);
								else
									PlayAnim(sequenceAnim[sequenceIndex]);
							}
						}

						switched = !switched;
						if (!switched && restoreYOnSwitch) {
							if (translateHeading == TranslateHeading.PerFrameSetLocal)
								transform.localPosition = GameUtil.SetY(transform.localPosition, orgPos.y);
							else
								transform.position = GameUtil.SetY (transform.position, orgPos.y);
							transform.localScale = orgScale;
							if (switchRotation != Vector3.zero) {
								if (switchRotationTime <= 0) {
									ResetRot();
								} else
									Invoke ("ResetRot", switchRotationTime);
							}
						}
					}

					if (translationPerFrameIsAbsolutePosition && translationSwitchTime >= 0) {
						if (usingSequence == false)
							translationPerFrame = absolutePosLast;
						absolutePosFullDelta = translationPerFrame - transform.position;
						absolutePosLast = transform.position;
						if (rectTransform) {
							absolutePosFullDelta = translationPerFrame - rectTransform.anchoredPosition3D;
							absolutePosLast = rectTransform.anchoredPosition3D;
						}
					}	
				}
			}

			if (translationResetTimer < translationResetTime) {
				translationResetTimer += Time.deltaTime;
				if (translationResetTimer >= translationResetTime) {
					translationResetTimer = 0;
					ResetPos();
				}
			}
		}
	}


	public void OnCollisionEnter(Collision collision) {

		if (!stopOnAnyHit && !stopOnPlayerHit)
			return;

		GameObject findMe = GameUtil.FindParentWithTag (collision.collider.gameObject, "Player");
		if (findMe != null && findMe == gameObject)
			return;


		bool tdhCheckOk = true;
		ToonDollHelper tdh = null;
		if (findMe != null)
			tdh = findMe.gameObject.GetComponent<ToonDollHelper> ();

		if (goalComponent != null) {
			// !!! NOTE: assumes that Goal component (if present) is ABOVE ToonDollHelper component in the object!!! (so that onCollision in Goal is called BEFORE this)
			tdhCheckOk = goalComponent.WasHitThisFrame ();

		} else {
			if (tdh != null) {
				if ((tdh.IsRagDoll () == true && (tdh.WasTossed () == false || (tdh.WasTossed () && !tdh.IsActive ()))) || tdh.IsGettingUp ())
					tdhCheckOk = false;
			}
		}

		if (tdhCheckOk == false)
			return;

		// !!! NOTE: assumes that Goal component (if present) is ABOVE ToonDollHelper component in the object!!! (so that onCollision in Goal is called BEFORE this)
		bool goalFinished = true;
		if (goalComponent != null) {
			goalFinished = goalComponent.GetNofTimesHit () >= goalComponent.GetNofRequiredHits (); 
		}

		if ((findMe != null && goalFinished) || stopOnAnyHit) {
			stopMe = true;
		}
	}


	// curling game specific, gets called through BroascastMessage by BreakableGlass when object with this component is a child of the glass object  ( string format: DelayBeforeChange_AnimName_xTransform_yT_zT )
	public void OnBrokenContainer(string msg) {

		if (msg != null && msg.Length > 0) {
			string [] msgParts = msg.Split (new char [] { '_' });

			msgParam = msg;
			Invoke ("ChangeT", float.Parse(msgParts [0]));
		}
	}
	private string msgParam;
	void ChangeT() {
		string [] msgParts = msgParam.Split (new char [] { '_' });

		translationSwitchTimer = 0;

		if (animator != null) {
			if (GetComponent<RandomAnimatorStart>() != null)
				PlayAnim(msgParts[1], true);
			else
				PlayAnim (msgParts [1]);
		}

		if (msgParts.Length >= 5) {
			translationPerFrame = new Vector3 (float.Parse(msgParts[2]), float.Parse(msgParts[3]), float.Parse(msgParts[4]));
		}

		if (msgParts.Length >= 6) {
			translationSwitchTime = float.Parse(msgParts[5]);
		}

		if (msgParts.Length >= 9) {
			rotationPerFrame = new Vector3 (float.Parse(msgParts[6]), float.Parse(msgParts[7]), float.Parse(msgParts[8]));
		}

	}

}
