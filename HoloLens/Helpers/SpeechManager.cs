using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class SpeechManager : MonoBehaviour
{
    // 通过输入语音尝试匹配列表中关键词的类
    KeywordRecognizer KeywordRecognizer;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    public GameObject gameobj;

    void Start ()
    {
        // 变色指令：Color
        keywords.Add("Color", () =>
        {
            // 广播（公告）
            gameobj.SendMessage("ChangeColor", SendMessageOptions.DontRequireReceiver);
        });

        // 重置指令：Reset
        keywords.Add("Reset", () =>
        {
            // 广播（公告）
            gameobj.SendMessage("OnReset", SendMessageOptions.DontRequireReceiver);
        });

        //下坠指令：Drop
        keywords.Add("Drop", () =>
        {
            gameobj.SendMessage("Drop", SendMessageOptions.DontRequireReceiver);

        });

        keywords.Add("Switch", () =>
        {
            gameobj.SendMessage("Switch", SendMessageOptions.DontRequireReceiver);

        });

        KeywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

        // 为 KeywordRecognizer 注册一个回调并开始识别
        KeywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        KeywordRecognizer.Start();

        // 更换图标颜色
        ShowMsg.ChangeColor(ShowMsg.MyIcons.speech);
        //if (ShowMsg.block == null)
        //{
        //    string path = "Main Camera/GameObject/Canvas_MAIN/Canvas_Popup/msg";
        //    ShowMsg.block = GameObject.Find(path).GetComponent<Text>();
        //}
        //ShowMsg.ShowMessage("语音指令就绪");
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action action;
        if (keywords.TryGetValue(args.text, out action)) 
        {
            action.Invoke();
        }
    }
}
