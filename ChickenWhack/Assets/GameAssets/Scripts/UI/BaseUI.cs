// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseUI : MonoBehaviour {
    public TweenElement[] tweenElements;

    public virtual void Hide()
    {
        foreach (var elem in tweenElements)
            elem.TweenDown();
    }
}