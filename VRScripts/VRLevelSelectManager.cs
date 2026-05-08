using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRLevelSelectManager : MonoBehaviour
{
    //原本完整流程通关逻辑
    public void PlayFullStory()
    {
        if (VRMain.instance != null)
        {
            //关闭单关模式，按照原流程走
            VRMain.instance.isSingleLevelMode = false;
            VRMain.instance.text = "组装机器人";
            VRMain.instance.ChangeSence("ForestScene_Fixed", "13", true);//true为直接进场景，不用播放视频
        }
    }
    public void PlaySingleLevel(string targetSceneName)
    {
        if(VRMain.instance != null)
        {
            //进入单关模式
            VRMain.instance.isSingleLevelMode = true;

            //VRMain.instance.text = "加载关卡中...";
            switch (targetSceneName)
            {
                case "ForestScene_Fixed": VRMain.instance.text = "组装机器人"; break;
                case "Squirrel": VRMain.instance.text = "观察小松鼠"; break;
                case "Hudie_School": VRMain.instance.text = "发现蝴蝶"; break;
                case "TropicPack": VRMain.instance.text = "热带丛林探索"; break;
                case "Balloons": VRMain.instance.text = "乘坐热气球"; break;
                case "Ball_School": VRMain.instance.text = "寻找乒乓球"; break;
                default: VRMain.instance.text = "加载中..."; break;
            }
            // 【关键修正】：这里必须传 true！！！
            // true 表示跳过视频直接进入场景，这样就不会触发咱们在 VRMain 里写的“打完关卡播结尾视频”的拦截逻辑了。
            VRMain.instance.ChangeSence(targetSceneName, "", true);
        }
        else
        {
            Debug.LogError("VRMain.instance 为空！请确保游戏是从 Main 场景启动，或者自带了 VRMain。");
        }
    }
}
