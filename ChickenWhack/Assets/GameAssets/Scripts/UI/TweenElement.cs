// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenElement : MonoBehaviour
{
    public AnimationCurve scaleCurve;
    public float duration;
    public float delay;

    private void OnEnable()
    {
        StopAllCoroutines();

        transform.localScale = Vector3.zero;
        StartCoroutine(TweenCoroutine(false));
    }

    public void TweenDown()
    {
		if (gameObject.activeInHierarchy)
			StartCoroutine(TweenCoroutine(true));
    }

    IEnumerator TweenCoroutine(bool reverse)
    {
        if (!reverse)
            yield return new WaitForSecondsRealtime(delay);

        float t = 0f;
        while (t < 1f)
        {
            transform.localScale = Vector3.one * scaleCurve.Evaluate(reverse ? 1f - t : t);
            t += Time.deltaTime / duration;
            yield return null;
        }

        transform.localScale = reverse ? Vector3.zero : Vector3.one;
    }
}
