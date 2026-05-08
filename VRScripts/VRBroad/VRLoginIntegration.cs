using UnityEngine;
using UnityEngine.UI;
using TMPro; // 引入 TextMeshPro 命名空间
using UnityEngine.SceneManagement; // 引入场景管理命名空间

public class VRLoginIntegration : MonoBehaviour
{
    [Header("输入框 (TMP)")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    [Header("按钮交互")]
    public Button loginButton;
    public Button registerButton;

    [Header("UI 提示弹窗 (Feedback)")]
    public GameObject messagePanel; // 提示框的背景图 (可空)
    public TMP_Text messageText;    // 显示具体提示的文字组件

    [Header("场景跳转")]
    [Tooltip("登录成功后要跳转的场景名称")]
    public string nextSceneName = "Main";

    void Start()
    {
        // 动态绑定大朋射线对按钮的点击事件
        if (loginButton != null) loginButton.onClick.AddListener(OnLoginClicked);
        if (registerButton != null) registerButton.onClick.AddListener(OnRegisterClicked);

        // 初始化时隐藏提示 UI
        HideMessage();
    }

    // --- 注册逻辑 ---
    public void OnRegisterClicked()
    {
        string user = usernameInput.text;
        string pass = passwordInput.text;

        // 1. 判断是否为空
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            ShowMessage("请输入正确的内容");
            return;
        }

        // 2. 检查本地数据库是否已有该账号
        if (PlayerPrefs.HasKey(user))
        {
            ShowMessage("该账号已存在，请直接登录！");
            return;
        }

        // 3. 写入本地数据库 (PlayerPrefs)
        PlayerPrefs.SetString(user, pass);
        PlayerPrefs.Save(); // 强制保存到本地硬盘

        ShowMessage("注册成功");

        // 4. 清除输入框内容
        usernameInput.text = "";
        passwordInput.text = "";
    }

    // --- 登录逻辑 ---
    public void OnLoginClicked()
    {
        string user = usernameInput.text;
        string pass = passwordInput.text;

        // 1. 判断是否为空
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            ShowMessage("请输入正确的内容");
            return;
        }

        // 2. 验证账号是否存在
        if (PlayerPrefs.HasKey(user))
        {
            // 3. 验证密码是否匹配
            string savedPass = PlayerPrefs.GetString(user);
            if (savedPass == pass)
            {
                ShowMessage("登录成功，正在进入系统...");
                // 延迟 1 秒跳转，让玩家能看清提示
                Invoke(nameof(LoadNextScene), 1f);
            }
            else
            {
                ShowMessage("账号或密码不正确，请重新输入");
            }
        }
        else
        {
            // 账号不存在也提示这个，防止别人猜账号
            ShowMessage("账号或密码不正确，请重新输入");
        }
    }

    // --- 辅助方法：显示与隐藏提示 ---
    private void ShowMessage(string msg)
    {
        if (messagePanel != null) messagePanel.SetActive(true);
        if (messageText != null) messageText.text = msg;

        // 每次显示新消息时，重置 3 秒后自动隐藏的计时器
        CancelInvoke(nameof(HideMessage));
        Invoke(nameof(HideMessage), 3f);
    }

    private void HideMessage()
    {
        if (messagePanel != null) messagePanel.SetActive(false);
        if (messageText != null) messageText.text = "";
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}