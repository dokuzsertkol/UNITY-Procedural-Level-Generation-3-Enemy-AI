using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class npcScreamer : MonoBehaviour
{

    //
    public float screamRange = 50f;

    // wandering
    public float wanderRadius = 10f;
    public float wanderWait = 5f;

    // hearing and seeing
    public bool hear = true, see = true, debugHear = false, debugSee = false;
    public int rayCount = 10;
    public float hearAngle = 360f,
        hearDetectionRange = 20f;
    public float seeDetectionRange = 3f,
        seeAngleV = 60f,
        seeAngleH = 120f;
    //
    public AudioSource wanderSound, screamSound; 

    public LayerMask playerLayer;

    private NavMeshAgent agent;
    private enum State
    {
        wander, scream
    }
    private State state = State.wander;

    private bool screamBool = true, canSee = false;
    private Animator anim;
    private Transform player;
    private List<npcSleeper> npcSleepers = new();
    private List<npcWanderer> npcWanderers = new();


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").transform;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("npcSleeper")) npcSleepers.Add(go.GetComponent<npcSleeper>());
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("npcWanderer")) npcWanderers.Add(go.GetComponent<npcWanderer>());
    }

    void Update()
    {
        switch (state)
        {
            case State.wander:
                {
                    if (!screamSound.isPlaying) StartCoroutine(PlayWithDelay(wanderSound));
                    if (isStanding()) Wander();
                    break;
                }
            case State.scream:
                {
                    if (wanderSound.isPlaying) wanderSound.Stop();
                    if (!screamSound.isPlaying) StartCoroutine(PlayWithDelay(screamSound));
                    if (screamBool) Scream();
                    break;
                }
        }

        if (TryToSee())
        {
            state = State.scream;
            canSee = true;
        }
        else canSee = false;
        if (TryToHear())
        {
            if (state == State.wander) agent.SetDestination(player.position);
        }
    }

    private void Wander()
    {
        anim.SetBool("scream", false);

        Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
        agent.SetDestination(newPos);
        Debug.DrawLine(transform.position, newPos, Color.green, 2f);
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

    private void Scream()
    {

        agent.SetDestination(transform.position);
        screamBool = false;
        anim.SetBool("scream", true);

        // scream
        foreach (var npc in npcSleepers)
        {
            if ((npc.transform.position - transform.position).sqrMagnitude <= screamRange * screamRange)
            {
                npc.HearScream();
            }
        }
        foreach (var npc in npcWanderers)
        {
            if ((npc.transform.position - transform.position).sqrMagnitude <= screamRange * screamRange)
            {
                npc.HearScream();
            }
        }


        StartCoroutine(ScreamWait());
    }
    private IEnumerator ScreamWait()
    {
        yield return new WaitForSeconds(5);
        if (!canSee) state = State.wander;
        screamBool = true;
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
}
