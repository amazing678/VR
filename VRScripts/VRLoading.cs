using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VRLoading:MonoBehaviour
{
    public Image slider;
    public Transform car;

    AsyncOperation async;
    float targetValue;

    bool isOpen;
    public void Update()
    {
        if (async == null) return;
        //targetValue = async.progress;
        //值最大为0.9
        // if (async.progress >= 0.9f) targetValue = 1.0f;
        //if (targetValue != slider.fillAmount)
        //{
        //     slider.fillAmount = Mathf.Lerp(slider.fillAmount, targetValue, Time.deltaTime);
        //}
        //if (async.progress <= 1)
        //{
        //    if (isOpen == true) return;
        //    isOpen = true;
        //    //允许异步加载完毕后自动切换场景
        //    VRPlayer.instance.IsOpenAllGroup1(true, false, ()=> {
        //        async.allowSceneActivation = true;

        //        if (iii == true)
        //        {
        //            if (VRMain.instance != null) VRMain.instance.OpenScene(VRMain.instance.text);
        //        }
        //    });
            
        //}
    }


    bool iii;
    public TextMeshPro text;
    public CanvasGroup startGroup;
    public IEnumerator Start()
    {
        VRMain.instance.transform.position = Vector3.zero;
        VRMain.instance.transform.eulerAngles = Vector3.zero;

        if (VRMain.instance.isStart == true)
        {
            VRMain.instance.isStart = false;
            yield return new WaitForSeconds(6);
            startGroup.alpha = 1;
            DOTween.To(()=> startGroup.alpha, x=> {
                startGroup.alpha = x;
            }, 0, 1f).OnComplete(()=> {

                StartAdapter();
            });
        }
        else {
            startGroup.alpha = 0;
            yield return null;
            StartAdapter();
        }
        
    }
    public void StartAdapter() {

        iii = false;
        slider.fillAmount = 0;
        DOTween.To(() => slider.fillAmount, x => {
            slider.fillAmount = x;
        }, 1, 5);

        car.transform.DOLocalMoveX(231, 5);


        if (VRMain.instance?.isOutVideo == true)
        {
            text.text = VRMain.instance.text;
            StartCoroutine(LoadScene(VRMain.instance?.sceneName));
            iii = true;
            return;
        }


        if (VRMain.instance?.isLookVideo == true)
        {
            text.text = VRMain.instance.text;
            StartCoroutine(LoadScene(VRMain.instance?.sceneName));
            iii = true;
        }
        else
        {
            text.text = VRMain.instance.loadingText;
            StartCoroutine(LoadScene("360SphereVideo"));
        }
    }


    public IEnumerator LoadScene(String sceneName) {

        yield return new WaitForSeconds(5);
        if (sceneName == null) yield break;

        //async = SceneManager.LoadSceneAsync(sceneName);
        VRPlayer.instance.IsOpenAllGroup1(true, false, () => {
            //async.allowSceneActivation = true;

            if (iii == true)
            {
                if (VRMain.instance != null) VRMain.instance.OpenScene(VRMain.instance.text);
            }

            SceneManager.LoadScene(sceneName);
            
        });

        

        //阻止当加载完成自动切换

        //async.allowSceneActivation = false;
        //读取完毕后返回，系统会自动进入C场景

        //yield return async;

        //Invoke("Close", 0.1f);
    }

}
