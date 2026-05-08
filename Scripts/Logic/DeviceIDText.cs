using UnityEngine;
using UnityEngine.UI;

public class ShowDeviceID : MonoBehaviour
{
    void Start()
    {
        GetComponent<Text>().text = "±¾»úID: " + SystemInfo.deviceUniqueIdentifier;
    }
}