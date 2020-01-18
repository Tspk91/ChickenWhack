using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public enum ExitType { MENU, WIN, LOSE }

public class ApplicationRefs : MonoBehaviour
{
    public int targetFps = 60;
    public float AR_scaledownFactor = 200f;

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

    private static bool inTransition = false;

    public static void Launch(ApplicationRefs refs)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = refs.targetFps;

        ApplicationController.refs = refs;

        StartCoroutine(InitializationCoroutine());

        refs.menuController.Open();
    }

    public static void StartGame()
    {
        if (inTransition)
            return;

        inTransition = true;
        refs.DelayedAction(() =>
        {
            inTransition = false;
            refs.menuController.Close();
            refs.gameController.StartGameplay();
        }, 0.25f);
    }

    public static void ExitGame(ExitType exitType)
    {
        if (inTransition)
            return;

        inTransition = true;
        refs.DelayedAction(() =>
        {
            inTransition = false;
            refs.menuController.Open();
            refs.gameController.StopGameplay();
        }, 0.25f);
    }

    public static void QuitApplication()
    {
        Application.Quit();
    }

    const int MSAA_ITERATIONS = 2;
    const int MSAA_FRAMES = 20;
    const int MSAA_FPS = 60;

    //Initialization coroutines

    private static IEnumerator InitializationCoroutine()
    {
        inTransition = true;
        yield return StartCoroutine(CalibrateSettings());
        yield return StartCoroutine(CheckAR());
        inTransition = false;
    }

    private static IEnumerator CalibrateSettings()
    {
        for(int i = 0; i < MSAA_FRAMES; i++) //Warmup rest
            yield return null;

        QualitySettings.antiAliasing = 4;

        int iterations = 0;

        float avgFramerate;

        while(iterations < MSAA_ITERATIONS)
        {
            avgFramerate = 60;

            int frameCount = 0;

            while (frameCount < MSAA_FRAMES)
            {
                yield return null;

                avgFramerate = Mathf.Lerp(avgFramerate, 1f / Time.deltaTime, 0.2f);

                frameCount++;
            }

            if (avgFramerate < MSAA_FPS - 1f)
            {
                QualitySettings.antiAliasing /= 2;
                iterations++;
            }
            else
            {
                break;
            }
        }

        Debug.Log("Calibrated MSAA to " + QualitySettings.antiAliasing + "x");
    }

    private static IEnumerator CheckAR()
    {        
        if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
        {
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            Debug.Log("AR unsupported!");
        }
        else
        {
            Debug.Log("Starting AR session!");

            var origin = refs.GetComponentInChildren<ARSessionOrigin>();
            origin.gameObject.SetActive(true);
            //origin.transform.localScale = refs.AR_scaledownFactor * Vector3.one;
            refs.gameController.gameplayCamera.transform.SetParent(origin.transform);
        }
    }

    private static Coroutine StartCoroutine(IEnumerator coroutine)
    {
        return refs.StartCoroutine(coroutine);
    }
}