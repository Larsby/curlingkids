using UnityEngine;

public class RefSimpleTransform : ExternalHit
{
	// format of TransformChange is: StartDelay_Animation_translationX_tY_tZ_translationSwitchTime_rotationX_rY_rZ  (don't need to provide more than the first two)

	public enum TranslationType { PerFrame, PerFrameLocal, Forward, Up, Strafe, Absolute, AbsoluteDelta };
	public enum RotationType { PerFrame, PerFrameLocal }; // TODO: implement Absolute/AbsoluteDelta for rotation and scaling too
	public enum ScaleType { PerFrame };
	public enum SwitchEaseType { Linear, InOut, In, Out, BounceOut, BounceIn };
	public enum TransformTimeType { Infinite, Switch, SingleStop, SingleKill, SingleRepeat, SingleContinue };
	public enum RandomStartType { None, Animation, Transform, AnimationAndTransform};

	public float startWaitTime = 0;
	public float startJumpTime = 0;

	public string startAnimation = null;
	public RandomStartType randomAnimStart = RandomStartType.None;

	public bool isRectTransform = false;

	public Vector3 translationOrMagnitudeInX = Vector3.zero;
	public TranslationType translationType = TranslationType.PerFrame;

	public Vector3 rotation = Vector3.zero;
	public RotationType rotationType = RotationType.PerFrameLocal;

	public Vector3 scaling = Vector3.zero;
	private ScaleType scalingType = ScaleType.PerFrame; // only PerFrame implemented yet (hence private)

	public TransformTimeType transformTimeType = TransformTimeType.Infinite;
	public float transformTime = -1;
	public SwitchEaseType switchEase = SwitchEaseType.Linear;
	public string singleStopAnim = null;

	public bool translationSwitch = true;
	public bool inverseMagnitudeOnSwitch = true;
	public bool restoreYOnSwitch = false;
	public bool rotationSwitch = false;
	public bool scalingSwitch = false;
	public Vector3 switchRotation = Vector3.zero;
	public float switchRotationTime = 0;
	public iTween.EaseType switchRotationEase = iTween.EaseType.easeOutExpo;

	public float[] sequenceTime = null;
	public Vector3[] sequenceTranslationOrMagnitudeInX = null;
	public Vector3[] sequenceRotation = null;
	public Vector3[] sequenceScaling = null;
	public SwitchEaseType[] sequenceSwitchEase = null;
	public string[] sequenceAnim = null;
	public Vector3[] sequenceSwitchRotation = null;
	public float[] sequenceSwitchRotationTime = null;

	public bool stopOnExternalHit = true;
	public float animationCrossFadeTime = 0.8f;

	private bool forceCrossfadePlay = true;
	private Vector3 absolutePosFullDelta, absolutePosLast;
	private RectTransform rectTransform = null;
	private int sequenceIndex = 0;
	private bool usingSequence = false;
	private Animator animator;
	private float transformTimer = float.MaxValue;
	private bool switched = false;
	private bool stopMe = false;
	private Vector3 orgPos, orgLocalPos, orgScale;
	private Quaternion orgRot, orgLocalRot;
	private bool initialized = false;
	private float translateHeadingMagnitude = 1;
	private int seqLen = 0;


