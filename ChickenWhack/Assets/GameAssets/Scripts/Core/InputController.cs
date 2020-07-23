using Lean.Touch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Wrapper for UnityEngine.Input, combines mouse and touch controls. It updates before all scripts.
/// </summary>
public class InputController : MonoBehaviour
{
    public static bool IsLandscape { get {
            if (!isLandscape.HasValue)
			{
				ScreenOrientation orientation = Application.isMobilePlatform ? Screen.orientation : (float)Screen.width / Screen.height > 1f ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
				isLandscape = orientation == ScreenOrientation.LandscapeLeft || orientation == ScreenOrientation.LandscapeRight;
			}
            return isLandscape.Value;
	} }

    public static event System.Action OnOrientationChanged = delegate { };

    public static bool MultiTouch()
    {
        return Input.touchCount > 1;
    }

    public static bool GetTapDown(out Vector2 screenPos)
    {
        return ApplicationController.refs.inputController.GetTapDownInternal(out screenPos);
    }

    public static bool GetTap(out Vector2 screenPos)
    {
        return ApplicationController.refs.inputController.GetTapInternal(out screenPos);
    }

    public static bool GetEscapeDown()
    {
        return ApplicationController.refs.inputController.GetEscapeDownInternal();
    }

    public static float GetTwistDegrees()
    {
        return LeanGesture.GetTwistDegrees();
    }

    public static float GetPinchScale()
    {
        return LeanGesture.GetPinchScale();
    }

    private bool GetTapDownInternal(out Vector2 screenPos)
    {
        screenPos = tapPos;
        return tapDown;
    }

    private bool GetTapInternal(out Vector2 screenPos)
    {
        screenPos = tapPos;
        return tap;
    }

    private bool GetEscapeDownInternal()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }

    private bool tapDown = false;
    private bool tap = false;
    private Vector2 tapPos = Vector2.zero;

    private static bool? isLandscape;
    private ScreenOrientation lastOrientation;

    private void Update()
    {
		if (Application.isMobilePlatform || Application.isEditor)
		{
			isLandscape = null;

			ScreenOrientation currentOrientation;

			if (Application.isMobilePlatform)
				currentOrientation = Screen.orientation;
			else
				currentOrientation = (float)Screen.width / Screen.height > 1f ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;

			if (currentOrientation != lastOrientation)
			{
				OnOrientationChanged();
				lastOrientation = currentOrientation;
			}
		}

		TapDetection();
    }

	private void TapDetection()
	{
		if (tapDown)
			tapDown = false;
		if (tap)
			tap = false;

		if (Input.touchSupported) //Touch detection
		{
			if (EventSystem.current.IsPointerOverGameObject() || EventSystem.current.currentSelectedGameObject != null) //UI block
			{
				EventSystem.current.SetSelectedGameObject(null);
				return;
			}

			var touch = LeanTouch.GetFingers(false, false, 1);
			if (touch != null)
			{
				tapPos = touch[0].ScreenPosition;

				if (touch[0].Tap)
				{
					tapDown = true;
				}
				else
				{
					tap = true;
				}
			}
		}
		else //Mouse detection
		{
			if (EventSystem.current.IsPointerOverGameObject() || EventSystem.current.alreadySelecting) //UI block
			{
				return;
			}

			tapPos = Input.mousePosition;

			if (Input.GetMouseButtonDown(0))
			{
				tapDown = true;
			}
			else if (Input.GetMouseButton(0))
			{
				tap = true;
			}
		}
	}
}
