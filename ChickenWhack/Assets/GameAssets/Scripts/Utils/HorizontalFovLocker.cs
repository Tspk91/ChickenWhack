// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class HorizontalFovLocker : MonoBehaviour {

	public float minVerticalFov = 42f;

    public float scalingDelay = 0.33f;
    public float scalingTime = 0.33f;

	public event System.Action onLandscapeShowStart;
	public event System.Action onLandscapeHideEnd;

    private Camera cam;
    private float portraitFov;
    private float landscapeFov;

    private Coroutine scalingCoroutine;

	private bool lastLandscape;

	private void Awake()
	{
		if (!Application.isMobilePlatform && !Application.isEditor)
			Destroy(this);

		cam = GetComponent<Camera>();

		bool isLandscape = InputController.IsLandscape;
		float aspect = cam.aspect;

		portraitFov = cam.fieldOfView;
		float targetHorizontalFov = GetOppositeFov(portraitFov, isLandscape ? aspect : 1f / aspect);
		landscapeFov = Mathf.Max(minVerticalFov, GetOppositeFov(targetHorizontalFov, isLandscape ? aspect : 1f / aspect));

		InputController.OnOrientationChanged += ScaleByOrientation;

		lastLandscape = isLandscape;
	}

    private void OnEnable()
    {
		cam.fieldOfView = InputController.IsLandscape ? landscapeFov : portraitFov;

		if (InputController.IsLandscape)
			onLandscapeShowStart();
	}

    private static float GetOppositeFov(float fov, float aspect)
    {
        var radAngle = fov * Mathf.Deg2Rad;
        var oFov = 2f * Mathf.Atan(Mathf.Tan(radAngle / 2f) / aspect);
        return Mathf.Rad2Deg * oFov;
    }

    private void ScaleByOrientation()
    {
        if (!ApplicationController.refs.AR_controller.AR_Enabled &&
			InputController.IsLandscape != lastLandscape)
        {
			lastLandscape = InputController.IsLandscape;

			if (scalingCoroutine != null)
				ApplicationController.refs.StopCoroutine(scalingCoroutine);

            scalingCoroutine = ApplicationController.refs.StartCoroutine(ScalingCoroutine(InputController.IsLandscape));
        }
    }

    private IEnumerator ScalingCoroutine(bool landscape)
    {
		if (landscape)
			onLandscapeShowStart();

		float t = 0f;
        float origin = landscape ? portraitFov : landscapeFov;
        float target = landscape ? landscapeFov : portraitFov;

        yield return new WaitForSecondsRealtime(scalingDelay);

        while (t < 1f)
        {
            t += Time.deltaTime / scalingTime;
            cam.fieldOfView = Mathf.Lerp(origin, target, t);
            yield return null;
        }
        cam.fieldOfView = target;

		if (!landscape)
			onLandscapeHideEnd();

		scalingCoroutine = null;
    }
}