using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trigger_OnOff : MonoBehaviour {

	public string actionTag = "ColliderDude";

	public GameObject activate;
	public GameObject kill;

	public SimpleTransform simpleTransform;
	public string transformActivate = string.Empty;
	public string transformKill = string.Empty;
	public string transformStay = string.Empty;

	public SingleSfx singleSfx = SingleSfx.Baloon2;

	private SoundEmitter soundEmitter = null;

	public bool if_TDH_AllowOnlyActive = false;
	public bool if_TDH_AllowRagdoll = true;
	public bool if_TDH_AllowRagdollEnterOnly = false;
	public Material onMaterial;
	public Material offMaterial;

	void Start () {
		soundEmitter = GetComponent<SoundEmitter> ();
		SetMaterial(offMaterial);
	}
	private void SetMaterial(Material m) {
		if (m)
		{
			MeshRenderer mr = GetComponent<MeshRenderer>();
			if (mr && mr.material)
			{
				mr.material = m;
			}
		}
	}
	private bool ActiveCheck(GameObject findMe, bool isEnter) {
		if (findMe != null) {
			ToonDollHelper tdh = findMe.GetComponent<ToonDollHelper> ();
			if (tdh != null && tdh.IsActive () == false && if_TDH_AllowOnlyActive == true)
				return false;
			if (tdh != null && tdh.IsRagDoll () == true && if_TDH_AllowRagdoll == false) {
				return false;
			}
			if (tdh != null && tdh.IsRagDoll () == true && if_TDH_AllowRagdollEnterOnly == true && isEnter == false) {
				return false;
			}

			return true;
		}
		return false;
	}

	private void OnTriggerEnter(Collider collider)
	{
		GameObject findMe = GameUtil.FindParentWithTag (collider.gameObject, actionTag);
		if (ActiveCheck(findMe, true)) {
			
			if (kill != null) kill.SetActive (false);	
			if (activate != null) activate.SetActive (true);

			if (simpleTransform != null && transformActivate != string.Empty)
				simpleTransform.OnBrokenContainer(transformActivate);

			if (soundEmitter == null) {
				SoundManager.instance.PlaySingleSfx (singleSfx, true, false, 0, 0.44f);
			} else {
				if (soundEmitter.emitterType == SoundEmitter.EmitterType.RemoteControlled)
					soundEmitter.PlaySound ();
			}

		}
		SetMaterial(onMaterial);
	}

	private void OnTriggerExit(Collider collider)
	{
		GameObject findMe = GameUtil.FindParentWithTag (collider.gameObject, actionTag);
		if (ActiveCheck(findMe, false)) {

			if (kill != null) kill.SetActive (true);	
			if (activate != null) activate.SetActive (false);

			if (simpleTransform != null && transformKill != string.Empty)
				simpleTransform.OnBrokenContainer(transformKill);
			
			if (soundEmitter == null) {
				SoundManager.instance.PlaySingleSfx (singleSfx, true, false, 0, 0.44f);
			} else {
				if (soundEmitter.emitterType == SoundEmitter.EmitterType.RemoteControlled)
					soundEmitter.PlaySound ();
			}
		}
		SetMaterial(offMaterial);

	}

	private void OnTriggerStay(Collider collider)
	{
		GameObject findMe = GameUtil.FindParentWithTag (collider.gameObject, actionTag);
		if (findMe != null) {
			if (simpleTransform != null && transformStay != string.Empty)
				simpleTransform.OnBrokenContainer(transformStay);
		}
	}

}
