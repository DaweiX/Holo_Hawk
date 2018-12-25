using System;
using UnityEngine;
using UnityEngine.UI;

public class LogTime : MonoBehaviour {

    // 用于显示当前时间

    Text text;

	void Start ()
    {
        text = gameObject.GetComponent<Text>();
	}
	
	void Update ()
    {
        var t = DateTime.Now.ToLocalTime().ToString();
        text.text = t.Remove(t.Length-2);
    }
}
