using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Colorful;
public class VRBaoShang : MonoBehaviour
{
    public Renderer[] meshRenderers;
    [NonSerialized]public Color color = Color.black;
    [NonSerialized]public float time;
    [NonSerialized]public bool isOpen;
    [NonSerialized]public Animator animator;
    [NonSerialized]public GameObject model;
    public Transform middle;
    public AudioClip clip;

    private bool isHovering = false;
    private float hoverTimer = 0f;
    public float openTime = 2f; // 停留几秒打开
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }
    public void OnRayHoverEnter()
    {
        if (isOpen) return;
        isHovering = true;
    }

    // ==========================================
    // 【XRI 核心接口 2】当手柄射线离开宝箱时，自动调用
    // ==========================================
    public void OnRayHoverExit()
    {
        if (isOpen) return;
        isHovering = false;

        // 射线移开，颜色和计时器全部重置归零
        hoverTimer = 0f;
        color = Color.black;
        foreach (Renderer item in meshRenderers)
        {
            item.material.SetColor("_EmissionColor", color);
        }
    }

    //VRBaoShang temp;
    //public AudioClip clip;

    //float timeX = 2;
    // Update is called once per frame
    void Update()
    {
        // 如果已经被打开了，就不再执行任何检测逻辑
        if (isOpen) return;

        // ==========================================
        // 只有在“正在被射线悬停”时，才进行时间和颜色计算
        // ==========================================
        if (isHovering)
        {
            hoverTimer += Time.deltaTime;

            // 视觉反馈：随着停留时间变长，宝箱慢慢变红
            color = Color.Lerp(Color.black, Color.red, hoverTimer / openTime);
            foreach (Renderer item in meshRenderers)
            {
                item.material.SetColor("_EmissionColor", color);
            }

            // 如果停留时间达到了设定的阈值（比如 3 秒）
            if (hoverTimer >= openTime)
            {
                ExecuteOpenSequence(); // 执行打开宝箱的华丽序列
            }
        }
    }

    private void ExecuteOpenSequence()
    {
        isOpen = true;
        isHovering = false; // 停止悬停计算

        // 关掉中心点
        VRPlayer.instance.isClosePoint = true;

        // 关掉自己的特效
        Transform aura = transform.Find("CFX3_MagicAura_A");
        if (aura != null) aura.gameObject.SetActive(false);

        animator.SetBool("IsOpen", true);
        VRPlayer.instance.aSource.PlayOneShot(clip);

        color = Color.black;
        foreach (Renderer item in meshRenderers) item.material.SetColor("_EmissionColor", color);

        // 生成自己的宝物
        VRModels obj = Instantiate(model, middle.position, middle.rotation).GetComponent<VRModels>();
        Vector3 tempScale = obj.transform.localScale;
        obj.transform.localScale = Vector3.zero;

        // 往上升起
        obj.transform.DOScale(tempScale, 1f);
        obj.transform.DOLocalMoveY(obj.transform.position.y + 1, 1f).OnComplete(() => {
            // 等待0.5秒
            DOTween.To(() => Vector2.zero, x => { }, Vector2.zero, 0.5f).OnComplete(() => {

                // 关掉特效
                if (obj.particle != null) obj.particle.SetActive(false);

                // 计算目的地
                float temp1 = 0;
                if (VRForestPlayer.instance.disSlider.value <= 0) temp1 = 0.01f;
                else temp1 = VRForestPlayer.instance.disSlider.value;

                temp1 = (0.4f / 0.25f) * temp1;

                Vector3 distance = obj.transform.position - VRPlayer.instance.RayCamera.transform.position;
                distance = Vector3.Normalize(distance) * temp1 + VRPlayer.instance.RayCamera.transform.position;

                obj.transform.DOMove(distance, 7).SetEase(Ease.OutQuint).OnComplete(() => {
                    // 之后再等待1秒钟， 消失即可 
                    DOTween.To(() => Vector2.zero, x => { }, Vector2.zero, 1).OnComplete(() => {
                        // 双双消失
                        transform.DOScale(Vector3.zero, 0.5f);
                        obj.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() => {

                            Destroy(obj.gameObject);
                            Destroy(gameObject); // 销毁宝箱自己

                            // 继续前行
                            if (VRForestPlayer.instance.isFixed == false)
                            {
                                VRPlayer.instance.KeepPlaying();
                            }
                            else
                            {
                                VRForestPlayer.instance.OpenBack();
                            }

                            // 打开中心点
                            VRPlayer.instance.isClosePoint = false;
                        });
                    });
                });
            });
        });
    }
}

