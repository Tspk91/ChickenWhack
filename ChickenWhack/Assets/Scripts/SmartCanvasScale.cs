using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class SmartCanvasScale : MonoBehaviour
{
    public float landscapeScaleFactor = 0.5f;

    private CanvasScaler scaler;

    private bool IsLandscape { get { return Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight; } }

    private bool lastLandscape = false;

    private void Awake()
    {
        scaler = GetComponent<CanvasScaler>();
        StartCoroutine(ScaleByOrientation());
    }

    private IEnumerator ScaleByOrientation()
    {
        while (true)
        {
            bool landscape = IsLandscape;
            if(landscape != lastLandscape)
            {
                scaler.scaleFactor = IsLandscape ? landscapeScaleFactor : 1f;
            }
            yield return null;
        }
    }

}
