// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour
{
    public MenuUI menuUI;
    public GameObject menuObjects;
    public Camera menuCamera;

    bool pressedStart;

    private void Awake()
    {
        menuUI.gameObject.SetActive(false);

        menuObjects.SetActive(true);
        menuCamera.gameObject.SetActive(true); //needs to be active for performance calibration purposes
    }

    public void Open()
    {
        enabled = true;
        menuUI.gameObject.SetActive(true);
        menuObjects.SetActive(true);
        menuCamera.gameObject.SetActive(true);

        pressedStart = false;
    }

    public void Close()
    {
        enabled = false;
        menuUI.gameObject.SetActive(false);
        menuObjects.SetActive(false);
        menuCamera.gameObject.SetActive(false);
    }

    /// <summary>
    /// Check for input to start the game or quit the app
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ApplicationController.QuitApplication();
        }
        else if (!pressedStart)
        {
            if (EventSystem.current.IsPointerOverGameObject() || EventSystem.current.currentSelectedGameObject != null)
                return;

            if (Input.touchSupported && Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    pressedStart = true;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                pressedStart = true;
            }

            if (pressedStart)
            {
                ApplicationController.StartGame();
                menuUI.Hide();
            }
        }
    }
}
