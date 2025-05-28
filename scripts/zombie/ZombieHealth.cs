using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public Rigidbody[] ragdollParts; // ��� �����
    public Animator anim;
    public GameObject smokeEffect;

    [Header("Ragdoll Sound FX")]
    public AudioClip impactSound;
    public float impactForceThreshold = 1.5f;
    public float hitPushForce = 5f;

    [SerializeField] private int humanityReward = 7;

    private bool isDead = false;
    private Vector3 lastHitDirection;

    void Start()
    {
        currentHealth = maxHealth;
        foreach (var rb in ragdollParts)
        {
            rb.isKinematic = true;
            AddCollisionSoundScript(rb);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        // ���������� ����������� ���������� �����
        lastHitDirection = (transform.position - Camera.main.transform.position).normalized;

        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            Die();
        }
    }

    void Die()
    {
        // �������
        HumanitySystem humanitySystem = FindObjectOfType<HumanitySystem>();
        if (humanitySystem != null)
            humanitySystem.AddHumanity(humanityReward);

        // ���������� ���������
        var ai = GetComponent<ZombieAI>();
        if (ai != null)
            ai.enabled = false;

        // ���������� ��������
        if (anim != null)
            anim.enabled = false;

        // ��������� ragdoll
        foreach (var rb in ragdollParts)
        {
            rb.isKinematic = false;
            rb.AddForce(lastHitDirection * hitPushForce, ForceMode.Impulse);
        }

        // ���
        if (smokeEffect != null)
            smokeEffect.SetActive(true);

        // ��������
        Destroy(gameObject, 4f);
    }

    void AddCollisionSoundScript(Rigidbody rb)
    {
        var cs = rb.gameObject.AddComponent<ZombieImpactSound>();
        cs.impactSound = impactSound;
        cs.forceThreshold = impactForceThreshold;
    }
}