	void PlayAnim(string stateName, bool randomStart = false) {
		if (animator != null) {
			if (forceCrossfadePlay) {
				if (randomStart)
					animator.CrossFadeInFixedTime(stateName, animationCrossFadeTime, -1, Random.value);
				else
					animator.CrossFadeInFixedTime(stateName, animationCrossFadeTime);
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

		translateHeadingMagnitude = translationOrMagnitudeInX.x;

		if (isRectTransform)
			rectTransform = GetComponent<RectTransform>();

		if (sequenceTime != null && sequenceTime.Length > 0)
			seqLen = sequenceTime.Length;
		
		if (seqLen > 0) {
			usingSequence = true;
			if (sequenceTranslationOrMagnitudeInX != null && sequenceTranslationOrMagnitudeInX.Length > sequenceIndex)
			{
				if (translationType == TranslationType.PerFrame || translationType == TranslationType.PerFrameLocal)
					translationOrMagnitudeInX = sequenceTranslationOrMagnitudeInX[sequenceIndex];
				else
					translateHeadingMagnitude = sequenceTranslationOrMagnitudeInX[sequenceIndex].x;
			}
			transformTime = sequenceTime[sequenceIndex];
			if (sequenceSwitchRotation != null && sequenceSwitchRotation.Length > sequenceIndex)
				switchRotation = sequenceSwitchRotation[sequenceIndex];
			if (sequenceSwitchRotationTime != null && sequenceSwitchRotationTime.Length > sequenceIndex)
				switchRotationTime = sequenceSwitchRotationTime[sequenceIndex];
			if (sequenceRotation != null && sequenceRotation.Length > sequenceIndex)
				rotation = sequenceRotation[sequenceIndex];
			if (sequenceScaling != null && sequenceScaling.Length > sequenceIndex)
				scaling = sequenceScaling[sequenceIndex];
			if (sequenceSwitchEase != null && sequenceSwitchEase.Length > sequenceIndex)
				switchEase = sequenceSwitchEase[sequenceIndex];

			translationSwitch = true;
			inverseMagnitudeOnSwitch = false;
		}

		if ((translationType == TranslationType.Absolute || translationType == TranslationType.AbsoluteDelta)) {
			if (transformTime < 0) {
				print("Absolute positioning needs a transformTime set. Stopping transform.");
				stopMe = true;
			} else
			{
				Vector3 useTranslation = translationOrMagnitudeInX;
				if (translationType == TranslationType.AbsoluteDelta) {
					if (rectTransform)
						useTranslation = rectTransform.anchoredPosition3D + translationOrMagnitudeInX;
					else
						useTranslation = transform.position + translationOrMagnitudeInX;
				}

				if (rectTransform) {
					absolutePosFullDelta = useTranslation - rectTransform.anchoredPosition3D;
					absolutePosLast = rectTransform.anchoredPosition3D;
				} else {
					absolutePosFullDelta = useTranslation - transform.position;
					absolutePosLast = transform.position;
				}
			}
		}

		if (startJumpTime > 0 || randomAnimStart == RandomStartType.Transform || randomAnimStart == RandomStartType.AnimationAndTransform) {

			if (randomAnimStart == RandomStartType.Transform || randomAnimStart == RandomStartType.AnimationAndTransform) {
				if (transformTime > 0)
					startJumpTime = Random.Range(0, transformTime);
				else {
					if (startJumpTime > 0)
						startJumpTime = Random.Range(0, startJumpTime);
					else
						startJumpTime = Random.Range(0, 1f);
				}

			} else {
				if (transformTime > 0 && startJumpTime >= transformTime)
					startJumpTime = transformTime;
			}

			Updater(startJumpTime);
		}
	}

	private Vector3 GetTranslationVector() {
		Vector3 usedTranslation = translationOrMagnitudeInX;

		switch(translationType) {
			case TranslationType.Forward: usedTranslation = transform.forward * translateHeadingMagnitude; break;
			case TranslationType.Up: usedTranslation = transform.up * translateHeadingMagnitude; break;
			case TranslationType.Strafe: usedTranslation = transform.right * translateHeadingMagnitude; break;
		}
		return new Vector3(usedTranslation.x, usedTranslation.y, usedTranslation.z);
	}


	void ResetRotation() {
		if (rotationType == RotationType.PerFrameLocal)
			transform.localRotation = orgLocalRot;
		else
			transform.rotation = orgRot;
	}

	void GetEaseValues(out float tt, out float mulMod) {
		tt = transformTimer / transformTime;
		mulMod = 1.52f;
		if (switchEase == SwitchEaseType.InOut)
		{
			if (tt > 0.5f)
				tt = 1 - tt;
			mulMod = 2.6f;
		}
		else if (switchEase == SwitchEaseType.In)
			tt = 1 - tt;
		else if (switchEase == SwitchEaseType.BounceOut)
		{
			if (!switched)
				tt = 1 - tt;
		}
		else if (switchEase == SwitchEaseType.BounceIn)
		{
			if (switched)
				tt = 1 - tt;
		}
	}

	void Update()
	{
		Updater(Time.deltaTime);
	}

	void Updater (float deltaTime) {

		if (stopMe)
			return;

		if (startWaitTime > 0) {
			startWaitTime -= deltaTime;
			return;
		}

		if (!initialized) {
			if (transformTime > 0)
				transformTimer = 0;

			animator = GetComponent<Animator> ();

			if (usingSequence == false || sequenceAnim.Length < 1) {
				if (animator != null && startAnimation != null) {
					if (GetComponent<RandomAnimatorStart>() != null || randomAnimStart == RandomStartType.Animation || randomAnimStart == RandomStartType.AnimationAndTransform)
						PlayAnim(startAnimation, true);
					else
						PlayAnim(startAnimation);
				}
			} else {
				if (usingSequence && sequenceAnim != null && sequenceAnim.Length > sequenceIndex && sequenceAnim[sequenceIndex].Length > 0 && animator != null) {
					PlayAnim(sequenceAnim[sequenceIndex]);
				}
			}

			initialized = true;
		}

		if (rotation != Vector3.zero)
		{
			if (switchEase != SwitchEaseType.Linear && transformTime > 0) {
				float tt, mulMod;
				GetEaseValues(out tt, out mulMod);
				transform.Rotate(rotation * 50 * deltaTime * Easing.Sinusoidal.Out(tt) * mulMod, rotationType == RotationType.PerFrameLocal? Space.Self : Space.World);
			} else
				transform.Rotate(rotation * 50 * deltaTime, rotationType == RotationType.PerFrameLocal ? Space.Self : Space.World);
		}

		if (scaling != Vector3.zero)
		{
			if (switchEase != SwitchEaseType.Linear && transformTime > 0)
			{
				float tt, mulMod;
				GetEaseValues(out tt, out mulMod);
				transform.localScale += scaling * 10 * deltaTime * Easing.Sinusoidal.Out(tt) * mulMod;
			}
			else
				transform.localScale += scaling * 10 * deltaTime;
		}

		//if (translation != Vector3.zero || rotationSwitch || scalingSwitch || translateHeading != TranslateHeading.PerFrameSet || (usingSequence && sequenceAnim.Length > 0 )) {
		{

			if ((translationType == TranslationType.Absolute || translationType == TranslationType.AbsoluteDelta) && transformTime >= 0)
			{
				float tt = transformTimer / transformTime;
				if (switchEase != SwitchEaseType.Linear)
				{
					switch(switchEase) {
						case SwitchEaseType.InOut: tt = Easing.Sinusoidal.InOut(tt); break;
						case SwitchEaseType.In: tt = Easing.Sinusoidal.In(tt); break;
						case SwitchEaseType.Out: tt = Easing.Sinusoidal.Out(tt); break;
						case SwitchEaseType.BounceIn: tt = Easing.Bounce.In(tt); break;
						case SwitchEaseType.BounceOut: tt = Easing.Bounce.Out(tt); break;
					}
				}
				if (!rectTransform)
					transform.position = absolutePosLast + absolutePosFullDelta * tt;
				else
				{
					rectTransform.anchoredPosition3D = absolutePosLast + absolutePosFullDelta * tt;
				}
			}
			else if (switchEase != SwitchEaseType.Linear && transformTime > 0)
			{
				float tt, mulMod;
				GetEaseValues(out tt, out mulMod);
				Vector3 useTranslation = GetTranslationVector();
				if (translationType == TranslationType.PerFrameLocal)
					transform.localPosition += (useTranslation * deltaTime * Easing.Sinusoidal.Out(tt) * mulMod);
				else
					transform.position += (useTranslation * deltaTime * Easing.Sinusoidal.Out(tt) * mulMod);
			}
			else
			{
				Vector3 useTranslation = GetTranslationVector();
				if (translationType == TranslationType.PerFrameLocal)
					transform.localPosition += (useTranslation * deltaTime);
				else
					transform.position += (useTranslation * deltaTime);
			}

			if (transformTimer < transformTime) {
				transformTimer += deltaTime;
				if (transformTimer >= transformTime) {
					if (transformTimeType == TransformTimeType.SingleKill && !(usingSequence && sequenceIndex < sequenceTime.Length - 1)) {
						Destroy(gameObject);
					}
					else if (transformTimeType == TransformTimeType.SingleRepeat && !(usingSequence && sequenceIndex < sequenceTime.Length - 1))
					{
						transformTimer = 0;
						transform.localPosition = orgLocalPos;
						transform.position = orgPos;
						transform.localRotation = orgLocalRot;
						transform.rotation = orgRot;
						transform.localScale = orgScale;
						sequenceIndex = -1;
					}
					else if (transformTimeType == TransformTimeType.SingleContinue && !(usingSequence && sequenceIndex < sequenceTime.Length - 1))
					{
						if (usingSequence) {
							sequenceIndex = -1;
							transformTimer = transformTime - 0.001f;
						} else
							transformTimer = 0;
					}
					else if (transformTimeType == TransformTimeType.SingleStop && !(usingSequence && sequenceIndex < sequenceTime.Length - 1)) {
						usingSequence = false;
						translationOrMagnitudeInX = Vector3.zero;
						rotation = Vector3.zero;
						scaling = Vector3.zero;
						if (animator != null && singleStopAnim != null && singleStopAnim.Length > 0)
							PlayAnim (singleStopAnim);
					} else {
						if (translationSwitch)
						{
							translationOrMagnitudeInX = -translationOrMagnitudeInX;
							if (inverseMagnitudeOnSwitch)
								translateHeadingMagnitude = -translateHeadingMagnitude;
						}
 						if (rotationSwitch)
							rotation = -rotation;
						if (scalingSwitch)
							scaling = -scaling;
						
						transformTimer = 0;
						if (switchRotation != Vector3.zero) {
							if (switchRotationTime <= 0)
								transform.Rotate(switchRotation, rotationType == RotationType.PerFrameLocal ? Space.Self : Space.World);
							else
							{
								iTween.EaseType oldET = iTween.Defaults.easeType;
								iTween.Defaults.easeType = switchRotationEase;
								iTween.RotateAdd(gameObject, switchRotation, switchRotationTime); // doesn't seem like iTween has any local rotation functions... using this for now in both cases (local and world)
								iTween.Defaults.easeType = oldET;
							}
						}

						if (usingSequence) {
							sequenceIndex++; if (sequenceIndex >= sequenceTime.Length) sequenceIndex = 0;

							transformTime = sequenceTime[sequenceIndex];

							if (sequenceTranslationOrMagnitudeInX != null && sequenceTranslationOrMagnitudeInX.Length > sequenceIndex)
							{
								if (translationType == TranslationType.PerFrame || translationType == TranslationType.PerFrameLocal)
									translationOrMagnitudeInX = sequenceTranslationOrMagnitudeInX[sequenceIndex];
								else
									translateHeadingMagnitude = sequenceTranslationOrMagnitudeInX[sequenceIndex].x;
							}
							if (sequenceSwitchRotation != null && sequenceSwitchRotation.Length > sequenceIndex)
								switchRotation = sequenceSwitchRotation[sequenceIndex];
							if (sequenceSwitchRotationTime != null && sequenceSwitchRotationTime.Length > sequenceIndex)
								switchRotationTime = sequenceSwitchRotationTime[sequenceIndex];

							if (sequenceRotation != null && sequenceRotation.Length > sequenceIndex)
								rotation = sequenceRotation[sequenceIndex];
							if (sequenceScaling != null && sequenceScaling.Length > sequenceIndex)
								scaling = sequenceScaling[sequenceIndex];
							if (sequenceSwitchEase != null && sequenceSwitchEase.Length > sequenceIndex)
								switchEase = sequenceSwitchEase[sequenceIndex];

							if (sequenceAnim != null && sequenceAnim.Length > sequenceIndex && sequenceAnim[sequenceIndex].Length > 0 && animator != null) {
								PlayAnim(sequenceAnim[sequenceIndex]);
							}
						}

						switched = !switched;
						if (!switched && restoreYOnSwitch) {
							if (translationType == TranslationType.PerFrameLocal)
								transform.localPosition = GameUtil.SetY(transform.localPosition, orgPos.y);
							else
								transform.position = GameUtil.SetY (transform.position, orgPos.y);
							transform.localScale = orgScale;
							if (switchRotation != Vector3.zero) {
								if (switchRotationTime <= 0) {
									ResetRotation();
								} else
									Invoke ("ResetRotation", switchRotationTime);
							}
						}
					}

					if ((translationType == TranslationType.Absolute || translationType == TranslationType.AbsoluteDelta) && transformTime >= 0) {

						Vector3 useTranslation = translationOrMagnitudeInX;
						if (translationType == TranslationType.AbsoluteDelta)
						{
							if (rectTransform)
								useTranslation = rectTransform.anchoredPosition3D + translationOrMagnitudeInX;
							else
								useTranslation = transform.position + translationOrMagnitudeInX;
						}

						if (usingSequence == false && transformTimeType != TransformTimeType.SingleRepeat && transformTimeType != TransformTimeType.SingleContinue)
							translationOrMagnitudeInX = useTranslation = absolutePosLast;
						if (rectTransform) {
							absolutePosFullDelta = useTranslation - rectTransform.anchoredPosition3D;
							absolutePosLast = rectTransform.anchoredPosition3D;
						} else {
							absolutePosFullDelta = useTranslation - transform.position;
							absolutePosLast = transform.position;
						}
					}	
				}
			}

		}
	}


	// SendMessage or Broadcast (or call directly) to have other objects affect the SimpleTransform ( string format: DelayBeforeChange_AnimName_xTransform_yT_zT )
	// This part is far from nice (and also very far from covering all cases). Refactor to make string more obvious with named params, or replace with an interface or similar
	public void TransformChange(string msg) {

		if (msg != null && msg.Length > 0) {
			string [] msgParts = msg.Split (new char [] { '_' });

			msgParam = msg;
			Invoke ("ChangeT", float.Parse(msgParts [0]));
		}
	}
	private string msgParam;
	void ChangeT() {
		string [] msgParts = msgParam.Split (new char [] { '_' });

		usingSequence = false;

		transformTimer = 0;

		if (animator != null) {
			if (GetComponent<RandomAnimatorStart>() != null)
				PlayAnim(msgParts[1], true);
			else
				PlayAnim (msgParts [1]);
		}

		if (msgParts.Length >= 5) {
			translationOrMagnitudeInX = new Vector3 (float.Parse(msgParts[2]), float.Parse(msgParts[3]), float.Parse(msgParts[4]));
		}

		if (msgParts.Length >= 6) {
			transformTime = float.Parse(msgParts[5]);
		}

		if (msgParts.Length >= 9) {
			rotation = new Vector3 (float.Parse(msgParts[6]), float.Parse(msgParts[7]), float.Parse(msgParts[8]));
		}

	}

	public override void OnExternalHit() {
		if (stopOnExternalHit)
			stopMe = true;
	}
	public override void OnExternalMinorHit() {
	}

}
