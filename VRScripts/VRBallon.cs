using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class VRBallon:MonoBehaviour
{
    public VRGizmo gim;
    public GameObject test;

    public GameObject obj01;
    public GameObject obj02;
    public GameObject obj03;
    public GameObject obj04;
    public GameObject obj05;
    public GameObject obj06;


    public Collider[] colliders;

    VRBallon temp;
    Color color = new Color(0.1f, 0.1f, 0.1f);
    Renderer[] meshRenderers;
    float time;

    // 【新增】XRI 悬停状态
    private bool isHovering = false;

    public void OpenObj(int i)
    {
        obj01.SetActive(i == 1);
        obj02.SetActive(i == 2);
        obj03.SetActive(i == 3);
        obj04.SetActive(i == 4);
        obj05.SetActive(i == 5);
        obj06.SetActive(i == 6);
    }

    
    public void Start() {

        VRPlayer.instance.isClosePoint = true;

        colliders = GetComponentsInChildren<Collider>();
        meshRenderers = GetComponentsInChildren<Renderer>();
        foreach (Collider item in colliders) item.enabled = false;

        Vector3 temp = (gim.transform.position - VRPlayer.instance.testGame.position).normalized 
            * VRBallonPlayer.instance.nearDistanceSlider.value
            + VRPlayer.instance.testGame.position;
        //test.transform.position = temp;
        //test = new GameObject();

        //  (最大距离  - 最小距离)  /  速度;

        if (VRBallonPlayer.instance.speedSlider.value == 0) VRBallonPlayer.instance.speedSlider.value = 0.001f;


        float time = (VRBallonPlayer.instance.farDistanceSlider.value - VRBallonPlayer.instance.nearDistanceSlider.value) 
            / (VRBallonPlayer.instance.speedSlider.value);

        //print(time);

        transform.DOMove(temp, time).SetEase(Ease.OutQuint).OnComplete(() =>
        {
            Vector3 temps = Vector2.zero;
            if (VRBallonPlayer.instance.count < 1 * 1) temps = gim.targetLeftTop.position;
            else if (VRBallonPlayer.instance.count < 1 * 2) temps = gim.targetLeftBottom.position;
            else if (VRBallonPlayer.instance.count < 1 * 3) temps = gim.targetRightTop.position;
            else if (VRBallonPlayer.instance.count < 1 * 4) temps = gim.targetRightBottom.position;

            //要距离人眼10m 
            temps = (temps - VRBallonPlayer.instance.middlePoint.position).normalized * 10 + VRBallonPlayer.instance.middlePoint.position;



            DOTween.To(() => Vector3.zero, x => { }, Vector3.zero, 5).OnComplete(() => {
                foreach (Collider item in colliders) item.enabled = true;
                isOpenBallon = true;
                VRPlayer.instance.isClosePoint = false;
            });

            transform.DOMove(temps, 8).SetEase(Ease.InQuint).OnComplete(() =>{
                
            });
        });
    }

    bool isOpenBallon;
    bool isOpen;
    public void OnRayHoverEnter()
    {
        // 如果气球还没飞到位，或者已经被点破了，则不响应
        if (!isOpenBallon) return;
        isHovering = true;
    }

    // ==========================================
    // 【XRI 事件】手柄射线移开时
    // ==========================================
    public void OnRayHoverExit()
    {
        if (!isOpenBallon) return;
        isHovering = false;

        // 恢复暗色，时间清零
        color = new Color(0.1f, 0.1f, 0.1f);
        foreach (Renderer item in meshRenderers)
        {
            if (item != null && item.materials.Length >= 2)
            {
                item.materials[0].SetColor("_EmissionColor", color);
                item.materials[1].SetColor("_EmissionColor", color);
            }
        }
        time = 0;
    }

    void Update()
    {
        // 只有气球准备好，且被手柄射线指着的时候，才执行变红逻辑
        if (isOpenBallon && isHovering)
        {
            color = Color.Lerp(color, Color.red, Time.deltaTime * 0.5f);
            foreach (Renderer item in meshRenderers)
            {
                if (item != null && item.materials.Length >= 2)
                {
                    item.materials[0].SetColor("_EmissionColor", color);
                    item.materials[1].SetColor("_EmissionColor", color);
                }
            }

            time += Time.deltaTime;
            if (time > 3f)
            {
                // 防止重复触发
                isOpenBallon = false;
                isHovering = false;
                DestroyThis();
            }
        }
    }

    // ==========================================
    // 销毁与流程推进（保留原版代码）
    // ==========================================
    public void DestroyThis()
    {
        Destroy(gameObject);
        if (VRPlayer.instance != null) VRPlayer.instance.isClosePoint = true;

        // 出现文字
        VRBallonPlayer.instance.OpenTip(VRBallonPlayer.instance.count == 3 ? "请向前看!" : "请向前看!", true, () => {
            DOTween.To(() => Vector2.zero, x => { }, Vector2.zero, VRBallonPlayer.instance.count == 3 ? 4 : 2).OnComplete(() => {
                VRBallonPlayer.instance.OpenTip(VRBallonPlayer.instance.count == 3 ? "请向前看!" : "请向前看!", false, () => {
                    if (VRBallonPlayer.instance.count == 3)
                    {
                        if (VRMain.instance != null && VRMain.instance.isAllPlayer == true)
                        {
                            VRBallonPlayer.instance.ChangeScene();
                        }
                        return;
                    }
                    VRBallonPlayer.instance.count++;
                    VRBallonPlayer.instance.InstanceBallon(x => { gim.obj = x; }, gim.transform, 0, VRBallonPlayer.instance.count + 1);
                });
            });
        });
    }
}