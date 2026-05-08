using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // 👈 加上这句极其关键的新版命名空间！

public class VRBeiKe:MonoBehaviour
{
    public bool isIns;
    public Transform points;
    public Renderer[] meshRenderers;
    public bool isDistance;
    public float distance = 1;

    [Header("XRI 交互设置")]
    public float stareTime = 3f; // 射线停留3秒收集
    private Color originalColor = Color.black;
    private Color hoverColor = Color.red;

    private bool isHovering = false;
    private bool isTriggered = false; // 是否已经开始收集动画
    private float timer = 0f;
    public void Start() {

        if (this.name == "Chuan001")
        {
            MeshCollider mc = GetComponent<MeshCollider>();
            if (mc != null) mc.enabled = false;
            StartCoroutine(Test());
        }

        if (points != null)
        {
            transform.DOMove(points.position, 5).SetEase(Ease.InOutBack);
            transform.DORotate(points.eulerAngles, 3);
        }

    }
    public IEnumerator Test() {
        yield return new WaitForSeconds(3);
        MeshCollider mc = GetComponent<MeshCollider>();
        if (this.name == "Chuan001") GetComponent<MeshCollider>().enabled = true;
    }

    public void OnRayHoverEnter()
    {
        // 如果系统正在收取别的贝壳，或者我已经飞起来了，就不响应
        if (isTriggered || (VRBeiKePlayer.instance != null && VRBeiKePlayer.instance.isOpen)) return;
        isHovering = true;
    }

    // ==========================================
    // 【XRI 事件】手柄射线移开时
    // ==========================================
    public void OnRayHoverExit()
    {
        if (isTriggered) return;
        isHovering = false;
        timer = 0f; // 计时归零

        // 颜色熄灭
        foreach (Renderer item in meshRenderers)
        {
            if (item != null) item.material.SetColor("_EmissionColor", originalColor);
        }
    }

    void Update()
    {
        if (isTriggered) return;

        // 只有被射线指着的时候，才变红加时间
        if (isHovering)
        {
            timer += Time.deltaTime;

            // 渐变变红
            Color currentColor = Color.Lerp(originalColor, hoverColor, timer / stareTime);
            foreach (Renderer item in meshRenderers)
            {
                if (item != null) item.material.SetColor("_EmissionColor", currentColor);
            }

            // 蓄满 3 秒！起飞！
            if (timer >= stareTime)
            {
                ExecuteCollectSequence();
            }
        }
    }

    // ==========================================
    // 【核心动画】贝壳飞向相机的华丽表现
    // ==========================================
    private void ExecuteCollectSequence()
    {
        isTriggered = true;
        isHovering = false;

        // 🚨【关键修复 1】：立刻剥夺物理和 XRI 交互组件，强行让手柄射线脱离，防止底层报错！
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;
        if (GetComponent<XRSimpleInteractable>()) GetComponent<XRSimpleInteractable>().enabled = false;

        // 通知总管：我正在执行动画，你先把锁关上（isOpen = true），别让玩家连点
        if (VRBeiKePlayer.instance != null) VRBeiKePlayer.instance.isOpen = true;
        if (VRPlayer.instance != null) VRPlayer.instance.isClosePoint = true;

        // 恢复黑色
        foreach (Renderer item in meshRenderers)
        {
            if (item != null) item.material.SetColor("_EmissionColor", originalColor);
        }

        // 小船的特殊脱离逻辑
        if (this.gameObject.name == "Chuan001")
        {
            transform.parent = null;
            transform.DORotate(Vector3.zero, 2);
        }

        // 获取相机位置（兼容旧架构）
        Transform camTrans = Camera.main.transform;
        if (VRPlayer.instance != null && VRPlayer.instance.RayCamera != null)
            camTrans = VRPlayer.instance.RayCamera.transform;

        // 计算目的地
        Vector3 dir = transform.position - camTrans.position;
        Vector3 targetPos;

        if (!isDistance)
        {
            float sliderVal = VRBeiKePlayer.instance != null ? VRBeiKePlayer.instance.slider.value : 1.5f;
            targetPos = Vector3.Normalize(dir) * ((2.5f / 1.5f) * sliderVal + 0.5f) + camTrans.position;
        }
        else
        {
            targetPos = Vector3.Normalize(dir) * this.distance + camTrans.position;
        }

        if (this.gameObject.name.Contains("Chuan"))
            transform.DOScale(Vector3.one * 0.0481f, 5f).SetEase(Ease.OutQuint);

        // 华丽飞行
        transform.DOMove(targetPos, 5).SetEase(Ease.OutQuint).OnComplete(() =>
        {
            // 停顿 1 秒
            DOTween.To(() => Vector2.zero, x => { }, Vector2.zero, 1).OnComplete(() =>
            {
                // 缩小消失
                transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
                {
                    // 销毁前，报告给总管！
                    if (VRBeiKePlayer.instance != null)
                    {
                        // 🚨【关键修复 2】：动画播完后，必须把总管的锁打开！！！否则后面的贝壳永远被锁死点不了！
                        VRBeiKePlayer.instance.isOpen = false;

                        VRBeiKePlayer.instance.OnBeiKeCollected(this.isIns);
                    }

                    if (VRPlayer.instance != null) VRPlayer.instance.isClosePoint = false; // 解除凝视点锁定

                    Destroy(this.gameObject);
                });
            });
        });
    }
}
