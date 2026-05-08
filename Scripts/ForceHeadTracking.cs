using UnityEngine;
using UnityEngine.XR; // Unity 最底层的 XR 硬件接口

public class ForceHeadTracking : MonoBehaviour
{
    void Update()
    {
        // 直接从硬件底层寻找“头显中心眼”的节点，无视任何上层 Input System 的配置
        InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);

        if (headDevice.isValid)
        {
            // 强行抽取硬件的真实位置，覆盖相机的本地坐标
            if (headDevice.TryGetFeatureValue(CommonUsages.centerEyePosition, out Vector3 pos))
            {
                transform.localPosition = pos;
            }

            // 强行抽取硬件的真实旋转，覆盖相机的本地旋转
            if (headDevice.TryGetFeatureValue(CommonUsages.centerEyeRotation, out Quaternion rot))
            {
                transform.localRotation = rot;
            }
        }
    }
}