using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
public class VRBallItem : MonoBehaviour
{
    [NonSerialized]public Transform toRegion;
    [NonSerialized]
    public TextMeshProUGUI line;
    [NonSerialized]
    public TextMeshProUGUI thisText;

    [NonSerialized]
    public Color textColor;
    // Start is called before the first frame update
    void Start()
    {
        thisText = GetComponent<TextMeshProUGUI>();
        line = transform.parent.Find(name + "_Bom").GetComponent<TextMeshProUGUI>();
        toRegion = transform.parent.Find(name + "_Pos");
        textColor = thisText.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
