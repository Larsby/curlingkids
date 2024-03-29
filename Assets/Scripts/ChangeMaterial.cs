using UnityEngine;

public class ChangeMaterial : MonoBehaviour {

	public PhysicMaterial physMat;

	public SingleSfx singleSfx = SingleSfx.None;
	public bool forceMaterial = false;

	void Start(){
	}

	private void OnTriggerEnter(Collider coll)
	{
		GameObject findMe = GameUtil.FindParentWithTag(coll.gameObject, "Player");
		ToonDollHelper tdh = null;
		if (findMe != null)
			tdh = findMe.GetComponent<ToonDollHelper>();

		if (findMe != null && tdh != null && !tdh.isPlayerHelper && !tdh.zombieHitMode)
		{
			tdh.SetMaterial(physMat, forceMaterial, true);;
			SoundManager.instance.PlaySingleSfx(singleSfx, true, false, 0, 0.44f);
		}
	}

}
