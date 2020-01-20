// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

static class RectTransformShortcuts
{
    [MenuItem("CONTEXT/RectTransform/Set anchors to rect")]
    private static void SetAnchorsToRect(MenuCommand menuCommand)
    {
        SetAnchorsToRectInternal(menuCommand, false);
    }
    [MenuItem("CONTEXT/RectTransform/Set anchors to rect recursive")]
    private static void SetAnchorsToRectRecursive(MenuCommand menuCommand)
    {
        SetAnchorsToRectInternal(menuCommand, true);
    }
    //Validation
    [MenuItem("CONTEXT/RectTransform/Set anchors to rect", true)]
    [MenuItem("CONTEXT/RectTransform/Set anchors to rect recursive", true)]
    private static bool SetAnchorsToRectValidation(MenuCommand menuCommand)
    {
        var rectTransform = menuCommand.context as RectTransform;
        return rectTransform.parent is RectTransform;
    }

    private static void SetAnchorsToRectInternal(MenuCommand menuCommand, bool recurseIntoChildren)
    {
        var rectTransform = menuCommand.context as RectTransform;

        List<Object> objects = new List<Object>();

        objects.Add(rectTransform);

        if (recurseIntoChildren)
        {
            foreach (RectTransform child in rectTransform)
            {
                objects.Add(child);
            }
        }

        foreach (Object obj in objects)
            Undo.RecordObject(obj, "RectTransform");

        foreach (Object obj in objects)
            SetAnchorsToRectOperation((RectTransform)obj);
    }

    private static void SetAnchorsToRectOperation(RectTransform rectTransform)
    {
        var parentRectTransform = (RectTransform)rectTransform.parent;
        Vector2 parentSizeFactor = new Vector2(1f / parentRectTransform.rect.size.x, 1f / parentRectTransform.rect.size.y);
        Vector2 normalizedPosition = Vector2.Scale(rectTransform.localPosition, parentSizeFactor);
        Rect rect = rectTransform.rect;
        rectTransform.anchorMin = parentRectTransform.pivot + normalizedPosition + Vector2.Scale(rect.min, parentSizeFactor);
        rectTransform.anchorMax = parentRectTransform.pivot + normalizedPosition + Vector2.Scale(rect.max, parentSizeFactor);
        rectTransform.anchorMin = new Vector2(Mathf.Clamp01(rectTransform.anchorMin.x), Mathf.Clamp01(rectTransform.anchorMin.y));
        rectTransform.anchorMax = new Vector2(Mathf.Clamp01(rectTransform.anchorMax.x), Mathf.Clamp01(rectTransform.anchorMax.y));
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
