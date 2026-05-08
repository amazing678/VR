using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VRLookMac : MonoBehaviour
{
    public TextMeshPro text;
    // Start is called before the first frame update
    void Start()
    {
        VRMain.instance.transform.position = Vector3.zero;
        VRMain.instance.transform.eulerAngles = Vector3.zero;
        text.text = VRMain.instance.onlyMac;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
