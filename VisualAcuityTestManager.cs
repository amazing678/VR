using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class VisualAcuityTestManager : MonoBehaviour
{
    public enum EDirection
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,
        None = -1
    }

    [Header("VR 与核心设置")]
    [Tooltip("拖真正的 VR Camera / Main Camera / CenterEye Camera，不要拖 XR Origin 根物体")]
    public Transform vrCamera;

    [Tooltip("E 字视标距离 VR Camera 的目标距离，单位：米。文档要求为 1m")]
    public float testDistance = 1.0f;

    [Header("空间定位")]
    [Tooltip("拖入大写 IMG，也就是 World Space Canvas。不要拖最顶层小写 img")]
    public Transform eCanvasPlane;

    [Tooltip("每次生成新的 E 字时，把大写 IMG 重新定位到相机前方 testDistance 米")]
    public bool forceCanvasToTestDistance = true;

    [Tooltip("如果其他 UI 脚本每帧会重算位置，可打开此项，在 LateUpdate 中最后强制定位")]
    public bool forcePositionInLateUpdate = false;

    [Tooltip("如果 E 字显示背面或看不到，可勾选这个反转朝向")]
    public bool flipCanvasFacing = false;

    [Header("UI 引用")]
    [Tooltip("拖最底层小写 Image，也就是 Source Image = E字_1 的那个")]
    public Image eTargetImage;

    public Text resultText;
    public GameObject[] directionButtons;

    [Header("答题设置")]
    public float maxAnswerTime = 5.0f;

    [HideInInspector]
    public bool isTimerPaused = false;

    [Header("Debug")]
    public bool enableDebugLog = true;
    public float debugInterval = 1.0f;

    [Serializable]
    public struct AcuityLevel
    {
        public float acuity;       // 小数视力
        public int targetCount;    // 当前视力等级需要测试的视标数量
        public float heightMm;     // E 字总高度，单位 mm
    }

    private readonly List<AcuityLevel> levelData = new List<AcuityLevel>
    {
        new AcuityLevel { acuity = 0.1f,  targetCount = 1, heightMm = 14.54f },
        new AcuityLevel { acuity = 0.12f, targetCount = 2, heightMm = 12.12f },
        new AcuityLevel { acuity = 0.15f, targetCount = 2, heightMm = 9.70f },
        new AcuityLevel { acuity = 0.2f,  targetCount = 3, heightMm = 7.27f },
        new AcuityLevel { acuity = 0.25f, targetCount = 3, heightMm = 5.82f },
        new AcuityLevel { acuity = 0.3f,  targetCount = 4, heightMm = 4.85f },
        new AcuityLevel { acuity = 0.4f,  targetCount = 4, heightMm = 3.64f },
        new AcuityLevel { acuity = 0.5f,  targetCount = 5, heightMm = 2.91f },
        new AcuityLevel { acuity = 0.6f,  targetCount = 5, heightMm = 2.42f },
        new AcuityLevel { acuity = 0.8f,  targetCount = 7, heightMm = 1.82f },
        new AcuityLevel { acuity = 1.0f,  targetCount = 8, heightMm = 1.45f },
        new AcuityLevel { acuity = 1.2f,  targetCount = 8, heightMm = 1.21f },
        new AcuityLevel { acuity = 1.5f,  targetCount = 8, heightMm = 0.97f },
        new AcuityLevel { acuity = 2.0f,  targetCount = 8, heightMm = 0.73f }
    };

    private int currentLevelIndex = 0;
    private int attemptsInLevel = 0;
    private int correctInLevel = 0;
    private EDirection currentCorrectDirection = EDirection.None;

    private bool isTesting = false;
    private float answerTimer = 0f;
    private float debugLogTimer = 0f;

    void Start()
    {
        if (eTargetImage == null)
        {
            Debug.LogError("[视力测试] eTargetImage 未绑定，请拖入最底层小写 Image");
            enabled = false;
            return;
        }

        if (vrCamera == null)
        {
            Debug.LogError("[视力测试] vrCamera 未绑定，请拖入真正的 VR Camera / Main Camera / CenterEye Camera");
            enabled = false;
            return;
        }

        // 如果没有手动拖大写 IMG，就默认使用 E 字所在的 Canvas
        if (eCanvasPlane == null)
        {
            eCanvasPlane = eTargetImage.canvas.transform;
            Debug.LogWarning("[视力测试] eCanvasPlane 未绑定，已默认使用 eTargetImage.canvas.transform。建议手动拖入大写 IMG。");
        }

        RectTransform rt = eTargetImage.rectTransform;

        // 将锚点、中心点统一固定到中心，便于精确控制
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;

        eTargetImage.gameObject.SetActive(false);

        StartCoroutine(WaitAndStartTest());
    }

    private System.Collections.IEnumerator WaitAndStartTest()
    {
        for (int i = 3; i > 0; i--)
        {
            if (resultText != null)
            {
                resultText.text = $"<color=yellow>请正视前方...</color>\n倒计时: {i} 秒";
            }

            yield return new WaitForSeconds(1f);
        }

        eTargetImage.gameObject.SetActive(true);
        StartLevel();
    }

    void Update()
    {
        if (!isTesting || isTimerPaused)
        {
            return;
        }

        answerTimer += Time.deltaTime;
        debugLogTimer += Time.deltaTime;

        if (resultText != null)
        {
            int remainTime = Mathf.Max(0, Mathf.CeilToInt(maxAnswerTime - answerTimer));
            resultText.text = $"当前测试视力: {levelData[currentLevelIndex].acuity:F2}\n倒计时: {remainTime}s";
        }

        if (enableDebugLog && debugLogTimer >= debugInterval)
        {
            PrintDistanceAndSizeDebug();
            debugLogTimer = 0f;
        }

        if (answerTimer >= maxAnswerTime)
        {
            Debug.Log("[视力测试] 5秒超时，自动判定为错误");
            SubmitAnswer(-1);
        }
    }

    void LateUpdate()
    {
        // 如果你的其他 UI 脚本会在 Update/LateUpdate 里重新移动 img 或 IMG，
        // 可以打开 forcePositionInLateUpdate，让本脚本最后再把大写 IMG 拉回 1m。
        if (isTesting && forceCanvasToTestDistance && forcePositionInLateUpdate)
        {
            PlaceECanvasAtTestDistance();
        }
    }

    private void StartLevel()
    {
        attemptsInLevel = 0;
        correctInLevel = 0;
        GenerateNewE();
        isTesting = true;
    }

    private void GenerateNewE()
    {
        answerTimer = 0f;

        if (forceCanvasToTestDistance)
        {
            PlaceECanvasAtTestDistance();
        }

        AcuityLevel currentData = levelData[currentLevelIndex];
        float targetHeightMm = currentData.heightMm;

        ApplyETargetPhysicalSize(targetHeightMm);

        RectTransform rt = eTargetImage.rectTransform;

        // 保证 E 字在父节点中心
        rt.anchoredPosition = Vector2.zero;
        rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0f);
        rt.localScale = Vector3.one;

        // 随机方向
        currentCorrectDirection = (EDirection)UnityEngine.Random.Range(0, 4);

        float zRotation = 0f;

        switch (currentCorrectDirection)
        {
            case EDirection.Up:
                zRotation = 90f;
                break;
            case EDirection.Right:
                zRotation = 0f;
                break;
            case EDirection.Down:
                zRotation = -90f;
                break;
            case EDirection.Left:
                zRotation = 180f;
                break;
        }

        rt.localRotation = Quaternion.Euler(0, 0, zRotation);

        if (enableDebugLog)
        {
            PrintDistanceAndSizeDebug();
        }
    }

    /// <summary>
    /// 不改最顶层小写 img，只移动大写 IMG，也就是 eCanvasPlane。
    /// 目标：让 E 字 Rect 的真实中心位于 VR Camera 正前方 testDistance 米。
    /// </summary>
    private void PlaceECanvasAtTestDistance()
    {
        if (vrCamera == null || eTargetImage == null)
        {
            return;
        }

        Transform plane = eCanvasPlane != null ? eCanvasPlane : eTargetImage.canvas.transform;

        // 目标中心点：VR Camera 正前方 testDistance 米
        Vector3 cameraForward = vrCamera.forward.normalized;
        Vector3 targetCenter = vrCamera.position + cameraForward * testDistance;

        // 先把大写 IMG 放到目标点附近
        plane.position = targetCenter;

        // 朝向处理：默认沿用你原来代码的朝向逻辑
        if (!flipCanvasFacing)
        {
            plane.rotation = Quaternion.LookRotation(plane.position - vrCamera.position, Vector3.up);
        }
        else
        {
            plane.rotation = Quaternion.LookRotation(vrCamera.position - plane.position, Vector3.up);
        }

        // 用 E 字真实 Rect 中心做二次校正
        Vector3 eCenter = GetRectCenterWorld(eTargetImage.rectTransform);
        Vector3 offset = targetCenter - eCenter;
        plane.position += offset;
    }

    /// <summary>
    /// 根据目标毫米高度，自动换算为 RectTransform 的 sizeDelta。
    /// 这个写法不再强依赖 Canvas Scale 必须为 0.001。
    /// 如果 IMG Scale = 0.001，则 14.54mm 会换算成 sizeDelta ≈ 14.54。
    /// 如果 IMG Scale = 1，则 14.54mm 会换算成 sizeDelta ≈ 0.01454。
    /// </summary>
    private void ApplyETargetPhysicalSize(float targetHeightMm)
    {
        RectTransform rt = eTargetImage.rectTransform;

        float targetHeightMeter = targetHeightMm / 1000f;

        Transform parent = rt.parent;

        if (parent == null)
        {
            Debug.LogError("[E字尺寸] eTargetImage 没有父节点，无法根据父级缩放换算物理尺寸");
            return;
        }

        Vector3 parentWorldScale = parent.lossyScale;

        if (Mathf.Approximately(parentWorldScale.x, 0f) || Mathf.Approximately(parentWorldScale.y, 0f))
        {
            Debug.LogError($"[E字尺寸] 父级世界缩放异常：{parentWorldScale}");
            return;
        }

        // 分别按 X/Y 缩放反推 sizeDelta，避免父级非等比缩放导致 E 字变形
        float targetLocalWidth = targetHeightMeter / Mathf.Abs(parentWorldScale.x);
        float targetLocalHeight = targetHeightMeter / Mathf.Abs(parentWorldScale.y);

        rt.localScale = Vector3.one;
        rt.sizeDelta = new Vector2(targetLocalWidth, targetLocalHeight);
    }

    private Vector3 GetRectCenterWorld(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        return (corners[0] + corners[1] + corners[2] + corners[3]) / 4f;
    }

    private float GetRectWorldHeightMm(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        // corners[0] 左下，corners[1] 左上
        return Vector3.Distance(corners[0], corners[1]) * 1000f;
    }

    private float GetRectWorldWidthMm(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        // corners[0] 左下，corners[3] 右下
        return Vector3.Distance(corners[0], corners[3]) * 1000f;
    }

    private void PrintDistanceAndSizeDebug()
    {
        if (vrCamera == null || eTargetImage == null)
        {
            return;
        }

        RectTransform rt = eTargetImage.rectTransform;

        Vector3 eCenter = GetRectCenterWorld(rt);
        float realDistance = Vector3.Distance(vrCamera.position, eCenter);

        float actualHeightMm = GetRectWorldHeightMm(rt);
        float actualWidthMm = GetRectWorldWidthMm(rt);

        AcuityLevel currentData = levelData[Mathf.Clamp(currentLevelIndex, 0, levelData.Count - 1)];

        Debug.Log(
            $"<color=cyan>[E字校验]</color> " +
            $"视力:{currentData.acuity:F2} | " +
            $"目标距离:{testDistance:F3}m | 实际距离:{realDistance:F4}m | " +
            $"目标高度:{currentData.heightMm:F2}mm | " +
            $"实际高度:{actualHeightMm:F2}mm | 实际宽度:{actualWidthMm:F2}mm | " +
            $"Camera:{vrCamera.name} Pos={vrCamera.position} | " +
            $"Plane:{(eCanvasPlane != null ? eCanvasPlane.name : "null")} Pos={(eCanvasPlane != null ? eCanvasPlane.position.ToString() : "null")} | " +
            $"E中心={eCenter}"
        );
    }

    public void SubmitAnswer(int directionIndex)
    {
        if (!isTesting)
        {
            return;
        }

        EDirection guessedDir = (EDirection)directionIndex;

        if (guessedDir == currentCorrectDirection)
        {
            correctInLevel++;
        }

        attemptsInLevel++;
        EvaluateLogic();
    }

    private void EvaluateLogic()
    {
        AcuityLevel currentData = levelData[currentLevelIndex];

        int requiredPassCount = Mathf.FloorToInt(currentData.targetCount / 2f) + 1;
        int remainingAttempts = currentData.targetCount - attemptsInLevel;

        // 已达到通过标准，进入下一等级
        if (correctInLevel >= requiredPassCount)
        {
            currentLevelIndex++;

            if (currentLevelIndex >= levelData.Count)
            {
                EndTest(levelData[levelData.Count - 1].acuity);
            }
            else
            {
                StartLevel();
            }

            return;
        }

        // 剩余机会全部答对也无法通过，结束
        if (correctInLevel + remainingAttempts < requiredPassCount)
        {
            float finalAcuity = currentLevelIndex > 0 ? levelData[currentLevelIndex - 1].acuity : 0f;
            EndTest(finalAcuity);
            return;
        }

        // 当前等级还没测完，继续生成新的 E 字
        GenerateNewE();
    }

    private void EndTest(float finalAcuity)
    {
        isTesting = false;

        if (eTargetImage != null)
        {
            eTargetImage.gameObject.SetActive(false);
        }

        if (directionButtons != null)
        {
            foreach (GameObject btn in directionButtons)
            {
                if (btn != null)
                {
                    btn.SetActive(false);
                }
            }
        }

        if (resultText != null)
        {
            resultText.text =
                $"测试结束！\n" +
                $"您的最终视力结果为: <color=#00FF00>{finalAcuity:F2}</color>\n" +
                $"<size=20>本测试仅为视力筛查参考，不作为临床诊断依据。</size>";
        }

        Debug.Log($"[视力测试结束] 最终视力 = {finalAcuity:F2}");
    }
}