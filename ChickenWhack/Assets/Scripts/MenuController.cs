using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject menuUI;
    public GameObject menuObjects;

    public void Open()
    {
        enabled = true;
        menuUI.SetActive(true);
        menuObjects.SetActive(true);
    }

    public void Close()
    {
        enabled = false;
        menuUI.SetActive(false);
        menuObjects.SetActive(false);
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
    }
}
