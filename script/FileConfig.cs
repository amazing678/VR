using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class FileConfig 
{
    public string file = Application.persistentDataPath + "/tet";
    public static FileConfig instan;
    public static FileConfig getInstans()
    {
        if (instan == null)
        {
            instan = new FileConfig();
        }
        return instan;
    }


    public void CreatFile()
    {
        if (!File.Exists(file))
        {
            WriteAllText(file,"");
        }
    }
    public void delete() {
        if (File.Exists(file))
        {
            File.Delete(file);
        }
    }


    public static bool WriteAllText(String path, String content)
    {
        //byte[] data = new byte[1024 * 1024 * 1];
        bool isOk = false;
        FileStream stream = null;

        try
        {
            stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            byte[] data = Encoding.UTF8.GetBytes(content);
            stream.Write(data, 0, data.Length);
            stream.Flush();
            isOk = true;
        }
        catch
        { isOk = false; }
        finally
        {
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
            }
        }
        return isOk;
    }
    public static String ReadAllText(String path)
    {
        String content = null;
        byte[] data = new byte[1024 * 1024 * 1];
        FileStream stream = null;
        try
        {
            stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            int line = stream.Read(data, 0, data.Length);
            content = Encoding.UTF8.GetString(data, 0, line);
        }
        catch
        { }
        finally
        {
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
            }
        }
        return content;
    }


}
