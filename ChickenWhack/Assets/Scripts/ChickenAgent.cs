// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static PlayerController;

public class ChickenAgent : MonoBehaviour
{
    enum BehaviorState { IDLE, WANDER, AVOID_PLAYER }

    public float minBehaviorDuration = 2f;
    public float maxBehaviorDuration = 10f;

    [Range(0f, 1f)]
    public float wanderProb = 0.5f;

    public float wanderCenteringForce = 0.25f;

    public float wanderRadius = 5f;

    public float fleeRecoverDuration = 2f;

    public float fleeRadius = 5f;

    public float runSpeedMultiplier = 1.8f;

    float sqrFleeRadiusInternal;
    float sqrFleeFastRadiusInternal;
    float sqrFleeRelaxedRadiusInternal;

    ChickenManager manager;

    Animator animator;
    NavMeshAgent navigation;

    float baseSpeed;

    BehaviorState state = BehaviorState.IDLE;

    bool isDying = false;

    float minWanderDist;

    int walkAnimID = Animator.StringToHash("Walk");
    int runAnimID = Animator.StringToHash("Run");
    int eatAnimID = Animator.StringToHash("Eat");
    int headAnimID = Animator.StringToHash("Turn Head");

    WaitForSeconds oneSecondWait = new WaitForSeconds(1f);
    WaitForSeconds halfSecondWait = new WaitForSeconds(0.5f);

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        navigation = GetComponent<NavMeshAgent>();

        baseSpeed = navigation.speed;

        sqrFleeRadiusInternal = fleeRadius * Random.Range(0.75f, 1.5f);
        sqrFleeRadiusInternal *= sqrFleeRadiusInternal;

        sqrFleeFastRadiusInternal = sqrFleeRadiusInternal * 0.7f * 0.7f;

        sqrFleeRelaxedRadiusInternal = sqrFleeRadiusInternal * 0.5f * 0.5f;

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        navigation.speed = baseSpeed;

        StartCoroutine(StateUpdateCoroutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public bool Spawn(ChickenManager manager, Vector3 spawnPos)
    {
        this.manager = manager;

        spawnPos.y = 0f;

        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit result, 1f, NavMesh.AllAreas))
        {
            navigation.Warp(result.position);
            transform.rotation = Quaternion.AngleAxis(360f * Random.value, Vector3.up);

            gameObject.SetActive(true);

            return true;
        }

