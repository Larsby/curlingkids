using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TurnOffBubbleCam : NetworkBehaviour {

    private ToonDollHelper currentDoll;
    Camera playerCamera;

    private void Start()
    {
        if (isLocalPlayer)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        if(!currentDoll.WasBubbled())
        {
            playerCamera.enabled = false;
            Debug.Log("Shutdown Bubbled Characters Camera");
        }
    }
}
