using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Helpers
{
    /// <summary>
    /// 用于提取回授数据并规范其为指定格式
    /// </summary>
    public static class DataFormat
    {
        //格式： （m为度，x为分）    mmxx.xxxxxx,N,mmmxx.xxxxxx,E
        static string test_beidou = "4542.284737,N,12636.885076,E";
        /// <summary>
        /// 将北斗定位信息转换为输出字符串
        /// </summary>
        public static string GetBeiDouInfo(string arg)
        {
            arg = test_beidou;
            var list = test_beidou.Split(',');
            StringBuilder builder = new StringBuilder();
            string Jing = list[0];
            string N_S = list[1];
            string Wei = list[2];
            string E_W = list[3];
            string Jing_Du = Jing.Substring(0, 2);
            string Wei_Du = Wei.Substring(0, 3);
            string Jing_Fen = Jing.Substring(2, 5);
            string Wei_Fen = Wei.Substring(3, 5);
            // 我想用插补表达式啊啊啊啊！
            builder.Append(Jing_Du);
            builder.Append("°");
            builder.Append(Jing_Fen);
            builder.Append('\'');
            builder.Append(N_S);
            builder.Append(Wei_Du);
            builder.Append("°");
            builder.Append(Wei_Fen);
            builder.Append('\'');
            builder.Append(E_W);
            return builder.ToString();
        }
    }
}

