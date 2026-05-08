using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class model : MonoBehaviour
{
    public Slider pos1Slider;
    public InputField pos1InputField;
    public Slider pos2Slider;
    public InputField pos2InputField;
    public Slider speedSlider;
    public InputField speedInputField;
    public Slider FlyspeedSlider;
    public InputField FlyspeedInputField;

    public Slider FlyoutspeedSlider;
    public InputField FlyoutspeedInputField;

    public Slider jinTimeSlider;
    public InputField jinTimeInputField;

    public Slider yuanTimeSlider;
    public InputField yuanTimeInputField;

    public GameObject pos1;
    public GameObject pos2;

    public hudie hudie;
    Animation Animation;
    Animator Animator;
    public pointUi pointui;

    public Transform point001;
    hudie hudie_clon;
    Animator Animator_clon;
    // Start is called before the first frame update

    public  Button Button;
    public GameObject com;

    public Transform cameraRig;
    void Start()
    {
        if (VRMain.instance == null) return;
        VRMain.instance.transform.position = cameraRig.position;
        VRMain.instance.transform.eulerAngles = cameraRig.eulerAngles;

        //QualitySettings.antiAliasing = 4;

        // Animation = hudie.transform.Find("Butterfly African Monarch LOD 0/Model").GetComponent<Animation>();
        Animator = hudie.transform.Find("Butterfly African Monarch LOD 0/Model").GetComponent<Animator>();

        try
        {
            pos1Slider.value = float.Parse(pos1InputField.text);
            SetDis(float.Parse(pos1InputField.text));
        }
        catch
        {
            pos1Slider.value = 0;
            SetDis(0);
            pos1InputField.text = "0";
        }
        try
        {
            pos2Slider.value = float.Parse(pos2InputField.text);
            SetDis2(float.Parse(pos2InputField.text));
        }
        catch {
            pos2Slider.value = 0;
            SetDis2(0);
            pos2InputField.text = "0";
        }

        try
        {
            speedSlider.value = float.Parse(speedInputField.text);
            SetAnimationSpeed(float.Parse(speedInputField.text));
        }
        catch {
            speedSlider.value = 1f;
            SetAnimationSpeed(1f);
            speedInputField.text = "1";
        }

        try
        {
            FlyspeedSlider.value = float.Parse(FlyspeedInputField.text);
            SetflySpeed(float.Parse(FlyspeedInputField.text));
        }
        catch {
            FlyspeedSlider.value = 0.1f;
            SetflySpeed(0.1f);
            FlyspeedInputField.text = "0.1";
        }
        try
        {
            FlyoutspeedSlider.value = float.Parse(FlyoutspeedInputField.text);
            SetflyoutSpeed(float.Parse(FlyoutspeedInputField.text));
        }
        catch
        {
            FlyspeedSlider.value = 0.1f;
            SetflySpeed(0.1f);
            FlyspeedInputField.text = "0.1";
        }


        try
        {
            jinTimeSlider.value = float.Parse(jinTimeInputField.text);
            hudie.jinTime = float.Parse(jinTimeInputField.text);
        }
        catch
        {
            jinTimeSlider.value = 0;
            hudie.jinTime = 0;
            jinTimeInputField.text = "0";
        }

        try
        {
            yuanTimeSlider.value = float.Parse(yuanTimeInputField.text);
            hudie.yuanTime = float.Parse(yuanTimeInputField.text);
        }
        catch {
            yuanTimeSlider.value = 0;
            hudie.yuanTime = 0;
            yuanTimeInputField.text = "0";
        }
        Button.onClick.AddListener(onClick) ;
        TextButton = Button.transform.Find("Text").GetComponent<Text>();
        //com.SetActive(false);
        TextButton.text = "打开设置";


        if (VRMain.instance != null)
        {
            if (VRMain.instance.isAllPlayer == true)
            {
                hudie.DoOver = true;
            }
            else {
                hudie.DoOver = false;
            }
        }

        new BaseButton(VRPlayer.instance.allCanvas.Find("Next")).onClick = (GameObject go) =>
        {
            ChangeScene();
        };
    }

    Text TextButton;
    void onClick()
    {
        string str = TextButton.text;
        if (str == "打开设置")
        {
            com.SetActive(true);
            TextButton.text = "关闭设置";
        }
        else {
            com.SetActive(false);
            TextButton.text = "打开设置";
        }
    }
    public CanvasGroup cg;
    public void ChangeScene()
    {
        if (VRMain.instance == null) return;

        cg =  VRMain.instance.transform.Find("NibiruXRSDK/MainCamera/Canvas/AllBack").GetComponent<CanvasGroup>();
        cg.alpha = 0;

        VRPlayer.instance.IsOpenAllGroup(cg,true, () =>
        {
            VRMain.instance.text = "不妨再近一点,和大海零距离";
            VRMain.instance.loadingText = "呀!蝴蝶要飞到哪里去呢?";
            //VRMain.instance.ChangeSence("TropicPack", "16");
            VRMain.instance.CloseScene();
            if (VRMain.instance.isLiyue == false)
            {
                VRMain.instance.ChangeSence("TropicPack", "16",false, false);
            }
            else
            {
                VRMain.instance.ChangeSence("TropicPack", "9", false, false);
            }

        });

    }
    void OnDrawGizmos()
    {
        
        Ray ray = new Ray(this.transform.position, Vector3.right);
        RaycastHit hit;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(this.transform.position, this.transform.position + Vector3.right * 100);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Default")))
        {
            Gizmos.DrawSphere(hit.point, 1);
        }
        Gizmos.DrawSphere(this.transform.position, 0.5f);

    }
    bool isOOO;

    
    //Update is called once per frame
    void Update()
    {


        ////OnDrawGizmos();
        //if (OnRay())
        //{
        //    hudie.canFly = true;
        //}
        //else
        //{
        //    hudie.canFly = false;

        //}
        //if (hudie.over == true) {

        //    if (isOOO == true) return;
        //    isOOO = true;

        //    if (VRMain.instance != null && VRMain.instance.isAllPlayer == true)
        //    {
        //        ChangeScene();
        //        return;
        //    }
        //    //VRMain.instance.sceneName = "TropicPack";
        //    //SceneManager.LoadScene(1);
        //}
        if (hudie.over == true)
        {
            if (isOOO == true) return;
            isOOO = true;

            if (VRMain.instance != null && VRMain.instance.isAllPlayer == true)
            {
                ChangeScene();
                return;
            }
        }

    }

    float fenlie = 0;

    public void Setpos1(string v)
    {
        float dis = 0;
        try
        {
            dis = float.Parse(v);    
        }
        catch {
            pos1InputField.text = dis.ToString();
        }
        pos1Slider.value = dis;
        SetDis(dis);
    }
    public void Setpos1(float  v)
    {
        pos1InputField.text = v.ToString();
    }

    public void Setpos2(string v)
    {
        float dis = 0;
        try
        {
            dis = float.Parse(v);
        }
        catch {
            pos2InputField.text = dis.ToString();
        }
        pos2Slider.value = dis;
        SetDis2(dis);
    }
    public void Setpos2(float v)
    {
        pos2InputField.text = v.ToString();
    }


    public void SetSpeed(float v)
    {
        speedInputField.text = v.ToString();
    }
    public void SetSpeed(string v)
    {
        float dis = 1;
        try
        {
            dis = float.Parse(v);
        }
        catch {
            speedInputField.text = dis.ToString();
        }
        speedSlider.value = dis;
        SetAnimationSpeed(dis);
    }

    public void SetFlySpeed(float v)
    {
        FlyspeedInputField.text = v.ToString();
    }
    public void SetFlySpeed(string v)
    {
        float dis = 0.1f;
        try
        {
            dis = float.Parse(v);
        }
        catch {
            FlyspeedInputField.text = dis.ToString();
        }
        FlyspeedSlider.value = dis;
        SetflySpeed(dis);
    }

    public void SetflySpeed(float speed) {
        hudie.speed = speed;
    }


    public void SetFlyoutSpeed(float v)
    {
        FlyoutspeedInputField.text = v.ToString();
    }
    public void SetFlyoutSpeed(string v)
    {
        float dis = 0.1f;
        try
        {
            dis = float.Parse(v);
        }
        catch
        {
            FlyoutspeedInputField.text = dis.ToString();
        }
        FlyoutspeedSlider.value = dis;
        SetflyoutSpeed(dis);
    }


    public void SetflyoutSpeed(float speed)
    {
        hudie.outspeed = speed;
    }
    public void SetjinTiem(string v)
    {
        float dis = 0;
        try
        {
            dis = float.Parse(v);
        }
        catch {
            jinTimeInputField.text = dis.ToString();
        }
        jinTimeSlider.value = dis;
        hudie.jinTime = dis;
    }

    public void SetjinTiem(float v)
    {
        jinTimeInputField.text = v.ToString();
    }

    public void SetyuanTiem(string v)
    {
        float dis = 0;
        try
        {
            dis = float.Parse(v);
        }
        catch {
            yuanTimeInputField.text= dis.ToString();
        }
        yuanTimeSlider.value = dis;
        hudie.yuanTime = dis;
    }

    public void SetyuanTiem(float v)
    {
        yuanTimeInputField.text = v.ToString();
    }

    public void SetAnimationSpeed(float speed)
    {
        //if (null == Animation)
        //{
        //    return;
        //}
        //AnimationState state = Animation["butterflap"];
        //if (!state) state.speed = speed;
        //else
        //    print(666);

        if (Animator == null ) { return; }

        Animator.speed = speed;

    }


    //Vector3 v1 = new Vector3(.03f, .03f, 1);
    //Vector3 v2 =  Vector3.one ;
    //bool OnRay()
    //{
    //    if (VRPlayer.instance.RayCamera == null) return false;
    //    Ray ray = VRPlayer.instance.RayCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
    //    RaycastHit hit;
    //    if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Default")))
    //    {
           
    //        if (hudie.show)
    //        {
    //            if (hit.transform.gameObject.name == "hudie")
    //            {
    //                pointui.SetScale(v1);
    //                pointui.Setpos(hit.point);
    //                return true;
    //            }
    //            else if (hit.transform.gameObject.name == "Sphere")
    //            {
    //                pointui.SetScale(v2);
    //                pointui.Setpos(hit.point + Vector3.forward);
    //            }
    //        }
    //        else {
    //            pointui.SetScale(Vector3.zero);
    //        }
    //    }

    //    return false;
    //}

    void SetDis(float dic)
    {
        pos1.transform.position = point001.transform.forward * dic + point001.transform.position;
        hudie.SetFrist();
    }
    void SetDis2(float dic)
    {
        pos2.transform.position = point001.transform.forward * dic + point001.transform.position;
    }

}
