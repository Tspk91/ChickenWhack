// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SpatialTracking;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARController : MonoBehaviour
{
    const string labelFindPlane = "Point at a flat surface";
    const string labelPlaceObject = "Tap to place the game world";

    public bool AR_Enabled { get { return session.isActiveAndEnabled; } }

    public ARSession session;
    public ARSessionOrigin origin;
    public ARRaycastManager raycastManager;

    public GameObject cancelButton;

    public Transform placementIndicator;
    public UnityEngine.UI.Text placementText;

    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    Camera AR_camera;
    float scaleFactor = 1f;

    bool isPlacing = false;

    Transform objectToPlace;
    System.Action onPlaced;

    private void Awake()
    {
        cancelButton.SetActive(false);
        placementText.gameObject.SetActive(true);
        placementIndicator.gameObject.SetActive(false);
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
    public IEnumerator TryInitAR(Camera AR_camera, float scaleFactor)
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

            origin.transform.localScale = scaleFactor * Vector3.one;
            AR_camera.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            yield return null;

            var poseDriver = AR_camera.gameObject.AddComponent<TrackedPoseDriver>();
            poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.ColorCamera);
            poseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            poseDriver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
            poseDriver.UseRelativeTransform = false;

            origin.camera = AR_camera;
            AR_camera.transform.SetParent(origin.transform);

            this.AR_camera = AR_camera;
            this.scaleFactor = scaleFactor;

            yield return null;
        }
    }

    public IEnumerator StartPlacement(Transform objectToPlace, System.Action onPlaced)
    {
        //enable camera so we have black fade over the AR initialization noise 
        AR_camera.gameObject.SetActive(true);
        CameraFader.FadeDown(true); //instant fade

        //Discard previous planes etc
        yield return Reset();

        isPlacing = true;

        this.objectToPlace = objectToPlace;
        this.onPlaced = onPlaced;

        cancelButton.SetActive(true);
        placementText.gameObject.SetActive(true);
    }

    public void CancelPlacement()
    {
        EndPlacement();

        ApplicationController.ExitGame(GameExitType.AR_PLACEMENT, 0f);
    }

    private void EndPlacement()
    {
        isPlacing = false;

        objectToPlace = null;
        onPlaced = null;

        cancelButton.SetActive(false);
        placementText.gameObject.SetActive(true);
        placementIndicator.gameObject.SetActive(false);
    }

    public void Place(Pose pose)
    {
        origin.MakeContentAppearAt(objectToPlace, pose.position, Quaternion.identity);
        onPlaced();

        EndPlacement();
    }

    /// <summary>
    /// If we are in placement phase, check for inputs and raycast to place the object
    /// </summary>
    void Update()
    {
        if (!isPlacing || EventSystem.current.IsPointerOverGameObject() || EventSystem.current.currentSelectedGameObject != null)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
            return;
        }

        Vector2 screenPoint = AR_camera.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));

        if (raycastManager.Raycast(screenPoint, hits, TrackableType.Planes))
        {
            placementIndicator.gameObject.SetActive(true);
            placementText.text = labelPlaceObject;

            Pose pose = hits[0].pose;
            placementIndicator.SetPositionAndRotation(Quaternion.Inverse(origin.transform.rotation) * (pose.position - origin.transform.position), Quaternion.identity);

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    Place(pose);
                }
            }
        }
        else
        {
            placementIndicator.gameObject.SetActive(false);
            placementText.text = labelFindPlane;
        }
    }
}
