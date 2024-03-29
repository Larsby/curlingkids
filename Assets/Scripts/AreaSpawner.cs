using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawn objects randomly from pool of objects, with start position random within the object's box collider

public class AreaSpawner : MonoBehaviour {

	Bounds boxBounds;

	public GameObject[] spawnPrefabs;
	public float minInitialSpawnWait = 0f;
	public float maxInitialSpawnWait = 0f;
	public float minSpawnWait = 0.5f;
	public float maxSpawnWait = 1.0f;

	public float spawnEndTimer = -1;
	public int spawnEndNofSpawns = -1;
	public Goal[] spawnEndGoals = null;

	private bool spawnStopped = false;
	private float endTime;

	private SoundEmitter soundEmitter = null;

	public bool forceInitialRotation = false;
	public Vector3 forcedInitialRotation = Vector3.zero;


	void Start () {
		if (spawnPrefabs == null || spawnPrefabs.Length < 1)
			return;

		BoxCollider coll = GetComponent<BoxCollider> ();
		if (coll != null)
			boxBounds = coll.bounds;
		else {
			boxBounds.extents = Vector3.zero;
		}

		endTime = Time.time + spawnEndTimer;

		soundEmitter = GetComponent<SoundEmitter> ();

		Invoke ("Spawn", Random.Range (minInitialSpawnWait, maxInitialSpawnWait));
	}
	
	void Spawn() {

		if (spawnEndTimer >= 0 && Time.time >= endTime)
			spawnStopped = true;

		if (spawnEndGoals != null && spawnEndGoals.Length > 0) {
			bool allClear = true;
			foreach (Goal g in spawnEndGoals)
				if (g.GetNofTimesHit () < g.GetNofRequiredHits ())
					allClear = false;

			if (allClear)
				spawnStopped = true;
		}

		if (!spawnStopped) {
			GameObject newSpawn = Instantiate(spawnPrefabs[Random.Range(0,spawnPrefabs.Length)], transform);
			newSpawn.transform.localPosition = new Vector3 (-boxBounds.extents.x + Random.Range(0, boxBounds.extents.x * 2), -boxBounds.extents.y + Random.Range(0, boxBounds.extents.y * 2), -boxBounds.extents.z + Random.Range(0, boxBounds.extents.z * 2));
			if (forceInitialRotation) newSpawn.transform.localRotation = Quaternion.Euler(forcedInitialRotation);

			if (soundEmitter != null)
				soundEmitter.PlaySound ();

			Invoke ("Spawn", Random.Range (minSpawnWait, maxSpawnWait));

			spawnEndNofSpawns--; if (spawnEndNofSpawns == 0)
				spawnStopped = true;
		}
	}
}
