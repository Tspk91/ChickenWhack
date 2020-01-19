using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public MenuUI menuUI;
    public GameObject menuObjects;
    public Camera menuCamera;

    private void Awake()
    {
        menuUI.gameObject.SetActive(false);

        menuObjects.SetActive(true);
        menuCamera.gameObject.SetActive(true);
    }

    public void Open()
    {
        enabled = true;
        menuUI.gameObject.SetActive(true);
        menuObjects.SetActive(true);
        menuCamera.gameObject.SetActive(true);
    }

    public void Close()
    {
        enabled = false;
        menuUI.gameObject.SetActive(false);
        menuObjects.SetActive(false);
        menuCamera.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ApplicationController.QuitApplication();
        }
    }

    public void OnPressStart()
    {
        ApplicationController.StartGame();

        menuUI.Hide();
    }
}
