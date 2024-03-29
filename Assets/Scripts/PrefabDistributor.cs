using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabDistributor : MonoBehaviour {

	public GameObject[] prefabs;

	public Vector3 deltaChange = Vector3.zero;

	public int columns = 1;
	public int rows = 1;
	public int stacks = 1;

	public int objCountX = 1;
	public int objCountY = 0;
	public int objCountZ = 0;

	void Start () {
		if (prefabs == null || prefabs.Length == 0 || columns < 1 || rows < 1|| stacks < 1)
			return;

		Vector3 currPos = Vector3.zero; //transform.position;
		int count = 0;

		for (int s = 0; s < stacks; s++) {

			currPos = GameUtil.SetZ(currPos, 0);
			for (int r = 0; r < rows; r++)
			{
				currPos = GameUtil.SetX(currPos, 0);
				for (int c = 0; c < columns; c++)
				{
					GameObject g = Instantiate(prefabs[count % prefabs.Length], transform);
					g.transform.localPosition = currPos;
					currPos = GameUtil.AddX(currPos, deltaChange.x);
					count += objCountX;
				}

				currPos = GameUtil.AddZ(currPos, deltaChange.z);
				count += objCountZ;
			}

			currPos = GameUtil.AddY(currPos, deltaChange.y);
			count += objCountY;
		}

	}
	
}
