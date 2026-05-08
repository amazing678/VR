using UnityEngine;

public class VRCenterReticleFixed : MonoBehaviour
{
    [Header("绑定 XR 相机")]
    public Transform xrCamera;

    [Header("显示参数")]
    public float distance = 1.5f;
    public float scale = 0.02f;

    void LateUpdate()
    {
        if (xrCamera == null) return;

        transform.position = xrCamera.position + xrCamera.forward * distance;
        transform.rotation = xrCamera.rotation;
        transform.localScale = Vector3.one * scale;
    }
}