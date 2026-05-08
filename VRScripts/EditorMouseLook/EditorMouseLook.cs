using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorMouseLook : MonoBehaviour
{
    public float sensitivity = 2.0f;
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    void Start()
    {
#if !UNITY_EDITOR
        //destroy(this);
#else

        // 【关键修复点】在 Unity 编辑器中，暂时禁用 Nibiru 的头部追踪，防止视角被强制锁死
        Behaviour nvrHead = GetComponent("NvrHead") as Behaviour;
        if (nvrHead != null)
        {
            nvrHead.enabled = false;
            Debug.Log("已在编辑器模式下自动禁用 NvrHead，可以使用鼠标右键模拟转头。");
        }

        // 记录一下初始角度，防止视角瞬间乱跳
        rotationX = transform.localEulerAngles.x;
        rotationY = transform.localEulerAngles.y;
#endif
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        // 按住鼠标右键时，滑动鼠标可以模拟头部转动
        if (Input.GetMouseButton(1))
        {
            rotationY += Input.GetAxis("Mouse X") * sensitivity;
            rotationX -= Input.GetAxis("Mouse Y") * sensitivity;
            rotationX = Mathf.Clamp(rotationX, -90, 90);

            transform.localEulerAngles = new Vector3(rotationX, rotationY, 0);
        }
#endif
    }
}
