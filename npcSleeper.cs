using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class npcSleeper : MonoBehaviour
{
    public float attackRange = 3f;
    public float loseInterestTime = 5f;

    public bool hear = true, see = true, debugHear = false, debugSee = false;
    public int rayCount = 10;
    public float hearAngle = 360f,
        hearDetectionRange = 1f;

    public LayerMask playerLayer;

    public float seeDetectionRange = 5f,
        seeAngleV = 60f,
        seeAngleH = 120f;
    public AudioSource walk, sleep;

    private enum State
    {
        sleep, chase, followScream
    }
    private State state = State.sleep;

    private NavMeshAgent agent;
    private Animator anim;
    private bool canSee = false, canAttack = true, attackWait = false;
    private float interestTime;
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
            canAttack = false;
        }
        switch (state)
        {
            case State.sleep:
                {
                    if (walk.isPlaying) walk.Stop();
                    StartCoroutine(PlayWithDelay(sleep));
                    anim.SetBool("walk", false);
                    break;
                }
            case State.chase:
                {
                    if (sleep.isPlaying) sleep.Stop();
                    StartCoroutine(PlayWithDelay(walk));
                    anim.SetBool("walk", true);
                    Chase();
                    break;
                }
            case State.followScream:
                {
                    if (sleep.isPlaying) sleep.Stop();
                    StartCoroutine(PlayWithDelay(walk));
                    anim.SetBool("walk", true);

                    if (isStanding()) state = State.sleep;
                    break;
                }
        }
        if (TryToHear())
        {
            state = State.chase;
            agent.SetDestination(player.position);
            interestTime = loseInterestTime;
        }
        if (TryToSee())
        {
            state = State.chase;
            agent.SetDestination(GameObject.FindWithTag("Player").transform.position);
            canSee = true;
            interestTime = loseInterestTime;
        }
        else canSee = false;
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

    private void Chase()
    {
        if (!canSee)
        {
            if (interestTime > 0)
            {
                agent.SetDestination(player.position);
                interestTime -= Time.deltaTime;
            }
            else
            {
                agent.SetDestination(transform.position);
                state = State.sleep;
                interestTime = 0;
            }
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
