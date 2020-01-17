using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public static Vector3 PlayerPosition { get; private set; }

    public static Vector3 PlayerVelocity { get; private set; }

    Animator animator;

    int speedAnimID = Animator.StringToHash("MoveSpeed");
    int attack0AnimID = Animator.StringToHash("Attack01");
    int attack1AnimID = Animator.StringToHash("Attack02");
    int dieAnimID = Animator.StringToHash("Die");

    NavMeshAgent navigation;
    NavMeshPath path;

    SphereCollider trigger;

    bool attacking = false;

    float smoothVelocity;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        navigation = GetComponent<NavMeshAgent>();
        trigger = GetComponent<SphereCollider>();

        path = new NavMeshPath();
    }

    private void OnEnable()
    {
        UpdateStaticVars();

        attacking = false;
        smoothVelocity = 0f;

        transform.position = Vector3.zero;
    }

    private void Update()
    {
        UpdateStaticVars();

        smoothVelocity = Mathf.Lerp(smoothVelocity, navigation.velocity.magnitude, Time.deltaTime * 4f);

        animator.SetFloat(speedAnimID, smoothVelocity / navigation.speed);
    }

    void UpdateStaticVars()
    {
        PlayerPosition = transform.position;
        PlayerVelocity = navigation.velocity;
    }

    public bool SetTargetPosition(Vector3 target, out Vector3 navPos)
    {
        bool valid = NavMesh.SamplePosition(target, out NavMeshHit result, 4f, NavMesh.AllAreas);

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

    private void OnTriggerEnter(Collider other)
    {
        if (!attacking)
        {
            attacking = true;

            animator.SetTrigger(Random.value > 0.5f ? attack0AnimID : attack1AnimID);

            ApplicationController.Invoke(CheckAttack, 0.4f);
        }
    }

    Collider[] hitArray = new Collider[5];

    private void CheckAttack()
    {
        attacking = false;

        int hits = Physics.OverlapSphereNonAlloc(transform.position + transform.rotation * trigger.center, trigger.radius, hitArray, 1 << 0);

        for (int i = 0; i < hits; i++)
        {
            ChickenAgent hitAgent = hitArray[i].GetComponent<ChickenAgent>();
            hitAgent.Takedown();
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(navigation.destination, 1f);
    }
}