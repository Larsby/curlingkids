using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskListManager : MonoBehaviour {

	public GameObject taskItemPrefab;
	public GameObject itemContainer;
	public bool dailyOrWeekly = false;
	public Color completed;
	public Color uncompleted;
	public Text tasksCompleted;
	public Text dailyCompleted;

	private bool CreateTaskListItemForQuest(int index) {
		int percentage = StaticTaskManager.GetPercentage(index);
		bool b = percentage == 100 ? true : false;
		return CreateTaskListItemForQuest(StaticTaskManager.GetName(index), StaticTaskManager.GetDecription(index), "" + StaticTaskManager.GetCredits(index), percentage, b);
	
	}
	private bool CreateTaskListItemForQuest(string _name, string description, string credits, int percentage, bool _completed) {

		GameObject g = Instantiate(taskItemPrefab, itemContainer.transform, false);

		Text[] t = g.GetComponentsInChildren<Text>();


		t[0].text = _name;
		t[1].text = description;
		t[2].text = credits;
		t[3].text = "" + percentage + "%";
		bool b = _completed;
		Image img = g.GetComponentInChildren<Image>();
		img.color = b ? completed : uncompleted;
		Color darker = GameUtil.IntColor(0, 40, 50);
		if (!b)
			foreach (Text tc in t)
				tc.color = darker;
		return b;
		
	}
	void Awake() {
		SoundManager.Create ();
		Application.targetFrameRate = 60;
	}

	void Start () {
		int completed = 0;
		int dcompleted = 0;
		int staticTasks = 0;
		int dailyTasks = 0;
		int padding = 0;
	
		for (int i = 0; i < StaticTaskManager.GetNumberOfTasks (); i++) {
			if (dailyOrWeekly == false)
			{
				StaticTaskManager.TaskOccurance occ = StaticTaskManager.GetOccurance(i);
				if (occ == StaticTaskManager.TaskOccurance.Once || occ ==StaticTaskManager.TaskOccurance.Other)
				{
					if(CreateTaskListItemForQuest(i)) {
						completed++;
					}
					staticTasks++;
				}
			} else {
				if (StaticTaskManager.GetOccurance(i) == StaticTaskManager.TaskOccurance.Daily) {
					if(CreateTaskListItemForQuest(i)) {
						dcompleted++;
					}
					dailyTasks++;
				}
			}
		}
		if(tasksCompleted) {
			tasksCompleted.text = "" + completed + "/" + staticTasks;
		}
		if(dailyCompleted) {
			dailyCompleted.text = "" + dcompleted + "/" + dailyTasks;
		}
		if(StaticTaskManager.IsGamePirated()) {
			CreateTaskListItemForQuest("Hax0r the game", "change things in the plist for profit", "", 100, true);	
		}
	}

	void Update () {}

	public void BackButton() {
		StaticManager.PopScene ();
	}

}
