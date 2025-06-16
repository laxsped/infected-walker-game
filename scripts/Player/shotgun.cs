using UnityEngine;

public class ShotgunAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int pelletCount = 8;
    public float spreadAngle = 8f;
    public float attackCooldown = 1.2f;
    public int pelletDamage = 10;
    public float pelletRange = 30f;
    public LayerMask zombieLayer;

    [Header("Visual & Sound")]
    public Animator animator;
    public AudioSource shootSound;
    public ParticleSystem smokeEffect;
    public Light muzzleFlash;

    [Header("References")]
    public Transform firePoint;
    public ParticleSystem bloodEffect;

    [Header("Recoil")]
    public Transform recoilTransform; // сюда кидаешь ту часть дробовика, которую надо отодвигать
    public float recoilDistance = 0.1f;
    public float recoilSpeed = 10f;

    private Vector3 originalPosition;
    private bool isRecoiling;
    private float recoilTimer;

    private float lastAttackTime;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (muzzleFlash) muzzleFlash.enabled = false;
        originalPosition = recoilTransform.localPosition;

        Debug.Log("Shotgun spawned, active: " + gameObject.activeSelf + ", position: " + transform.position);
        gameObject.SetActive(true); // Форсируем активацию
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
        {
            Shoot();
            lastAttackTime = Time.time;
        }

        if (isRecoiling)
        {
            recoilTimer += Time.deltaTime * recoilSpeed;
            float recoilProgress = Mathf.PingPong(recoilTimer, 1f);

            recoilTransform.localPosition = originalPosition + Vector3.back * recoilDistance * recoilProgress;

            if (recoilTimer >= 1f)
            {
                isRecoiling = false;
                recoilTransform.localPosition = originalPosition;
            }
        }
    }

    void Shoot()
    {
        if (animator) animator.SetTrigger("Shoot");
        if (shootSound) shootSound.Play();
        if (smokeEffect) smokeEffect.Play();
        if (muzzleFlash) StartCoroutine(FlashMuzzle());
        isRecoiling = true;
        recoilTimer = 0f;

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 direction = GetSpreadDirection();

            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, pelletRange, zombieLayer))
            {
                ZombieHealth zh = hit.collider.GetComponentInParent<ZombieHealth>();
                if (zh != null)
                {
                    zh.TakeDamage(pelletDamage);

                    if (bloodEffect)
                    {
                        var blood = Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
                        blood.Play();
                        Destroy(blood.gameObject, 2f);
                    }
                }
            }
        }
    }

    Vector3 GetSpreadDirection()
    {
        float spreadX = Random.Range(-spreadAngle, spreadAngle);
        float spreadY = Random.Range(-spreadAngle, spreadAngle);
        Quaternion spreadRotation = Quaternion.Euler(spreadY, spreadX, 0);
        return spreadRotation * firePoint.forward;
    }

    System.Collections.IEnumerator FlashMuzzle()
    {
        muzzleFlash.enabled = true;
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.enabled = false;
    }
}
