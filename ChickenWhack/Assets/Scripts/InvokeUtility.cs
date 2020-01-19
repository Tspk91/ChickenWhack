// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom delayed invoke cancellable by instance or by object, and no strings attached
/// </summary>
public static class InvokeUtility
{
    private static uint nextID = 0;

    private static Dictionary<uint, Coroutine> coroutinesByInvoke = new Dictionary<uint, Coroutine>();
    private static Dictionary<uint, object> objectsByInvoke = new Dictionary<uint, object>();
    private static Dictionary<object, HashSet<uint>> invokesByObject = new Dictionary<object, HashSet<uint>>();

    public static uint DelayedAction(this object obj, System.Action action, float time)
    {
        uint id = nextID++;

        var coroutine = ApplicationController.refs.StartCoroutine(InvokeCoroutine(action, time, id));

        coroutinesByInvoke.Add(id, coroutine);
        objectsByInvoke.Add(id, obj);

        if (invokesByObject.TryGetValue(obj, out HashSet<uint> hashSet))
        {
            hashSet.Add(id);
        }
        else
        {
            var newHashSet = new HashSet<uint>();
            newHashSet.Add(id);
            invokesByObject.Add(obj, newHashSet);
        }

        return id;
    }

    public static void CancelAllActions(this object obj)
    {
        if (invokesByObject.TryGetValue(obj, out HashSet<uint> invokes))
            foreach (var invoke in invokes)
                obj.CancelAction(invoke);
    }

    public static void CancelAction(this object obj, uint invoke)
    {
        if (coroutinesByInvoke.TryGetValue(invoke, out Coroutine coroutine))
        {
            ApplicationController.refs.StopCoroutine(coroutine);
            RemoveInvoke(invoke);
        }
    }

    private static void RemoveInvoke(uint invoke)
    {
        coroutinesByInvoke.Remove(invoke);
        objectsByInvoke.Remove(invoke);

        if (objectsByInvoke.TryGetValue(invoke, out object obj))
        {
            invokesByObject[obj].Remove(invoke);
        }
    }

    private static IEnumerator InvokeCoroutine(System.Action action, float time, uint id)
    {
        yield return new WaitForSeconds(time);
        action();

        RemoveInvoke(id);
    }
}
