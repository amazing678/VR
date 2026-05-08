using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VRLoginFadeIn : MonoBehaviour
{
    [Header("黑屏褪去所需的时间（秒）")]
    public float fadeDuration = 1.5f;

    void Start()
    {
        // 场景启动后，开启渐亮协程
        StartCoroutine(FadeInRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        // 稍微等 0.2 秒，确保场景里的模型和 UI 都彻底加载稳妥，防止卡顿闪烁
        yield return new WaitForSeconds(0.2f);

        // 全局寻找那个叫 "allblack" 的黑屏图片
        GameObject blackObj = GameObject.Find("AllBack");

        if (blackObj != null)
        {
            Image blackScreen = blackObj.GetComponent<Image>();
            if (blackScreen != null)
            {
                Color c = blackScreen.color;
                float timer = 0;

                // 核心视觉魔法：在设定好的时间内，平滑地把 Alpha 从 1 降到 0
                while (timer < fadeDuration)
                {
                    timer += Time.deltaTime;
                    c.a = Mathf.Lerp(1, 0, timer / fadeDuration);
                    blackScreen.color = c;
                    yield return null; // 等待下一帧
                }

                // 【TA 防坑细节】：完全透明后，必须关掉它的射线检测，否则它会像隐形玻璃一样挡住你的视线交互！
                blackScreen.raycastTarget = false;

                // 可选：为了彻底省性能，渐变结束后直接把这个全黑图片隐藏掉
                blackObj.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("场景中没有找到名字叫 'allblack' 的物体，渐亮特效跳过。");
        }
    }
}