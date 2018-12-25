using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Helpers
{
    /// <summary>
    /// 自定义 Log 类，用于显示即时信息
    /// </summary>
    public static class MyLog
    {

        static Text text_console;

        // 初始化
        public static bool Init()
        {
            try
            {
                text_console = GameObject.Find("Main Camera/GameObject/Canvas_MAIN/Text_Time").GetComponent<Text>();
                return true;
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine("TEXT INIT ERROR" + e.Message);
                return false;
            }
        }

        // 打印控制台信息
        public static void Log(string str)
        {
            text_console.text = str;
        }

        public static void DebugLog(string msg, bool isNewLine = true)
        {
            if (isNewLine)
                System.Diagnostics.Debug.WriteLine(msg);
            else
                System.Diagnostics.Debug.Write(msg + '\t' + '\t');
        }

    }
}

