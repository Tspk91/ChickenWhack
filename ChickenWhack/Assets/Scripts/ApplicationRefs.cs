using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public enum ExitType { MENU, WIN, LOSE, AR_PLACEMENT }

public class ApplicationRefs : MonoBehaviour
{
    public int targetFps = 60;
    public float AR_scaledownFactor = 200f;

    public ARController AR_controller;

    public GameObject loadingScreen;

    public MenuController menuController;
    public GameController gameController;

    public GameObject gameOver;

    public float transitionsDuration = 0.5f;

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

        CameraFader.Init(refs.transitionsDuration, refs.menuController.menuCamera, refs.gameController.gameplayCamera);

        StartCoroutine(LoadCoroutine());
    }

    private static IEnumerator LoadCoroutine()
    {
        refs.loadingScreen.SetActive(true);

        inTransition = true;
        yield return StartCoroutine(CalibrateSettings());
        yield return StartCoroutine(refs.AR_controller.TryInitAR(refs.gameController.gameplayCamera, refs.AR_scaledownFactor));
        inTransition = false;

        refs.loadingScreen.SetActive(false);
        refs.menuController.Open();
    }

    public static void StartGame()
    {
        if (inTransition)
            return;

        inTransition = true;

        CameraFader.FadeDown();

        refs.DelayedAction(() =>
        {
            inTransition = false;
            refs.menuController.Close();

            if (refs.AR_controller.AR_Enabled)
                refs.AR_controller.StartPlacement(refs.gameController.gameplayObjects.transform, refs.gameController.StartGameplay);
            else
                refs.gameController.StartGameplay();

            CameraFader.FadeUp();

        }, refs.transitionsDuration);
    }

    public static void ExitGame(ExitType exitType, float delay)
    {
        if (inTransition)
            return;

        inTransition = true;

        refs.DelayedAction(() =>
        {
            CameraFader.FadeDown();

            refs.DelayedAction(() =>
            {
                inTransition = false;
                refs.menuController.Open();
                if (exitType != ExitType.AR_PLACEMENT)
                    refs.gameController.StopGameplay();

                CameraFader.FadeUp();

            }, refs.transitionsDuration);
        }, delay);
    }

    public static void QuitApplication()
    {
        Application.Quit();
    }

    //Antialiasing calibration

    const int MSAA_ITERATIONS = 2;
    const int MSAA_FRAMES = 20;

    private static IEnumerator CalibrateSettings()
    {
        for(int i = 0; i < MSAA_FRAMES; i++) //Warmup rest
            yield return null;

        QualitySettings.antiAliasing = 4;

        int iterations = 0;

        float avgFramerate;

        while(iterations < MSAA_ITERATIONS)
        {
            avgFramerate = refs.targetFps;

            int frameCount = 0;

            while (frameCount < MSAA_FRAMES)
            {
                yield return null;

                avgFramerate = Mathf.Lerp(avgFramerate, 1f / Time.deltaTime, 0.2f);

                frameCount++;
            }

            if (avgFramerate < refs.targetFps - 1f)
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

    private static Coroutine StartCoroutine(IEnumerator coroutine)
    {
        return refs.StartCoroutine(coroutine);
    }
}