using UnityEngine;
using TMPro;
using System.Text;

public class CameraTransformDebug : MonoBehaviour
{
    [Header("UI 文本引用")]
    [Tooltip("将场景中的 TextMeshProUGUI 组件拖到这里")]
    public TextMeshProUGUI debugText;

    private Transform mainCamTransform;

    [Header("日志设置")]
    [Tooltip("欧拉角变化超过这个阈值(度)才打印，避免刷屏")]
    public float rotationLogThreshold = 0.5f;

    [Tooltip("位置变化超过这个阈值(米)才打印，避免刷屏")]
    public float positionLogThreshold = 0.001f;

    private Quaternion lastLoggedRotation;
    private Vector3 lastLoggedPosition;
    private bool hasInitLogState = false;

    // 记录三个阶段最近一次的读数
    private Vector3 updatePos, latePos, beforeRenderPos;
    private Vector3 updateEuler, lateEuler, beforeRenderEuler;
    private Vector3 updateForward, lateForward, beforeRenderForward;

    private string lastPhaseDetected = "尚未检测到变化";

    void Start()
    {
        if (Camera.main != null)
        {
            mainCamTransform = Camera.main.transform;
            Debug.Log($"[CameraTransformDebug] 找到 Main Camera: {mainCamTransform.name}");
        }
        else
        {
            Debug.LogError("[CameraTransformDebug] 找不到主相机！请确认头显相机的 Tag 是否为 MainCamera");
            return;
        }

        Application.onBeforeRender += HandleBeforeRender;

        lastLoggedRotation = mainCamTransform.rotation;
        lastLoggedPosition = mainCamTransform.position;
        hasInitLogState = true;
    }

    void OnDestroy()
    {
        Application.onBeforeRender -= HandleBeforeRender;
    }

    void Update()
    {
        if (mainCamTransform == null) return;

        updatePos = mainCamTransform.position;
        updateEuler = mainCamTransform.eulerAngles;
        updateForward = mainCamTransform.forward;

        CheckAndLog("Update", updatePos, mainCamTransform.rotation);
    }

    void LateUpdate()
    {
        if (mainCamTransform == null || debugText == null) return;

        latePos = mainCamTransform.position;
        lateEuler = mainCamTransform.eulerAngles;
        lateForward = mainCamTransform.forward;

        CheckAndLog("LateUpdate", latePos, mainCamTransform.rotation);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<color=yellow>Main Camera 实时数据</color>");
        sb.AppendLine();

        sb.AppendLine($"<b>检测到最近一次明显变化的阶段:</b> {lastPhaseDetected}");
        sb.AppendLine();

        sb.AppendLine("<b>Update:</b>");
        sb.AppendLine($"Pos  X:{updatePos.x:F3} Y:{updatePos.y:F3} Z:{updatePos.z:F3}");
        sb.AppendLine($"Rot  X:{updateEuler.x:F2} Y:{updateEuler.y:F2} Z:{updateEuler.z:F2}");
        sb.AppendLine($"Fwd  X:{updateForward.x:F3} Y:{updateForward.y:F3} Z:{updateForward.z:F3}");
        sb.AppendLine();

        sb.AppendLine("<b>LateUpdate:</b>");
        sb.AppendLine($"Pos  X:{latePos.x:F3} Y:{latePos.y:F3} Z:{latePos.z:F3}");
        sb.AppendLine($"Rot  X:{lateEuler.x:F2} Y:{lateEuler.y:F2} Z:{lateEuler.z:F2}");
        sb.AppendLine($"Fwd  X:{lateForward.x:F3} Y:{lateForward.y:F3} Z:{lateForward.z:F3}");
        sb.AppendLine();

        sb.AppendLine("<b>OnBeforeRender:</b>");
        sb.AppendLine($"Pos  X:{beforeRenderPos.x:F3} Y:{beforeRenderPos.y:F3} Z:{beforeRenderPos.z:F3}");
        sb.AppendLine($"Rot  X:{beforeRenderEuler.x:F2} Y:{beforeRenderEuler.y:F2} Z:{beforeRenderEuler.z:F2}");
        sb.AppendLine($"Fwd  X:{beforeRenderForward.x:F3} Y:{beforeRenderForward.y:F3} Z:{beforeRenderForward.z:F3}");

        debugText.text = sb.ToString();
    }

    private void HandleBeforeRender()
    {
        if (mainCamTransform == null) return;

        beforeRenderPos = mainCamTransform.position;
        beforeRenderEuler = mainCamTransform.eulerAngles;
        beforeRenderForward = mainCamTransform.forward;

        CheckAndLog("OnBeforeRender", beforeRenderPos, mainCamTransform.rotation);
    }

    private void CheckAndLog(string phase, Vector3 currentPos, Quaternion currentRot)
    {
        if (!hasInitLogState)
        {
            lastLoggedPosition = currentPos;
            lastLoggedRotation = currentRot;
            hasInitLogState = true;
            return;
        }

        float posDelta = Vector3.Distance(lastLoggedPosition, currentPos);
        float rotDelta = Quaternion.Angle(lastLoggedRotation, currentRot);

        if (posDelta > positionLogThreshold || rotDelta > rotationLogThreshold)
        {
            lastPhaseDetected = phase;

            Vector3 euler = currentRot.eulerAngles;
            Vector3 fwd = mainCamTransform.forward;

            Debug.Log(
                $"[CameraTransformDebug][{phase}] 检测到相机变化 | " +
                $"Pos=({currentPos.x:F3},{currentPos.y:F3},{currentPos.z:F3}) | " +
                $"RotEuler=({euler.x:F2},{euler.y:F2},{euler.z:F2}) | " +
                $"Forward=({fwd.x:F3},{fwd.y:F3},{fwd.z:F3}) | " +
                $"ΔPos={posDelta:F4} | ΔRot={rotDelta:F3}"
            );

            lastLoggedPosition = currentPos;
            lastLoggedRotation = currentRot;
        }
    }
}