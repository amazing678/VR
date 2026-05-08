using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VRGizmo : MonoBehaviour
{
    public Transform target;
    public Transform target1;

    public bool isSpace;

    public Transform targetLeft;
    public Transform targetLeftTop;
    public Transform targetLeftBottom;

    public Transform targetRight;
    public Transform targetRightTop;
    public Transform targetRightBottom;

    public void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 3);

        //if (target == null || target1 == null) return;
        //Gizmos.DrawSphere(target.position, 0.1f);
        Gizmos.DrawSphere(target1.position, 2);
        Gizmos.DrawLine(target1.position, transform.position);
        //Gizmos.DrawLine(target1.position, target.position);

        if (isSpace == true) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(targetLeft.position, 1f);
            Gizmos.DrawSphere(targetLeftTop.position, 1f);
            Gizmos.DrawSphere(targetLeftBottom.position, 1f);
            Gizmos.DrawSphere(targetRight.position, 1f);
            Gizmos.DrawSphere(targetRightTop.position, 1f);
            Gizmos.DrawSphere(targetRightBottom.position, 1f);
        }
    }

    public void Start()
    {
        Instance();


        targetLeft.GetChild(0).gameObject.SetActive(false);
        targetLeftTop.GetChild(0).gameObject.SetActive(false);
        targetLeftBottom.GetChild(0).gameObject.SetActive(false);

        targetRight.GetChild(0).gameObject.SetActive(false);
        targetRightTop.GetChild(0).gameObject.SetActive(false);
        targetRightBottom.GetChild(0).gameObject.SetActive(false);

        targetLeft.GetChild(1).gameObject.SetActive(false);
        targetLeftTop.GetChild(1).gameObject.SetActive(false);
        targetLeftBottom.GetChild(1).gameObject.SetActive(false);

        targetRight.GetChild(1).gameObject.SetActive(false);
        targetRightTop.GetChild(1).gameObject.SetActive(false);
        targetRightBottom.GetChild(1).gameObject.SetActive(false);
    }

    public GameObject obj;

    public void Instance()
    {
        VRBallonPlayer.instance.InstanceBallon(x => {
            obj = x;
        }, transform, 2, 1);
    }

    VRBallonText temp;
    float timeX = 0;
   
    public void Update() {

        if (VRPlayer.instance.RayCamera == null) return;
        Ray ray = VRPlayer.instance.RayCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("SongShu")))
        {
            return;
            temp = hit.collider.GetComponent<VRBallonText>();
            if (temp == null) return;
            temp.text.color = Color.Lerp(temp.text.color, temp.toColor, Time.deltaTime * 0.5f);

            
            if ((timeX += Time.deltaTime) > 3) {

                if (temp.isEnter == true) return;
                temp.isEnter = true;
                Destroy(temp.gameObject);
                Destroy(temp.transform.parent.Find("Cube").gameObject);

                VRBallonPlayer.instance.OpenTip(VRBallonPlayer.instance.count == 5? "已收集完成!" : "已收集，请向前看!", true, ()=> {

                    DOTween.To(()=>Vector2.zero, x=> { }, Vector2.zero, VRBallonPlayer.instance.count == 5?4:2).OnComplete(()=> {
                        
                        VRBallonPlayer.instance.OpenTip(VRBallonPlayer.instance.count == 5 ? "已收集完成!" : "已收集，请向前看!", false, () =>
                            {
                                if (VRBallonPlayer.instance.count == 5)
                                {
                                    if (VRMain.instance != null && VRMain.instance.isAllPlayer == true)
                                    {
                                        VRBallonPlayer.instance.ChangeScene();
                                        return;
                                    }
                                }

                                VRBallonPlayer.instance.count++;
                                VRBallonPlayer.instance.InstanceBallon(x => { this.obj = x; }, this.transform, 0, VRBallonPlayer.instance.count + 1);

                            });
                        
                    });

                });

            }

        }
        else
        {
            timeX = 0;
            if (temp != null) temp.text.color = temp.color;
        }

    }


}
