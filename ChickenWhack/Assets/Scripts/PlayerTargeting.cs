using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTargeting : MonoBehaviour
{
    public PlayerController player;

    public ParticleSystem destinationEffectPrefab;

    private GenericPool<ParticleSystem> destinationEffectPool;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        destinationEffectPool = new GenericPool<ParticleSystem>(destinationEffectPrefab, 5);
    }

    private void Update()
    {
        if (Input.touchSupported)
        {
            if(Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    SetTarget(touch.position, true);
                }
            }
        }
        else
        {
            bool mouseDown = Input.GetMouseButtonDown(0);

            if(mouseDown || Input.GetMouseButton(0))
                SetTarget(Input.mousePosition, mouseDown);
        }
    }

    private void SetTarget(Vector2 input, bool spawnEffect)
    {
        Vector3 camPos = cam.transform.position;
        Vector3 wPos = cam.ScreenToWorldPoint(new Vector3(input.x, input.y, 1f));
        Vector3 dir = (wPos - camPos).normalized;
        float dot = Vector3.Dot(dir, Vector3.up);
        Vector3 planePos = camPos - dir * camPos.y / dot;

        planePos = Vector3.ClampMagnitude(planePos, ApplicationController.refs.gameController.gameAreaRadius);

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