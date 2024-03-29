using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AwardTrigger : MonoBehaviour
{
	public StaticTaskManager.TaskType taskType;
	public bool isTrigger = false;
	public bool destroyOnHit = true;
	public string description;
	private Dictionary<GameObject, int> hitDictionary;
	// Use this for initialization
	void Start()
	{
		hitDictionary = new Dictionary<GameObject, int>();
	}
	void OnTriggerEnter(Collider collider)
	{
		if (isTrigger)
		{
			Route(collider);
		}
	}
	void Route(Collider collider)
	{
		if (this.enabled)
		{
			GameObject findMe = GameUtil.FindParentWithTag(collider.gameObject, "Player");
			ToonDollHelper tdh = null;
			if (findMe != null)
				tdh = findMe.GetComponent<ToonDollHelper>();

			if (findMe != null && tdh != null && !tdh.isPlayerHelper && !tdh.zombieHitMode)
			{	
				if(hitDictionary == null) {
					hitDictionary = new Dictionary<GameObject, int>();
				}
				if (hitDictionary.ContainsKey(findMe) == false)
				{
					hitDictionary.Add(findMe, 1);
					StaticTaskManager.RouteTask(taskType, -1, description);
					if (destroyOnHit)
					{
						Destroy(this);
					}
				}
			}
		}
	}
	void OnCollisionEnter(Collision collision)
	{
		if (!isTrigger)
		{
			Route(collision.collider);
		}
	}
	// Update is called once per frame
	void Update()
	{

	}
}
