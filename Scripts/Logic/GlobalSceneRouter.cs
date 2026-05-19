using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class GlobalSceneRouter : MonoBehaviour
{
    private float sceneStartTime = 0f;   // 记录进入场景时的秒表起算点
    private int currentAssetId = 0;      // 记录当前正在体验的 ID
    private bool isTiming = false;       // 防止重复发送的开关
    private Coroutine tickerCoroutine;//持续发送时间协程
    void OnEnable()
    {
        // 监听来自 WebSocket 的所有视频和游戏指令
        WebSocketManager.OnVideoCommandReceived += HandleGlobalNavigation;
    }

    void OnDisable()
    {
        WebSocketManager.OnVideoCommandReceived -= HandleGlobalNavigation;
    }

    private void HandleGlobalNavigation(WsVideoControlMessage msg)
    {
        switch (msg.subType)
        {
            // =========================
            // 1. 去视频场景
            // =========================
            case 3012:
                if(msg.content != null)
                {
                    Debug.Log("[全局路由] 收到播放指令，准备跳转: VideoScene");
                    StartContinuousReporting(msg.content.visualAssetId);
                    SceneManager.LoadScene("PlayVideo");
                }
                break;

            // =========================
            // 2. 去游戏场景
            // =========================
            case 3101:
                if (msg.content != null && !string.IsNullOrEmpty(msg.content.url))
                {
                    Debug.Log($"[全局路由] 收到游戏指令，准备跳转: {msg.content.url}");
                    StartContinuousReporting(msg.content.visualAssetId);
                    SceneManager.LoadScene(msg.content.url);
                }
                break;

            // =========================
            // 3. 停止并返回大厅
            // =========================
            case 3017:
            case 3018: // 视频关闭
            case 3102: // 游戏停止
                Debug.Log("[全局路由] 收到停止指令，带上免死金牌返回: newmain");
                StopContinuousReporting(); // 停止每秒发送

                // 给 LogicManager 发免死金牌，确保退回大厅时不刷新验证码
                LogicManager.preserveStateOnLoad = true;

                // 退回主大厅
                SceneManager.LoadScene("NewMain");
                break;
        }
    }

    private void StartContinuousReporting(int assetId)
    {
        // 1. 如果之前有正在运行的计时器，先强行停止，防止 ID 冲突或多重计时
        StopContinuousReporting();

        // 2. 初始化计时参数
        currentAssetId = assetId;
        sceneStartTime = Time.time;
        isTiming = true;

        // 3. 开启协程，每秒执行一次发送
        tickerCoroutine = StartCoroutine(DurationTickerLoop());
        Debug.Log($"[数据埋点] 开始实时上报，当前 ID: {assetId}");
    }

    private IEnumerator DurationTickerLoop()
    {
        // 只要处于计时状态，就无限循环
        while (isTiming)
        {
            // 等待 1 秒
            yield return new WaitForSeconds(1f);

            // 计算从开始到现在经过的总秒数
            int currentElapsed = Mathf.RoundToInt(Time.time - sceneStartTime);

            // 构造消息
            WsProgressMessage logMsg = new WsProgressMessage
            {
                type = 5,
                subType = 5001,
                fromId = SystemInfo.deviceUniqueIdentifier,
                content = new WsProgressContent
                {
                    visualAssetId = currentAssetId,
                    position = currentElapsed // 这里发送的是实时动态增加的秒数
                }
            };

            string jsonToSend = JsonUtility.ToJson(logMsg);

            // 通过 WebSocket 发送
            if (WebSocketManager.Instance != null)
            {
                WebSocketManager.Instance.SendJsonMessage(jsonToSend);
                // Debug.Log($"[实时埋点] ID:{currentAssetId} 已运行 {currentElapsed} 秒");
            }
        }
    }

    private void StopContinuousReporting()
    {
        isTiming = false;
        if (tickerCoroutine != null)
        {
            StopCoroutine(tickerCoroutine);
            tickerCoroutine = null;
        }
        currentAssetId = 0;
    }
}