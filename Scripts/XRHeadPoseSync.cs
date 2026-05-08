using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// 手动同步HMD头部姿态到Camera Offset
/// 挂载到XR Origin物体上使用
/// </summary>
public class XRHeadPoseSync : MonoBehaviour
{
    [Header("绑定XR Origin下的Camera Offset")]
    public Transform cameraOffset; // 拖入Camera Offset子物体
    private InputDevice headDevice; // HMD设备

    void Start()
    {
        // 初始化获取HMD设备
        List<InputDevice> headDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.Head, headDevices);
        if (headDevices.Count > 0)
        {
            headDevice = headDevices[0];
        }
        else
        {
            Debug.LogError("未检测到HMD设备！请检查OpenXR配置或连接设备");
        }
    }

    void Update()
    {
        if (!headDevice.isValid) return;

        // 同步HMD位置（LocalPosition，因为XR Origin是根节点）
        if (headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 headPos))
        {
            cameraOffset.localPosition = headPos;
        }

        // 同步HMD旋转（核心：解决视角不跟随问题）
        if (headDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion headRot))
        {
            cameraOffset.localRotation = headRot;
        }
    }
}