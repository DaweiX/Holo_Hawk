using UnityEngine;

public class HideShow : MonoBehaviour
{
	void Switch()
    {
        gameObject.SetActive(!gameObject.activeSelf);
	}
}
