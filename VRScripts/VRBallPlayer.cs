using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VRBallPlayer : MonoBehaviour
{
    public CanvasGroup group;
    public Transform black;

    public Transform cameraRig;

    public static VRBallPlayer instance;
    // Start is called before the first frame update
    void Start()
    {
        if (VRMain.instance == null) return;
        VRMain.instance.transform.position = cameraRig.position;
        VRMain.instance.transform.eulerAngles = cameraRig.eulerAngles;

        instance = this;
        Collider[] colliders = group.GetComponentsInChildren<Collider>();
        foreach (Collider item in colliders)
            item.enabled = false;

        VRPlayer.instance.isClosePoint = true;
        VRPlayer.instance.isOpenHudiePoint = true;
    }

    bool isOpen;
    public void OpenAll() {

        tet temp = ballmodel.instance.curtet;
        group.transform.Find("0").GetComponent<TextMeshProUGUI>().text = temp.txt[0].ToString();
        group.transform.Find("1").GetComponent<TextMeshProUGUI>().text = temp.txt[1].ToString();
        group.transform.Find("2").GetComponent<TextMeshProUGUI>().text = temp.txt[2].ToString();
        group.transform.Find("3").GetComponent<TextMeshProUGUI>().text = temp.txt[3].ToString();
        group.transform.Find("4").GetComponent<TextMeshProUGUI>().text = temp.txt[4].ToString();
        group.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = temp.value + ":";

        black.DOMove(new Vector3(1.11f, -2.22f, 0.11f), 2).SetEase(Ease.OutQuint).OnComplete(()=> {
            DOTween.To(() => group.alpha, x => group.alpha = x, 1, 1).OnComplete(() => {
                
                DOTween.To(() => Vector3.zero, x => { }, Vector3.zero, 1).OnComplete(()=> {
                
                    VRPlayer.instance.isClosePoint = false;
                    isOpen = true;
                    Collider[] colliders = group.GetComponentsInChildren<Collider>();
                    foreach (Collider item in colliders)
                        item.enabled = true;
                });
            });
        });

       
    }

    Vector3 dir;
    VRBallItem temp;

    int ij;
    bool isOver;
    // Update is called once per frame
    void Update()
    {
        if (ij >= 5)
        {
            if (isOver == true) return;
            isOver = true;
            VRPlayer.instance.isClosePoint = true;

            DOTween.To(() => Vector3.zero, x =>{}, Vector3.zero, 2).OnComplete(() =>
            {
                if (VRMain.instance != null && VRMain.instance.isAllPlayer == true)
                {
                    ball.instance.ChangeScene();
                }
            });

            return;
        }


        if (isOpen == false) return;

        if (VRPlayer.instance.RayCamera == null) return;
        Ray ray1 = VRPlayer.instance.RayCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        Debug.DrawRay(ray1.origin, ray1.direction * 1000, Color.red);
        RaycastHit hit1;
        if (Physics.Raycast(ray1, out hit1, Mathf.Infinity, 1 << LayerMask.NameToLayer("Cube")))
        {
            dir = hit1.point;
        }

        if (temp != null)
        {
            temp.transform.position = dir;
            //print(Vector3.Distance(temp.transform.position, temp.transform.parent.parent.position));
            if (Vector3.Distance(temp.transform.position, temp.toRegion.position) < 0.5f)
            {
                temp.transform.position = temp.toRegion.position;
                temp.transform.rotation = temp.toRegion.rotation;
                Destroy(temp.GetComponent<Collider>());
                temp.thisText.color = temp.textColor;
                temp.line.color = temp.textColor;
                temp = null;
                ij++;
            }
            return;
        }
        else
        {
            Ray ray = VRPlayer.instance.RayCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("TT")))
            {
                temp = hit.collider.gameObject.GetComponent<VRBallItem>();
                temp.thisText.color = Color.red;
                temp.line.color = Color.red;
            }
        }
    }
}
