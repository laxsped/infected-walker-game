using UnityEngine;
using System.Collections;

public class KnifeAttack : MonoBehaviour
{
    [Header("Attack")]
    public float attackRange = 1.5f;
    public int knifeDamage = 10;
    public float attackCooldown = 0.6f;
    public LayerMask zombieLayer;
    public Transform attackOrigin;

    [Header("Critical Hit")]
    [Range(0f, 1f)] public float criticalChance = 0.2f;
    [Range(0f, 1f)] public float backstabBonusChance = 0.15f;
    public float criticalMultiplier = 2f;
    public AudioClip criticalHitClip;
    public GameObject criticalTextPrefab; // ������ ������
    public float slowMoTimeScale = 0.5f;
    public float slowMoDuration = 0.15f;
    public Light criticalFlashLight; // �����������: ������� �����

    [Header("Animation & VFX")]
    public Animator animator;
    public AudioClip slashClip;
    public AudioClip hitClip;
    public ParticleSystem bloodEffect;

    [Header("Textures")]
    public SkinnedMeshRenderer knifeRenderer;
    public Material[] bloodStates;

    private float lastAttackTime;
    private Camera cam;
    private int killsWithKnife;

    void Start()
    {
        cam = Camera.main;
        UpdateBloodTexture();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        if (animator) animator.SetTrigger("Attack");

        if (slashClip)
            AudioSource.PlayClipAtPoint(slashClip, transform.position);

        RaycastHit hit;
        Vector3 origin = attackOrigin ? attackOrigin.position : cam.transform.position;
        Vector3 direction = cam.transform.forward;

        if (Physics.Raycast(origin, direction, out hit, attackRange, zombieLayer))
        {
            ZombieHealth zh = hit.collider.GetComponentInParent<ZombieHealth>();
            if (zh != null)
            {

                // �������� ����-���
                bool isCritical = false;
                int finalDamage = knifeDamage;

                // ���� ����� ������������ ������ � �����
                Vector3 toZombie = (hit.transform.position - transform.position).normalized;
                float angleToZombie = Vector3.Angle(toZombie, hit.transform.forward);

                float totalCritChance = criticalChance;
                if (angleToZombie > 120f) // ���� ����� (��������)
                    totalCritChance += backstabBonusChance;

                if (Random.value <= totalCritChance)
                {
                    isCritical = true;
                    finalDamage = Mathf.RoundToInt(knifeDamage * criticalMultiplier);
                    Debug.Log($"����������� ����! ����: {finalDamage}");

                    if (criticalHitClip)
                        AudioSource.PlayClipAtPoint(criticalHitClip, hit.point);

                    
                    StartCoroutine(DoSlowMotion());

                    if (criticalFlashLight)
                        StartCoroutine(FlashLight());
                }
                else
                {
                    if (hitClip)
                        AudioSource.PlayClipAtPoint(hitClip, hit.point);
                }

                zh.TakeDamage(finalDamage);
                killsWithKnife++;
                UpdateBloodTexture();

                if (bloodEffect)
                {
                    ParticleSystem blood = Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    blood.Play();
                    Destroy(blood.gameObject, 2f);
                }
            }
        }
    }

    void SpawnCriticalText(Vector3 position)
    {
        if (criticalTextPrefab == null) return;

        // ������ ������ � ����� �����
        GameObject critTextInstance = Instantiate(criticalTextPrefab, position, Quaternion.identity);

        // ����������� � ������������ ����� � ������, ����� ������ ��� ����� � ������
        critTextInstance.transform.LookAt(cam.transform);
        critTextInstance.transform.Rotate(0, 180f, 0); // ����� ����� �� ��� ���������

        // ���������� ����� 1.5 ������� (������ ������)
        Destroy(critTextInstance, 1.5f);
    }

    void UpdateBloodTexture()
    {
        if (killsWithKnife >= 20)
            knifeRenderer.material = bloodStates[2];
        else if (killsWithKnife >= 10)
            knifeRenderer.material = bloodStates[1];
        else
            knifeRenderer.material = bloodStates[0];
    }

    IEnumerator DoSlowMotion()
    {
        Time.timeScale = slowMoTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        yield return new WaitForSecondsRealtime(slowMoDuration);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    IEnumerator FlashLight()
    {
        criticalFlashLight.enabled = true;
        yield return new WaitForSeconds(0.1f);
        criticalFlashLight.enabled = false;
    }
}
