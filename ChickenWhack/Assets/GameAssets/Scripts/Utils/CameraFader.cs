// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalSalmon.Fade;

/// <summary>
/// Small fade util using DigitalSalmon.Fade plugin to do camera transitions
/// </summary>
static class CameraFader
{
    static FadePostProcess[] effects;

    public static void Init(float duration, params Camera[] cameras)
    {
        effects = new FadePostProcess[cameras.Length];
        for (int i = 0; i < cameras.Length; i++)
        {
            effects[i] = cameras[i].GetComponent<FadePostProcess>();
            effects[i].effectDuration = duration;
        }
    }

    /// <summary>
    /// Hides the fade
    /// </summary>
    public static void FadeUp(bool instant = false)
    {
        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i].isActiveAndEnabled)
                effects[i].FadeUp(instant);
        }
    }

    /// <summary>
    /// Displays the fade
    /// </summary>
    public static void FadeDown(bool instant = false)
    {
        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i].isActiveAndEnabled)
                effects[i].FadeDown(instant);
        }
    }
}
