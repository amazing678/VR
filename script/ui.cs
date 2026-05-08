using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ui : MonoBehaviour
{
    [DllImport("user32.dll")]
    static extern bool ShowWindow(System.IntPtr hwnd, int nCmdShow);
    [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
    static extern System.IntPtr GetForegroundWindow();
    const int SW_SHOWMINIMIZED = 2;//(最小化窗口)


    CanvasGroup group;
    Transform Gamecom;
    Transform Vidiocom; ScrollRect Gamerect; ScrollRect Vidiorect;

    CanvasGroup close;
    CanvasGroup select;

    GameObject clip01;
    GameObject clip02;

    void Start()
    {

        Gamecom = this.transform.Find("game/Viewport/Content");
        Vidiocom = this.transform.Find("vidio/Viewport/Content"); SetItem();
        Gamerect = this.transform.Find("game").GetComponent<ScrollRect>();
        Vidiorect = this.transform.Find("vidio").GetComponent<ScrollRect>();
        group = this.transform.Find("Image").GetComponent<CanvasGroup>();
        close = this.transform.Find("Close").GetComponent<CanvasGroup>();
        select = this.transform.Find("Select").GetComponent<CanvasGroup>();
        clip01 = select.transform.Find("Clip01/Clip/1").gameObject;
        clip02 = select.transform.Find("Clip02/Clip/1").gameObject;


        clip01.SetActive(VRMain.instance.isClip1 == true);
        clip02.SetActive(VRMain.instance.isClip1 == false);

        new BaseButton(select.transform.Find("Clip02/Image")).onClick = (GameObject go) => {
            clip01.SetActive(false);
            clip02.SetActive(true);
            if (VRMain.instance != null) {
                VRMain.instance.isClip1 = false;
            }
        };
        new BaseButton(select.transform.Find("Clip01/Image")).onClick = (GameObject go) => {

            clip01.SetActive(true);
            clip02.SetActive(false);
            if (VRMain.instance != null){
                VRMain.instance.isClip1 = true;
            }
        };

        new BaseButton(this.transform.Find("btn/1")).onClick = (GameObject go) => {


            float temp = 0;
            select.blocksRaycasts = true;
            DOTween.To(() => temp, x => select.alpha = x, 1, 0.2f);
            select.transform.DOScale(Vector3.one, 0.2f);

            //OpenGroup(() =>
            //{
            //    VRMain.instance.isAllPlayer = true;

            //    VRMain.instance.text = "组装机器人";
            //    VRMain.instance?.ChangeSence("ForestScene_Fixed", "13", true);

            //});

        };
        new BaseButton(this.transform.Find("2")).onClick = (GameObject go) => {
            ShowWindow(GetForegroundWindow(), SW_SHOWMINIMIZED);
        };
        new BaseButton(this.transform.Find("3")).onClick = (GameObject go) => {

            float temp = 0;
            close.blocksRaycasts = true;
            DOTween.To(() => temp, x => close.alpha = x, 1, 0.2f);
            close.transform.DOScale(Vector3.one, 0.2f);

        };
        new BaseButton(close.transform.Find("Close")).onClick = (GameObject go) => {

            float temp = 1;
            close.blocksRaycasts = false;
            DOTween.To(() => temp, x => close.alpha = x, 0, 0.2f);
            close.transform.DOScale(Vector3.one * 0.4f, 0.2f);
        };
        new BaseButton(close.transform.Find("Quit/Image")).onClick = (GameObject go) => {
            print("Quit");
            Application.Quit();
        };

        //select
        new BaseButton(select.transform.Find("Close")).onClick = (GameObject go) => {

            float temp = 1;
            select.blocksRaycasts = false;
            DOTween.To(() => temp, x => select.alpha = x, 0, 0.2f);
            select.transform.DOScale(Vector3.one * 0.4f, 0.2f);
        };
        new BaseButton(select.transform.Find("T01")).onClick = (GameObject go) => {
           
            OpenGroup(() =>
            {
                VRMain.instance.isAllPlayer = true;
                VRMain.instance.isLiyue = true;
                VRMain.instance.text = "组装机器人";
                VRMain.instance?.ChangeSence("ForestScene_Fixed", "13", true);
            });
        };
        new BaseButton(select.transform.Find("T02")).onClick = (GameObject go) => {
            
            OpenGroup(() =>
            {
                VRMain.instance.isAllPlayer = true;
                VRMain.instance.isLiyue = false;
                VRMain.instance.text = "组装机器人";
                VRMain.instance?.ChangeSence("ForestScene_Fixed", "13", true);
            });
        };
    }

    public void OpenGroup(Action action) {
        float temp = 0;
        group.blocksRaycasts = true;
        DOTween.To(()=> temp,  x=> { group.alpha = x; } , 1, 0.5f).OnComplete(()=> {
            action();
        });
    }

    void SetItem()
    {
        for (int i=0; i< Gamecom.childCount;i++)
        {
            GameObject clon = Gamecom.GetChild(i).gameObject;

            EventTriggerListener.Get(clon).newOnDrag = (PointerEventData v) => { Gamerect.OnDrag(v); };
            EventTriggerListener.Get(clon).newInPoDrag = (PointerEventData v) => { Gamerect.OnInitializePotentialDrag(v); };
            EventTriggerListener.Get(clon).newEndDrag = (PointerEventData v) => { Gamerect.OnEndDrag(v); };
            EventTriggerListener.Get(clon).newOnBeingDrag = (PointerEventData v) => { Gamerect.OnBeginDrag(v); };
            EventTriggerListener.Get(clon).newOnScroll = (PointerEventData v) => { Gamerect.OnScroll(v); };
            EventTriggerListener.Get(clon).onEnter = (GameObject go) => {
                clon.transform.Find("text").gameObject.SetActive(true);
                clon.transform.DOScale(new Vector3(1.1f, 1.1f, 1), 0.2f);
                float value = 0.5f;
                DOTween.To(() => value, x => { clon.transform.Find("text").GetComponent<CanvasGroup>().alpha = x; }, 1, 0.2f);
            };
            EventTriggerListener.Get(clon).onExit = (GameObject go) => {
                clon.transform.DOScale(Vector3.one, 0.2f);
                clon.transform.Find("text").gameObject.SetActive(false);
                float value = 1f;
                DOTween.To(() => value, x => { clon.transform.Find("text").GetComponent<CanvasGroup>().alpha = x; }, 0, 0.2f);
            };


            //--------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------
            Vector2 temp1 = Vector2.zero;
            EventTriggerListener.Get(clon).onDown = (GameObject go, Vector2 v) =>
            {
                temp1 = v;
            };
            EventTriggerListener.Get(clon).onUp = (GameObject go, Vector2 v) => {

                if (Vector2.Distance(v, temp1) > 0.1f) return;

                OpenGroup(()=> { 

                    if (go.name == "1")
                    {
                        VRMain.instance.isAllPlayer = false;
                        SceneManager.LoadScene("ForestScene_Fixed");
                    }
                    else if (go.name == "2")
                    {
                        VRMain.instance.isAllPlayer = false;
                        SceneManager.LoadScene("Squirrel");
                    }
                    else if (go.name == "3")
                    {
                        VRMain.instance.isAllPlayer = false;
                        SceneManager.LoadScene("Hudie");
                    }
                    else if (go.name == "3_1")
                    {
                        VRMain.instance.isAllPlayer = false;
                        SceneManager.LoadScene("Hudie_School");
                    }
                    else if (go.name == "4")
                    {
                        VRMain.instance.isAllPlayer = false;
                        SceneManager.LoadScene("TropicPack");
                    }
                    else if (go.name == "5")
                    {
                        VRMain.instance.isAllPlayer = false;
                        SceneManager.LoadScene("Balloons");
                    }
                    else if (go.name == "6")
                    {
                        VRMain.instance.isAllPlayer = false;
                        SceneManager.LoadScene("Ball_School");
                    }
                });
            };
            //--------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------
        }

        for (int i = 0; i < Vidiocom.childCount; i++)
        {
            GameObject clon = Vidiocom.GetChild(i).gameObject;
            EventTriggerListener.Get(clon).newOnDrag = (PointerEventData v) => { Vidiorect.OnDrag(v); };
            EventTriggerListener.Get(clon).newInPoDrag = (PointerEventData v) => { Vidiorect.OnInitializePotentialDrag(v); };
            EventTriggerListener.Get(clon).newEndDrag = (PointerEventData v) => { Vidiorect.OnEndDrag(v); };
            EventTriggerListener.Get(clon).newOnBeingDrag = (PointerEventData v) => { Vidiorect.OnBeginDrag(v); };
            EventTriggerListener.Get(clon).newOnScroll = (PointerEventData v) => { Vidiorect.OnScroll(v); };

            EventTriggerListener.Get(clon).onEnter = (GameObject go) => {
                //clon.transform.Find("text").gameObject.SetActive(true);
                clon.transform.DOScale(new Vector3(1.1f, 1.1f, 1), 0.2f);
                float value = 0.5f;
                DOTween.To(() => value, x => { clon.transform.Find("text").GetComponent<CanvasGroup>().alpha = x; }, 1, 0.2f);
            };
            EventTriggerListener.Get(clon).onExit = (GameObject go) => {
                clon.transform.DOScale(Vector3.one, 0.2f);
                //clon.transform.Find("text").gameObject.SetActive(false);
                float value = 1f;
                DOTween.To(() => value, x => { clon.transform.Find("text").GetComponent<CanvasGroup>().alpha = x; }, 0, 0.2f);
            };

            //--------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------
            new BaseButton(clon.transform).onClick = (GameObject go) => {
               // print(clon.name);
            };

            Vector2 temp1 = Vector2.zero;
            EventTriggerListener.Get(clon).onDown = (GameObject go, Vector2 v) =>
            {
                temp1 = v;
            };
            EventTriggerListener.Get(clon).onUp = (GameObject go, Vector2 v) => {

                if (Vector2.Distance(v, temp1) > 0.1f) return;

                OpenGroup(() => {

                    VRMain.instance.isAllPlayer = false;
                    VRMain.instance.videoName = go.name;
                    SceneManager.LoadScene("360SphereVideo");

                });
            };
            //--------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------

        }



    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
