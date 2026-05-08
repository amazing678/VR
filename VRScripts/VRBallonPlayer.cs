using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using TMPro;

public class VRBallonPlayer : MonoBehaviour
{
    public CanvasGroup tipText;

    public Slider speedSlider;
    public Slider stayNearSlider;
    public Slider farDistanceSlider;
    public Slider nearDistanceSlider;

    public InputField speedInput;
    public InputField stayNearInput;
    public InputField farDisInput;
    public InputField nearDisInput;

    public static VRBallonPlayer instance;
    public List<VRGizmo> gizmosList;
    public int count;
    //
    public MeshRenderer render;
    public Texture2D[] cubes;

    public GameObject itemRaw;
    public Transform content;


    public Transform middlePoint;
    #region
    public void Clear() {
        if (gizmosList == null || gizmosList.Count <= 0) return;
       
        foreach (VRGizmo item in gizmosList) {
            if (item.obj != null) Destroy(item.obj);
            item.Instance();
        }

    }
    public void SetSpeed(float value) {

        speedInput.text = Math.Round(value, 3).ToString();
        Clear();
    }
    public void SetSpeedInput(String text)
    {
        float value = 2f;
        if (float.TryParse(text, out value))
        {
            if (value > 10) value = 10;
            if (value < 0) value = 0;
        }

        speedInput.text = value.ToString();
        speedSlider.value = value;
        Clear();
    }

    public void SetStayNear(float value)
    {
        stayNearInput.text = Math.Round(value, 3).ToString();
        Clear();
    }
    public void SetStayNearInput(String text)
    {
        float value = 1f;
        if (float.TryParse(text, out value))
        {
            if (value > 10) value = 10;
            if (value < 0) value = 0;
        }

        stayNearInput.text = value.ToString();
        stayNearSlider.value = value;
        Clear();
    }


    public void SetFarDistance(float value)
    {
        farDisInput.text = Math.Round(value, 3).ToString();
        Clear();
    }
    public void SetFarDistanceInput(String text)
    {
        float value = 20f;
        if (float.TryParse(text, out value))
        {
            if (value > 100) value = 100;
            if (value < 0) value = 0;
        }

        farDisInput.text = value.ToString();
        farDistanceSlider.value = value;
        Clear();
    }


    public void SetNearDistance(float value)
    {
        nearDisInput.text = Math.Round(value, 3).ToString();
        Clear();
    }
    public void SetNearDistanceInput(String text)
    {
        float value = 0.5f;
        if (float.TryParse(text, out value))
        {
            if (value > 100) value = 100;
            if (value < 0) value = 0;
        }

        nearDisInput.text = value.ToString();
        nearDistanceSlider.value = value;
        Clear();
    }
    #endregion


    public void OpenTip(String text,bool isOpen, Action action = null) {

        tipText.GetComponentInChildren<TextMeshProUGUI>().text = text;
        DOTween.To(()=> tipText.alpha, x=> { tipText.alpha = x; }, isOpen? 1:0, 0.5f).OnComplete(()=>{
            action?.Invoke();
        });

    }
    public Transform cameraRig;
    public MeshRenderer render01;

    public void Awake() {
        instance = this; 

    }
    // Start is called before the first frame update
    void Start()
    {
        VRPlayer.instance.isClosePoint = true;

        if (VRMain.instance == null) return;
        VRMain.instance.transform.position = cameraRig.position;
        VRMain.instance.transform.eulerAngles = cameraRig.eulerAngles;

        tipText = VRMain.instance.transform.Find("NibiruXRSDK/MainCamera/Canvas2/Image").GetComponent<CanvasGroup>();

        if (VRMain.instance != null)
        {
            if (VRMain.instance.isClip1 == true)
            {
                render01.material.color = new Color(0.84f, 0.84f, 0.84f);
                RenderSettings.ambientLight = new Color(0.84f, 0.84f, 0.84f);
            }
            else
            {
                render01.material.color = Color.white;
                RenderSettings.ambientLight = Color.white;
            }
        }




        count = 0;
        //QualitySettings.antiAliasing = 4;
        new BaseButton(VRPlayer.instance.allCanvas.Find("Next")).onClick = (GameObject go) =>
        {
            ChangeScene();
        };

        if (cubes != null || cubes.Length > 0)
        {
            foreach (Texture2D item in cubes)
            {
                Texture2D objTex = item;
                GameObject obj = Instantiate(itemRaw, content);
                obj.SetActive(true);
                obj.GetComponent<RawImage>().texture = item;
                obj.transform.localScale = Vector3.one;
                new BaseButton(obj.transform).onClick = (GameObject go) =>
                {
                    render.material.mainTexture = objTex;
                };
            }
        }
    }

    public void ChangeScene()
    {
        if (VRMain.instance == null) return;

        VRPlayer.instance.IsOpenAllGroup1(true, false, () =>
        {
            VRMain.instance.loadingText = "乘着热气球去看看大好河山吧";
            VRMain.instance.text = "寻找乒乓球上的文字";
            VRMain.instance.CloseScene();
            VRMain.instance.ChangeSence("Ball_School", "6");//找齐气球之后跳转到球场景
        });
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void InstanceBallon(Action<GameObject> action, Transform point, float time, int i)
    {
        DOTween.To(()=> Vector2.zero , x=> { }, Vector2.one , time).OnComplete(()=> {
            //计算应该生成的点
            Vector3 disPoint =
                (point.position - VRPlayer.instance.testGame.position).normalized * VRBallonPlayer.instance.farDistanceSlider.value + VRPlayer.instance.testGame.position;

            VRBallon temp = Instantiate(VRPlayer.instance.ballon, disPoint, point.rotation).GetComponent<VRBallon>();
            temp.gim = point.GetComponent<VRGizmo>();
            temp.OpenObj(i);
            action(temp.gameObject);
        });

    }
}
