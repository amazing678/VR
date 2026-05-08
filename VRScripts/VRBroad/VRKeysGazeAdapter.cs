using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRKeys;

public class VRKeysGazeAdapter : MonoBehaviour
{
    [Header("凝视几秒触发输入")]
    public float triggerTime = 2.0f;
    [Header("凝视时的变深颜色")]
    public Color hoverColor = new Color(0.4f, 0.4f, 0.4f);

    [HideInInspector] public bool isHovered = false;

    private Key vrKey;
    private float currentTime = 0;
    private MeshRenderer meshRenderer;
    private Color originalColor;
    private bool isTriggered = false;

    void Start()
    {
        vrKey = GetComponent<Key>();
        meshRenderer = GetComponent<MeshRenderer>();
        if(meshRenderer!= null && meshRenderer.material != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    void Update()
    {
        if (vrKey == null || vrKey.keyboard == null || vrKey.keyboard.disabled || !vrKey.keyboard.initialized) return;
        if (isHovered && !isTriggered)
        {
            currentTime += Time.deltaTime;

            if (meshRenderer != null)
                meshRenderer.material.color = Color.Lerp(originalColor, hoverColor, currentTime / triggerTime);
            if (currentTime >= triggerTime)
            {
                isTriggered = true;
                currentTime = 0;
                isHovered = false;

                //直接调用VRKey原生的触发逻辑和反馈
                vrKey.HandleTriggerEnter(null);
                vrKey.ActivateFor(0.3f);

                if (meshRenderer != null) meshRenderer.material.color = originalColor;
            }
        }
        else if (!isHovered)
        {
            isTriggered = false;
            //视线移开，倒计时衰退，颜色恢复
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime * 3f;
                if (currentTime < 0) currentTime = 0;
                if (meshRenderer != null)
                    meshRenderer.material.color = Color.Lerp(originalColor, hoverColor, currentTime / triggerTime);
            }
        }
        isHovered = false;//每帧末尾重置，等待射线呼叫
    }
}
