using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaledLayout : MonoBehaviour
{
	public int refScreenWidth = 2160;

	int screenWidth;

    void OnEnable()
    {
		Update();
    }

    void Update()
    {
		if (Screen.width != screenWidth)
		{
			screenWidth = Screen.width;
			transform.localScale = Vector3.one * screenWidth / refScreenWidth;
		}
    }
}
