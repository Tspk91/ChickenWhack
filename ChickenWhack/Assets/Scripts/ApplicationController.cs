// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using UnityEngine;

public enum GameExitType { MENU, WIN, LOSE, AR_PLACEMENT }

/// <summary>
/// Responsible for overall app flow
/// </summary>
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
        //Show loading screen
        refs.loadingScreen.SetActive(true);

        //Wait for calibration and AR initialization
        inTransition = true;
        yield return StartCoroutine(CalibrateSettings());
        yield return StartCoroutine(refs.AR_controller.TryInitAR(refs.gameController.gameplayCamera, refs.AR_scaledownFactor));
        inTransition = false;

        //Hide loading screen
        refs.loadingScreen.SetActive(false);
        refs.menuController.Open();
    }

    /// <summary>
    /// Async call to start gameplay (or AR placement)
    /// </summary>
    public static void StartGame()
    {
        if (inTransition)
            return;

        inTransition = true;

        StartCoroutine(StartGameInternal(refs.AR_controller.AR_Enabled));
    }

    private static IEnumerator StartGameInternal(bool doPlacement)
    {
        CameraFader.FadeDown();

        yield return new WaitForSecondsRealtime(refs.transitionsDuration);

        inTransition = false;
        refs.menuController.Close();

        //If we are in AR, do placement and then start the game, else start the game directly
        if (doPlacement)
            yield return refs.AR_controller.StartPlacement(refs.gameController.gameplayObjects.transform, refs.gameController.StartGameplay);
        else
            refs.gameController.StartGameplay();

        CameraFader.FadeUp();
    }

    /// <summary>
    /// Async call to stop gameplay (or AR placement)
    /// </summary>
    public static void ExitGame(GameExitType exitType, float delay)
    {
        if (inTransition)
            return;

        inTransition = true;

        StartCoroutine(ExitGameInternal(exitType, delay));
    }

    private static IEnumerator ExitGameInternal(GameExitType exitType, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        CameraFader.FadeDown();

        yield return new WaitForSecondsRealtime(refs.transitionsDuration);

        inTransition = false;

        refs.menuController.Open();

        if (exitType == GameExitType.AR_PLACEMENT)
            refs.gameController.gameplayCamera.gameObject.SetActive(false);
        else
            refs.gameController.StopGameplay();

        CameraFader.FadeUp();
    }

    public static void QuitApplication()
    {
        Application.Quit();
    }

    //Antialiasing calibration

    const int MSAA_ITERATIONS = 2;
    const int MSAA_FRAMES = 20;

    /// <summary>
    /// Sets highest antialiasing where it maintains the target framerate
    /// </summary>
    private static IEnumerator CalibrateSettings()
    {
        for (int i = 0; i < MSAA_FRAMES; i++) //Warmup rest
            yield return null;

        QualitySettings.antiAliasing = 4;

        int iterations = 0;

        float avgFramerate;

        while (iterations < MSAA_ITERATIONS)
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

    /// <summary>
    /// Shortcut to start coroutines directly from this class
    /// </summary>
    private static Coroutine StartCoroutine(IEnumerator coroutine)
    {
        return refs.StartCoroutine(coroutine);
    }
}