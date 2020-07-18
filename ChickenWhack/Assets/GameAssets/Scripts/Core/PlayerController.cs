// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls the player character, its lifecycle is controlled by its game object activation.
/// -Exposes some useful static variables
/// -Sets nav agent destination
/// -Controls animations
/// -Performs attacks against the chickens
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static Vector3 PlayerPosition { get; private set; }

    public static Vector3 PlayerVelocity { get; private set; }

	public ParticleSystem impactEffectPrefab;

	private GenericPool<ParticleSystem> impactEffectPool;

	Animator animator;

    //ids for faster execution
    int speedAnimID = Animator.StringToHash("MoveSpeed");
    int attack0AnimID = Animator.StringToHash("Attack01");
    int attack1AnimID = Animator.StringToHash("Attack02");
    int dieAnimID = Animator.StringToHash("Die");

    NavMeshAgent navigation;
    NavMeshPath path;

    SphereCollider trigger;

    bool canAttack = false;

    float smoothVelocity;

	float navSpeed;

    private void Awake()
    {
		impactEffectPool = new GenericPool<ParticleSystem>(impactEffectPrefab, 3);

		animator = GetComponent<Animator>();
        navigation = GetComponent<NavMeshAgent>();
		navSpeed = navigation.speed;
        trigger = GetComponent<SphereCollider>();

        path = new NavMeshPath();

        ApplicationController.refs.gameController.onGameEnded += OnGameEnded;
    }

    private void OnEnable()
    {
        UpdateStaticVars();

        canAttack = true;
        smoothVelocity = 0f;

		navigation.speed = navSpeed;

		navigation.Warp(Vector3.zero);
    }

    private void OnDisable()
    {
        this.CancelAllActions();
    }

    private void Update()
    {
        UpdateStaticVars();

        //Set animation speed parameter with smoothing
        smoothVelocity = Mathf.Lerp(smoothVelocity, navigation.velocity.magnitude, Time.deltaTime * 5f);
        animator.SetFloat(speedAnimID, smoothVelocity / navigation.speed);
    }

    void UpdateStaticVars()
    {
        PlayerPosition = transform.position;
        PlayerVelocity = navigation.velocity;
    }

    /// <summary>
    /// Tries to set a destination for the nav agent.
    /// Returns true if position search was succesful, and the found navmesh position
    /// </summary>
    public bool SetTargetPosition(Vector3 target, out Vector3 navPos)
    {
        bool valid = NavMesh.SamplePosition(target, out NavMeshHit result, 10f, NavMesh.AllAreas);

        if (valid)
        {
            navigation.CalculatePath(result.position, path);
            navigation.SetPath(path);
            navPos = path.corners[path.corners.Length - 1];
        }
        else
        {
            navPos = Vector3.zero;
        }

        return valid;
    }

    private void OnGameEnded(bool win)
    {
		this.CancelAllActions();

        canAttack = false;
        navigation.ResetPath();
        if (!win)
            animator.SetTrigger(dieAnimID);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Do random attack if we hit a trigger (chicken)
        if (canAttack)
        {
            canAttack = false;

            animator.SetTrigger(Random.value > 0.5f ? attack0AnimID : attack1AnimID);

			this.DelayedAction(CheckAttack, 0.25f);

			navigation.speed = 0f;
        }
    }

    Collider[] hitArray = new Collider[5];

    private void CheckAttack()
    {
        canAttack = true;

        //Check the attack did connect
        int hits = Physics.OverlapSphereNonAlloc(transform.position + transform.rotation * trigger.center, trigger.radius, hitArray, 1 << 0);

		if (hits > 0)
		{
			ApplicationController.refs.audioController.PlayEvent(AudioEvent.PLAY_IMPACT);

			for (int i = 0; i < hits; i++)
			{
				Collider hit = hitArray[i];
				//Hit the chicken
				hit.GetComponent<ChickenAgent>().Whack();

				var effect = impactEffectPool.GetObject(4f);
				effect.transform.position = Vector3.Lerp(hit.transform.position, hit.ClosestPoint(transform.position), 0.66f) + Vector3.up * 1.5f;
				effect.Play();
			}
		}
		else
		{
			ApplicationController.refs.audioController.PlayEvent(AudioEvent.PLAY_SWING);
		}

		navigation.speed = navSpeed;
	}

	private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(navigation.destination, 1f);
    }
}