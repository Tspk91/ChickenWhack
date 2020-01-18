using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FpsCounter : MonoBehaviour
{
    Text text;

    string[] numStrings = new string[256];

    void Start()
    {
        text = GetComponentInChildren<Text>();
        text.enabled = false;

        for (int i = 0; i < numStrings.Length; i++)
        {
            numStrings[i] = i.ToString();
        }
    }

    float smoothFps = 0f;

    void Update()
    {
        if(Time.deltaTime > 0f)
            smoothFps = Mathf.Lerp(smoothFps, 1f / Time.deltaTime, Time.deltaTime * 2f);

        text.text = numStrings[Mathf.CeilToInt(smoothFps)];

        if (Input.GetKeyDown(KeyCode.F1))
            SwitchVisible();
    }

    public void SwitchVisible()
    {
        text.enabled = !text.enabled;
    }
}
