using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class songshu : MonoBehaviour
{
    public point point;
    Animator Animator;
    void Start()
    {
        Animator = this.transform.Find("songshu").GetComponent<Animator>();
    }

    void setAni(bool _run)
    {
        Animator.SetBool("stay", _run); stay = _run;
    }
    int run = 0;
    bool stay = false;
    AnimatorStateInfo animatorInfo;
    public  bool towalk = true;
    void Update()
    {
        //if (towalk ==false && Input .GetKeyDown(KeyCode.A))
        //{
        //    towalk = true; 
        //}

        //if (point != null && towalk)
        //{
        //    this.transform.LookAt(point.transform);
        //    this.transform.Translate(new Vector3(0,0,.1f));
        //    if (stay)
        //    {
        //        setAni(false); towalk = true;
        //    }
        //    if (Vector3.Distance(this.transform .position,point.transform .position ) <= 0.05f) {

        //        if (point.stay ==true)
        //        {
        //            if (stay == false )
        //            {
        //                setAni(true); towalk = false;
        //            }
        //        }
        //        point = point.next;
        //    }
        //}

        if (towalk)
        {

            this.transform.position += this.transform.forward * 1f * Time.deltaTime;
            this.transform.rotation = Quaternion.Slerp(
                      this.transform.rotation,
                      Quaternion.LookRotation(point.transform.position - this.transform.position),
                      3f * Time.deltaTime);

            if (Vector3.Distance(this.transform.position, point.transform.position) <= 0.05f)
            {
                if (point.stay == true)
                {
                    if (stay == false)
                    {
                        setAni(true); towalk = false;
                    }
                }
                point = point.next;
            }
        }
        else {
            animatorInfo = Animator.GetCurrentAnimatorStateInfo(0);
            if ((animatorInfo.normalizedTime >= 1.0f) && (animatorInfo.IsName("Stay")))
            {
                setAni(false); towalk = true;
            }
        }
    }
}
