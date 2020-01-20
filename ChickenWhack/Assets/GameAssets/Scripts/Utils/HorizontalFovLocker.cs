// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class HorizontalFovLocker : MonoBehaviour {

    private bool lastLandscape = false;

    private Camera cam;
    private float portraitFov;
    private float landscapeFov;

    private Coroutine scalingCoroutine;

    private void Start()
    {
        if (!Application.isMobilePlatform)
            return;

        cam = GetComponent<Camera>();

        portraitFov = cam.fieldOfView;
        float aspect = cam.aspect;
        float hFov = GetOppositeFov(cam.fieldOfView, InputController.IsLandscape ? 1f / aspect : aspect);
        landscapeFov = GetOppositeFov(hFov, InputController.IsLandscape ? aspect : 1f / aspect);

        StartCoroutine(ScaleByOrientation());
    }

    private float GetOppositeFov(float fov, float aspect)
    {
        var radAngle = cam.fieldOfView * Mathf.Deg2Rad;
        var oFov = 2f * Mathf.Atan(Mathf.Tan(radAngle / 2f) / aspect);
        return Mathf.Rad2Deg * oFov;
    }

    private IEnumerator ScaleByOrientation()
    {
        while (true)
        {
            bool landscape = InputController.IsLandscape;
            if (landscape != lastLandscape)
            {
                if (!ApplicationController.refs.AR_controller.AR_Enabled)
                {
                    if (scalingCoroutine != null)
                    {
                        StopCoroutine(scalingCoroutine);
                    }
                    scalingCoroutine = StartCoroutine(ScalingCoroutine(landscape));
                }

                lastLandscape = landscape;
            }
            yield return null;
        }
    }

    private IEnumerator ScalingCoroutine(bool landscape)
    {
        float t = 0f;
        float origin = landscape ? portraitFov : landscapeFov;
        float target = landscape ? landscapeFov : portraitFov;

        yield return new WaitForSecondsRealtime(0.33f);

        while (t < 1f)
        {
            t += Time.deltaTime * 2.5f;
            cam.fieldOfView = Mathf.Lerp(origin, target, t);
            yield return null;
        }
        cam.fieldOfView = target;

        scalingCoroutine = null;
    }

}