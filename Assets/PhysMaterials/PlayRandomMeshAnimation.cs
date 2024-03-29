using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSG.MeshAnimator;
public class PlayRandomMeshAnimation : MonoBehaviour {
	public string[] animatonsNames;
	public bool addToChildren = false;
	private int currentIndex = 0;
	private MeshAnimator anim;
	// Use this for initialization
	void Start () {
		Invoke("AddToChildren", 1.0f);
	}
	void AddToChildren()
	{
		foreach (Transform t in transform)
		{
			GameObject obj = t.gameObject;
			PlayRandomMeshAnimation child = obj.AddComponent<PlayRandomMeshAnimation>();
			child.addToChildren = false;
			child.animatonsNames = animatonsNames;
			child.DoAnimation(animatonsNames);
		}
	}
	void SetClip(int index) {
		anim.defaultAnimation = anim.animations[currentIndex];
		anim.playAutomatically = true;
		anim.enabled = false;
		anim.enabled = true;

	}

	public IEnumerator PlayRandomDelayed(float delay, string[] anims ) {
		yield return new WaitForSeconds(delay);
		anim = GetComponent<MeshAnimator>();
			
		currentIndex = UnityEngine.Random.Range(0, anims.Length);
		SetClip(currentIndex);	

		anim.OnAnimationFinished += (string name) =>
		{
		//	Debug.Log(name + " finished.");
			currentIndex++;
			if(currentIndex>=this.animatonsNames.Length) {
				currentIndex = 0;
			}
			SetClip(currentIndex);
		};
//		anim.PlayQueued();
	
	}
	public void DoAnimation(string [] animations) {
		StartCoroutine(PlayRandomDelayed((float)Random.Range(1,7),animations));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
