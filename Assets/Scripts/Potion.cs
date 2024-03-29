using UnityEngine;

public class Potion : InvokeWithObject {

	public enum PotionType { None, ScaleRelative, ScaleAbsolute };

	public PotionType potionType = PotionType.None;
	public Vector3 value = new Vector3(0.5f, 0.5f, 0.5f);
	public float animTime = 0.5f;
	public LeanTweenType easeType = LeanTweenType.linear;
	public bool destroyPotion = true;
	public float restorePotionEffectTime = -1;
	public float restoreTriggerTime = 2f;
	public bool scaleJoints = false;
	public bool scaleJointsAnimated = true;

	private bool wasUsed = false;

	private Renderer _renderer;
	private Collider _collider;

	class cbScaleData {
		public Transform t;
		public Vector3 value;
		public cbScaleData(Transform _t, Vector3 _value) { t=_t; value = _value; }
	}

	void Start () {
		_renderer = GetComponent<Renderer>();
		_collider = GetComponent<Collider>();
	}
	
	private void Reset() {
		if (_renderer != null) _renderer.enabled = true;
		if (_collider != null) _collider.enabled = true;
		// gameObject.SetActive (true);
		wasUsed = false;
	}

	void OnCollisionEnter(Collision collision) {
		OnTriggerEnter (collision.collider);
	}

	void OnTriggerEnter(Collider other) {

		GameObject findMe = GameUtil.FindParentWithTag (other.gameObject, "Player");

		if (findMe != null && !wasUsed) {

			ToonDollHelper tdh = findMe.GetComponent<ToonDollHelper> ();

			if (tdh != null) {

				Transform t = tdh.gameObject.transform.Find ("Root");
				Vector3 restoreScale = Vector3.zero;

				switch (potionType) {

				case PotionType.ScaleRelative:
						restoreScale = -value;
						LeanTween.scale (t.gameObject, t.gameObject.transform.localScale + value, animTime).setEase(easeType);
						if (scaleJoints) ScaleJoints(t.gameObject, t.gameObject.transform.localScale + value, scaleJointsAnimated? animTime : -1);
						break;

				case PotionType.ScaleAbsolute:
						restoreScale = t.gameObject.transform.localScale - value;
						LeanTween.scale (t.gameObject, value, animTime).setEase (easeType);
						if (scaleJoints) ScaleJoints(t.gameObject, value, scaleJointsAnimated ? animTime : -1);
						break;
				}

				if (restorePotionEffectTime > 0 && (potionType == PotionType.ScaleAbsolute || potionType == PotionType.ScaleRelative)) {
					cbScaleData cbsd = new cbScaleData (t, restoreScale);
					InvokeWO (RestoreScale, restorePotionEffectTime + animTime, cbsd);
				}

				if (destroyPotion) {
					//gameObject.SetActive (false); // Setting the object to inactive immediately stops all coroutines! (i.e. not compatible with e.g. InvokeWO/InvokeWA)
					if (_renderer != null) _renderer.enabled = false;
					if (_collider != null) _collider.enabled = false;
				}

				if (restoreTriggerTime > 0)
					InvokeWA (Reset, restoreTriggerTime);

				wasUsed = true;
			}
		}
	}

	private void RestoreScale(System.Object myObject) {
		cbScaleData cbsd = myObject as cbScaleData;
		if (cbsd != null) {
			LeanTween.scale (cbsd.t.gameObject, cbsd.t.gameObject.transform.localScale + cbsd.value, animTime).setEase(easeType);
			if (scaleJoints) ScaleJoints(cbsd.t.gameObject, cbsd.t.gameObject.transform.localScale + cbsd.value, scaleJointsAnimated ? animTime : -1);
		}
	}

	private void ScaleJoints(GameObject g, Vector3 newScale, float time = -1) {

		// complicated (possible?) to get animated joint scaling to work properly, with the weirdness surrounding autoConfigureConnectedAnchor
			if (time > 0) {
			ScaleJointsAnimated(g, newScale, time);
			return;
		} 

		Joint[] cjs = g.GetComponentsInChildren<Joint>();

		Vector3 relativeChange = new Vector3(newScale.x / g.transform.localScale.x, newScale.y / g.transform.localScale.y, newScale.z / g.transform.localScale.z);

		if (cjs != null && cjs.Length > 0) {
			//print(relativeChange.ToString("F4"));
			foreach (Joint cj in cjs) {
				cj.autoConfigureConnectedAnchor = false;
				cj.connectedAnchor = new Vector3(cj.connectedAnchor.x * relativeChange.x, cj.connectedAnchor.y * relativeChange.y, cj.connectedAnchor.z * relativeChange.z);
			}
		}

		// If we don't restore autoConfigureConnectedAnchor to true, then further joint scaling has no effect, e.g. when restoring potion effect! Don't ask me...
		InvokeWO(RestoreAutoconfigure, animTime + 0.05f, g);
	}

	void RestoreAutoconfigure(System.Object myObject) {
		Joint[] cjs = ((GameObject)myObject).GetComponentsInChildren<Joint>();
		if (cjs != null && cjs.Length > 0)
			foreach (Joint cj in cjs)
				cj.autoConfigureConnectedAnchor = true;
	}


	private Vector3 [] jointScaleOrgValues;
	private Vector3 [] jointScaleTargetValues;
	private Joint [] affectedJoints;

	private void ScaleJointsAnimated(GameObject g, Vector3 newScale, float time)
	{
		iTween.Stop(gameObject);
		affectedJoints = g.GetComponentsInChildren<Joint>();

		Vector3 relativeChange = new Vector3(newScale.x / g.transform.localScale.x, newScale.y / g.transform.localScale.y, newScale.z / g.transform.localScale.z);

		if (affectedJoints != null && affectedJoints.Length > 0)
		{
			jointScaleOrgValues = new Vector3[affectedJoints.Length];
			jointScaleTargetValues = new Vector3[affectedJoints.Length];

			int i = 0;
			foreach (Joint cj in affectedJoints)
			{
				cj.autoConfigureConnectedAnchor = false;
				jointScaleOrgValues[i] = cj.connectedAnchor;
				jointScaleTargetValues[i]= new Vector3(cj.connectedAnchor.x * relativeChange.x, cj.connectedAnchor.y * relativeChange.y, cj.connectedAnchor.z * relativeChange.z);
				i++;
			}

			iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", 1, "time", time, "easetype", "linear", "onUpdate", "UpdateJointScaling"));
		}
	}

	void UpdateJointScaling(float t) {
		int i = 0;

		foreach (Joint cj in affectedJoints) {
			cj.autoConfigureConnectedAnchor = false;
			cj.connectedAnchor = new Vector3(jointScaleOrgValues[i].x * (1 - t) + jointScaleTargetValues[i].x * t,
			                                 jointScaleOrgValues[i].y * (1 - t) + jointScaleTargetValues[i].y * t,
			                                 jointScaleOrgValues[i].z * (1 - t) + jointScaleTargetValues[i].z * t );
			i++;
		}
	}

	private void LateUpdate()
	{
		if (affectedJoints != null)
			foreach (Joint cj in affectedJoints)
				cj.autoConfigureConnectedAnchor = true;

		if (iTween.Count(gameObject) == 0)
			affectedJoints = null;
	}

}
