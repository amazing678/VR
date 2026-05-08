using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRVideo : MonoBehaviour
{
    public Texture2D[] textures;
    public MeshRenderer render;
    public int i = 0;
    // Start is called before the first frame update

    
    
    // Update is called once per frame
    void Update()
    {

        render.material.mainTexture = textures[i];

        i++;
        if (i >= textures.Length)
        {
            i = 0;
            //Over
        }
    }
}
