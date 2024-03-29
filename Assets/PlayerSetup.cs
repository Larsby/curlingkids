using UnityEngine;
using UnityEngine.Networking;

public class PlayerSetup : NetworkBehaviour {

    [SerializeField]
    Behaviour[] componentsToDisable;

    GameObject camera;
    Camera cam;

    private ToonDollHelper currentDoll;

    Camera playerCamera;
    //Camera cam2;

    void Start()
    {
        camera = GameObject.FindGameObjectWithTag("SceneCamera");
        cam = camera.GetComponent<Camera>();
        playerCamera = Camera.main;

        if (!isLocalPlayer)
        {
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
                Debug.Log("Closed down: " + (i + 1) + "Components");
            }
            playerCamera.enabled = false;
        }
        else
        {  
            if (camera != null)
            {
                Debug.Log("SceneCamera deactivated");
                //camera.gameObject.SetActive(false);
                //AudioListener Listener = camera.GetComponent<AudioListener>();
                
                cam.enabled = false;
                //Listener.enabled = false;
            }
            if (camera == null)
            {
                Debug.Log("No Camera Found!");
            }
        }
    }

    void OnDisable()
    {
        if (camera != null)
        {
            Debug.Log("SceneCamera activated");
            //camera.gameObject.SetActive(true);
            cam.enabled = true;
        }
    }

    /*public override void OnStartLocalPlayer()
    {
        Renderer[] rens = GetComponentsInChildren<Renderer>();
        foreach (Renderer ren in rens)
        {
            ren.enabled = false;
        }

        GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
        
    }

    public override void PreStartClient()
    {
        GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
    }???*/ 
}