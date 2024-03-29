using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalChecker : MonoBehaviour {

	public enum AutoFillIfEmptyType { None, Children, All };
	public enum AutoFillGoalType { All, ToonDollHelpers, Zombies };

	public Goal[] goalChildren = null;

	public AutoFillIfEmptyType autoFillIfEmptyType = AutoFillIfEmptyType.Children;
	public AutoFillGoalType autoFillGoalType = AutoFillGoalType.All;

	private bool wasTriggered = false;

	public string broadcastMessageOnBreak = string.Empty;
	public GameObject [] enableObjects = null;
	public GameObject [] disableObjects = null;
	public string enableMessage = string.Empty;
	public string disableMessage = string.Empty;

	private SoundEmitter soundEmitter; 


	void Start () {

		soundEmitter = gameObject.GetComponent<SoundEmitter> ();

		if ((goalChildren == null || goalChildren.Length < 1) && autoFillIfEmptyType != AutoFillIfEmptyType.None) {

			if (autoFillIfEmptyType == AutoFillIfEmptyType.Children)
				goalChildren = gameObject.GetComponentsInChildren<Goal> ();
			else if (autoFillIfEmptyType == AutoFillIfEmptyType.All)
				goalChildren = GameObject.FindObjectsOfType<Goal> ();

			if (goalChildren != null && autoFillGoalType != AutoFillGoalType.All) {
				List<Goal> restrictedGoals = new List<Goal> ();
				foreach (Goal g in goalChildren) {
					ToonDollHelper tdh = g.gameObject.GetComponent<ToonDollHelper> ();
					if (tdh != null) {
						if (autoFillGoalType == AutoFillGoalType.ToonDollHelpers || (autoFillGoalType == AutoFillGoalType.Zombies && tdh.zombieHitMode == true))
							restrictedGoals.Add (g);
					}
				}
				goalChildren = null;
				if (restrictedGoals.Count > 0)
					goalChildren = restrictedGoals.ToArray ();
			}
		}

	}
	
	void Update () {
		if (goalChildren == null || goalChildren.Length < 1)
			return;

		if (wasTriggered)
			return;

		bool allDone = true;
		foreach (Goal g in goalChildren) {
			if (g.GetNofTimesHit() < g.GetNofRequiredHits ())
				allDone = false;
		}

		if (allDone) {
			wasTriggered = true;

			if (soundEmitter != null)
				soundEmitter.PlaySound ();

			if (broadcastMessageOnBreak.Length > 0)
			{
				BroadcastMessage("OnBrokenContainer", broadcastMessageOnBreak, SendMessageOptions.DontRequireReceiver);
				/* if (goalChildren != null && goalChildren.Length > 0) // Disabled. If want to send message outside of itself, use enableObjects (could be already active) and enableMessage
					foreach (Goal g in goalChildren)
						g.gameObject.SendMessage("OnBrokenContainer", broadcastMessageOnBreak, SendMessageOptions.DontRequireReceiver); */  
			}

			if (enableObjects != null && enableObjects.Length > 0)
				foreach (GameObject go in enableObjects) {
					go.SetActive(true);
					if (enableMessage != string.Empty) go.SendMessage("OnBrokenContainer", enableMessage, SendMessageOptions.DontRequireReceiver);
				}

			if (disableObjects != null && disableObjects.Length > 0)
				foreach (GameObject go in disableObjects) {
					if (disableMessage == string.Empty) go.SetActive(false);
					if (disableMessage != string.Empty) go.SendMessage("OnBrokenContainer", disableMessage, SendMessageOptions.DontRequireReceiver);
				}
		}

	}
}
