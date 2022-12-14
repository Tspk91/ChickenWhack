// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static PlayerController;

public class ChickenAgent : MonoBehaviour
{
    /// <summary>
    /// Idle: stay in place playing a random animation.
    /// Wander: choose a random nearby destination and walk there.
    /// Avoid player: flee from the player, walking or running depending on the danger.
    /// </summary>
    enum BehaviorState { IDLE, WANDER, AVOID_PLAYER }

    //Duration range of the random states (idle, wander)
    public float minBehaviorDuration = 2f;
    public float maxBehaviorDuration = 10f;

    //Probability of the random states (chance of wander respect to idle)
    [Range(0f, 1f)]
    public float wanderProb = 0.5f;

    //Stength of tendency to the center of the map when wandering
    public float wanderCenteringForce = 0.25f;

    //Maximum wander point distance
    public float wanderRadius = 5f;

    //Time until stopping player avoidance when there is no danger anymore
    public float fleeRecoverDuration = 2f;

    //Distance to the player to start fleeing at
    public float fleeRadius = 5f;

    //Speed multiplier when running respect to walking
    public float runSpeedMultiplier = 1.8f;

	//Delay between hit and death
	public float whackDelay = 0.15f;

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

    //Ids for faster execution
    int walkAnimID = Animator.StringToHash("Walk");
    int runAnimID = Animator.StringToHash("Run");
    int eatAnimID = Animator.StringToHash("Eat");
    int headAnimID = Animator.StringToHash("Turn Head");

    //Cache waits to avoid gc
    WaitForSeconds oneSecondWait = new WaitForSeconds(1f);
    WaitForSeconds halfSecondWait = new WaitForSeconds(0.5f);

    //Cache some variables, initialize randomized values
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

    /// <summary>
    /// Spawn (enable) in the given position if it is valid.
    /// </summary>
    public bool Spawn(ChickenManager manager, Vector3 spawnPos)
    {
        this.manager = manager;

        spawnPos.y = 0f;

        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit result, 1f, NavMesh.AllAreas))
        {
            navigation.Warp(result.position);
            transform.rotation = Quaternion.AngleAxis(360f * Random.value, Vector3.up);

            gameObject.SetActive(true);

            navigation.speed = baseSpeed;

            StartCoroutine(StateUpdateCoroutine());

            return true;
        }

        return false;
    }

    /// <summary>
    /// Despawn (disable) and return to pool
    /// </summary>
    public void Despawn()
    {
        this.CancelAllActions();
        StopAllCoroutines();

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

        //Start new state
        switch (newState)
        {
            case BehaviorState.IDLE: //Stop moving
                navigation.speed = 0f;
                break;
            case BehaviorState.WANDER: //Start wandering by setting a random destination
                navigation.speed = baseSpeed;
                animator.SetBool(walkAnimID, true);
                SetDestination(GetWanderDestinationCandidate);
                break;
            case BehaviorState.AVOID_PLAYER: //Start avoiding by setting a flee destination
                SetDestination(GetFleeDestinationCandidate);
                UpdateFleeState(false);
                break;
        }

        state = newState;
    }

    //Main update coroutine, lazy updating with waits. Can change state if state dependent conditions happen.
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

    //Runs in the background selecting either wander or idle at random intervals (except if avoiding the player)
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
			if (!animator.GetBool(runAnimID))
			{
				navigation.speed = baseSpeed * runSpeedMultiplier;
				animator.SetBool(walkAnimID, false);
				animator.SetBool(runAnimID, true);

				ApplicationController.refs.audioController.PlayEvent(AudioEvent.PLAY_CHICKENFLY);
			}

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

    //Flee condition (walk)
    bool CheckFlee()
    {
        bool playerIncoming = PlayerVelocity.sqrMagnitude > 1f &&
               Vector3.Dot(PlayerVelocity, transform.position - PlayerPosition) > 0f;

        return (transform.position - PlayerPosition).sqrMagnitude < (playerIncoming ? sqrFleeRadiusInternal : sqrFleeRelaxedRadiusInternal);
    }

    //Flee condition (run)
    bool CheckFastFlee()
    {
        return (transform.position - PlayerPosition).sqrMagnitude < sqrFleeFastRadiusInternal &&
               PlayerVelocity.sqrMagnitude > 1f &&
               Vector3.Dot(PlayerVelocity, transform.position - PlayerPosition) > 0.5f;
    }

    //Receive player hit
    public void Whack()
    {
		if (!isDying)
        {
            isDying = true;
			manager.ChickenWhacked();
			this.DelayedAction(DelayedWhack, whackDelay);
        }
    }

    private void DelayedWhack()
    {
		ApplicationController.refs.audioController.PlayEvent(AudioEvent.PLAY_WHACK);

		manager.PlayWhackEffect(transform.position);
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
