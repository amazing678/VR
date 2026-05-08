using UnityEngine;
using RenderHeads.Media.AVProVideo; // 引入 AVPro 命名空间
using UnityEngine.SceneManagement;

public class VRVideoOffsetManager : MonoBehaviour
{
    [Header("视频播放器")]
    public MediaPlayer leftPlayer;
    public MediaPlayer rightPlayer;

    [Header("巨幕渲染器")]
    public MeshRenderer screenRenderer;

    [Header("时间异步参数")]
    [Tooltip("左眼提前的毫秒数")]
    public float leftEyeAheadMs = 86f;

    private Material stereoMaterial;

    private int currentVisualAssetId = 0;

    void OnEnable()
    {
        WebSocketManager.OnVideoCommandReceived += HandleVideoCommand;
    }

    void OnDisable()
    {
        WebSocketManager.OnVideoCommandReceived -= HandleVideoCommand;
    }

    void Start()
    {
        // 1. 获取刚刚赋予巨幕的 StereoVideoMat 材质
        if (screenRenderer != null)
        {
            stereoMaterial = screenRenderer.material;
        }

        // 2. 延迟一点点播放，确保 AVPro 初始化完成
        //Invoke(nameof(PlayWithOffset), 0.5f);
        if (WebSocketManager.CachedPlayCommand != null)
        {
            Debug.Log("[VR 视差] 检测到跨场景缓存指令，开始自动加载网络视频！");
            Invoke(nameof(ExecuteCachedCommand), 0.5f);
        }
        StartCoroutine(ReportProgressLoop());
    }

    private void ExecuteCachedCommand()
    {
        if (WebSocketManager.CachedPlayCommand != null)
        {
            HandleVideoCommand(WebSocketManager.CachedPlayCommand);
            WebSocketManager.CachedPlayCommand = null;
        }
    }

    //处理小程序指令
    private void HandleVideoCommand(WsVideoControlMessage msg)
    {
        if (leftPlayer == null || rightPlayer == null) return;

        IMediaControl leftCtrl = leftPlayer.Control;
        IMediaControl rightCtrl = rightPlayer.Control;

        switch (msg.subType)
        {
            case 3012: // EVENT_PLAY (播放全新的网络视频)
                if (msg.content == null || string.IsNullOrEmpty(msg.content.url))
                {
                    Debug.LogError("[VR 视差报错] 播放指令解析失败！content为空，或者url为空。请检查后端JSON格式。");
                    return;
                }

                currentVisualAssetId = msg.content.visualAssetId;
                Debug.Log($"[VR 视差] 准备加载网络视频: {msg.content.url}");

                // 构造网络路径
                MediaPath path = new MediaPath(msg.content.url, MediaPathType.AbsolutePathOrURL);

                // 先打开媒体（不自动播放，我们要保证时间错位）
                leftPlayer.OpenMedia(path, autoPlay: false);
                rightPlayer.OpenMedia(path, autoPlay: false);

                // 网络视频缓冲需要一点时间，延迟 1 秒后执行同步错位播放
                // 如果你的网络慢导致左右眼黑屏时间不一致，可以把 1.0f 调大
                Invoke(nameof(PlayWithOffset), 1.0f);
                break;

            case 3013: // EVENT_PAUSE (暂停)
                if (leftCtrl != null) leftCtrl.Pause();
                if (rightCtrl != null) rightCtrl.Pause();
                break;

            case 3014: // EVENT_RESUME (恢复播放)
                if (leftCtrl != null) leftCtrl.Play();
                if (rightCtrl != null) rightCtrl.Play();
                break;

            case 3015: // EVENT_FAST_FORWARD (快进/倍速)
                if (leftCtrl != null) leftCtrl.SetPlaybackRate(msg.content.fastForward);
                if (rightCtrl != null) rightCtrl.SetPlaybackRate(msg.content.fastForward);
                break;

            case 3016: // EVENT_JUMP (跳转进度)
                if (leftCtrl != null && rightCtrl != null && msg.content != null)
                {
                    // 1. 记录当前是不是正在播放，并强行暂停双眼
                    bool wasPlaying = rightCtrl.IsPlaying();
                    if (wasPlaying)
                    {
                        leftCtrl.Pause();
                        rightCtrl.Pause();
                    }

                    // 【核心修复】：AVPro 的 Seek 单位就是“秒”！千万不能乘 1000！
                    double targetTimeSeconds = msg.content.position;

                    // 左眼的提前量 (86ms) 需要除以 1000 换算成秒 (0.086秒)
                    double leftOffsetSeconds = leftEyeAheadMs / 1000.0;

                    // 2. 执行精准错位跳转
                    rightCtrl.Seek(targetTimeSeconds);
                    leftCtrl.Seek(targetTimeSeconds + leftOffsetSeconds);

                    // 3. 恢复播放
                    if (wasPlaying)
                    {
                        leftCtrl.Play();
                        rightCtrl.Play();
                    }

                    Debug.Log($"[VR 视差] 进度跳转至: {targetTimeSeconds} 秒，左眼提前: {leftOffsetSeconds} 秒");
                }
                break;

            case 3017: // EVENT_STOP (停止)
            case 3018: // EVENT_CLOSE (关闭)
                if (leftCtrl != null) leftCtrl.Stop();
                if (rightCtrl != null) rightCtrl.Stop();
                leftPlayer.CloseMedia();
                rightPlayer.CloseMedia();
                currentVisualAssetId = 0;//视频停止后关闭ID
                LogicManager.preserveStateOnLoad = true;//生成跨场景保存
                SceneManager.LoadScene("NewMain");//回到登录场景
                break;
        }
    }

