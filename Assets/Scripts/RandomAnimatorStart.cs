using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimatorStart : MonoBehaviour {

	void Start () {
		Animator animator = GetComponent<Animator> ();
		animator.Play (0, -1, Random.value);
	}
	
	void Update () {
	}
}
