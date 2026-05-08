using SWS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
// using HVRCORE; // 如果你不用旧的 HVR 插件了，这行可以删掉，保留也不影响

public class VRPlayer : MonoBehaviour
{
    public String sceneName;
    splineMove spline;
    float minTime;
    float secondTime;
    public Transform[] points;
    public Transform baoXiangPoint;
    public VRAnim[] agents;
    public GameObject baoxiang;
    public GameObject quad;
    bool isPause;
    public GameObject[] models;
    public GameObject medium;
    public Transform mediumPoint;
    public static VRPlayer instance;

    public GameObject image;
    public BezierPathManager bezier;

    public bool isShongShu;
    public GameObject songShu;

    public bool isBeiKe;
    public GameObject beiKe;
    public Transform[] beiKePoints;

    public bool isBallon;
    public GameObject ballon;
    public Transform ballonTarget;
    public Transform ballonTarget2;
    public Transform ballonIns;
    public GameObject ballonButton;
    public AudioSource aSource;
    public Transform testGame;

    public Transform allCanvas;

    public bool isHalfAuto;

    public CanvasGroup group;
    public Camera RayCamera;
    public GameObject RayCameraObj;
    public CanvasGroup allGroup;

    public bool isHudie;
    public bool isLoading;

    void Awake()
    {
        if (VRMain.instance == null)
        {
            SceneManager.LoadScene("Main");
            return;
        }
        Application.targetFrameRate = 60;

        instance = this;
        if (image != null)
            image.transform.localScale = Vector3.zero;

        bool isHiddle = false;
        if (VRMain.instance != null) isHiddle = VRMain.instance.isAllPlayer;
        if (allCanvas != null && allCanvas.Find("Next") != null)
            allCanvas.Find("Next").gameObject.SetActive(isHiddle);

        // 获取并重置全屏黑底 Canvas
        Transform allBackTrans = VRMain.instance.transform.Find("XR Origin (XR Rig)/Camera Offset/Main Camera/Canvas/AllBack");
        if (allBackTrans != null)
        {
            allGroup = allBackTrans.GetComponent<CanvasGroup>();
            allGroup.alpha = 1;
        }

        aSource = GetComponent<AudioSource>();

        if (VRMain.instance != null)
        {
            if (isLoading == false)
            {
                if (VRMain.instance.isClip1)
                    aSource.clip = VRMain.instance.clip1;
                else
                    aSource.clip = VRMain.instance.clip2;
                aSource.Play();
            }
        }

        IsOpenAllGroup1(true, true);
    }

    public Transform leftCam01;
    public Transform leftCam02;
    public Transform rightCam01;
    public Transform rightCam02;

    public Transform leftCam;
    public Transform rightCam;

    public void IsOpenAllGroup1(bool isAlpha, bool isOpen, Action action = null)
    {
        if (allGroup != null)
        {
            if (isAlpha == true) allGroup.alpha = isOpen ? 1 : 0;
            else allGroup.alpha = isOpen ? 0 : 1;
            if (isAlpha == true) DOTween.To(() => allGroup.alpha, x => allGroup.alpha = x, isOpen ? 0 : 1, 2);
        }

        if (aSource != null)
        {
            aSource.volume = isOpen ? 0 : 1;
            DOTween.To(() => aSource.volume, x => aSource.volume = x, isOpen ? 1 : 0, 2).OnComplete(() => {
                action?.Invoke();
            });
        }
        else
        {
            action?.Invoke();
        }
    }

    public void IsOpenAllGroup(CanvasGroup cg, bool isOpen, Action action = null)
    {
        if (aSource != null)
        {
            aSource.volume = isOpen ? 0 : 1;
            DOTween.To(() => aSource.volume, x => aSource.volume = x, isOpen ? 1 : 0, 2).OnComplete(() => {
                action?.Invoke();
            });
        }
        else
        {
            action?.Invoke();
        }
    }

    public bool isBlueLoading;
    public bool isFixed;

    void Start()
    {
        QualitySettings.shadowDistance = 80;
        spline = GetComponent<splineMove>();

        if (isBeiKe == true) InstanceBeiKe();

        if (isBallon == true)
        {
            if (ballonButton != null) ballonButton.SetActive(false);
        }

        if (isHalfAuto == true)
        {
            if (spline != null) spline.Pause();
        }

        // 把旧的屏幕中心光标彻底隐藏掉，把舞台交给 XRI 手柄射线
        if (quad != null) quad.SetActive(false);
    }

    public void ReStart()
    {
        //SceneManager.LoadScene(0);
    }

    void OnDrawGizmos()
    {
        // 保留原有的调试绘制
        if (baoXiangPoint != null)
        {
            Ray ray = new Ray(baoXiangPoint.position, Vector3.down);
            RaycastHit hit;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(baoXiangPoint.position, baoXiangPoint.position + Vector3.down * 100);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Default")))
            {
                Gizmos.DrawSphere(hit.point, 1);
            }
            Gizmos.DrawSphere(baoXiangPoint.position, 0.5f);
        }

        if (points == null || points.Length == 0) return;
        foreach (Transform item in points)
        {
            if (item == null) continue;
            Ray ray = new Ray(item.position, Vector3.down);
            RaycastHit hit;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(item.position, item.position + Vector3.down * 100);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Default")))
            {
                Gizmos.DrawSphere(hit.point, 3);
            }
            Gizmos.DrawSphere(item.position, 1);
        }
    }

    public Transform hvrCamera;
    [NonSerialized] public int i;

