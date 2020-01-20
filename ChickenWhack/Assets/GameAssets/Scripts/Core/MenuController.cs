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
        if (InputController.GetEscapeDown())
        {
            ApplicationController.QuitApplication();
        }
        else if (!pressedStart)
        {
            if (InputController.GetTapDown(out Vector2 tapPos))
            {
                pressedStart = true;

                ApplicationController.StartGame();
                menuUI.Hide();
            }
        }
    }
}
