using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoHitSound : MonoBehaviour {

	public enum NoHitSoundType { Never, OnlyComingFromAir };

	public NoHitSoundType type = NoHitSoundType.Never;

	void Start () {}
}
