using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorRandom : MonoBehaviour {
    Animator m_Animator;
	// Use this for initialization
	void Start () {
        m_Animator = gameObject.GetComponent<Animator>();
        m_Animator.speed = Random.Range(0.9f, 1.0f);
        m_Animator.playbackTime = Random.Range(0.2f, 1.0f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
