using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class WsPingMessage
{
    public int type = 0;
    public int subType = 0;
    public string fromId;
}

[Serializable]
public class WsPongMessage
{
    public string msgId;
    public int type;
    public int subType;
    public string fromId;
    public string toId;
}

[Serializable]
public class WsBindStatusMessage
{
    public string msgId;
    public int type;
    public int subType; // 2000=解绑/失败, 2001=绑定成功
    public string fromId;
    public string toId;
}

[Serializable]
public class WsVideoContent
{
    public int visualAssetId;
    public string url;
    //public string @event; // 使用 @ 符号转义 C# 保留字 event
    public float fastForward;
    public float position;
}

[Serializable]
public class WsVideoControlMessage
{
    public string msgId;
    public int type;
    public int subType;
    public string fromId;
    public string toId;
    public WsVideoContent content;
}

//视频内容上报
[Serializable]
public class WsProgressContent
{
    public int visualAssetId;
    public float position;
}

[Serializable]
public class WsProgressMessage
{
    public int type;
    public int subType;
    public string fromId;
    public WsProgressContent content;
}

public class WebSocketManager : MonoBehaviour
{
    public static WebSocketManager Instance { get; private set; }

    public static WsVideoControlMessage CachedPlayCommand;//缓存箱，防止视频播放场景未加载就推送消息导致的崩溃

    //广播事件，用来保证连接成功后发送验证请求和关闭验证码
    public static Action OnBindSuccessEvent;
    public static Action OnUnbindEvent;

    public static Action<WsVideoControlMessage> OnVideoCommandReceived;

    [Header("UI 引用")]
    public Text wsStatusText; // 用来显示 WS 状态的文本

    [Header("网络设置")]
    public string serverIpAndPort = "csj.tlinkai.com:9610";
    public int heartbeatInterval = 10; // 心跳间隔（秒）

    private ClientWebSocket ws;
    private CancellationTokenSource cts;
    private string myDeviceId;

    // 线程安全队列：用来把后台线程的消息转移到 Unity 主线程去更新 UI
    private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    void Awake()
    {
        // 1. 如果当前场景还没有这个管理器，就把自己设为老大，并免死（不销毁）
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // 2. 如果场景里已经有老大，就把多余的自己销毁掉，防止出现多个连接！
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        myDeviceId = SystemInfo.deviceUniqueIdentifier;
        ConnectWebSocket();
    }

    void Update()
    {
        // 每一帧检查队列，如果有后台抛过来的 UI 任务，就在主线程执行它
        while (mainThreadActions.TryDequeue(out var action))
        {
            action?.Invoke();
        }
    }

    // --- 核心：连接 WebSocket ---
    private async void ConnectWebSocket()
    {
        if (ws != null && ws.State == WebSocketState.Open) return;

        ws = new ClientWebSocket();
        cts = new CancellationTokenSource();

        // 拼接包含设备 ID 的 URL
        string wsUrl = $"ws://{serverIpAndPort}/vts/ws/vr?clientId={myDeviceId}";
        Uri uri = new Uri(wsUrl);

        try
        {
            EnqueueMainThreadAction(() => UpdateStatus($"[WS] 正在连接服务器..."));

            // 发起异步连接
            await ws.ConnectAsync(uri, cts.Token);

            EnqueueMainThreadAction(() => UpdateStatus($"[WS] 连接成功！"));
            Debug.Log("[WebSocket] 连接成功: " + wsUrl);

            // 连接成功后，同时开启“接收监听”和“心跳发送”两个任务
            _ = ReceiveLoop();
            _ = HeartbeatLoop();
        }
        catch (Exception e)
        {
            Debug.Log($"[WebSocket] 连接失败: {e.Message}");
            EnqueueMainThreadAction(() => UpdateStatus($"[WS] 连接失败，5秒后重试..."));

            // 失败后自动重连逻辑
            await Task.Delay(5000);
            if (this != null && gameObject.activeInHierarchy) ConnectWebSocket();
        }
    }

    // --- 核心：持续接收消息 ---
    private async Task ReceiveLoop()
    {
        byte[] buffer = new byte[4096]; // 4KB 缓冲区，收普通 JSON 足够了

        while (ws.State == WebSocketState.Open)
        {
            try
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    EnqueueMainThreadAction(() => UpdateStatus("[WS] 服务器主动断开连接"));
                }
                else
                {
                    // 把收到的字节流转成字符串
                    string jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    HandleReceivedMessage(jsonMessage);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[WebSocket] 接收异常/连接断开: {e.Message}");
                break; // 跳出循环
            }
        }

