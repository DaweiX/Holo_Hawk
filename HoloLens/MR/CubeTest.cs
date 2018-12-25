using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CubeTest : MonoBehaviour
{
    public Text text;

    Vector3 originalPosition;

    void Start()
    {
        // 获取物体最初的位置
        originalPosition = this.transform.localPosition;
    }

    // 在 GazeGestureManager 中定义
    void OnSelect()
    {
        ChangeColor();
    }

    // 在 SpeechManager 中定义
    void OnReset()
    {
        var rigidbody = this.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            // 移除物理效果
            rigidbody.isKinematic = true;
            Destroy(rigidbody);
        }
        // 移回原位置
        this.transform.localPosition = originalPosition;
    }

    void ChangeColor()
    {
        gameObject.GetComponent<MeshRenderer>().material.color =
            new Color(Random.Range(0, 255) / 255f, Random.Range(0, 255) / 255f, Random.Range(0, 255) / 255f);
        Color color = gameObject.GetComponent<MeshRenderer>().material.color;
		if(text != null)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("RGB:(");
			builder.Append((color.r*255).ToString("0"));
			builder.Append(",");
			builder.Append((color.g*255).ToString("0"));
			builder.Append(",");
			builder.Append((color.b*255).ToString("0"));
			builder.Append(")");
			text.text = builder.ToString();
		}
    }

    void Drop()
    {
        // 添加刚体使物体下坠
        if (!this.GetComponent<Rigidbody>())
        {
            var rigidbody = this.gameObject.AddComponent<Rigidbody>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    void SocketReceive(byte[] data)
    {
        text.text = data.Length.ToString();
    }

    void Update()
    {
        float speed = 16f;
        gameObject.transform.Rotate(Vector3.up * Time.deltaTime * speed);
    }
}
