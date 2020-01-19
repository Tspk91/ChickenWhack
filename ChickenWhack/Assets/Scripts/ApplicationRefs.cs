// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using UnityEngine;

/// <summary>
/// Component exposing key references and settings.
/// </summary>
public class ApplicationRefs : MonoBehaviour
{
    public int targetFps = 60;
    
    /// <summary>
    /// This is the factor that the game world (units) is scaled down relative to real world (meters)
    /// </summary>
    public float AR_scaledownFactor = 200f;

    public ARController AR_controller;

    public GameObject loadingScreen;

    public MenuController menuController;
    public GameController gameController;

    public GameObject gameOver;

    public float transitionsDuration = 0.5f;
}