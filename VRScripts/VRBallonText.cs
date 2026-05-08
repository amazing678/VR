using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VRBallonText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Color color;
    public Color toColor;
    public bool isEnter;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        color = text.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
