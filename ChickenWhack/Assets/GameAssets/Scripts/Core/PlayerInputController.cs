// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Processes inputs to set the player character target position
/// </summary>
public class PlayerInputController : MonoBehaviour
{
    public PlayerController player;
    private GameController gameController;

    public ParticleSystem destinationEffectPrefab;

    private GenericPool<ParticleSystem> destinationEffectPool;

    private Camera cam;

    private void Awake()
    {
        gameController = ApplicationController.refs.gameController;

        cam = gameController.gameplayCamera;

        destinationEffectPool = new GenericPool<ParticleSystem>(destinationEffectPrefab, 5);
    }

    private void OnEnable()
    {
        //Turn character towards the camera
        Vector3 camDir = (cam.transform.position - player.transform.position);
        camDir.y = 0f;
        player.transform.rotation = Quaternion.LookRotation(camDir);
    }

    //Check for input while player is active, unless game is over
    private void Update()
    {
        if (gameController.GameEnded)
            return;

        if (InputController.GetTapDown(out Vector2 tapPos))
        {
            SetTarget(tapPos, true);
        }
        else if(!Input.touchSupported && InputController.GetTap(out tapPos))
        {
            SetTarget(tapPos, false);
        }
    }

    /// <summary>
    /// Sends target position to playercontroller, if valid, spawn effect
    /// </summary>
    private void SetTarget(Vector2 input, bool spawnEffect)
    {
        Vector3 camPos = cam.transform.position;
        Vector3 wPos = cam.ScreenToWorldPoint(new Vector3(input.x, input.y, 1f));
        Vector3 dir = (wPos - camPos).normalized;
        float dot = Vector3.Dot(dir, Vector3.up);
        Vector3 planePos = camPos - dir * camPos.y / dot;

        planePos = Vector3.ClampMagnitude(planePos, gameController.gameAreaRadius);

        if (player.SetTargetPosition(planePos, out Vector3 navPos))
        {
            if (spawnEffect)
            {
                var effect = destinationEffectPool.GetObject(4f);
                effect.transform.position = navPos;
                effect.Play();
            }
        }
    }
}