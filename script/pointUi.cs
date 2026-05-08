using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointUi : MonoBehaviour
{
    public GameObject cam;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SetScale(Vector3 Vec)
    {
        this.transform.localScale =  Vec;

    }
    public void Setpos(Vector3 Vec) {
        this.transform.position = Vec;
    }
    // Update is called once per frame
    void Update()
    {
        this.transform.LookAt(cam.transform);
    }
}
