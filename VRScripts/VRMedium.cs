using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using DG.Tweening;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VRMedium : MonoBehaviour
{
    [Header("第 1 组")]
    public GameObject part1;   // 散落的零件 1
    public GameObject socket1; // 透明的插槽 1

    [Header("第 2 组")]
    public GameObject part2;
    public GameObject socket2;

    [Header("第 3 组")]
    public GameObject part3;
    public GameObject socket3;

    [Header("第 4 组")]
    public GameObject part4;
    public GameObject socket4;

    [Header("第 5 组")]
    public GameObject part5;
    public GameObject socket5;

    public Color modelColor = new Color(0.3962f, 0.1925f, 0.1925f);

    public static VRMedium instance;

    // ==========================================
    // --- 核心修改：牵引光束控制系统 ---
    // ==========================================
    private Transform rayOrigin;             // 射线的发射点 (右手柄)
    private Transform currentFollowingPart;  // 当前正被射线吸附跟着走的零件
    private Transform currentTargetSocket;   // 这个零件对应的半透明目标
    private float followDistance;            // 悬浮在手柄前方的距离

    public bool isRotate;
    int i = 0; // 拼装成功的数量

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        // 1. 如果玩家还在加载，不执行
        if (VRPlayer.instance != null && VRPlayer.instance.isBlueLoading) return;

        // 2. 自动寻找右手柄的射线发射器
        if (rayOrigin == null)
        {
            XRRayInteractor[] interactors = FindObjectsOfType<XRRayInteractor>();
            foreach (var interactor in interactors)
            {
                if (interactor.name.Contains("Right"))
                {
                    rayOrigin = interactor.transform;
                    break;
                }
            }
            if (rayOrigin == null && interactors.Length > 0) rayOrigin = interactors[0].transform;
            if (rayOrigin == null) return;
        }

        // 3. 状态机
        if (currentFollowingPart == null)
        {
            SearchForPart();
        }
        else
        {
            FollowRayAndCheckSnap();
        }
    }

    void SearchForPart()
    {
        // 从手柄向前发射物理射线，只检测 "TT" 层
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, 100f, 1 << LayerMask.NameToLayer("TT")))
        {
            Transform hitObj = hit.collider.transform;

            // 【黑魔法防御】：使用 IsChildOf 防止碰撞体挂在子物体上导致不识别！
            if (part1 != null && (hitObj == part1.transform || hitObj.IsChildOf(part1.transform))) StartFollowing(part1.transform, socket1.transform, hit.distance);
            else if (part2 != null && (hitObj == part2.transform || hitObj.IsChildOf(part2.transform))) StartFollowing(part2.transform, socket2.transform, hit.distance);
            else if (part3 != null && (hitObj == part3.transform || hitObj.IsChildOf(part3.transform))) StartFollowing(part3.transform, socket3.transform, hit.distance);
            else if (part4 != null && (hitObj == part4.transform || hitObj.IsChildOf(part4.transform))) StartFollowing(part4.transform, socket4.transform, hit.distance);
            else if (part5 != null && (hitObj == part5.transform || hitObj.IsChildOf(part5.transform))) StartFollowing(part5.transform, socket5.transform, hit.distance);
        }
    }

    void StartFollowing(Transform part, Transform socket, float hitDistance)
    {
        currentFollowingPart = part;
        currentTargetSocket = socket;

        // 保持在 1.5 米到 3 米的舒适距离
        followDistance = Mathf.Clamp(hitDistance, 1.5f, 3.0f);

        // 瞬间把自己变成 "Ignore Raycast" 层，防止挡住自己的射线
        part.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        // 触发高亮颜色
        if (part.GetComponent<MeshRenderer>())
            part.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", modelColor);
    }

    void FollowRayAndCheckSnap()
    {
        // 零件平滑地飞到射线前方指定距离
        Vector3 targetPos = rayOrigin.position + rayOrigin.forward * followDistance;
        currentFollowingPart.position = Vector3.Lerp(currentFollowingPart.position, targetPos, Time.deltaTime * 15f);

        // 如果零件距离目标插槽小于 0.5 米，触发自动吸附！
        if (Vector3.Distance(currentFollowingPart.position, currentTargetSocket.position) < 0.5f)
        {
            SnapAndFinish();
        }
    }

    void SnapAndFinish()
    {
        Transform part = currentFollowingPart;
        Transform socket = currentTargetSocket;

        currentFollowingPart = null;
        currentTargetSocket = null;

        // 完美对齐
        part.position = socket.position;
        part.rotation = socket.rotation;

        // 恢复渲染并隐藏虚影
        if (part.GetComponent<MeshRenderer>() && socket.GetComponent<MeshRenderer>())
        {
            part.GetComponent<MeshRenderer>().material = socket.GetComponent<MeshRenderer>().material;
        }
        socket.gameObject.SetActive(false);

        // 累计过关
        i++;
        if (i >= 5)
        {
            isRotate = true;
            DOTween.To(() => Vector3.zero, x => { }, Vector3.zero, 1).OnComplete(() => {
                DOTween.To(() => Vector3.zero, x => { }, Vector3.zero, 0.5f).OnComplete(() => {
                    if (VRForestPlayer.instance != null) VRForestPlayer.instance.isPin = true;
                });
            });
        }
    }
}