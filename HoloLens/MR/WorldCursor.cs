using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCursor : MonoBehaviour
{
    MeshRenderer meshRenderer;

    void Start ()
    {
        meshRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();	
	}

    void Update()
    {
        // 镭射
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;
        RaycastHit hit;
        if (Physics.Raycast(headPosition, gazeDirection, out hit))
        {
            // 击中目标时，将光标还原并置为绿色
            meshRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
            meshRenderer.material.color = new Color(0.25f, 1f, 0f);
            // 移动位置至击中处
            this.transform.position = hit.point;
            // 旋转角度以匹配物体
            this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
        else
        {
            // 未击中目标时，将光标缩小并置为白色，角度还原
            meshRenderer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            meshRenderer.material.color = new Color(1f, 1f, 1f);
            meshRenderer.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        }
    }
}
