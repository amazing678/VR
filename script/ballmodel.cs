using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class tet {
    public string txt;
    public string value;

    public tet(string _txt, string _value)
    {
        txt = _txt;
        value = _value;
    }
}
public class ballmodel : MonoBehaviour
{
    public Slider pos1Slider;
    public InputField pos1InputField;
    public Slider pos2Slider;
    public InputField pos2InputField;
    public Slider FlyspeedSlider;
    public InputField FlyspeedInputField;

    public Slider jinTimeSlider;
    public InputField jinTimeInputField;

    public Slider yuanTimeSlider;
    public InputField yuanTimeInputField;

    public GameObject pos1;
    public GameObject pos2;

    public GameObject camer;
    public InputField InputField;

    public ball ball;
    public static ballmodel instance;


 

    void Start()
    {
         FileConfig.getInstans().CreatFile();

        instance = this;
        try
        {
            pos1Slider.value = float.Parse(pos1InputField.text);
            SetDis(float.Parse(pos1InputField.text));
        }
        catch
        {
            pos1Slider.value = 0;
            SetDis(0);
            pos1InputField.text = "0";
        }
        try
        {
            pos2Slider.value = float.Parse(pos2InputField.text);
            SetDis2(float.Parse(pos2InputField.text));
        }
        catch
        {
            pos2Slider.value = 0;
            SetDis2(0);
            pos2InputField.text = "0";
        }
        try
        {
            FlyspeedSlider.value = float.Parse(FlyspeedInputField.text);
            SetSpeed(float.Parse(FlyspeedInputField.text));
        }
        catch
        {
            FlyspeedSlider.value = 0.1f;
            SetSpeed(0.1f);
            FlyspeedInputField.text = "0.1";
        }

        try
        {
            jinTimeSlider.value = float.Parse(jinTimeInputField.text);
            ball.jinTime = float.Parse(jinTimeInputField.text);
        }
        catch
        {
            jinTimeSlider.value = 0;
            ball.jinTime = 0;
            jinTimeInputField.text = "0";
        }

        try
        {
            yuanTimeSlider.value = float.Parse(yuanTimeInputField.text);
            ball.yuanTime = float.Parse(yuanTimeInputField.text);
        }
        catch
        {
            yuanTimeSlider.value = 0;
            ball.yuanTime = 0;
            yuanTimeInputField.text = "0";
        }
        texts.Add(new tet("Actor", "男演员"));
        texts.Add(new tet("Bread", "面包"));
        texts.Add(new tet("Clock", "时钟"));
        texts.Add(new tet("Earth", "地球"));
        texts.Add(new tet("Faith", "信仰"));
        texts.Add(new tet("Grape", "葡萄"));
        texts.Add(new tet("Honey", "蜂蜜"));
        texts.Add(new tet("Jewel", "宝石"));
        texts.Add(new tet("Knife", "刀"));
        texts.Add(new tet("Lemon", "柠檬"));
        texts.Add(new tet("Match", "比赛"));
        texts.Add(new tet("Nurse", "护士"));
        texts.Add(new tet("Opera", "歌剧"));
        texts.Add(new tet("Piano", "钢琴"));
        texts.Add(new tet("Queen", "女王"));
        texts.Add(new tet("River", "河"));
        texts.Add(new tet("Theft", "盗窃"));
        texts.Add(new tet("Scarf", "围巾"));
        texts.Add(new tet("Uncle", "叔叔"));
        texts.Add(new tet("Video", "视频"));
        texts.Add(new tet("Water", "水"));
        texts.Add(new tet("Young", "年轻人"));
        texts.Add(new tet("Zebra", "斑马"));
        texts.Add(new tet("Adult", "成年人"));
        texts.Add(new tet("Belly", "腹部"));
        texts.Add(new tet("Daily", "每天的"));
        texts.Add(new tet("Draft", "草稿"));
        texts.Add(new tet("Empty", "空白的"));
        texts.Add(new tet("Fever", "发热"));
        texts.Add(new tet("Glass", "玻璃"));
        texts.Add(new tet("Honor", "荣誉"));
        texts.Add(new tet("Infer", "推断"));
        texts.Add(new tet("Issue", "问题"));
        texts.Add(new tet("Juice", "果汁"));
        texts.Add(new tet("Knock", "敲"));
        texts.Add(new tet("Learn", "学习"));
        texts.Add(new tet("Music", "音乐"));
        texts.Add(new tet("Night", "夜晚"));
        texts.Add(new tet("Ocean", "海洋"));
        texts.Add(new tet("panda", "熊猫"));
        texts.Add(new tet("quiet", "安静的"));
        texts.Add(new tet("range", "范围"));
        texts.Add(new tet("score", "分数"));
        texts.Add(new tet("table", "桌子"));
        texts.Add(new tet("urban", "城市的"));
        texts.Add(new tet("virus", "病毒"));
        texts.Add(new tet("white", "白色的"));
        texts.Add(new tet("yield", "出产"));
        string fileStr = ""; 
        fileStr = FileConfig.ReadAllText(Application.persistentDataPath + "/tet");
        string[] astr = fileStr.Split('|');
        List<string> astrl = new List<string>(astr);
        List<tet> l = new List<tet>();
        foreach (tet s in texts) {
            if (!astrl.Contains(s.txt))
            {
                l.Add(s); 
            }
        }
        if (l.Count <= 0)
        {
            l.AddRange(texts);
            fileStr = "";
            FileConfig.getInstans().delete();
        }


        int i = UnityEngine. Random.Range(0, l.Count-1);
        curtet = l[i];
        FileConfig.WriteAllText(Application.persistentDataPath + "/tet", fileStr+"|"+ curtet.txt);

        //print(curtet.txt);
        SetTextlist();
        ball.transform.Find("Ball/Text (TMP)").GetComponent<TextMeshProUGUI>().text = GetText(0);
    }
    [NonSerialized]public tet curtet;
    public void SetFlySpeed(float v)
    {
        FlyspeedInputField.text = v.ToString();
    }
    public void SetFlySpeed(string v)
    {
        float dis = 0.1f;
        try
        {
            dis = float.Parse(v);
        }
        catch
        {
            FlyspeedInputField.text = dis.ToString();
        }
        FlyspeedSlider.value = dis;
        SetSpeed(dis);
    }
    public void SetSpeed(float speed)
    {
        ball.speed = speed;
    }