    public void PlayWithOffset()
    {
        // 换算成秒
        float offsetSeconds = leftEyeAheadMs / 1000f;

        if (rightPlayer.Control != null && leftPlayer.Control != null)
        {
            // 右眼从 0 开始
            rightPlayer.Control.Seek(0f);
            // 左眼提前 offsetSeconds
            leftPlayer.Control.Seek(offsetSeconds);

            //倍速重置为 1
            rightPlayer.Control.SetPlaybackRate(1f);
            leftPlayer.Control.SetPlaybackRate(1f);

            // 同时按下播放键
            rightPlayer.Control.Play();
            leftPlayer.Control.Play();

            Debug.Log($"[VR 视差] 播放已同步。左眼进度: {offsetSeconds}s, 右眼进度: 0s");
        }
        else
        {
            Debug.LogWarning("[VR 视差] 视频流加载过慢，正在重试...");
            // 如果网太慢，隔 0.5 秒再试一次
            Invoke(nameof(PlayWithOffset), 0.5f);
        }
    }

    //每秒上传视频进度
    private System.Collections.IEnumerator ReportProgressLoop()
    {
        while (true)
        {
            // 等待 1 秒
            yield return new WaitForSeconds(1f);

            // 只有当右眼（主参考眼）组件存在，并且正在播放时，才上报进度
            if (rightPlayer != null && rightPlayer.Control != null)
            {
                // 抓取 AVPro 的两个关键状态
                bool isPlaying = rightPlayer.Control.IsPlaying();
                bool isFinished = rightPlayer.Control.IsFinished();

                // 【核心防线】：必须是“正在播放” 且 “没有结束” 且 “当前有视频ID” 时才推送
                if (isPlaying && !isFinished && currentVisualAssetId != 0)
                {
                    float currentPos = (float)rightPlayer.Control.GetCurrentTime();

                    WsProgressMessage progressMsg = new WsProgressMessage
                    {
                        type = 5,
                        subType = 5001,
                        fromId = SystemInfo.deviceUniqueIdentifier,
                        content = new WsProgressContent
                        {
                            visualAssetId = currentVisualAssetId,
                            position = currentPos
                        }
                    };

                    string json = JsonUtility.ToJson(progressMsg);

                    // 呼叫 WebSocket 管理器发射消息
                    if (WebSocketManager.Instance != null)
                    {
                        WebSocketManager.Instance.SendJsonMessage(json);
                    }
                }
            }
        }
    }

    void Update()
    {
        if (stereoMaterial == null) return;

        // 实时抓取两个播放器的纹理，塞给 GPU 的 Shader

        // 抓取左眼画面
        if (leftPlayer != null && leftPlayer.TextureProducer != null)
        {
            Texture leftTex = leftPlayer.TextureProducer.GetTexture();
            if (leftTex != null) stereoMaterial.SetTexture("_LeftTex", leftTex);
        }

        // 抓取右眼画面
        if (rightPlayer != null && rightPlayer.TextureProducer != null)
        {
            Texture rightTex = rightPlayer.TextureProducer.GetTexture();
            if (rightTex != null) stereoMaterial.SetTexture("_RightTex", rightTex);
        }
    }
}