using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hudie : MonoBehaviour
{
    public GameObject pos1;
    public GameObject pos2;
    public GameObject pos3;
    public GameObject pos4;
    public GameObject pos5;
    public GameObject pos6;
    public GameObject pos7;
    public GameObject fenpos;

    public Renderer render;
    public Texture2D hudie1;
    public Texture2D hudie2;

    public int cishu = 0;

    public GameObject pos;

    public float jinTime = 3;
    public float yuanTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        pos = pos1; Init_Rotate();
    }


    public void init(GameObject _pos1, GameObject _pos2)
    {
        pos1 = _pos1;
        pos2 = _pos2;
        pos = pos1;
        Init_Rotate();
    }


    public void SetFrist()
    {
        pos = pos1; Init_Rotate(); over = false; cishu = 0;
        this.transform.position = pos.transform.position;
    }
    private Quaternion raw_rotation;
    // 准备面向的角度
    private Quaternion lookat_rotation;
    // 转身速度(每秒能转多少度)  
    private float per_second_rotate = 1080.0f;
    // 旋转角度越大, lerp变化速度就应该越慢 
    float lerp_speed = 0.0f;
    // lerp的动态参数
    float lerp_tm = 0.0f;
    void Init_Rotate()
    {
        // 记录转身前的角度
        raw_rotation = this.transform.rotation;
        // 记录目标角度
        this.transform.LookAt(pos.transform);
        lookat_rotation = this.transform.rotation;
        // 还原当前角度
        this.transform.rotation = raw_rotation;
        // 计算旋转角度
        float rotate_angle = Quaternion.Angle(raw_rotation, lookat_rotation);
        // 获得lerp速度
        lerp_speed = per_second_rotate / rotate_angle;
        //Debug.Log("Angle:" + rotate_angle.ToString() + " speed:" + lerp_speed.ToString());
        lerp_tm = 0.0f;
    }


    bool torotate = true;
    void Rotate_Func()
    {
        lerp_tm += Time.deltaTime * lerp_speed*0.8f;
        this.transform.rotation = Quaternion.Lerp(raw_rotation, lookat_rotation, lerp_tm);
        if (lerp_tm >= 1)
        {
            this.transform.rotation = lookat_rotation;
            torotate = false;
        }
    }


    public float dic = 1;

    public float speed = 0.1f;
    public float outspeed = 0.1f;

    public bool over = false;
    public bool DoOver = false;
    bool wait = false;
    void changePos()
    {
        if (pos == pos1 || pos == pos4|| pos == pos5|| pos == pos6|| pos == pos7)
        {
            pos = pos2;
            doTime = true;
            dotime = yuanTime;
            wait = true;
             cishu++;
        }
        else if (pos == pos2)
        {
            if (cishu == 1) { pos = pos4; }
            else if (cishu == 2) { pos = pos5; }
            else if (cishu == 3) { pos = pos6; }
            else if (cishu == 4) { pos = pos7; }
            doTime = true;
            dotime = jinTime;
            if (cishu >= 5 )
            {
                if (DoOver)
                {
                    pos = pos3;
                }
                else {
                    pos = pos1;
                    cishu = 0;
                }
            }
        }
        else {
            over = true;
        }

        Init_Rotate();
        torotate = true;
    }

    bool doTime = false;
    float dotime = 0;

    public BoxCollider boxCollider;
    public bool canFly = false;
    public bool show = false;

    public void OnRayHoverEnter()
    {
        // 射线指上来，允许飞行！
        canFly = true;
    }

    // ==========================================
    // 【XRI 事件】当手柄射线移开时
    // ==========================================
    public void OnRayHoverExit()
    {
        // 射线移开，停止飞行，原地扇翅膀等待
        canFly = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (doTime)
        {
            if (wait)
            {
                show = true;
                if (canFly)
                {
                    dotime -= Time.deltaTime;
                    render.material.mainTexture = hudie1;
                }
                else
                {
                    dotime = yuanTime;
                    render.material.mainTexture = hudie2;
                }
            }
            else
            {
                dotime -= Time.deltaTime;
            }
            if (dotime<=0)
            {
                doTime = false; wait = false;
                render.material.mainTexture = hudie2;
            }
            return;
        }

        show = false;
        if (torotate) { Rotate_Func(); }
        else
        {
            this.transform.LookAt(pos.transform);
            if (pos == pos2)
            {
                this.transform.position += this.transform.forward * speed * Time.deltaTime;
            }
            else if (pos == pos3)
            {
                boxCollider.enabled = true;
                VRPlayer.instance.isOpenHudiePoint = true;
                //if (canFly)
                //{
                    this.transform.position += this.transform.forward * outspeed * Time.deltaTime;
                //    render.material.mainTexture = hudie1;
               // }
               // else
               // {
               //     render.material.mainTexture = hudie2;
               // }
            }
            else
            {
                this.transform.position += this.transform.forward * speed * 8 * Time.deltaTime;
            }
        }
        if (Vector3.Distance(this.transform.position, pos.transform.position) <= 0.05f)
        {
            changePos();
        }
    }
}
