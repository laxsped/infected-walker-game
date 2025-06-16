using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDuration = 120f; // ������������ ������� ����� (� ��������)
    private float timeOfDay; // ������� ����� (0-1)

    [Header("Lighting Settings")]
    public Light sunLight; // ������ �� Directional Light (������)
    public float sunIntensityDay = 1f; // ������������� ����� ���
    public float sunIntensityNight = 0.1f; // ������������� ����� �����

    [Header("Sky and Fog Colors")]
    public Gradient skyColor; // �������� ��� ����� ����
    public Gradient fogColor; // �������� ��� ����� ������

    [Header("Stars")]
    public Transform player; // ������ �� ������
    public ParticleSystem stars; // ������ �� Particle System (�����)
    private Vector3 offset; // �������� ���� ������������ ������

    private Volume globalVolume; // ������ �� ���������� Volume ��� URP
    private ColorAdjustments colorAdjustments; // ��������� ��� ��������������

    void Start()
    {
        timeOfDay = 0.4f;

        // ������� ���������� Volume �� �����
        globalVolume = FindObjectOfType<Volume>();
        if (globalVolume == null)
        {
            Debug.LogError("Global Volume not found! Add a Volume with Color Adjustments to the scene.");
            return;
        }

        // �������� ��������� ColorAdjustments �� ������� Volume
        if (!globalVolume.profile.TryGet(out colorAdjustments))
        {
            Debug.LogError("ColorAdjustments not found in Volume profile!");
        }

        // ���� Directional Light �� �����, ���� ��� � �������� �������
        if (sunLight == null)
        {
            sunLight = GetComponent<Light>();
        }

        // ������������� ��������� �������� ��� ����
        if (player != null && stars != null)
        {
            offset = stars.transform.position - player.position;
        }
        else
        {
            if (player == null) Debug.LogError("Player not assigned! Assign the player transform in the Inspector.");
            if (stars == null) Debug.LogError("Stars Particle System not assigned!");
        }
    }

    void LateUpdate()
    {
        if (player != null && stars != null)
        {
            // ��������� ������� ����, ������ �� �������
            Vector3 newPosition = player.position + offset;
            newPosition.y = stars.transform.position.y; // ��������� ������ ����
            stars.transform.position = newPosition;

            // ���������� �������� ����, ����� ��� �� ��������� � �������
            stars.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    void Update()
    {
        // ��������� ����� �����
        timeOfDay += Time.deltaTime / dayDuration;
        if (timeOfDay > 1) timeOfDay -= 1;

        // ������� ������ �� ��� X (�������� �������� ������)
        float sunAngle = timeOfDay * 360f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 45f, 0f);

        // ��������� ������������� �����
        float intensity = Mathf.Lerp(sunIntensityNight, sunIntensityDay, Mathf.Sin(timeOfDay * Mathf.PI * 2));
        sunLight.intensity = intensity;

        // ��������� ���� ���� � ������ ����� URP
        UpdateSkyAndFog();
    }

    void UpdateSkyAndFog()
    {
        // ��������� ���� ���� ����� ColorAdjustments (Tint)
        if (colorAdjustments != null)
        {
            colorAdjustments.colorFilter.value = skyColor.Evaluate(timeOfDay);
        }

        // ��������� ���� ������
        RenderSettings.fogColor = fogColor.Evaluate(timeOfDay);

        if (stars != null)
        {
            var emission = stars.emission;
            emission.enabled = (timeOfDay >= 0.0f && timeOfDay <= 0.3f) || (timeOfDay >= 0.7f && timeOfDay <= 1.0f); // ����� ����� � 0-0.3 � 0.7-1
        }
    }
}