using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ball : MonoBehaviour
{
    public GameObject pos1;
    public GameObject pos2; public GameObject pos3;
    public int cishu = 0;
    public GameObject pos;
    public float jinTime = 3;
    public float yuanTime = 0;

    public Image image;
    public ballmodel ballmodel;
    public static ball instance;

    TextMesh textMesh;
    void Start()
    {
        textMesh = transform.Find("Ball/New Text").GetComponent<TextMesh>();
        instance = this;
        //QualitySettings.antiAliasing = 4;
        pos = pos1; SetFrist();
        new BaseButton(VRPlayer.instance.allCanvas.Find("Next")).onClick = (GameObject go) =>
        {
            ChangeScene();
        };
    }

    public void ChangeScene()
    {
        VRMain.instance.CloseScene();
        VRPlayer.instance.IsOpenAllGroup1(true, false, () =>
        {
            //------------
            SceneManager.LoadScene("Over");
            //Application.Quit();
            //VRMain.instance.ToMain();
        });
    }

    public void init(GameObject _pos1, GameObject _pos2)
    {
        pos1 = _pos1;
        pos2 = _pos2;
        pos = pos1;

    }

    
    public void SetText(int ids)
    {
        transform.Find("Ball/Text (TMP)").GetComponent<TextMeshProUGUI>().text = ballmodel.GetText(ids);
        //textMesh.text = ballmodel.GetText(ids);
    } 

    public void SetFrist()
    {
        pos = pos1; 
        this.transform.position = pos.transform.position;
        curV = 1.5f; cishu = 0; ci = 0;
    }
    bool doTime = false;
    float dotime = 0;
    public float dic = 1;

    public float speed = 0.5f;

    int jj = 0;
    void changePos()
    {

        if (ci >= 5)
        {
            //ballmodel.SetTextlist();
            //cishu = 0; ci = 0; curV = 1.5f;
            doTime = true;
            dotime = yuanTime;
        }
        //SetText(ci);
        if (pos == pos1)
        {
            pos = pos2;
            doTime = true;
            dotime = yuanTime;
            cishu++;  jj++;
            if (cishu == 5) {
                curV = 0f;
            }
            if (cishu == 6)
            {
                cishu = 0;
                curV = 1.5f;
            }
        }
        else if (pos == pos2)
        {
            pos = pos1;  
            doTime = true;
            dotime = jinTime;
        }
    }

    float Gettime(float speed)
    {
        return Vector3.Distance(pos1.transform.position, pos2.transform.position) / speed;
    }


    void set(float v) {

        //float t =  1 - v / 2f;
        //float s = t * (100f - 5f);
        //s = s + 5f;
        //if (s < 1) s = 1;
        //textMesh.fontSize = (int)s;
        //float u = 1 - t;
        //u *= (0.1f - 0.005f);
        //u += 0.005f;
        //textMesh.characterSize = u;

        image.material.SetFloat("_Size", v);
    }

    int ci = 0;

    public void close()
    {
        this.gameObject.SetActive(false);

        VRBallPlayer.instance.OpenAll();

    }
    float curV = 0;
    bool isOO;
    void Update()
    {

        if (jj == 7)
        {
            if (VRMain.instance != null && VRMain.instance.isAllPlayer == true)
            {
                if (isOO == true) return;
                isOO = true;
                ChangeScene();
                return;
            }
            //VRMain.instance.sceneName = "TropicPack";
            //SceneManager.LoadScene(1);
        }


        if (doTime)
        {
            dotime -= Time.deltaTime;
            if (dotime <= 0)
            {
                if (ci >= 5) { close(); }
                SetText(ci);
                if (pos == pos1) { ci++; }
                doTime = false;

            }
            return;
        }
        this.transform.LookAt(pos3.transform);
        if (pos == pos1)
        {
            if (cishu <= 4)
            {
                this.transform.position += this.transform.forward * speed * 6 * Time.deltaTime;
                curV = curV += (1.5f / Gettime(speed * 6) * Time.deltaTime);
            }
            else
            {
                this.transform.position += this.transform.forward * speed * Time.deltaTime;
                curV = curV -= (1.5f / Gettime(speed) * Time.deltaTime);
            }
            //curV = (1.5f - curV) / Gettime() * Time.deltaTime;
        }
        else
        {
            if (cishu <= 4)
            {
                this.transform.position += this.transform.forward * speed * -1 * Time.deltaTime;
                curV = curV -= (1.5f / Gettime(speed) * Time.deltaTime);
            }
            else
            {
                this.transform.position += this.transform.forward * speed * 1 * -1 * Time.deltaTime;
                curV = curV += (1.5f / Gettime(speed * 1) * Time.deltaTime);
            }
        }


        set(curV);
        

        if (pos == pos1)
        {
            if (this.transform.position.x >= pos.transform.position.x)
            {
                //print(8888);
                changePos();
            }
        }
        else if (pos == pos2)
        {
            if (this.transform.position.x <= pos.transform.position.x)
            {
                changePos();
            }
        }

        if (Input.GetKeyDown(KeyCode.A)) {
            close();
        }

        //if (Vector3.Distance(this.transform.position, pos.transform.position) <= 0.05f)
        //{
        //    changePos();
        //}

    }



}
