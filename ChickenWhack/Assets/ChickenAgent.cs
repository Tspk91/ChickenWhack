using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChickenAgent : MonoBehaviour
{
    enum BehaviorState { IDLE, WANDER, AVOID_PLAYER }

    public float minBehaviorDuration = 2f;
    public float maxBehaviorDuration = 10f;

    [Range(0f, 1f)]
    public float wanderProb = 0.5f;

    public float wanderRadius = 5f;

    public float fleeRecoverDuration = 2f;

    public float fleeRadius = 5f;

    float sqrFleeRadiusInternal;
    float sqrFleeFastRadiusInternal;
    float sqrFleeRelaxedRadiusInternal;

    Animator animator;
    NavMeshAgent navigation;

    float baseSpeed;

    BehaviorState state = BehaviorState.IDLE;

    float sqrMinWanderDist;

    int walkAnimID = Animator.StringToHash("Walk");
    int runAnimID = Animator.StringToHash("Run");
    int eatAnimID = Animator.StringToHash("Eat");
    int headAnimID = Animator.StringToHash("Turn Head");

    WaitForSeconds oneSecondWait = new WaitForSeconds(1f);
    WaitForSeconds halfSecondWait = new WaitForSeconds(0.5f);

    void Awake()
    {
        animator = GetComponent<Animator>();

        navigation = GetComponent<NavMeshAgent>();

        baseSpeed = navigation.speed;

        sqrMinWanderDist = (navigation.radius * 2f);
        sqrMinWanderDist *= sqrMinWanderDist;

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

    IEnumerator RandomStateCoroutine()
    {
        while(true)
        {
            if (state != BehaviorState.AVOID_PLAYER)
                SelectRandomState();

            yield return new WaitForSecondsRealtime(Random.Range(minBehaviorDuration, maxBehaviorDuration));
        }
    }

    IEnumerator StateUpdateCoroutine()
    {
        yield return new WaitForSeconds(Random.value); //Randomize behavior start time

        animator.playbackTime = 0f;

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

    bool CheckFlee()
    {
        bool playerIncoming = PlayerController.PlayerVelocity.sqrMagnitude > 1f &&
               Vector3.Dot(PlayerController.PlayerVelocity, transform.position - PlayerController.PlayerPosition) > 0f;

        return (transform.position - PlayerController.PlayerPosition).sqrMagnitude < (playerIncoming ? sqrFleeRadiusInternal : sqrFleeRelaxedRadiusInternal);
    }

    bool CheckFastFlee()
    {
        return (transform.position - PlayerController.PlayerPosition).sqrMagnitude < sqrFleeFastRadiusInternal &&
               PlayerController.PlayerVelocity.sqrMagnitude > 1f &&
               Vector3.Dot(PlayerController.PlayerVelocity, transform.position - PlayerController.PlayerPosition) > 0.5f;
    }

    void UpdateFleeState(bool computeDest)
    {
        if (CheckFastFlee())
        {
            navigation.speed = baseSpeed * 2f;
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

    void SetDestination(System.Func<Vector3> destinationFunction)
    {
        Vector3 pos;

        do
        {
            pos = destinationFunction();
        }
        while (!NavMesh.SamplePosition(pos, out NavMeshHit result, 2f, NavMesh.AllAreas));

        navigation.SetDestination(pos);
    }

    Vector3 GetWanderDestinationCandidate()
    {
        Vector3 pos;

        do
        {
            pos = transform.position + Random.insideUnitSphere * wanderRadius;
            pos.y = 0f;
        } 
        while ((pos - transform.position).sqrMagnitude < sqrMinWanderDist);

        return pos;
    }

    Vector3 GetFleeDestinationCandidate()
    {
        Vector3 fleeDirection = (transform.position - PlayerController.PlayerPosition).normalized;
        Vector3 fleePosition = transform.position + fleeDirection * 20f + Random.insideUnitSphere * 10f;
        fleePosition.y = 0f;
        return fleePosition;
    }

    public bool Spawn(Vector3 spawnPos)
    {
        spawnPos.y = 0f;

        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit result, 1f, NavMesh.AllAreas))
        {
            transform.position = spawnPos;
            transform.rotation = Quaternion.AngleAxis(360f * Random.value, Vector3.up);
            gameObject.SetActive(true);

            alive = true;

            return true;
        }

        return false;
    }

    public void Despawn()
    {
        gameObject.SetActive(false);

        alive = false;

        GenericPool<ChickenAgent>.ReturnObject(this);
    }

    public event System.Action<Vector3> onTakedown = delegate { };

    bool alive = false;

    public void Takedown()
    {
        if (alive)
        {
            alive = false;
            ApplicationController.Invoke(TakedownInternal, 0.15f);
        }
    }

    private void TakedownInternal()
    {
        onTakedown(transform.position);
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