    void Update()
    {
        if (VRForestPlayer.instance != null)
        {
            if (VRForestPlayer.instance.isFixed == false) AnimRun();
            else Anim2Update();
        }

        if (spline == null) return;

        if (isPause == true) return;

        if (i >= 5)
        {
            // 说明已经收集完成了
            OpenMedium();
            return;
        }

        // 1分钟 为进度
        if ((minTime += Time.deltaTime) > 60)
        {
            // 停下全部
            isPause = true;
            spline.Pause();
            InsBaoXiang();
        }
    }

    public Transform cameraRig;

    // ==========================================
    // 【核心魔法：幽灵骑士跟随】
    // ==========================================
    void LateUpdate()
    {
        if (RayCamera == null)
        {
            RayCamera = Camera.main;
        }

        // 极其安全且丝滑的做法：只要玩家和游览车座位 (cameraRig) 都存在，
        // 每一帧都把跨场景的真玩家（XR Origin）死死绑在座位上！
        if (VRMain.instance != null && cameraRig != null)
        {
            VRMain.instance.transform.position = cameraRig.position;
            VRMain.instance.transform.rotation = cameraRig.rotation;
        }
    }

    public void Home()
    {
        SceneManager.LoadScene("Main_2");
    }

    //public void InsBaoXiang()
    //{
    //    // 出现宝箱
    //    Ray ray = new Ray(baoXiangPoint.position, Vector3.down);
    //    RaycastHit hit;
    //    if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("SpawnCube")))
    //    {
    //        VRBaoShang obj = Instantiate(baoxiang, hit.point, baoXiangPoint.rotation).GetComponent<VRBaoShang>();
    //        obj.model = models[i];
    //        obj.transform.localScale = Vector2.zero;
    //        obj.transform.DOScale(Vector3.one, 0.5f);
    //        i++;
    //    }
    //}

    public void InsBaoXiang()
    {
        // 出现宝箱
        Ray ray = new Ray(baoXiangPoint.position, Vector3.down);
        RaycastHit hit;

        // ==========================================
        // 【降维打击修改】：把原来的 "Default" 改成 "Ignore Raycast"
        // 这条射线现在变成了“幽灵猎手”，它无视一切实体，专门去抓 Ignore Raycast 层！
        // ==========================================
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ignore Raycast")))
        {
            VRBaoShang obj = Instantiate(baoxiang, hit.point, baoXiangPoint.rotation).GetComponent<VRBaoShang>();
            obj.model = models[i];
            obj.transform.localScale = Vector2.zero;
            obj.transform.DOScale(Vector3.one, 0.5f);
            i++;
        }
    }

    public void KeepPlaying()
    {
        isPause = false;
        minTime = 0;
        if (spline != null) spline.Resume();
    }

    public void AnimRun()
    {
        if (points == null || points.Length <= 0) return;

        for (int i = 0; i < points.Length; i++)
        {
            if (agents[i] == null) continue;
            bool isRun = agents[i].agent.velocity != Vector3.zero;
            agents[i].anim.SetBool("IsRun", isRun);
        }

        if (isPause) return;

        // 10秒 为进度
        if ((secondTime += Time.deltaTime) > 10)
        {
            secondTime = 0;
            for (int i = 0; i < points.Length; i++)
            {
                Ray ray = new Ray(points[i].position, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Default")))
                {
                    agents[i].SetPoint(hit.point);
                }
            }
        }
    }

    public void Anim2Update()
    {
        if (points == null || points.Length <= 0) return;

        for (int i = 0; i < points.Length; i++)
        {
            if (agents[i] == null) continue;
            bool isRun = agents[i].agent.velocity != Vector3.zero;
            agents[i].anim.SetBool("IsRun", isRun);
        }
    }

    public void AnimRun2()
    {
        if (points == null || points.Length <= 0) return;

        for (int i = 0; i < points.Length; i++)
        {
            if (agents[i] == null) continue;
            Ray ray = new Ray(points[i].position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Default")))
            {
                agents[i].SetPoint(hit.point);
            }
        }
    }

    public bool isOpenHudiePoint;
    public bool isHudie02;
    public bool isClosePoint;

    // 【重要清理】：由于全面转入 XRI 架构，旧的凝视射线点逻辑已彻底废弃。
    // 为了防止别的脚本报错，保留这三个空方法，但里面不再有任何阻碍射线的性能浪费。
    public void Point() { }
    public void Point1() { }
    public void Point2() { }

    public void OpenMedium()
    {
        isPause = true;
        if (spline != null) spline.Pause();

        if (image != null) image.transform.DOScale(Vector3.one * 2.4489f, 0.2f);
        DOTween.To(() => Vector3.zero, x => { }, Vector3.zero, 2).OnComplete(() => {

            if (image != null) image.transform.DOScale(Vector3.zero, 0.2f);

            VRMedium temp = Instantiate(medium, mediumPoint.position, mediumPoint.rotation).GetComponent<VRMedium>();
            Vector3 tempScale = temp.transform.localScale;
            temp.transform.localScale = Vector3.zero;
            temp.transform.DOScale(tempScale, 1).OnComplete(() => {

                //一秒钟之后进行拼合
                DOTween.To(() => Vector3.zero, x => { }, Vector3.zero, 1).OnComplete(() => {
                    DOTween.To(() => Vector3.zero, x => { }, Vector3.zero, 0.5f).OnComplete(() => {
                        if (VRForestPlayer.instance != null) VRForestPlayer.instance.isPin = true;
                    });
                });
            });
        });
    }

    int beikeI = 0;
    public void InstanceBeiKe()
    {
        if (beiKePoints == null || beiKePoints.Length == 0) return;

        VRBeiKe test = Instantiate(beiKe,
            beiKePoints[beikeI].position,
            beiKePoints[beikeI].rotation).GetComponent<VRBeiKe>();

        test.points = beiKePoints[beikeI].Find("1");

        beikeI++;
        if (beikeI >= beiKePoints.Length) beikeI = 0;
    }
}