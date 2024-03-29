using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StopRotation : NetworkBehaviour {

    Quaternion rotation;
    public Transform root;
    public GameObject camera;
    public float cameraHeight = 10.0f;
    public float cameraBack = 10.0f;

    void Awake()
    {
        rotation = camera.transform.rotation;
    }

    void Update()
    {
        Vector3 pos = root.transform.position;
        pos.z -= cameraHeight;
        pos.y += cameraBack;
        camera.transform.position = pos;
        camera.transform.rotation = rotation;
    }
}
