using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook;

public class FaceBookIntegrationOnStart : MonoBehaviour {

    // Use this for initialization
    void Start () {
          Facebook.Unity.FB.Init(this.OnInitComplete);
 

    }

    private void OnInitComplete()
    {

        print("FACEBOOK STARTED AND STUFF");
        Facebook.Unity.FB.LogAppEvent(
            "ApplicationStarted",
            null,
            new Dictionary<string, object>()
            {
                        { "Started", "Game Started" }
            });


       
    }



    // Update is called once per frame
    void Update () {
        
    }
}
