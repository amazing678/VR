using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VRGazeButton : MonoBehaviour
{
    [Header("注视触发秒数")]
    public float triggerTime = 2f;
    public UnityEvent onClick = new UnityEvent();
    public Image progressImage;

    [HideInInspector]
    public bool isHovered = false;
    private float currentTime = 0;
    private bool isTriggered = false; // 防止单次凝视触发多次

    void Update()
    {
        if (isHovered && !isTriggered)
        {
            currentTime += Time.deltaTime;

            //更新环形UI进度条
            if(progressImage != null)
                progressImage.fillAmount = currentTime / triggerTime;

            if(currentTime >= triggerTime)
            {
                isTriggered = true; // 锁定，防止重复调用
                currentTime = 0;
                isHovered = false;
                if(progressImage != null) progressImage.fillAmount = 0;

                //防止空指针
                if (onClick != null)
                {
                    Debug.Log("射线按钮被成功触发！准备执行跳转...");
                    onClick.Invoke();
                }
            }  
        }
        else if (!isHovered)
        {
            isTriggered = false; // 视线移开后重置状态
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime * 2f;
                if (progressImage != null) progressImage.fillAmount = currentTime / triggerTime;
            }
            else
            {
                currentTime = 0;
            }
        }
        isHovered = false;
    }
}
