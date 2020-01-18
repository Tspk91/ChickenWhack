using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ExitType { MENU, WIN, LOSE }

public class ApplicationRefs : MonoBehaviour
{
    public MenuController menuController;
    public GameController gameController;
    public GameObject gameOver;

    private void Awake()
    {
        ApplicationController.Launch(this);
    }
}

public static class ApplicationController
{    
    public static ApplicationRefs refs;

    public static void Launch(ApplicationRefs refs)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        ApplicationController.refs = refs;

        ApplicationController.refs.menuController.Open();
    }

    public static void StartGame()
    {
        refs.menuController.Close();
        refs.gameController.StartGameplay();
    }

    public static void ExitGame(ExitType exitType)
    {
        refs.menuController.Open();
        refs.gameController.StopGameplay();
    }

    public static void QuitApplication()
    {
        Application.Quit();
    }
}