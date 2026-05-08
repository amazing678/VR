using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VRLoginManager : MonoBehaviour
{
    public TMP_InputField usernameInput;//账号输入框
    public TMP_InputField passwordInput;//密码输入框
    public TextMeshProUGUI feedbackText;//提示文字
    
    public void RegisterAccount()
    {
        string user = usernameInput.text;
        string pass = passwordInput.text;

        if(string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            ShowFeedback("账号密码不能为空",Color.red);
            return;
        }
        if (PlayerPrefs.HasKey(user))
        {
            ShowFeedback("用户已存在,请直接登录",Color.red);
        }
        else
        {
            PlayerPrefs.SetString(user, pass);
            PlayerPrefs.Save();
            ShowFeedback("注册成功！请凝视登录进入游戏", Color.green);
        }
    }
    public void LoginAccount()
    {
        string user = usernameInput.text;
        string pass = passwordInput.text;

        if (PlayerPrefs.HasKey(user))
        {
            if(PlayerPrefs.GetString(user) == pass)
            {
                ShowFeedback("登录成功！正在载入...", Color.green);

                if(VRMain.instance != null)
                {
                    VRMain.instance.isSingleLevelMode = true;
                    VRMain.instance.text = "选择关卡";
                    VRMain.instance.ChangeSence("Main", "", true);
                }
            }
            else
            {
                ShowFeedback("密码错误，请重试！", Color.red);
            }
        }
        else
        {
            ShowFeedback("账号不存在，请先注册！", Color.red);
        }
    }

    private void ShowFeedback(string msg,Color color)
    {
        feedbackText.text = msg;
        feedbackText.color = color;
    }
}