        // 如果跳出了循环，说明掉线了，触发重连
        EnqueueMainThreadAction(() => UpdateStatus("[WS] 连接已断开，准备重连..."));
        await Task.Delay(3000);
        ConnectWebSocket();
    }

    // --- 核心：处理收到的 JSON ---
    private void HandleReceivedMessage(string json)
    {
        // 这里只是做简单的类型检查，实际业务中你可能会用更复杂的 JSON 解析
        if (json.Contains("\"type\": 0") || json.Contains("\"type\":0"))
        {
            if (json.Contains("\"subType\": 1") || json.Contains("\"subType\":1"))
            {
                // 解析 PONG 消息
                WsPongMessage pong = JsonUtility.FromJson<WsPongMessage>(json);
                Debug.Log($"[WebSocket] 收到心跳 PONG: {pong.msgId}");
                return;
            }
        }
        // 2. 判断是否是绑定状态消息 (type:2)
        if (json.Contains("\"type\": 2") || json.Contains("\"type\":2"))
        {
            WsBindStatusMessage bindMsg = JsonUtility.FromJson<WsBindStatusMessage>(json);

            if (bindMsg.subType == 2001)
            {
                Debug.Log("[WebSocketManager] 收到指令：绑定成功");
                EnqueueMainThreadAction(() =>
                {
                    UpdateStatus($"[WS状态] 绑定成功！");
                    //告诉所有监听的人，绑定成功了
                    OnBindSuccessEvent?.Invoke();
                });
            }
            else if (bindMsg.subType == 2000)
            {
                Debug.Log("[WebSocketManager] 收到指令：解绑 / 绑定失败");
                EnqueueMainThreadAction(() =>
                {
                    UpdateStatus($"[WS状态] 设备已解绑");
                    //告诉大家，被解绑了
                    OnUnbindEvent?.Invoke();
                });
            }
            return;
        }

        if (json.Contains("\"type\": 3") || json.Contains("\"type\":3"))
        {
            Debug.Log($"[准备抓包] 后端发来的原始视频 JSON 是：\n{json}");
            WsVideoControlMessage videoMsg = JsonUtility.FromJson<WsVideoControlMessage>(json);
            EnqueueMainThreadAction(() =>
            {
                UpdateStatus($"[视频指令] 执行: {videoMsg.subType}");
                //如果是 3012(播放新视频),存进缓存箱
                if (videoMsg.subType == 3012)
                {
                    CachedPlayCommand = videoMsg;
                }
                // 广播给视频控制器
                OnVideoCommandReceived?.Invoke(videoMsg);
            });
            return;
        }

        // 3. 其他未知或后续扩展的业务消息
        Debug.Log($"[WebSocketManager] 收到未知业务消息: {json}");
        EnqueueMainThreadAction(() => UpdateStatus($"[WS收到消息] {json}"));
    }

    // --- 核心：每隔 X 秒发送 PING ---
    private async Task HeartbeatLoop()
    {
        while (ws.State == WebSocketState.Open)
        {
            // 构造 PING 对象
            WsPingMessage pingData = new WsPingMessage
            {
                type = 0,
                subType = 0,
                fromId = myDeviceId // 这里传入咱们的设备 32位 ID
            };

            string jsonToSend = JsonUtility.ToJson(pingData);
            byte[] bytesToSend = Encoding.UTF8.GetBytes(jsonToSend);

            try
            {
                await ws.SendAsync(new ArraySegment<byte>(bytesToSend), WebSocketMessageType.Text, true, cts.Token);
                Debug.Log("[WebSocket] 发送心跳 PING");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[WebSocket] 心跳发送失败: {e.Message}");
                break;
            }

            // 等待指定的秒数再发下一次
            await Task.Delay(heartbeatInterval * 1000, cts.Token);
        }
    }

    public void UpdateUIReference(Text newStatusText)
    {
        wsStatusText = newStatusText;
        UpdateStatus("[WS] 已重新绑定新场景 UI");
    }


    // --- 辅助工具 ---
    private void EnqueueMainThreadAction(Action action)
    {
        mainThreadActions.Enqueue(action);
    }

    private void UpdateStatus(string msg)
    {
        if (wsStatusText != null) wsStatusText.text = msg;
    }

    public async void SendJsonMessage(string jsonToSend)
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            try
            {
                byte[] bytesToSend = Encoding.UTF8.GetBytes(jsonToSend);
                await ws.SendAsync(new ArraySegment<byte>(bytesToSend), WebSocketMessageType.Text, true, cts.Token);
                // Debug.Log($"[WebSocketManager] 成功发送消息: {jsonToSend}"); 
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[WebSocketManager] 主动发送消息失败: {e.Message}");
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            if (cts != null)
            {
                cts.Cancel(); // 取消所有异步任务
                cts.Dispose();
            }
            if (ws != null)
            {
                ws.Dispose(); // 释放 WebSocket 资源
            }
        }
    }
}
