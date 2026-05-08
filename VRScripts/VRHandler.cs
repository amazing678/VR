using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRHandler : MonoBehaviour
{
    //public SteamVR_Behaviour_Pose pose;
    //public SteamVR_Action_Boolean teleport = SteamVR_Input.GetBooleanAction("InteractUI");
    public Camera cameraChild;
    //
    
    // Start is called before the first frame update
    void Start()
    {
        Recenter();
    }

    void Recenter() {

        //UnityEngine.XR.InputTracking.Recenter();
       // Valve.VR.OpenVR.Compositor.SetTrackingSpace(Valve.VR.ETrackingUniverseOrigin.TrackingUniverseSeated);

    }

    // Update is called once per frame
    void Update()
    {
        //if (teleport.GetStateDown(pose.inputSource) || Input.GetKeyDown(KeyCode.Space))
        //{
        //    Recenter();
        //}

    }
}
