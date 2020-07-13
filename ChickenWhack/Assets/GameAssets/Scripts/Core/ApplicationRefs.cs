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
    /// This is the factor that the game world (units) is scaled down relative to real world (meters) when placing it
    /// </summary>
    public float AR_initialScaledownFactor = 200f;

    public ARController AR_controller;

    public GameObject loadingScreen;

    public InputController inputController;
    public MenuController menuController;
    public GameController gameController;

    public float transitionsDuration = 0.5f;
}