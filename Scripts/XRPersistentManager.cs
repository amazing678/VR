using UnityEngine;

public class XRPersistentManager : MonoBehaviour
{
    public static XRPersistentManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject); // 从其他场景退回 Main 时，销毁多余的玩家
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject); // 跨场景保留
    }
}