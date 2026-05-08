using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRGazeRaycaster : MonoBehaviour
{
    [Header("射线最大长度")]
    public float maxRayDistance = 100f;

    // 画线器组件
    private LineRenderer lineRenderer;

    void Start()
    {
        // 自动查找或添加 LineRenderer 组件
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // 【TA视觉魔法】：设置射线的宽度和颜色
        lineRenderer.startWidth = 0.01f; // 射线起点宽度 (比较细，防止挡视野)
        lineRenderer.endWidth = 0.01f;   // 射线终点宽度
        lineRenderer.positionCount = 2;  // 射线由两个点组成：起点和终点

        // 分配一个不受光照影响的默认材质，并染成纯红色
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }

    void Update()
    {
        // 如果找不到主摄像机，直接跳过防报错
        if (Camera.main == null) return;

        // 构造一条从摄像机中心朝前发射的射线
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        // 【画线第一步】：把射线的起点死死绑在摄像机的位置
        lineRenderer.SetPosition(0, ray.origin);

        if (Physics.Raycast(ray, out hit, maxRayDistance))
        {
            // 【新增的测谎仪代码】：疯狂打印射线究竟摸到了谁！
            Debug.Log("💥 射线当前打中的物体名字是：[" + hit.collider.gameObject.name + "]");

            // 【画线第二步-命中目标】：如果射线打中了任何带有碰撞体的东西，终点就在击中点！
            lineRenderer.SetPosition(1, hit.point);

            // 1. 原本检测 UI 登录大按钮的逻辑
            VRGazeButton gazeBtn = hit.collider.GetComponent<VRGazeButton>();
            if (gazeBtn != null) gazeBtn.isHovered = true;

            // 2. 检测 VRKeys 键盘按键的逻辑
            VRKeysGazeAdapter keyBtn = hit.collider.GetComponent<VRKeysGazeAdapter>();
            if (keyBtn != null) keyBtn.isHovered = true;
        }
        else
        {
            // 【画线第二步-未命中】：如果没打中任何东西，射线就一直射向远方
            lineRenderer.SetPosition(1, ray.origin + ray.direction * maxRayDistance);
        }
    }
}
