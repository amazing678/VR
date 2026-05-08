using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using SWS;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit; // 引入XRI

public class VRShongShu : MonoBehaviour
{
    public splineMove spline;
    public Animator animator;
    public Renderer[] meshRenderers;
    public Texture texture;
    public VRPlayer player;
    public AudioClip clip;

    Color color = Color.black;
    bool isOpen = false;
    float time;
    float timeX = 3f; // 初始延迟，保留你原来的逻辑

    // XRI 悬停状态标识
    private bool isHovering = false;

    public void Awake() 
    {
        player = VRPlayer.instance;
    }
    public void Stay()
    {
        if (isOpen == true) return;
        spline.Pause();
        animator.SetBool("IsIdle", true);
    }

    
    public void Close()
    {
        VRSongShuPlayer.instance.count++;
        
        if (VRSongShuPlayer.instance.count == 5)
        {
            //VRMain.instance.sceneName = "Hudie";
            //SceneManager.LoadScene(1);
            if (VRMain.instance != null && VRMain.instance.isAllPlayer == true) {
                VRSongShuPlayer.instance.ChangeScene();
                return;
            }
        }
        VRSongShuPlayer.instance.testCount++;
        if (VRSongShuPlayer.instance.testCount >= 3) VRSongShuPlayer.instance.testCount = 0;
        VRSongShuPlayer.instance.InstancePoint(VRSongShuPlayer.instance.testCount);

        Destroy(gameObject);

        
    }

    public void OnRayHoverEnter()
    {
        if (isOpen) return;
        isHovering = true;
    }

    // ==========================================
    // 【XRI 事件接口】当手柄射线移开时
    // ==========================================
    public void OnRayHoverExit()
    {
        if (isOpen) return;
        isHovering = false;

        // 射线移开，重置颜色和计时器
        color = Color.black;
        foreach (Renderer item in meshRenderers)
        {
            item.material.SetColor("_EmissionColor", color);
        }
        time = 0;
    }

    //void Update()
    //{
    //    if (VRPlayer.instance.RayCamera == null) return;
    //    if ((timeX -= Time.deltaTime) > 0) return;
    //    Ray ray = VRPlayer.instance.RayCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
    //    Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
    //    RaycastHit hit;
    //    if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("SongShu")))
    //    {
    //        temp = hit.collider.GetComponentInParent<VRShongShu>();
    //        if (temp.isOpen == true) return;

    //        temp.color = Color.Lerp(temp.color, Color.red, Time.deltaTime * 0.5f);
    //        foreach (Renderer item in temp.meshRenderers) item.material.SetColor("_EmissionColor", temp.color);
    //        if ((temp.time += Time.deltaTime) > 3)
    //        {
    //            //关掉自己的特效
    //            temp.color = Color.black;
    //            foreach (Renderer item in temp.meshRenderers) {
    //                item.material.SetColor("_EmissionColor", temp.color);
    //                item.material.mainTexture = temp.texture;
    //            }
    //            temp.isOpen = true;

    //            VRPlayer.instance.aSource.PlayOneShot(clip);
    //            temp.animator.SetBool("IsRun", true);
    //            temp.spline.Resume();
    //            temp.spline.ChangeSpeed (3);
    //        }
    //    }
    //    else
    //    {
    //        if (temp != null)
    //        {
    //            temp.color = Color.black;
    //            foreach (Renderer item in temp.meshRenderers) item.material.SetColor("_EmissionColor", temp.color);
    //            temp.time = 0;
    //        }
    //    }
    //}

    void Update()
    {
        // 初始延迟保护
        if ((timeX -= Time.deltaTime) > 0) return;

        // 如果松鼠已经被吓跑了，停止检测
        if (isOpen) return;

        // 只有当射线正在照射松鼠时，才执行变红和计时的逻辑
        if (isHovering)
        {
            // 颜色逐渐变红
            color = Color.Lerp(color, Color.red, Time.deltaTime * 0.5f);
            foreach (Renderer item in meshRenderers)
            {
                item.material.SetColor("_EmissionColor", color);
            }

            // 计时器累加
            time += Time.deltaTime;

            // 盯满 3 秒，触发逃跑
            if (time > 3f)
            {
                TriggerRunAway();
            }
        }
    }
    private void TriggerRunAway()
    {
        isOpen = true;     // 标记为已触发
        isHovering = false; // 停止悬停检测

        // 关掉红色特效，替换贴图
        color = Color.black;
        foreach (Renderer item in meshRenderers)
        {
            item.material.SetColor("_EmissionColor", color);
            item.material.mainTexture = texture;
        }

        // 播放音效与动画
        if (player != null && player.aSource != null)
        {
            player.aSource.PlayOneShot(clip);
        }

        animator.SetBool("IsRun", true);

        // 恢复 SWS 路径运动并加速
        spline.Resume();
        spline.ChangeSpeed(3);
    }
}
