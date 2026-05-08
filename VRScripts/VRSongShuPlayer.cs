using SWS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VRSongShuPlayer : MonoBehaviour
{
    public BezierPathManager test001;
    public BezierPathManager test002;
    public BezierPathManager test003;

    public GameObject songShu1;
    public GameObject songShu2;
    public GameObject songShu3;

    public Transform point01;
    public Transform point02;
    public Transform point03;

    public static VRSongShuPlayer instance;


    public Slider slider;
    public InputField sliderInput;
    public Transform disPoint;

    public int count;
    public int testCount = 0;
    public Transform cameraRig;
    float a;
    // Start is called before the first frame update
    void Awake() {
        instance = this;
        if (VRMain.instance == null) return;
        VRMain.instance.transform.position = cameraRig.position;
        VRMain.instance.transform.eulerAngles = cameraRig.eulerAngles;
    }
    void Start()
    {


        count = 0;
        testCount = 0;
        //QualitySettings.antiAliasing = 4;
        new BaseButton(VRPlayer.instance.allCanvas.Find("Next")).onClick = (GameObject go) =>
        {
            ChangeScene();
        };

        InstancePoint(0);

        a = Vector3.Distance(disPoint.position, transform.position) / 5;
    }
    
    public void ChangeScene() {

        if (VRMain.instance == null) return;

        VRPlayer.instance.IsOpenAllGroup1(true, false, () =>
        {
            VRMain.instance.text = "发现蝴蝶";
            VRMain.instance.loadingText = "咦?小松鼠们去哪了呢?";
            VRMain.instance.CloseScene();

            VRMain.instance.ChangeSence("Hudie_School", "14");//找到5只松鼠跳转到蝴蝶场景
            
        });

    }

    public void SetSpeed(float value)
    {
        sliderInput.text = Math.Round(value, 3).ToString();
    }
    public void SetSpeedInput(String text)
    {
        float value = 5f;
        if (float.TryParse(text, out value))
        {
            if (value > 20) value = 20;
            if (value < 1) value = 1;
        }

        sliderInput.text = value.ToString();
        slider.value = value;
    }


    // Update is called once per frame
    void Update()
    {

        float distance = slider.value * a;
        Vector3 point = (transform.position - disPoint.position).normalized * distance + disPoint.position;
        transform.position = point;

    }
    public void InstancePoint(int i)
    {

        if (i == 0) InstanceShongShu(songShu1, test001, point01);
        else if (i == 1) InstanceShongShu(songShu2, test002, point02);
        else InstanceShongShu(songShu3, test003, point03);

    }
    
    
    public void InstanceShongShu(GameObject songShu, BezierPathManager bezier,  Transform point)
    {
        VRShongShu test = Instantiate(songShu, point.position, point.rotation).GetComponent<VRShongShu>();
        test.spline.pathContainer = bezier;

        int j = 0;
        for (int i = 0; i < bezier.pathPoints.Length; i++)
        {
            UnityEvent temp = new UnityEvent();
            temp.AddListener(() => {

                if (j == 4)
                {
                    test.Stay();
                }
                else if (j == 9)
                {
                    test.Close();
                }
                j++;
            });
            test.spline.events.Add(temp);

        }
    }
}
