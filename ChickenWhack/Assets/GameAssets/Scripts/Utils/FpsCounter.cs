// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple fps counter, touch screen location or press F1 to toggle
/// </summary>
public class FpsCounter : MonoBehaviour
{
    Text text;

    string[] numStrings = new string[256]; //cached strings to avoid gc

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
        if(Time.unscaledDeltaTime > 0f)
            smoothFps = Mathf.Lerp(smoothFps, 1f / Time.unscaledDeltaTime, Time.unscaledDeltaTime * 4f);

        text.text = numStrings[Mathf.CeilToInt(smoothFps)];

        if (Input.GetKeyDown(KeyCode.F1))
            SwitchVisible();
    }

    public void SwitchVisible()
    {
        text.enabled = !text.enabled;
    }
}