    public void Setpos1(string v)
    {
        float dis = 0;
        try
        {
            dis = float.Parse(v);
        }
        catch
        {
            pos1InputField.text = dis.ToString();
        }
        pos1Slider.value = dis;
        SetDis(dis);
    }
    public void Setpos1(float v)
    {
        pos1InputField.text = v.ToString();
    }

    public void SetZi(string v)
    {
        SetTextlist();
        ball.transform.Find("Ball/Text (TMP)").GetComponent<TextMeshProUGUI>().text = GetText(0);
    }


    List<char> strlist =new List<char> ();
    List<tet> texts = new List<tet>();
    public void SetTextlist()
    {
        strlist.Clear();
        //List<char> g = new List<char>(InputField.text.ToCharArray());
        strlist = new List<char>(curtet.txt.ToCharArray());
        //for (int i = 0;i <= 4;i++)
        //{
        //    int ids = Random.Range(0, 4-i);
        //    strlist.Add(g[ids]);
        //    g.Remove(g[ids]);
        //}
    }
    public  string GetText(int ids)
    {
        if (strlist.Count < ids)
        {
            return "!";
        }

        switch (ids)
        {
            case 0: return strlist[3].ToString();
            case 1: return strlist[4].ToString();
            case 2: return strlist[2].ToString();
            case 3: return strlist[0].ToString();
            case 4: return strlist[1].ToString();
        }

        return "!";
    }

    public void Setpos2(string v)
    {
        float dis = 0;
        try
        {
            dis = float.Parse(v);
        }
        catch
        {
            pos2InputField.text = dis.ToString();
        }
        pos2Slider.value = dis;
        SetDis2(dis);
    }
    public void SetjinTiem(string v)
    {
        float dis = 0;
        try
        {
            dis = float.Parse(v);
        }
        catch
        {
            jinTimeInputField.text = dis.ToString();
        }
        jinTimeSlider.value = dis;
        ball.jinTime = dis;
    }

    public void SetjinTiem(float v)
    {
        jinTimeInputField.text = v.ToString();
    }

    public void SetyuanTiem(string v)
    {
        float dis = 0;
        try
        {
            dis = float.Parse(v);
        }
        catch
        {
            yuanTimeInputField.text = dis.ToString();
        }
        yuanTimeSlider.value = dis;
        ball.yuanTime = dis;
    }

    public void SetyuanTiem(float v)
    {
        yuanTimeInputField.text = v.ToString();
    }
    public void Setpos2(float v)
    {
        pos2InputField.text = v.ToString();
    }
    void SetDis(float dic)
    {
        pos1.transform.position = camer.transform.forward * dic + camer.transform.position;
        ball.SetFrist();
    }

    void SetDis2(float dic)
    {
        pos2.transform.position = camer.transform.forward * dic + camer.transform.position;
    }
}
