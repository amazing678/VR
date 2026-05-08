using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;
using TMPro;

public class VRBeiKePlayer : MonoBehaviour
{
    public static VRBeiKePlayer instance; // 【新增】极简单例，方便贝壳汇报

    public InputField input;
    public Slider slider;
    public CanvasGroup groupText;
    public Transform cameraRig;

    [NonSerialized] public bool isOpen = false; // 控制是否正在收取动画中，防止多拿
    private int count = 0;

    void Awake()
    {
        instance = this;
        if (VRMain.instance == null) return;
        VRMain.instance.transform.position = cameraRig.position;
        VRMain.instance.transform.eulerAngles = cameraRig.eulerAngles;
    }

    void Start()
    {
        // 兼容性获取
        if (VRMain.instance != null)
        {
            Transform t = VRMain.instance.transform.Find("NibiruXRSDK/MainCamera/Canvas1/Image");
            if (t != null) groupText = t.GetComponent<CanvasGroup>();
        }

        count = 0;

        // 【注意】这行旧的 UI 点击代码已经被废弃，UI 交互现在交给 XRI 的射线组件了
        // 请确保这个 Next 按钮所在的 Canvas 身上挂着 Tracked Device Graphic Raycaster 组件！
        /*
        new BaseButton(VRPlayer.instance.allCanvas.Find("Next")).onClick = (GameObject go) => {
            ChangeScene();
        };
        */
    }

    // ==========================================
    // UI 更新与转场逻辑（原封不动保留）
    // ==========================================
    public void OpenTip(int i, Action action = null)
    {
        float temp = 0, temp2 = 0;
        if (i == 1) { temp = 0.2f; temp2 = -12.78f; }
        if (i == 2) { temp = 0.4f; temp2 = -4.97f; }
        if (i == 3) { temp = 0.6f; temp2 = 4.35f; }
        if (i == 4) { temp = 0.8f; temp2 = 13.73f; }
        if (i >= 5) { temp = 1f; temp2 = 20.69f; }

        if (groupText != null)
        {
            groupText.transform.Find("Back").GetComponent<Image>().fillAmount = temp;
            groupText.transform.Find("Pos").transform.localPosition = new Vector3(temp2, -0.23f, 0);

            if (i < 5)
                groupText.GetComponentInChildren<TextMeshProUGUI>().text = "已收集" + i + "次, 共5次";
            else
                groupText.GetComponentInChildren<TextMeshProUGUI>().text = "已收集完成!";

            DOTween.To(() => groupText.alpha, x => { groupText.alpha = x; }, 1, 0.5f).OnComplete(() => {
                DOTween.To(() => Vector2.zero, x => { }, Vector2.zero, 2f).OnComplete(() => {
                    DOTween.To(() => groupText.alpha, x => { groupText.alpha = x; }, 0, 0.5f).OnComplete(() => {
                        action?.Invoke();
                    });
                });
            });
        }
    }

    public void ChangeScene()
    {
        if (VRMain.instance == null) return;

        VRPlayer.instance.IsOpenAllGroup1(true, false, () =>
        {
            VRMain.instance.text = "乘坐热气球";
            VRMain.instance.loadingText = "带着战利品继续游山玩水~";
            VRMain.instance.CloseScene();
            VRMain.instance.ChangeSence("Balloons", "17");
        });
    }

    public void SetInput(string text)
    {
        float value = 1.5f;
        if (float.TryParse(text, out value))
        {
            if (value > 2) value = 2;
            if (value < 0) value = 0;
        }
        else value = 1.5f;

        input.text = value.ToString();
        slider.value = value;
    }

    public void SetSlider(float value)
    {
        value = (float)Math.Round(value, 2);
        input.text = value.ToString();
    }

    // ==========================================
    // 【新增接口】贝壳动画播完后，自己调用这个方法报告
    // ==========================================
    public void OnBeiKeCollected(bool isIns)
    {
        count++;

        if (count >= 5)
        {
            OpenTip(count, () => {
                if (VRMain.instance != null && VRMain.instance.isAllPlayer == true)
                {
                    isOpen = false;
                    if (VRPlayer.instance != null) VRPlayer.instance.isClosePoint = false;
                    ChangeScene();
                }
            });
        }
        else
        {
            OpenTip(count, () => {
                isOpen = false;
                if (VRPlayer.instance != null) VRPlayer.instance.isClosePoint = false;
            });

            if (isIns && VRPlayer.instance != null)
            {
                VRPlayer.instance.InstanceBeiKe();
            }
        }
    }
}