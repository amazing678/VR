using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[Serializable]
public class BaseRequest
{
    public string deviceId;
}

[Serializable]
public class BaseResponse
{
    public int code;
    public string message;
}

[Serializable]
public class BindCodeResponse : BaseResponse
{
    public string data; // 验证码
}

[Serializable]
public class UserInfoResponse : BaseResponse
{
    public UserInfoData data;
}

[Serializable]
public class UserInfoData
{
    public string nickname; // 用户名 (例如: bucc)
    public string mobile;   // 手机号 (例如: 17698910705)
    public bool online;     // 是否在线 (true/false)
}

public enum UserState
{
    WaitingToBind,  // 还没人绑定（耐心等待，不刷新验证码）
    Online,         // 已绑定且在线（隐藏验证码）
    Offline,        // 已绑定但离线（需要重置清空）
    NetworkError    // 断网或异常（保持现状，等网好）
}

// ==========================================
// 2. 核心逻辑管理器
// ==========================================
public class LogicManager : MonoBehaviour
{
    [Header("UI 引用")]
    public CodeManager codeManager;
    public Text statusText;

    [Header("网络设置")]
    //public string baseUrl = "http://192.168.10.20:9610/vts/vrDeviceApi";
    public string baseUrl = "http://csj.tlinkai.com:9610/vts/vrDeviceApi";
    public float checkInterval = 30f; // 每 30 秒查一次状态

    private string myDeviceId;
    private Coroutine mainWorkflowCoroutine;

    private bool forceCheckBindNow = false;
    private bool forceUnbindNow = false;

    public static bool preserveStateOnLoad = false;//跨场景存储

    // 监听 WebSocket 的广播
    void OnEnable()
    {
        WebSocketManager.OnBindSuccessEvent += HandleWsBindSuccess;
        WebSocketManager.OnUnbindEvent += HandleWsUnbind;
        WebSocketManager.OnVideoCommandReceived += HandleVideoCommandInLogin;
    }

    void OnDisable()
    {
        WebSocketManager.OnBindSuccessEvent -= HandleWsBindSuccess;
        WebSocketManager.OnUnbindEvent -= HandleWsUnbind;
        WebSocketManager.OnVideoCommandReceived -= HandleVideoCommandInLogin;
    }

    private void HandleVideoCommandInLogin(WsVideoControlMessage msg)
    {
        if (msg.subType == 3012)
        {
            Debug.Log("[LogicManager] 收到播放指令，准备跨越至视频播放场景...");

            //视频播放场景名称
            SceneManager.LoadScene("PlayVideo");
        }
    }

    private void HandleWsBindSuccess() { StartCoroutine(DelayCheckBindFlag()); } // 听到绑定成功，查询 Flag
    private IEnumerator DelayCheckBindFlag()
    {
        // 【核心修复】：给后端数据库留出 2 秒的同步写入时间
        yield return new WaitForSeconds(2f);
        forceCheckBindNow = true;
    }
    private void HandleWsUnbind() { forceUnbindNow = true; }       // 听到解绑，立刻立起解绑 Flag

    void Start()
    {
        myDeviceId = SystemInfo.deviceUniqueIdentifier;
        UpdateStatus($"[初始化] 设备ID: {myDeviceId}");

        // 启动主业务流
        mainWorkflowCoroutine = StartCoroutine(MainWorkflowLoop());
    }

