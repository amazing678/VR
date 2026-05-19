using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EVRDwellButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("核心引用")]
    public VisualAcuityTestManager testManager;
    [Tooltip("代表的方向：0上, 1右, 2下, 3左")]
    public int directionIndex;

    [Header("悬停设置")]
    [Tooltip("需要射线停留的秒数")]
    public float dwellTime = 2.0f;
    [Tooltip("可选：用于显示读条加载的环形 Image 组件 (Image Type 设为 Filled)")]
    public Image progressRing;

    [Header("交互变色反馈")]
    [Tooltip("要变色的背景图 (如果不填，会自动获取物体身上的 Image)")]
    public Image buttonBackground;
    public Color normalColor = Color.white; // 默认颜色
    public Color hoverColor = Color.gray;   // 射线击中时的颜色

    private bool isHovering = false;
    private float timer = 0f;

    void Start()
    {
        if (buttonBackground == null)
        {
            buttonBackground = GetComponent<Image>();
        }

        ResetDwell();
    }

    void Update()
    {
        if (isHovering)
        {
            timer += Time.deltaTime;

            // 如果有视觉进度条，更新进度
            if (progressRing != null)
            {
                progressRing.fillAmount = timer / dwellTime;
            }

            // 达到 2 秒，触发提交并重置
            if (timer >= dwellTime)
            {
                testManager.SubmitAnswer(directionIndex);
                ResetDwell();
            }
        }
    }

    // 当射线打中 UI 时触发
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        timer = 0f;

        if (buttonBackground != null)
        {
            buttonBackground.color = hoverColor;
        }
    }

    // 当射线离开 UI 时触发
    public void OnPointerExit(PointerEventData eventData)
    {
        ResetDwell();
    }

    // 重置状态
    private void ResetDwell()
    {
        isHovering = false;
        timer = 0f;
        if (progressRing != null)
        {
            progressRing.fillAmount = 0f;
        }
        if (buttonBackground != null)
        {
            buttonBackground.color = normalColor;
        }
    }
}