using UnityEngine;
using Unity.XR.CoreUtils;

public class XRSceneInitializer : MonoBehaviour
{
    [Header("森林场景中的中心点")]
    [Tooltip("请把森林场景里的 quad 拖入这里")]
    public Transform centerQuad;

    void Start()
    {
        // 1. 寻找穿越过来的大朋玩家
        XROrigin xrOrigin = FindObjectOfType<XROrigin>();

        if (xrOrigin != null)
        {
            // 2. 动态注入相机给旧代码
            if (VRPlayer.instance != null)
            {
                VRPlayer.instance.RayCamera = xrOrigin.Camera;
            }

            Camera realHeadCamera = Camera.main;
            if (realHeadCamera == null)
            {
                realHeadCamera = xrOrigin.Camera; // 兜底
            }
            if (centerQuad != null && realHeadCamera != null)
            {
                // 强制挂载到真实的头部相机上！
                centerQuad.SetParent(realHeadCamera.transform, false);

                // 重置坐标：悬浮在前方 2 米处
                centerQuad.localPosition = new Vector3(0, 0, 2f);
                // 重置旋转：对齐眼睛
                centerQuad.localRotation = Quaternion.identity;
                // 重置缩放
                centerQuad.localScale = new Vector3(0.05f, 0.05f, 0.05f);

                centerQuad.gameObject.SetActive(true);
                Debug.Log("[XR架构] 中心点已成功绑定，缩放与旧代码均已重置！");
            }
        }
    }
}