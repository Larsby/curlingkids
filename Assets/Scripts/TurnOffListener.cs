using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffListener : MonoBehaviour {

    public AudioListener Listener;

    void Start ()
    {
        Listener.enabled = false;
    }
}
