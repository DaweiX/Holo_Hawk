using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ShowMsg : MonoBehaviour
{
	public Text text;
    //public static int LinesInPopups = 0;

    public enum MyIcons
    {
        speech,
        vision,
        uwb,
        gps
    }

    public static Text block;
    public static string MsgtoShow;
    public static string msg_under_cube;

	void Start ()
    {
        msg_under_cube = "working";
    }

    public static void UpdateCubeMsg(string msg)
    {
        msg_under_cube = msg;
    }

    void Update()
    {
        if (text == null) return;
        text.text = msg_under_cube;
    }

    // 弃用
    //public static void ShowMessage(string msg)
    //{
    //    if (!block.isActiveAndEnabled) return;
    //    if (string.IsNullOrEmpty(msg)) return;
    //    StringBuilder builder = new StringBuilder();
    //    if (LinesInPopups < 5)
    //    {
    //        builder.Append(block.text);
    //        builder.AppendLine(msg);
    //        block.text = builder.ToString();
    //    }
    //    else
    //    {
    //        var array = block.text.Split('\n');
    //        for (int i = 1; i < 5; i++)
    //        {
    //            builder.AppendLine(array[i]);
    //        }
    //        builder.AppendLine(msg);
    //    }
    //    MsgtoShow = builder.ToString();
    //    LinesInPopups++;
    //}

    public static void ChangeColor(MyIcons icons, bool light = true)
    {
        string path = "Main Camera/GameObject/Canvas_MAIN/Icons/" + icons.ToString() + "_0";
        SpriteRenderer renderer = GameObject.Find(path).GetComponent<SpriteRenderer>();
        renderer.color = light
            ? new Color(255, 255, 255, 255)
            : new Color(104, 104, 104, 255);
    }
}
