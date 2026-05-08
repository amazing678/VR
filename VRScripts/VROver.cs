using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VROver : MonoBehaviour
{
    // Start is called before the first frame update
    public IEnumerator Start()
    {
        VRMain.instance.transform.position = Vector3.zero;
        VRMain.instance.transform.eulerAngles = Vector3.zero;
        yield return new WaitForSeconds(5);
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
