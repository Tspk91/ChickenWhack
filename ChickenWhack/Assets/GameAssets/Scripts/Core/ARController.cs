// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SpatialTracking;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Initializes and resets the AR session.
/// Changes the session scale and rotation according to user input.
/// Controls the object placement on planes operation.
/// </summary>
public class ARController : MonoBehaviour
{
    enum AR_State { NONE, ACTIVE, FIND_PLANE, PLACING }

    const string labelResetting = "Starting tracking...";
    const string labelFindPlane = "Point at a flat surface";
    const string labelPlaceObject = "Tap to place the game world";

    public bool AR_Enabled { get { return state != AR_State.NONE; } }

    //AR Foundation components
    public ARSession session;
    public ARSessionOrigin origin;
    public ARRaycastManager raycastManager;

    //Cancel placement phase button
    public GameObject cancelButton;

    public float twistContentSpeed = 1f;
    public float pinchContentSpeed = 1f;

    public float placementIndicatorRefSize = 10;
    public Transform placementIndicator;
    public UnityEngine.UI.Text placementText;

    AR_State state = AR_State.NONE;

    Camera AR_camera;
    float initialScaleFactor = 1f;

    Transform objectToPlace;
    System.Action onPlaced; //is cleared after being called

    List<ARRaycastHit> hits = new List<ARRaycastHit>(); //reused array for plane raycasting

    private void Awake()
    {
        cancelButton.SetActive(false);
        placementText.gameObject.SetActive(false);
        placementIndicator.gameObject.SetActive(false);

        placementIndicator.transform.localScale = Vector3.one * (2f * ApplicationController.refs.gameController.gameAreaRadius) / placementIndicatorRefSize;
    }

    private void SetScaleFactor(float factor)
    {
        origin.transform.localScale = factor * Vector3.one;
    }

    /// <summary>
    /// Resets all AR session data, and waits to avoid hiccups
    /// </summary>
    private IEnumerator Reset()
    {
        session.Reset();
        yield return ARSession.CheckAvailability();
        while (ARSession.state == ARSessionState.SessionInitializing)
            yield return null;
    }

    /// <summary>
    /// Try to initialize AR, if not found fallback to stationary camera, else wait for initialization and configure AR objects
    /// </summary>
    public IEnumerator TryInitAR(Camera AR_camera, float initialScaleFactor)
    {
        if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
        {
            session.gameObject.SetActive(true);
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            Debug.Log("AR unsupported!");

            session.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Starting AR session!");

            SetScaleFactor(initialScaleFactor);

            AR_camera.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            yield return null;

            var poseDriver = AR_camera.gameObject.AddComponent<TrackedPoseDriver>();
            poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.ColorCamera);
            poseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            poseDriver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
            poseDriver.UseRelativeTransform = true;

            origin.camera = AR_camera;
            AR_camera.transform.SetParent(origin.transform);

            this.AR_camera = AR_camera;
            this.initialScaleFactor = initialScaleFactor;

            yield return null;

            state = AR_State.ACTIVE;
        }
    }

    public IEnumerator StartPlacement(Transform objectToPlace, System.Action onPlaced)
    {
        //enable camera so we have black fade over the AR initialization noise 
        AR_camera.gameObject.SetActive(true);
        CameraFader.FadeDown(true); //instant fade

        placementText.gameObject.SetActive(true);
        placementText.text = labelResetting;

        //Discard previous planes etc
        yield return Reset();
        
        state = AR_State.FIND_PLANE;
        placementText.text = labelFindPlane;

        this.objectToPlace = objectToPlace;
        this.onPlaced = onPlaced;

        cancelButton.SetActive(true);
    }

    public void CancelPlacement()
    {
        EndPlacement();

        ApplicationController.ExitGame(GameExitType.AR_PLACEMENT, 0f);
    }

    private void EndPlacement()
    {
        state = AR_State.ACTIVE;

        onPlaced = null;

        cancelButton.SetActive(false);
        placementText.gameObject.SetActive(false);
        placementIndicator.gameObject.SetActive(false);
    }

    public void PlaceContent(Vector3 position)
    {
        origin.MakeContentAppearAt(objectToPlace, position, Quaternion.identity);
        onPlaced();

        EndPlacement();

        SetScaleFactor(initialScaleFactor);
    }

    private void ScaleContent(float scale)
    {
        if (scale == 0f)
            return;
        SetScaleFactor(origin.transform.localScale.x / scale);
    }

    private void RotateContent(float degrees)
    {
        origin.MakeContentAppearAt(objectToPlace, Quaternion.AngleAxis(degrees, -Vector3.up) * Quaternion.Inverse(origin.transform.rotation) * objectToPlace.rotation);
    }

    /// <summary>
    /// If we are in placement phase, check for inputs and raycast to place the object
    /// </summary>
    private void Update()
    {
        if (state == AR_State.ACTIVE || state == AR_State.NONE )
        {
            if (objectToPlace != null && InputController.MultiTouch()) //Process content rotation and scaling input if it is placed
            {
                RotateContent(InputController.GetTwistDegrees() * twistContentSpeed);
                ScaleContent(Mathf.Pow(InputController.GetPinchScale(), pinchContentSpeed));
            }
        }
        else //We are in placement phase
        {
            PlacementUpdate();
        }
    }

    private void PlacementUpdate()
    {
        if (InputController.GetEscapeDown())
        {
            CancelPlacement();
            return;
        }

        Vector2 screenPoint = AR_camera.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));

        if (raycastManager.Raycast(screenPoint, hits, TrackableType.Planes))
        {
            if (state != AR_State.PLACING) //change state to placing
            {
                state = AR_State.PLACING;
                placementText.text = labelPlaceObject;
                placementIndicator.gameObject.SetActive(true);
            }

            Pose pose = hits[0].pose;

            //Calculate placement position and rotation
            Vector3 placementPosition = Quaternion.Inverse(origin.transform.rotation) * (pose.position - origin.transform.position);
            Vector3 camDirHorizontal = AR_camera.transform.forward;
            camDirHorizontal.y = 0f;
            Quaternion placementRotation = Quaternion.LookRotation(camDirHorizontal, Vector3.up);

            //Update the placement indicator
            placementIndicator.SetPositionAndRotation(placementPosition, placementRotation);

            //If user taps, place the object
            if (InputController.GetTapDown(out Vector2 tapPos))
                PlaceContent(pose.position);
        }
        else if (state != AR_State.FIND_PLANE) //change state to finding plane
        {
            state = AR_State.FIND_PLANE;
            placementText.text = labelFindPlane;
            placementIndicator.gameObject.SetActive(false);
        }
    }
}
