using System.Collections.Generic;
using UnityEngine;

public enum RandomRegenerationLoop { None, Loop, LoopNonRepeating };

public class RandomNonRepeating : Object {

	private List<int> pool;
	private List<int> oldPool;
	private RandomRegenerationLoop regenerate = RandomRegenerationLoop.LoopNonRepeating;
	private bool preventLoopedSame = false;
	private int preventLoopedSameValue = -1;
	public bool linearNotRandom = false;

	public RandomNonRepeating(int [] inData, RandomRegenerationLoop regenerate = RandomRegenerationLoop.LoopNonRepeating) {
		pool = new List<int> ();
		oldPool = new List<int> ();
		this.regenerate = regenerate;
		AddData (inData, true);
		preventLoopedSame = false;
	}

	// Non-inclusive max, same as Random.Range(int, int)
	public RandomNonRepeating(int min = 0, int maxNonInclusive = 0, RandomRegenerationLoop regenerate = RandomRegenerationLoop.LoopNonRepeating) {
		SetRange (min, maxNonInclusive, regenerate);
	}

	public void SetRange(int min = 0, int maxNonInclusive = 0, RandomRegenerationLoop regenerate = RandomRegenerationLoop.LoopNonRepeating) {
		pool = new List<int> ();
		oldPool = new List<int> ();
		this.regenerate = regenerate;

		if (min >= maxNonInclusive)
			return;

		for (int i = min; i < maxNonInclusive; i++)
			pool.Add (i);

		if (regenerate != RandomRegenerationLoop.None)
			DuplicatePool ();

		preventLoopedSame = false;
	}

	public int GetRandom()
	{
		if (pool.Count < 1)
			return int.MinValue;

		int loopCount = 0;
		int index, retVal, nonRandomIndex = 0;

		do {
			if (!linearNotRandom)
				index = Random.Range(0, pool.Count);
			else
				index = nonRandomIndex;
			retVal = pool[index];
			nonRandomIndex = (nonRandomIndex + 1) % pool.Count;
		} while (preventLoopedSame && pool.Count > 1 && retVal == preventLoopedSameValue && loopCount++ < 1000);
	
		pool.RemoveAt (index);

		preventLoopedSame = false;

		if (pool.Count == 0 && regenerate != RandomRegenerationLoop.None) {
			pool = new List<int> ();
			foreach (int i in oldPool)
				pool.Add (i);
			if (pool.Count > 1 && regenerate == RandomRegenerationLoop.LoopNonRepeating) {
				preventLoopedSame = true;
				preventLoopedSameValue = retVal;
			}
		}

		return retVal;
	}

	public int GetNofRemainingNumbers() {
		return pool.Count;
	}

	private void DuplicatePool() {
		oldPool = new List<int> ();
		foreach (int i in pool)
			oldPool.Add (i);
	}

	public void AddNumber(int number, int amount = 1, bool savePool = true) {
		if (amount < 1)
			return;

		for (int i = 0; i < amount; i++) {
			pool.Add (number);
			if (savePool)
				oldPool.Add (number);
		}
	}

	public void RemoveNumber(int number, bool savePool = true) {
		pool.Remove (number);
		if (savePool)
			oldPool.Remove (number);
	}

	public void AddData(int [] inData, bool savePool = true) {
		if (inData.Length  < 1)
			return;

		for (int i = 0; i < inData.Length; i++) {
			pool.Add (inData[i]);
			if (savePool)
				oldPool.Add (inData[i]);
		}
	}

}
