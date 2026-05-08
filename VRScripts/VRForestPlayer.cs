using DG.Tweening;
using SWS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using Valve.VR;

public class VRForestPlayer : MonoBehaviour
{
    

    public Slider speedSlider;
    public InputField speedInput;
    public Slider disSlider;
    public InputField disInput;
    splineMove splineMove;
    public bool isPin;

    public bool isFixed;

    public Transform[] points;
    public CanvasGroup back;

    public GameObject[] games;
    public Transform cameraRig;
    public static VRForestPlayer instance;

    void Awake() {

        if (VRMain.instance == null) return;

        instance = this;
        splineMove = GetComponent<splineMove>();

        back = VRMain.instance.transform.Find("XR Origin (XR Rig)/Camera Offset/Main Camera/Canvas/Back").GetComponent<CanvasGroup>();

        VRMain.instance.transform.position = cameraRig.position;
        VRMain.instance.transform.eulerAngles = cameraRig.eulerAngles;

        bool isHiddle = true;
        if (VRMain.instance != null) isHiddle = !VRMain.instance.isAllPlayer;
        if (games != null && games.Length > 0)
        {
            for (int i = 0; i < games.Length; i++)
            {
                games[i].SetActive(isHiddle);
            }
        }


    }


    public void ChangeScene() {

        if (VRMain.instance == null) return;

        VRPlayer.instance.IsOpenAllGroup1(true, false, ()=> {

            VRMain.instance.CloseScene();
            VRMain.instance.text = "观察小松鼠";
            VRMain.instance.loadingText = "跟着机器人去看看山水吧";
            VRMain.instance?.ChangeSence("Squirrel", "15");//完成后跳转松鼠场景


        });

        

    }

    public int i;
    IEnumerator Start() {

        
        new BaseButton(VRPlayer.instance.allCanvas.Find("Next")).onClick = (GameObject go) =>
        {
            ChangeScene();
        };

        new BaseButton(VRPlayer.instance.allCanvas.Find("Auto")).onClick = (GameObject go) => {

            if(VRMain.instance != null) VRMain.instance.isAllPlayer = false;
            SceneManager.LoadScene("ForestScene_Auto");  
        };
        new BaseButton(VRPlayer.instance.allCanvas.Find("HalfAuto")).onClick = (GameObject go) => {
            if (VRMain.instance != null) VRMain.instance.isAllPlayer = false;
            SceneManager.LoadScene("ForestScene_Half_Auto");
        };
        new BaseButton(VRPlayer.instance.allCanvas.Find("Fixed")).onClick = (GameObject go) => {
            if (VRMain.instance != null) VRMain.instance.isAllPlayer = false;
            SceneManager.LoadScene("ForestScene_Fixed");
        };

        //transform.position = points[4].position;
        //transform.rotation = points[4].rotation;
        //VRPlayer.instance.OpenMedium();
        //yield break;


        if (isFixed)
        {
            transform.position = points[0].position;
            transform.rotation = points[0].rotation;

            VRMain.instance.transform.position = cameraRig.position;
            VRMain.instance.transform.eulerAngles = cameraRig.eulerAngles;
        }
        i++;
        yield return new WaitForSeconds(1);

        if (isFixed == true)
        {
            VRPlayer.instance.InsBaoXiang();
            VRPlayer.instance.AnimRun2();
        }
    }

    public void OpenBack() {

        if (i >= points.Length)
        {
            //说明已经收集完成了
            VRPlayer.instance.OpenMedium();
            return;
        }



        
        float value = 0;
        DOTween.To(()=> value, x=> back.alpha = x , 1, 1f).OnComplete(()=> {

            transform.position = points[i].position;
            transform.rotation = points[i].rotation;
            i++;

            VRMain.instance.transform.position = cameraRig.position;
            VRMain.instance.transform.eulerAngles = cameraRig.eulerAngles;

            float x2 = 1;
            DOTween.To(() => x2, x1 => back.alpha = x1, 0, 1f).OnComplete(() => {
                VRPlayer.instance.AnimRun2();
                VRPlayer.instance.InsBaoXiang();

            });

        });
    }

    public void SetSpeed(float value)
    {
        speedInput.text = Math.Round(value, 3).ToString();
    }
    public void SetSpeedInput(String text)
    {
        float value = 1.5f;
        if (float.TryParse(text, out value))
        {
            if (value > 3) value = 3;
            if (value < 0) value = 0;
        }

        speedInput.text = value.ToString();
        speedSlider.value = value;
    }

    
    public void SetDistance(float value)
    {
        disInput.text = Math.Round(value, 3).ToString();
    }
    public void SetDistanceInput(String text)
    {
        float value = 0.25f;
        if (float.TryParse(text, out value))
        {
            if (value > 3) value = 3;
            if (value < 0) value = 0;
        }

        disInput.text = value.ToString();
        disSlider.value = value;
    }

    void Update()
    {
        if(splineMove != null)
            splineMove.ChangeSpeed(speedSlider.value);
    }
}
