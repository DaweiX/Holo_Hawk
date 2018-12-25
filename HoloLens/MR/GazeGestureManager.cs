using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;


public class GazeGestureManager : MonoBehaviour
{
    // 当前实例
    public static GazeGestureManager Instance { get; private set; }

    public GameObject FocusedObject { get; private set; }

    GestureRecognizer recognizer;
    public GameObject cube;

    void Start ()
    {
        Instance = this;
        recognizer = new GestureRecognizer();
        recognizer.Tapped += (args) =>
        {
            //if (FocusedObject != null)
            //{
            //    // 为聚焦的物体及其父对象发送“OnSelect”信息
            //    FocusedObject.SendMessageUpwards("OnSelect", SendMessageOptions.DontRequireReceiver);
            //}
            CapturePhoto.isSendWarning = true;
            if (cube != null)
            {
                cube.SendMessage("ChangeColor", SendMessageOptions.DontRequireReceiver);
            }
            ShowMsg.UpdateCubeMsg("警报");
        };
        // 开始捕捉手势
        recognizer.StartCapturingGestures();
	}
	
	void Update ()
    {
#if TEST_MR
        // 暂存
        GameObject oldFocusObject = FocusedObject;

        // 镭射
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;
        RaycastHit hit;
        if (Physics.Raycast(headPosition, gazeDirection, out hit))
        {
            // 如果有碰撞，使用新物体代替原有
            FocusedObject = hit.collider.gameObject;
        }
        else
        {
            FocusedObject = null;
        }

        // 如果聚焦的物体发生变化，再一次开始全新的测探
        if (FocusedObject != oldFocusObject)
        {
            recognizer.CancelGestures();
            recognizer.StartCapturingGestures();
        }
#endif
	}
}
