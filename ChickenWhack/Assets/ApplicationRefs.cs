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
    public static ApplicationRefs go;

    public static void Launch(ApplicationRefs refs)
    {
        go = refs;

        go.menuController.Open();
    }

    public static void StartGame()
    {
        go.menuController.Close();
        go.gameController.StartGameplay();
    }

    public static void ExitGame(ExitType exitType)
    {
        go.menuController.Open();
        go.gameController.StopGameplay();
    }

    public static void QuitApplication()
    {
        Application.Quit();
    }

    //Invoke utility

    public static Coroutine Invoke(System.Action action, float time)
    {
        return go.StartCoroutine(InvokeCoroutine(action, time));
    }

    public static void CancelInvoke(Coroutine invokeHandle)
    {
        go.StopCoroutine(invokeHandle);
    }

    private static IEnumerator InvokeCoroutine(System.Action action, float time)
    {
        yield return new WaitForSeconds(time);
        action();
    }

}