using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public static class StaticTaskManager : System.Object
{

	public enum TaskType
	{
		UnlockWorld = 1, CharacterThrow = 2, bounce = 3, Loop = 4, Ring = 5, Portal = 6, Glass = 7,
		Boost = 8, Zoombie = 9, Cones = 10, Balloon = 11, Umpabumpa = 12, ThreeStars = 13, Airborne = 14, Speedy = 15, Other = 16
	}
	public enum TaskOccurance { Once = 100, Daily = 200, Weekly = 300, Other = 400 };
	public enum AwardType { Star, Coin, Gem }
	[Serializable]
	public class TaskDelegate
	{
		public delegate void IncreasDelegate();
		public delegate int CountDelegate();
		public delegate void IncreaseWithDelegate(int increase);
		public delegate int CountDelegateWith(int index);
		public IncreasDelegate increaseDelegate;
		public CountDelegate countDelegate;
		public IncreaseWithDelegate increaseWithDelegate;
		public CountDelegateWith countDelegateWith;
	}

	[Serializable]
	public class Task
	{
		public string name;
		public string description;
		public string triggerNameForOther;


		string hash;
		public int credits;
		public TaskType taskType;
		public bool awarded;
		public AwardType awardType;
		public int value1;
		public int value2;
		public int value3;
		private int currentTaskValue;
		TaskDelegate taskDelegate;
		public TaskOccurance occurance;
		DateTime availableDate;
		private bool evaluateValue;
		public Task(string name, string description, int credits, TaskType taskType, AwardType awardType, int value1 = 0, int value2 = 0, int value3 = 0, TaskOccurance occurance = TaskOccurance.Once, string triggerNameForOther = null,bool evaluateValue = false)
		{
			this.awarded = false;
			this.name = I2.Loc.LocalizationManager.GetTranslation(name);
			    
			this.description = I2.Loc.LocalizationManager.GetTranslation(description);

			this.taskType = taskType;
			this.awardType = awardType;
			this.credits = credits;

			this.value1 = value1;
			this.value2 = value2;
			this.value3 = value3;
			taskDelegate = GetDelegate();
			this.occurance = occurance;
			this.triggerNameForOther = triggerNameForOther;
			this.hash = GetHash(triggerNameForOther);  
			availableDate = DateTime.Now;
		}
		private string GetHash(string name)
		{
			string description = "";
			if (name != null)
			{
				description = name;
			}
			string hash = "Task_Type " + taskType + "_" + description + "_" + occurance + "_value" + value1;
			return hash;
		}
		private string GetHash()
		{
			return GetHash(null);
		}
		public void RunDelegate(int val, string description, bool increase = true,bool reward = false, bool rawvalue = false)
		{
			int count = 0;
			if (occurance == TaskOccurance.Once)
			{
				if (taskDelegate != null)
				{

					if (val == -1)
					{
						if (increase)
						{
							taskDelegate.increaseDelegate();
						}
						count = taskDelegate.countDelegate();
						Evaluate(this.taskType, count);
					}
					else
					{
						Evaluate(this.taskType, val);
					}

				}
			}
			else
			{


				if (description != null)
				{
					if (triggerNameForOther != null)
					{
						if (triggerNameForOther.Equals(description) == false)
						{
							return; // This is not our task even though its an Task.Other, the descriptions don't match so no saving player prefs
						}
					}
				}
				if (PlayerPrefs.HasKey(hash) == false)
				{
					PlayerPrefs.SetInt(hash, 1);
					PlayerPrefs.Save();
					count = 1;
				}
				else
				{
					if (evaluateValue == false)
					{
						count = PlayerPrefs.GetInt(hash);

						if (val == -1)
						{
							count++;
						}
						else
						{
							count = count + val;
						}
						PlayerPrefs.SetInt(hash, count);
						PlayerPrefs.Save();
					}
					else
					{
						count = val;
					}
					if(rawvalue) {
						count = val;
					}
					Evaluate(this.taskType, count, 0, 0, description, reward);
				}
			}
		}

		private int WorldCount()
		{
			int result = 0;
			if (taskType == TaskType.UnlockWorld)
			{


				if (awarded)
				{
					result = 100;
				}
			}

			return result;
		}
		private TaskDelegate GetDelegate()
		{

			TaskDelegate taskDelegate = new TaskDelegate();
			taskDelegate.increaseWithDelegate = null;
			taskDelegate.increaseDelegate = null;
			taskDelegate.countDelegate = null;
			switch (taskType)
			{
				case TaskType.UnlockWorld:
					taskDelegate.increaseDelegate = null;
					taskDelegate.countDelegate = new TaskDelegate.CountDelegate(WorldCount);
					break;
				case TaskType.CharacterThrow:
					taskDelegate.increaseDelegate = new TaskDelegate.IncreasDelegate(StaticManager.IncreaseTossCountByOne);
					taskDelegate.countDelegate = new TaskDelegate.CountDelegate(StaticManager.GetTotalTossCount);

					break;
				case TaskType.bounce:
					taskDelegate.increaseDelegate = new TaskDelegate.IncreasDelegate(StaticManager.IncreaseCurbTake);
					taskDelegate.countDelegate = new TaskDelegate.CountDelegate(StaticManager.GetCurbTakeCount);
					break;
				case TaskType.Loop:
					taskDelegate.increaseDelegate = new TaskDelegate.IncreasDelegate(StaticManager.IncreaseLoopCount);
					taskDelegate.countDelegate = new TaskDelegate.CountDelegate(StaticManager.GetTotalLoopCount);

					break;
				case TaskType.Ring:
					taskDelegate.increaseDelegate = new TaskDelegate.IncreasDelegate(StaticManager.IncreaseRingsCount);
					taskDelegate.countDelegate = new TaskDelegate.CountDelegate(StaticManager.GetTotalRingsCount);

					break;
				case TaskType.Boost:
					taskDelegate.increaseDelegate = new TaskDelegate.IncreasDelegate(StaticManager.IncreaseBoostCount);
					taskDelegate.countDelegate = new TaskDelegate.CountDelegate(StaticManager.GetTotalBoostCount);

					break;
				case TaskType.Portal:
					taskDelegate.increaseDelegate = new TaskDelegate.IncreasDelegate(StaticManager.IncreasePortalsCount);
					taskDelegate.countDelegate = new TaskDelegate.CountDelegate(StaticManager.GetTotalPortalsCount);

					break;
				case TaskType.Glass:
					taskDelegate.increaseDelegate = new TaskDelegate.IncreasDelegate(StaticManager.IncreaseBrokenGlassCount);
					taskDelegate.countDelegate = new TaskDelegate.CountDelegate(StaticManager.GetBrokenGlassCount);

					break;
				case TaskType.Zoombie:
					taskDelegate.increaseDelegate = new TaskDelegate.IncreasDelegate(StaticManager.IncreaseZoombieHitCount);
					taskDelegate.countDelegate = new TaskDelegate.CountDelegate(StaticManager.GetZoombieHitCount);

					break;
				case TaskType.Cones:
					taskDelegate.increaseDelegate = new TaskDelegate.IncreasDelegate(StaticManager.IncreaseConeHitCount);
					taskDelegate.countDelegate = new TaskDelegate.CountDelegate(StaticManager.GetConeHitCount);

					break;
				case TaskType.Balloon:
					taskDelegate.increaseDelegate = new TaskDelegate.IncreasDelegate(StaticManager.IncreaseBalloonHitCount);
					taskDelegate.countDelegate = new TaskDelegate.CountDelegate(StaticManager.GetBalloonHitCount);

					break;
				case TaskType.ThreeStars:
					taskDelegate.increaseDelegate = new TaskDelegate.IncreasDelegate(StaticManager.IncreaseThreeStars);
					taskDelegate.countDelegate = new TaskDelegate.CountDelegate(StaticManager.GetThreeStarsCount);
					break;

				default:
			//			Debug.Log("Got unhandled task in GetDelegate" + taskType);
					break;
			}
			return taskDelegate;
		}
		public Task()
		{
			this.awarded = false;
		}
		public void SetValueToPlayerPrefs(int val)
		{
			PlayerPrefs.SetInt(hash, val);
			PlayerPrefs.Save();
		}
		public int GetValueFromPlayerPrefs()
		{
			if (PlayerPrefs.HasKey(hash))
			{
				return PlayerPrefs.GetInt(hash);
			}
			return 0;
		}
		public bool IsAvailableToPlayAgain()
		{
			int val = 0;
			if (occurance == TaskOccurance.Other)
				return false;
			if (occurance != TaskOccurance.Once)
			{
				val = GetValueFromPlayerPrefs();
				if (val != value1)
				{
					DateTime now = DateTime.Now;
					int result = availableDate.CompareTo(now);
					if (result == -1)
					{
						awarded = false;
						return true;
					}
				}

			}
			return false;
		}

		public int GetTaskPercentage()
		{
			int val = 0;
			if (awarded)
				return 100;
			if (taskType == TaskType.Other)
			{
				if (triggerNameForOther != null && triggerNameForOther.StartsWith("Star"))
				{
					int world = -1;
					if (triggerNameForOther.Contains("Star0")) {
						world = 0;
					}
					if (triggerNameForOther.Contains("Star1"))
					{
						world = 1;
					}
					if(triggerNameForOther.Contains("Star2")) {
						world = 2;
					}
					if(world >-1) {
						int nofMax = StaticLevels.levelsPerWorld[world] * 3;
						int collected = StaticManager.GetNofStars(world);

						float f = (float)collected / (float)nofMax;
						f = f * 100.0f;
						int v = (int)f;

						return v;
					}
				}
				if (triggerNameForOther != null && triggerNameForOther.StartsWith("LevelUnlock"))
				{

					int world = -1;
					if (triggerNameForOther.Contains("LevelUnlock1"))
					{
						world = 0;
					}
					if (triggerNameForOther.Contains("LevelUnlock2"))
					{
						world = 1;
					}
					if(PlayerPrefs.HasKey("WorldStars_" + world) == false) {
						return 0;
					}
					int collectedStarsForWorld = PlayerPrefs.GetInt("WorldStars_" + world);
					if (collectedStarsForWorld >= value1)
					{
						return 100;
					}
					else
					{
						float f = (float)collectedStarsForWorld / (float)value1;
						f = f * 100.0f;
						int v = (int)f;

						return v;
					}

				}

				if (PlayerPrefs.HasKey(hash))
				{
					int value = PlayerPrefs.GetInt(hash);
					if (value >= value1)
					{
						return 100;
					}
					else
					{
						float f = (float)value / (float)value1;
						f = f * 100.0f;
						int v = (int)f;

						return v;
					}
				}
			}
			if (occurance != TaskOccurance.Once)
			{
				val = GetValueFromPlayerPrefs();
				if (val >= value1)
				{

					if (IsAvailableToPlayAgain())
					{
						val = 0;
						this.awarded = false;
						SetValueToPlayerPrefs(0);

					}
					else
					{
						return 100;
					}
				}
			}
			else
			{
				if (awarded)
					return 100;
			
				TaskDelegate taskdelegate = GetDelegate();
				if (taskdelegate != null && taskdelegate.countDelegate != null)
				{
					val = taskdelegate.countDelegate();
				}
			}
			if (val == 0) return 0;
			float percent = (float)val / (float)this.value1;
			float res =percent * 100.0f;
			if (res > 100)
			{
				res = 100;
			}

			 val = (int)res;
			return val;
		}
		public bool IsAwarded()
		{
			bool award = awarded;
			if (occurance != TaskOccurance.Once && award)
			{
				award = !IsAvailableToPlayAgain();
			}
			return award;
		}
		public bool Evaluate(TaskType taskType, int value1, int value2 = 0, int value3 = 0, string otherTriggerName = null,bool reward = false)
		{
			bool done = false;
			if (taskType != this.taskType)
				return false;
			currentTaskValue = value1;
			if (awarded)
			{
				if (occurance == TaskOccurance.Once || occurance == TaskOccurance.Other)
					return false;

				DateTime now = DateTime.Now;
				int result = now.CompareTo(availableDate);
				if (result < 0)
				{
					return false;
				}
				else if (result == 1)
				{
					this.awarded = false;
				}


			}

			if (occurance == TaskOccurance.Other)
			{
				if (otherTriggerName == null) return false;
				if (otherTriggerName.Equals(triggerNameForOther) == false)
				{
					return false;
				}
			}
			if(taskType == TaskType.bounce)
			{
			//	Debug.Log("Boounce!!");
			}

			switch (taskType)
			{


				case TaskType.CharacterThrow:

					if (value1 >= this.value1)
					{
						done = true;
					}
					break;
				case TaskType.Speedy:
					if (value1 <= this.value1)
					{
						done = true;
					}
					break;
				case TaskType.Other:
					done = this.value1 == value1;
					//hardcoded for now. 
					/*
					if(otherTriggerName.Equals("LevelUnlock1")) {
						done = StaticManager.HaveFinishedAllLevelsForWorld(0);
					} else if(otherTriggerName.Equals("LevelUnlock2")) {
						done = StaticManager.HaveFinishedAllLevelsForWorld(0);
					}*/
					break;

				default:
					done = this.value1 == value1;
					break;

			}
			if(reward) {
				done = true;
			}
			if (done)
			{

				awarded = true;
				PlayerPrefs.SetInt("CurlingKidsTask"+this.name+"_"+this.value1,1);
				bNeedsSave = true;
				if (this.awardType == AwardType.Coin)
				{
					StaticManager.AddCredits(credits);
				}
				if (this.awardType == AwardType.Gem)
				{
					StaticManager.AddGems(credits);
				}
				if (CharacterManager.instance != null)
				{
					if (this.awardType == AwardType.Coin)
					{
						CharacterManager.instance.ShowCoinTaskRewardPopup(name, description, credits);
					}
					if (this.awardType == AwardType.Gem)
					{
						CharacterManager.instance.ShowGemTaskRewardPopup(name, description, credits);
					}
				}
				double day = 0;
				double minute = 0;
				if (occurance == TaskOccurance.Daily)
				{

					minute = 0;
					day = 1;
				}
				if (occurance == TaskOccurance.Weekly)
				{
					day = 7;
					minute = 0;
				}
				if (occurance == TaskOccurance.Daily || occurance == TaskOccurance.Weekly)
				{
					//		PlayerPrefs.SetInt(GetHash(), 0);
					//this.value1 = 0;
					availableDate = DateTime.Now;

					availableDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0, 0,
												 availableDate.Kind);

					availableDate = availableDate.AddDays(day);
					availableDate = availableDate.AddMinutes(minute);
					bNeedsSave = true;
					this.awarded = true;
				}
			}

			return done;
		}
	}

	[Serializable]
	public class TaskData
	{
		public Task[] tasks;
	}

	private static TaskData taskData;
	private const string saveName = "/TaskInfo1.dat";
	private static bool bNeedsSave = false;
	private static bool bWasLoaded = false;

	public static void Load(bool erase = false)
	{

		if (erase && File.Exists(Application.persistentDataPath + saveName))
		{
			File.Delete(Application.persistentDataPath + saveName);
		}

		// Initial values
		taskData = new TaskData();

		taskData.tasks = new Task[]
		{

			new Task("Baby toss",  "Throw one time",10,TaskType.CharacterThrow,AwardType.Coin,1),
			new Task("Throw 10 times", "Throw 10 times",10,TaskType.CharacterThrow,AwardType.Coin,10),
			new Task("Throw 250 times", "Throw 250 times",25,TaskType.CharacterThrow,AwardType.Coin,250),
			new Task("Throw 1000 times", "Throw 1000 times",100,TaskType.CharacterThrow,AwardType.Coin,1000),
			new Task("Take 5 curbs", "Slide along a curbed wall 5 times",10,TaskType.bounce,AwardType.Coin,5),
			new Task("Take 100 curbs", "Slide along a curbed wall 100 times",50,TaskType.bounce,AwardType.Coin,100),
			new Task("Loop 10 times", "Get dizzy, loop 100 times",50,TaskType.Loop,AwardType.Coin,10),
			new Task("Use boost 5 times", "Live life with speed",10,TaskType.Boost,AwardType.Coin,5),
			new Task("Use boost 50 times", "Live life with speed",50,TaskType.Boost,AwardType.Coin,50),
			new Task("Break 10 glass windows", "Shatter windows for fun and profit",10,TaskType.Glass,AwardType.Coin,10),
			new Task("Use teleporter 5 times", "Zap to the right spot using the teleporter",10,TaskType.Portal,AwardType.Coin,5),
			new Task("Save  100 Zoombies", "Hit a zombie to save it. Now repeat 99 times",100,TaskType.Zoombie,AwardType.Coin,500),
			new Task("Cone anger", "Kick those cones",35,TaskType.Cones,AwardType.Coin,30,0,0,TaskOccurance.Once),
			new Task("Cone rage", "Kick those cones",90,TaskType.Cones,AwardType.Coin,350,0,0,TaskOccurance.Once),
			new Task("Three stars I", "Unlock 5 three star stages",25,TaskType.ThreeStars,AwardType.Coin,5),
			new Task("Three stars II", "Unlock 10 three star stages",50,TaskType.ThreeStars,AwardType.Coin,20),
			new Task("Three stars III", "Unlock 15 three star stages",75,TaskType.ThreeStars,AwardType.Coin,35),
			new Task("Three stars IV", "Unlock 25 three star stages",100,TaskType.ThreeStars,AwardType.Coin,50),
			new Task("Hug a bear", "Smash into a bear",25,TaskType.Other,AwardType.Coin,1,0,0,TaskOccurance.Other,"Teddybear"),
			new Task("Interference I", "Smash into a referee",25,TaskType.Other,AwardType.Coin,1,0,0,TaskOccurance.Other,"Referee"),
			new Task("Interference II", "Smash into 25 referee",50,TaskType.Other,AwardType.Coin,25,0,0,TaskOccurance.Other,"Referee"),
			new Task("Curling star", "Hit 10 curling stones",10,TaskType.Other,AwardType.Coin,10,0,0,TaskOccurance.Other,"CurlingHit"),
			new Task("Bench press athlete", "Hit 10 benches",30,TaskType.Other,AwardType.Coin,10,0,0,TaskOccurance.Other,"BenchPress"),
			new Task("Bench press star", "Hit 50 benches",60,TaskType.Other,AwardType.Coin,50,0,0,TaskOccurance.Other,"BenchPress"),
			new Task("Box collector", "Push 5 boxes around",10,TaskType.Other,AwardType.Coin,5,0,0,TaskOccurance.Other,"Boxes"),
			new Task("Dimond hunter", "Collect 10 diamonds",10,TaskType.Other,AwardType.Coin,10,0,0,TaskOccurance.Other,"DiamondHunter"),
			new Task("Big deal dimond hunter", "Collect 20 diamonds",25,TaskType.Other,AwardType.Coin,50,0,0,TaskOccurance.Other,"DiamondHunter"),
			new Task("Trophy dimond hunter", "Collect 100 diamonds",100,TaskType.Other,AwardType.Coin,200,0,0,TaskOccurance.Other,"DiamondHunter"),
			new Task("Unlock all levels for world 1", "Get 1 or more stars on every level!",100,TaskType.Other,AwardType.Coin,StaticLevels.levelsPerWorld[0],0,0,TaskOccurance.Other,"LevelUnlock1"),
			new Task("Unlock all levels for world 2", "Get 1 or more stars on every level!",100,TaskType.Other,AwardType.Coin,StaticLevels.levelsPerWorld[1],0,0,TaskOccurance.Other,"LevelUnlock2"),
			new Task("Collect all stars!", "Collect all stars for world 1",200,TaskType.Other,AwardType.Coin,StaticLevels.levelsPerWorld[0],0,0,TaskOccurance.Other,"Star0",true),
			new Task("Collect all stars!", "Collect all stars for world 2",200,TaskType.Other,AwardType.Coin,StaticLevels.levelsPerWorld[1],0,0,TaskOccurance.Other,"Star1",true),
			new Task("Collect all stars!", "Collect all stars for Christmas", 200, TaskType.Other, AwardType.Coin, StaticLevels.levelsPerWorld[2], 0, 0, TaskOccurance.Other, "Star2", true)};
		if (File.Exists(Application.persistentDataPath + saveName))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + saveName, FileMode.Open);
			TaskData loadedData = new TaskData();
			loadedData = (TaskData)bf.Deserialize(file);
			file.Close();
			if(loadedData.tasks.Length < taskData.tasks.Length) {
				// don't read the old one save the new tasks over the old.
				Save();
			} else {
				taskData = loadedData;
			}
		}
		else
			Save();

		foreach(Task t in taskData.tasks) {
			if(PlayerPrefs.HasKey("CurlingKidsTask" + t.name + "_" + t.value1)){
				t.awarded = true;

			}
		}
		bWasLoaded = true;
		bNeedsSave = false;
	}
	public static TaskOccurance GetOccurance(int index)
	{
		return taskData.tasks[index].occurance;
	}
	public static bool IsGamePirated() {
	return	StaticManager.GetNumberOfCredits() > 200000;
	}

	public static int CalculateCoinsValue(int coin) {
		if (coin > 90 && coin < 100)
			return 100;
		if (coin > 100 && coin < 150)
			return 150;
		
		return coin;
	} 
	public static int GetNumberOfTasks()
	{
		Prepare();
		return taskData.tasks.Length;
	}

	public static int GetCredits(int index)
	{
		Prepare();
		return taskData.tasks[index].credits;
	}
	public static string GetName(int index)
	{
		Prepare();
		return taskData.tasks[index].name;
	}
	public static string GetDecription(int index)
	{
		Prepare();
		return taskData.tasks[index].description;
	}
	public static bool GetAwarded(int index)
	{
		Prepare();
		return taskData.tasks[index].IsAwarded();
	

	}


	public static int GetPercentage(int index)
	{
		Prepare();
		return taskData.tasks[index].GetTaskPercentage();
	}
	public static bool Evaluate(int index, TaskType taskType, int value1, int value2 = 0, int value3 = 0)
	{
		Prepare();
		bool awarded = taskData.tasks[index].Evaluate(taskType, value1, value2, value3);
		return awarded;
	}


	public static int EvaluateAll(TaskType taskType, int value1, int value2 = 0, int value3 = 0)
	{
		int awardNof = 0;

		Prepare();

		foreach (Task task in taskData.tasks)
		{
			bool awarded = task.Evaluate(taskType, value1, value2, value3);
			awardNof += awarded ? 1 : 0;
		}
		/*
		if(bNeedsSave) {
			Save();
			PlayerPrefs.Save();
			bNeedsSave = false;
		}*/
		return awardNof;
	}


	public static void Save()
	{
		if (taskData != null)
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + saveName, FileMode.OpenOrCreate);

			bf.Serialize(file, taskData);
			file.Close();
		} else {
			//Debug.Log("Could not save data since taskdata is null");
		}
	}
	public static void RouteTask(TaskType tasker, int val, string description, bool reward = false, bool sendRawVal = false)
	{
		Prepare();
		Dictionary<string, int> increasedForTaskType = new Dictionary<string, int>();
		int i = 0;
		foreach (Task task in taskData.tasks)
		{
			i++;
			if (i > 30)
			{
				//Debug.Log("");	
			}
			if (task.taskType == tasker)
			{

				bool increase = false;

				string key = "Tasktype";
				bool doEval = true;
				if (description != null && tasker == TaskType.Other)
				{
					if (task.description == null || description.Equals(task.triggerNameForOther) == false)
					{
						doEval = false;
					}
					else
					{
						//Debug.Log("**");
					}

				}

				if (doEval)
				{
					if (task.taskType != TaskType.Other)
					{
						key += task.taskType;
					}
					else
					{
						key += description;
					}
					if (increasedForTaskType.ContainsKey(key) == false)
					{
						increase = true;
						increasedForTaskType.Add(key, 1);
					}
					if (sendRawVal) { 
						increase = false; 
					}
					task.RunDelegate(val, description, increase, reward,sendRawVal);
				}
			}
		}
	}
	
	
	public static void RouteTask(TaskType tasker)
	{

		RouteTask(tasker, -1,null);


	}

	private static void Prepare()
	{
		if (!bWasLoaded)
		{
			Load();
		}
	}

}
