// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using UnityEngine;

/// <summary>
/// Bootstrapper launching the ApplicationController before any other script
/// </summary>
public class ApplicationLauncher : MonoBehaviour {

    public ApplicationRefs references;

    private void Awake()
    {
        ApplicationController.Launch(references);
    }
}