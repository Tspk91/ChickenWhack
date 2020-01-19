using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARController : MonoBehaviour
{
    public bool AR_Enabled { get { return session.isActiveAndEnabled; } }

    public ARSession session;
    public ARSessionOrigin origin;
    public ARRaycastManager raycastManager;

    public GameObject cancelButton;

    public Transform placementIndicator;

    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    Camera AR_camera;
    float scaleFactor = 1f;

    Transform objectToPlace;
    System.Action onPlaced;

    private void Awake()
    {
        cancelButton.SetActive(false);
        placementIndicator.gameObject.SetActive(false);
    }

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
            origin.camera = AR_camera;
            AR_camera.transform.SetParent(origin.transform);

            this.AR_camera = AR_camera;
            this.scaleFactor = scaleFactor;
        }
    }

    public void StartPlacement(Transform objectToPlace, System.Action onPlaced)
    {
        this.objectToPlace = objectToPlace;
        this.onPlaced = onPlaced;

        cancelButton.SetActive(true);
        placementIndicator.gameObject.SetActive(true);
        AR_camera.gameObject.SetActive(true);
    }

    public void CancelPlacement()
    {
        EndPlacement();

        ApplicationController.ExitGame(ExitType.AR_PLACEMENT, 0f);
    }

    private void EndPlacement()
    {
        objectToPlace = null;
        onPlaced = null;

        cancelButton.SetActive(false);
        placementIndicator.gameObject.SetActive(false);
    }

    public void Place(Pose pose)
    {
        origin.MakeContentAppearAt(objectToPlace, pose.position, Quaternion.identity);
        onPlaced();

        EndPlacement();
    }

    void Update()
    {
        if (objectToPlace == null)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
            return;
        }

        Vector2 screenPoint = AR_camera.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));

        if (raycastManager.Raycast(screenPoint, hits, TrackableType.Planes))
        {
            Pose pose = hits[0].pose;
            placementIndicator.SetPositionAndRotation(pose.position, pose.rotation);

            if (!EventSystem.current.IsPointerOverGameObject() && Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    Place(pose);
                }
            }
        }

    }
}