        return false;
    }

    public void Despawn()
    {
        this.CancelAllActions();

        gameObject.SetActive(false);

        isDying = false;

        GenericPool<ChickenAgent>.ReturnObject(this);
    }

    void SetState(BehaviorState newState)
    {
        //Stop previous state animations if new state is different
        if (state != newState)
        {
            switch (state)
            {
                case BehaviorState.IDLE:
                    animator.SetBool(eatAnimID, false);
                    animator.SetBool(headAnimID, false);
                    break;
                case BehaviorState.WANDER:
                    animator.SetBool(walkAnimID, false);
                    break;
                case BehaviorState.AVOID_PLAYER:
                    animator.SetBool(walkAnimID, false);
                    animator.SetBool(runAnimID, false);
                    break;
            }
        }

        switch (newState)
        {
            case BehaviorState.IDLE:
                navigation.speed = 0f;
                break;
            case BehaviorState.WANDER:
                navigation.speed = baseSpeed;
                animator.SetBool(walkAnimID, true);
                SetDestination(GetWanderDestinationCandidate);
                break;
            case BehaviorState.AVOID_PLAYER:
                SetDestination(GetFleeDestinationCandidate);
                UpdateFleeState(false);
                break;
        }

        state = newState;
    }

    IEnumerator StateUpdateCoroutine()
    {
        yield return new WaitForSeconds(Random.value); //Randomize behavior start time, this will spread perf cost

        StartCoroutine(RandomStateCoroutine());

        while (true)
        {
            switch(state)
            {
                case BehaviorState.IDLE:
                    if (CheckFlee())
                    {
                        SetState(BehaviorState.AVOID_PLAYER);
                        yield return null;
                    }
                    else
                    {
                        UpdateIdleState();
                        yield return oneSecondWait;
                    }
                    break;
                case BehaviorState.WANDER:
                    if (CheckFlee())
                    {
                        SetState(BehaviorState.AVOID_PLAYER);
                        yield return null;
                    }
                    else if (navigation.remainingDistance < 1f)
                    {
                        SelectRandomState();
                        yield return null;
                    }
                    else
                    {
                        yield return oneSecondWait;
                    }
                    break;
                case BehaviorState.AVOID_PLAYER:
                    if (!CheckFlee())
                    {
                        yield return new WaitForSeconds(fleeRecoverDuration * Random.value);
                        SelectRandomState();
                    }
                    else
                    {
                        UpdateFleeState(true);
                        yield return halfSecondWait;
                    }
                    break;
            }
        }
    }

    IEnumerator RandomStateCoroutine()
    {
        while (true)
        {
            if (state != BehaviorState.AVOID_PLAYER)
                SelectRandomState();

            yield return new WaitForSecondsRealtime(Random.Range(minBehaviorDuration, maxBehaviorDuration));
        }
    }

    void SelectRandomState()
    {
        var prob = Random.value;
        if (prob < wanderProb)
        {
            SetState(BehaviorState.WANDER);
        }
        else
        {
            SetState(BehaviorState.IDLE);
        }
    }

    void UpdateFleeState(bool computeDest)
    {
        if (CheckFastFlee())
        {
            navigation.speed = baseSpeed * runSpeedMultiplier;
            animator.SetBool(walkAnimID, false);
            animator.SetBool(runAnimID, true);
            if (computeDest)
                SetDestination(GetFleeDestinationCandidate);
        }
        else if (!animator.GetBool(walkAnimID))
        {
            navigation.speed = baseSpeed;
            animator.SetBool(runAnimID, false);
            animator.SetBool(walkAnimID, true);
        }
    }

    void UpdateIdleState()
    {
        if (animator.GetBool(eatAnimID))
        {
            animator.SetBool(eatAnimID, false);
        }
        else if (animator.GetBool(headAnimID))
        {
            animator.SetBool(headAnimID, false);
        }
        else
        {
            var prob = Random.value;
            if (prob < 0.33f)
            {
                animator.SetBool(eatAnimID, true);
            }
            else if (prob < 0.66f)
            {
                animator.SetBool(headAnimID, true);
            }
        }
    }

    void SetDestination(System.Func<Vector3> destinationFunction)
    {
        Vector3 pos;

        NavMeshHit result;

        float gameRadiusSqr = ApplicationController.refs.gameController.gameAreaRadius;
        gameRadiusSqr *= gameRadiusSqr;

        //Try destination candidates until they are inside game bounds and navmesh area
        do
        {
            pos = destinationFunction();
        }
        while (pos.sqrMagnitude > gameRadiusSqr || !NavMesh.SamplePosition(pos, out result, 4f, NavMesh.AllAreas));

        navigation.SetDestination(result.position);
    }

    Vector3 GetWanderDestinationCandidate()
    {
        Vector3 direction = Quaternion.AngleAxis(360f * Random.value, Vector3.up) * Vector3.forward;
        direction += (Vector3.zero - transform.position).normalized * Random.value * wanderCenteringForce;
        return transform.position + direction.normalized * Random.Range(navigation.radius * 2f, wanderRadius);
    }

    Vector3 GetFleeDestinationCandidate()
    {
        Vector3 fleeDirection = (transform.position - PlayerPosition).normalized;
        Vector3 fleePosition = transform.position + fleeDirection * 20f;
        fleePosition = Vector3.ClampMagnitude(fleePosition, ApplicationController.refs.gameController.gameAreaRadius);
        fleePosition += Random.insideUnitSphere * 10f;
        fleePosition.y = 0f;
        return fleePosition;
    }

    bool CheckFlee()
    {
        bool playerIncoming = PlayerVelocity.sqrMagnitude > 1f &&
               Vector3.Dot(PlayerVelocity, transform.position - PlayerPosition) > 0f;

        return (transform.position - PlayerPosition).sqrMagnitude < (playerIncoming ? sqrFleeRadiusInternal : sqrFleeRelaxedRadiusInternal);
    }

    bool CheckFastFlee()
    {
        return (transform.position - PlayerPosition).sqrMagnitude < sqrFleeFastRadiusInternal &&
               PlayerVelocity.sqrMagnitude > 1f &&
               Vector3.Dot(PlayerVelocity, transform.position - PlayerPosition) > 0.5f;
    }

    public void Whack()
    {
        if (!isDying)
        {
            isDying = true;
            this.DelayedAction(WhackInternal, 0.15f);
        }
    }

    private void WhackInternal()
    {
        manager.ChickenWhacked(transform.position);
        Despawn();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, state.ToString());
#endif
    }

}
