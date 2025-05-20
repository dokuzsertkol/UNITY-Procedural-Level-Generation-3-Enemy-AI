using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class npcWanderer : MonoBehaviour
{
    //
    public float attackRange = 3f;
    public float walkSpeed = 1.5f;
    public float runSpeed = 3.5f;
    public float loseCuriousTime = 5f;
    public float loseVisionTime = 5f;

    // wandering
    public float wanderRadius = 10f;
    public float wanderWait = 5f;

    // hearing and seeing
    public bool hear = true, see = true, debugHear = false, debugSee = false;
    public int rayCount = 10;
    public float hearAngle = 360f, 
        hearDetectionRange = 1f;
    public float seeDetectionRange = 5f, 
        seeAngleV = 60f, 
        seeAngleH = 120f;
    //

    public LayerMask playerLayer;

    // audio sources
    public AudioSource walk, enter_walk, enter_curious, curious, enter_chase, chase;

    private NavMeshAgent agent;

    private enum State
    {
        wander, chase, curious, followScream
    }
    private State state = State.wander;

    private bool wanderBool = true, canSee = false, canAttack = false, attackWait = false;
    private float visionTime = 0;
    private float curiousTime = 0;
    private Animator anim;
    private Transform player;
    private Healthbar healthbar;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").transform;
        healthbar = GameObject.FindWithTag("healthbar").GetComponent<Healthbar>();
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            canAttack = true;
            if (!attackWait)
            {
                attackWait = true;
                StartCoroutine(Attack());
            }
        }
        else
        {
            anim.SetBool("stand", isStanding());
            canAttack = false;
        }


        switch (state)
        {
            case State.wander: {
                agent.speed = walkSpeed;
                anim.SetBool("run", false);
                if (chase.isPlaying) chase.Stop();
                if (curious.isPlaying) curious.Stop();
                StartCoroutine(PlayWithDelay(walk));
                //Debug.Log("wander " + visionTime);
                if (wanderBool) Wander();
                break;
            }
            case State.chase: {
                agent.speed = runSpeed;
                anim.SetBool("run", true);
                if (walk.isPlaying) walk.Stop();
                if (curious.isPlaying) curious.Stop();
                StartCoroutine(PlayWithDelay(chase));
                //Debug.Log("chase " + visionTime);
                Chase();
                break;
            }
            case State.curious: {
                agent.speed = runSpeed;
                anim.SetBool("run", true);
                if (walk.isPlaying) walk.Stop();
                if (chase.isPlaying) chase.Stop();
                StartCoroutine(PlayWithDelay(curious));
                //Debug.Log("curious " + curiousTime);
                Curious();
                break;
            }
            case State.followScream:
                {
                    agent.speed = runSpeed;
                    anim.SetBool("run", true);
                    if (walk.isPlaying) walk.Stop();
                    if (chase.isPlaying) chase.Stop();
                    StartCoroutine(PlayWithDelay(curious));

                    if (isStanding()) state = State.wander;
                    break;
                }
        }
        if (TryToSee())
        {
            if (state != State.chase)
            {
                enter_chase.Play();
            }
            curiousTime = 0;
            state = State.chase;
            agent.SetDestination(player.position);
            canSee = true;
        }
        else canSee = false;
        if (TryToHear() && (state == State.wander || state == State.curious))
        {
            if (state == State.wander)
            {
                enter_curious.Play();
            }
            curiousTime = loseCuriousTime;
            state = State.curious;
            agent.SetDestination(player.position);
        }
    }

    private void Wander()
    {
        Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
        agent.SetDestination(newPos);
        Debug.DrawLine(transform.position, newPos, Color.green, 2f);
        wanderBool = false;
        StartCoroutine(WanderWait());
    }
    IEnumerator WanderWait()
    {
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }
        yield return new WaitForSeconds(wanderWait);
        wanderBool = true;
    }
    Vector3 RandomNavSphere(Vector3 origin, float distance, int layerMask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, distance, layerMask))
        {
            return navHit.position;
        }

        return origin;
    }

    private void Chase()
    {
        if (!canSee)
        {
            if (visionTime > 0)
            {
                agent.SetDestination(GameObject.FindWithTag("Player").transform.position);
                visionTime -= Time.deltaTime;
            }
            else
            {
                agent.SetDestination(transform.position);
                state = State.curious;
                curiousTime = loseCuriousTime;
                visionTime = 0;
            }
        }
    }
    IEnumerator Attack()
    {
        anim.SetBool("attack", true);
        yield return new WaitForSeconds(0.5f); // preparing

        if (canAttack && canSee)
        {
            healthbar.RemoveHealth();
            yield return null;
        }
        anim.SetBool("attack", false);

        yield return new WaitForSeconds(1); // cooldown
        attackWait = false;
    }

    private void Curious()
    {
        if (curiousTime > 0)
        {
            curiousTime -= Time.deltaTime;
            if (isStanding())
            {
                Vector3 pos = transform.position;
                float radius = 5;

                Vector3 newPos = RandomNavSphere(pos, radius, -1);
                agent.SetDestination(newPos);
                Debug.DrawLine(transform.position, newPos, Color.green, 2f);
            }
        }
        else
        {
            curiousTime = 0;
            state = State.wander;
        }
    } 

    private bool TryToSee()
    {
        // seeing
        for (int i = 0; i < rayCount; i++)
        {
            if (!see) break;
            float angleV = Random.Range(-seeAngleV / 2, seeAngleV / 2);
            for (int j = 0; j < rayCount; j++)
            {
                // Spread rays inside the cone
                float angleH = Random.Range(-seeAngleH / 2, seeAngleH / 2);
                Quaternion rotation = Quaternion.Euler(angleV, angleH, 0);
                Vector3 direction = rotation * transform.forward;

                RaycastHit hit;
                if (Physics.Raycast(transform.position + new Vector3(0, 1, 0), direction, out hit, seeDetectionRange))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        visionTime = loseVisionTime;
                        return true;
                    }
                }

                if (debugSee) Debug.DrawRay(transform.position + new Vector3(0, 1, 0), direction * seeDetectionRange, Color.red);
            }
        }
        return false;
    }

    private bool TryToHear()
    {
        // hearing
        for (int i = 0; i < rayCount; i++)
        {
            if (!hear) break;
            float angleV = Random.Range(-seeAngleV / 2, seeAngleV / 2);
            for (int j = 0; j < rayCount; j++)
            {
                float angleH = Random.Range(-hearAngle / 2, hearAngle / 2);
                Quaternion rotation = Quaternion.Euler(angleV, angleH, 0);
                Vector3 direction = rotation * transform.forward;

                RaycastHit hit;
                if (Physics.Raycast(transform.position + new Vector3(0, 1, 0), direction, out hit, hearDetectionRange, playerLayer))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        return true;
                    }
                }

                if (debugHear) Debug.DrawRay(transform.position + new Vector3(0, 1, 0), direction * hearDetectionRange, Color.green);
            }
        }
        return false;
    }
    IEnumerator PlayWithDelay(AudioSource audio)
    {
        yield return null;
        if (!audio.isPlaying) audio.Play();
    }
    private bool isStanding()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }


    public void HearScream()
    {
        state = State.followScream;
        agent.SetDestination(player.position);
    }
}
