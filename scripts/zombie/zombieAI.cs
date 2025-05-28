using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour
{
    public float detectionRadius = 15f;
    public float attackRadius = 2f;
    public float attackCooldown = 2f;

    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private float lastAttackTime;
    private bool navMeshReady = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        StartCoroutine(WaitForNavMesh());
    }

    System.Collections.IEnumerator WaitForNavMesh()
    {
        yield return new WaitForSeconds(0.1f); // чуть подождать после спавна
        if (agent.isOnNavMesh)
        {
            navMeshReady = true;
        }
        else
        {
            Debug.LogWarning($"[ZombieAI] Агент {gameObject.name} не стоит на NavMesh! AI будет отключён.");
            enabled = false; // отключаем AI если он не на mesh
        }
    }

    void Update()
    {
        if (!navMeshReady || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRadius)
        {
            if (distance > attackRadius)
            {
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                }
                animator.SetFloat("Speed", agent.velocity.magnitude);
            }
            else
            {
                if (agent.isOnNavMesh)
                    agent.isStopped = true;

                animator.SetFloat("Speed", 0f);

                // Поворачиваемся к игроку
                Vector3 lookDir = (player.position - transform.position).normalized;
                lookDir.y = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);

                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    animator.SetTrigger("Attack");
                    lastAttackTime = Time.time;

                    InfectionSystem playerInfection = player.GetComponent<InfectionSystem>();
                    if (playerInfection != null)
                    {
                        playerInfection.AddInfection(15f); // можно варьировать
                    }
                }
            }
        }
        else
        {
            if (agent.isOnNavMesh)
                agent.isStopped = true;

            animator.SetFloat("Speed", 0f);
        }
    }
}
