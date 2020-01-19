using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalSalmon.Fade;

static class CameraFader {
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

    public static void FadeUp()
    {
        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i].isActiveAndEnabled)
                effects[i].FadeUp();
        }
    }

    public static void FadeDown()
    {
        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i].isActiveAndEnabled)
                effects[i].FadeDown();
        }
    }
}
