using UnityEngine;
using TMPro; // 引入 TextMeshPro 命名空间

public class RaycastDebugger : MonoBehaviour
{
    [Header("把场景里的 TextMeshPro 拖到这里")]
    public TextMeshProUGUI debugText;
    // ⚠️ 如果你用的是旧版普通的 Text，请把上面这行换成：
    // public UnityEngine.UI.Text debugText;

    void Update()
    {
        RaycastHit hit;
        // 发射无限长的探测射线
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1000f))
        {
            // 如果碰到了东西，在 UI 上打印出它的名字和层级
            if (debugText != null)
            {
                debugText.text = $"<color=red>挡路物:</color> {hit.collider.name}\n" +
                                 $"<color=yellow>层级:</color> {LayerMask.LayerToName(hit.collider.gameObject.layer)}";
            }

            // 电脑屏幕上的调试线依然保留
            Debug.DrawLine(transform.position, hit.point, Color.magenta);
        }
        else
        {
            // 射线前方没有任何阻挡
            if (debugText != null)
            {
                debugText.text = "<color=green>射线畅通无阻</color>";
            }
        }
    }
}