using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseUI : MonoBehaviour
{
    public TweenElement[] tweenElements;

    public virtual void Hide()
    {
        foreach (var elem in tweenElements)
            elem.TweenDown();
    }
}

public class MenuUI : BaseUI
{

}
