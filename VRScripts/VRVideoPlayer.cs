using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RenderHeads.Media.AVProVideo.MediaPlayer;

public class VRVideoPlayer : MonoBehaviour
{
    public MediaPlayer player;
    // Start is called before the first frame update
    void Awake()
    {
        //QualitySettings.antiAliasing = 0;
        if (VRMain.instance == null) return;
        if (String.IsNullOrWhiteSpace(VRMain.instance.videoName)) return;

        String path = "D:/Unity Work/VR2019_2_Phone/VRVideo/";
        if (Application.platform == RuntimePlatform.Android)
            path = "/sdcard/VRVideo/";

        //player.OpenVideoFromFile(FileLocation.AbsolutePathOrURL, path + VRMain.instance.videoName + ".mp4", true);
        // 1. 注册 AVPro 的播放事件监听器
        if (player != null)
        {
            player.Events.AddListener(OnVideoEvent);
            //player.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, path + VRMain.instance.videoName + ".mp4", true);
            // 第一步：组合出完整的视频路径字符串
            string fullVideoPath = path + VRMain.instance.videoName + ".mp4";

            // 第二步：使用 MediaPath 包装路径，并将原本的 FileLocation 改为 MediaPathType
            MediaPath mediaPath = new MediaPath(fullVideoPath, MediaPathType.AbsolutePathOrURL);

            // 第三步：调用新的 OpenMedia 接口并设置自动播放 (true)
            player.OpenMedia(mediaPath, autoPlay: true);
        }
    }

    // 2. 自动监听视频状态
    private void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType eventType, ErrorCode code)
    {
        // 当 AVPro 报告“视频已经播放完毕”时，自动触发结束逻辑
        if (eventType == MediaPlayerEvent.EventType.FinishedPlaying)
        {
            Debug.Log("[VR架构] 视频播放完毕，准备自动跳转场景...");
            IsFinishing();
        }
    }

    public void IsFinishing()
    {
        if (VRMain.instance == null) return;
        if (VRMain.instance.isAllPlayer == false) return;

        if (VRPlayer.instance != null)
        {
            VRPlayer.instance.IsOpenAllGroup1(true, false, () => {
                VRMain.instance.isLookVideo = true;
                VRMain.instance.ToChangeSence(); // 视频播放完跳转
            });
        }



    }
    // 严谨的架构师习惯：销毁时注销事件，防止内存泄漏
    void OnDestroy()
    {
        if (player != null)
        {
            player.Events.RemoveListener(OnVideoEvent);
        }
    }
}