    /// <summary>
    /// 主业务循环控制 (状态机)
    /// </summary>
    private IEnumerator MainWorkflowLoop()
    {
        while (true)
        {
            // 每次从头开始时，重置所有标志
            forceUnbindNow = false;
            forceCheckBindNow = false;

            if (preserveStateOnLoad)
            {
                Debug.Log("[LogicManager] 从视频场景返回，保持当前绑定，跳过清除！");
                preserveStateOnLoad = false; //删除跨场景存储

                // 强行触发一次状态查询，用来瞬间恢复用户名和隐藏6位数的 UI
                forceCheckBindNow = true;
            }
            else
            {
                // 1. 初始化阶段：只在刚启动或确认离线后才执行
                UpdateStatus("[流程] 正在清除历史绑定...");
                yield return StartCoroutine(ClearBindRequest());

                UpdateStatus("[流程] 正在生成新适配码...");
                yield return StartCoroutine(GenerateCodeRequest());
            }

            // 2. 轮询监控阶段
            bool inPollingPhase = true;
            float timer = 0f;

            while (inPollingPhase)
            {
                // WS 强行解绑（比如小程序点了退出）
                if (forceUnbindNow)
                {
                    Debug.Log("[LogicManager] 收到 WS 强制解绑信号，准备重置...");
                    forceUnbindNow = false;
                    inPollingPhase = false; // 打破内层循环，回到上面重新 Clear
                    break;
                }
                
                // 倒计时 30 秒到了，或者 WS 刚报了绑定成功
                if (timer >= checkInterval || forceCheckBindNow)
                {
                    timer = 0f;
                    forceCheckBindNow = false;

                    // 去查询精准状态
                    yield return StartCoroutine(CheckUserInfoRequest((state) =>
                    {
                        switch (state)
                        {
                            case UserState.Online:
                                // 查到绑定了 -> 把验证码变成横杠，继续留在内层循环监控
                                if (codeManager != null) codeManager.SetDisplayCode("------");
                                break;

                            case UserState.WaitingToBind:
                                // 【修复点】：查不到人（code!=0）-> 什么都不做，保持 6 位码不变，继续等！
                                break;

                            case UserState.Offline:
                                UpdateStatus("[警告] 绑定用户当前不在线...");
                                if (codeManager != null) codeManager.SetDisplayCode("------");
                                // “只要不在线就立马踢掉他重新生成”，就把下面这行解注：
                                // inPollingPhase = false;

                                break;

                            case UserState.NetworkError:
                                // 网络抖动 -> 保持现状，等下一个 30 秒再查
                                break;
                        }
                    }));
                }

                // 如果依然在监控阶段，让计时器继续走
                if (inPollingPhase)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
            }

            // 如果代码走到这里，说明 inPollingPhase 变成 false 了（要重置了）
            UpdateStatus("[状态] 准备重置设备绑定状态...");
            if (codeManager != null) codeManager.SetDisplayCode("------");
            yield return new WaitForSeconds(2f); // 稍微缓冲一下
        }
    }

    // ----------------------------------------------------
    // --- 接口 1: 清除绑定 (精简版) ---
    // ----------------------------------------------------
    private IEnumerator ClearBindRequest()
    {
        string url = baseUrl + "/clearBind";
        string jsonToSend = JsonUtility.ToJson(new BaseRequest { deviceId = myDeviceId });

        using (UnityWebRequest request = CreatePostRequest(url, jsonToSend))
        {
            yield return request.SendWebRequest();
            // 不管前后端返回什么 code，我们只要发了就行
            Debug.Log("[API] 发送了清除绑定请求");
        }
    }

    // ----------------------------------------------------
    // --- 接口 2: 生成适配码 ---
    // ----------------------------------------------------
    private IEnumerator GenerateCodeRequest()
    {
        string url = baseUrl + "/generateBindCode";
        string jsonToSend = JsonUtility.ToJson(new BaseRequest { deviceId = myDeviceId });

        using (UnityWebRequest request = CreatePostRequest(url, jsonToSend))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                BindCodeResponse resData = JsonUtility.FromJson<BindCodeResponse>(request.downloadHandler.text);
                if (resData.code == 0)
                {
                    UpdateStatus($"[等待绑定] 适配码已刷新 ({DateTime.Now:HH:mm:ss})");
                    if (codeManager != null) codeManager.SetDisplayCode(resData.data);
                }
                else
                {
                    UpdateStatus($"[生成失败] {resData.message}");
                }
            }
            else
            {
                UpdateStatus($"[网络错误] 生成适配码失败");
            }
        }
    }

    // ----------------------------------------------------
    // --- 接口 3: 查询在线状态 ---
    // ----------------------------------------------------
    private IEnumerator CheckUserInfoRequest(Action<UserState> onResult)
    {
        string url = baseUrl + "/getBindUserInfo";
        string jsonToSend = JsonUtility.ToJson(new BaseRequest { deviceId = myDeviceId });

        using (UnityWebRequest request = CreatePostRequest(url, jsonToSend))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                UserInfoResponse resData = JsonUtility.FromJson<UserInfoResponse>(request.downloadHandler.text);

                // 1. 成功查到绑定数据
                if (resData.code == 0 && resData.data != null)
                {
                    if (resData.data.online)
                    {
                        UpdateStatus($"[运行中] 绑定用户: {resData.data.nickname}");
                        onResult?.Invoke(UserState.Online);
                    }
                    else
                    {
                        Debug.Log("[API] 状态查询：绑定的用户已离线。");
                        onResult?.Invoke(UserState.Offline);
                    }
                }
                // 2. 没有任何人绑定（比如 code 是 200050）
                else
                {
                    Debug.Log($"[API] 状态查询：暂无用户绑定，继续等待。");
                    onResult?.Invoke(UserState.WaitingToBind);
                }
            }
            else
            {
                Debug.LogWarning("[API] 状态查询网络失败...");
                onResult?.Invoke(UserState.NetworkError);
            }
        }
    }

    // ----------------------------------------------------
    // --- 辅助工具方法 ---
    // ----------------------------------------------------
    private UnityWebRequest CreatePostRequest(string url, string json)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}