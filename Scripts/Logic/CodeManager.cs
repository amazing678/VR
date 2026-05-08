using UnityEngine;
using UnityEngine.UI; // 使用原生 Text
using System.Collections.Generic;

public class CodeManager : MonoBehaviour
{
    [Header("按顺序拖入那 6 个原生 Text 组件")]
    public List<Text> digitTexts;

    /// <summary>
    /// 接收完整字符串，并将其分开到 6 个 Text 中
    /// </summary>
    /// <param name="fullCode">传入如 "399582" 的字符串</param>
    public void SetDisplayCode(string fullCode)
    {
        // 容错处理：确保传进来的真的是 6 位数
        if (string.IsNullOrEmpty(fullCode) || fullCode.Length != 6)
        {
            Debug.LogWarning("[CodeManager] 传入的验证码不是 6 位！当前值为: " + fullCode);
            return;
        }

        // 核心拆分逻辑：遍历 6 个 Text 框
        for (int i = 0; i < digitTexts.Count; i++)
        {
            // 防止数组越界
            if (i < fullCode.Length && digitTexts[i] != null)
            {
                // 将字符串的第 i 个字符转成字符串，塞给第 i 个 Text 框
                digitTexts[i].text = fullCode[i].ToString();
            }
        }
    }
}