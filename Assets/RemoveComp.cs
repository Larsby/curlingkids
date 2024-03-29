using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RemoveComp : NetworkBehaviour
{
    //[SerializeField]
    public AudioListener Listener;

    private void Start()
    {
        if (!isLocalPlayer)
        {
            Listener.enabled = false;
            Debug.Log("Listener Turned Off");
        }
    }
    void OnDisable()
    {
        if (Listener != null)
        {
            Debug.Log("Listener Activated");
            Listener.enabled = true;
        }
    }
}