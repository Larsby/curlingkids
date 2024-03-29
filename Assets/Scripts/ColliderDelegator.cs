using UnityEngine;

public abstract class ExternalHit : MonoBehaviour
{
	public abstract void OnExternalHit();
	public abstract void OnExternalMinorHit();
}

public class ColliderDelegator : MonoBehaviour {

	public enum HitType { None, Any, Tag, ParentTag };

	public HitType stopOnHit = HitType.Any;
	public string[] stopHitTags;

	private ExternalHit[] externalHitDelegators;

	public int maxHits = -1;
	public float hitDelay = 0;
	private bool stopMe = false;

	void Start () {
		externalHitDelegators = GetComponents<ExternalHit>();
	}
	
	public void OnCollisionEnter(Collision collision)
	{
		OnTriggerEnter(collision.collider);
	}


	public void OnTriggerEnter(Collider collider)
	{
		bool wasHit = false;

		if (stopMe || stopOnHit == HitType.None)
			return;

		if (stopOnHit == HitType.Any)
		{
			wasHit = true;
		}

		if (stopHitTags != null && stopHitTags.Length > 0)
		{
			foreach (string hitTag in stopHitTags)
			{
				if (stopOnHit == HitType.Tag && collider.gameObject.name == hitTag)
				{
					wasHit = true;
				}
				if (stopOnHit == HitType.ParentTag && GameUtil.FindParentWithTag(collider.gameObject, tag) != null)
				{
					wasHit = true;
				}
			}
		}

		if (wasHit)
		{
			maxHits--;
			if (maxHits == 0) {
				stopMe = true;
			}

			if (stopMe == false && hitDelay > 0) {
				stopMe = true;
				Invoke("ActivateCollision", hitDelay);
			}

			if (externalHitDelegators != null && externalHitDelegators.Length > 0)
				foreach (ExternalHit eh in externalHitDelegators)
					eh.OnExternalHit();
		}
	}

	void ActivateCollision() {
		stopMe = false;
	}

}
