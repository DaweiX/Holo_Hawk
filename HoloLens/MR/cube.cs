using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cube : MonoBehaviour {

	void Start ()
    {
		
	}
	
	void Update ()
    {
        float speed = 16f;
        gameObject.transform.Rotate(Vector3.up * Time.deltaTime * speed);
    }

    void ChangeColor()
    {
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0);
        Invoke("White", 1);
    }

    void White()
    {
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1);
    }
}
