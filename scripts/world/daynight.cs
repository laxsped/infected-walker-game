using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDuration = 120f; // Длительность полного цикла (в секундах)
    private float timeOfDay; // Текущее время (0-1)

    [Header("Lighting Settings")]
    public Light sunLight; // Ссылка на Directional Light (солнце)
    public float sunIntensityDay = 1f; // Интенсивность света днём
    public float sunIntensityNight = 0.1f; // Интенсивность света ночью

    [Header("Sky and Fog Colors")]
    public Gradient skyColor; // Градиент для цвета неба
    public Gradient fogColor; // Градиент для цвета тумана

    [Header("Stars")]
    public Transform player; // Ссылка на игрока
    public ParticleSystem stars; // Ссылка на Particle System (звёзды)
    private Vector3 offset; // Смещение звёзд относительно игрока

    private Volume globalVolume; // Ссылка на глобальный Volume для URP
    private ColorAdjustments colorAdjustments; // Компонент для цветокоррекции

    void Start()
    {
        timeOfDay = 0.4f;

        // Находим глобальный Volume на сцене
        globalVolume = FindObjectOfType<Volume>();
        if (globalVolume == null)
        {
            Debug.LogError("Global Volume not found! Add a Volume with Color Adjustments to the scene.");
            return;
        }

        // Получаем компонент ColorAdjustments из профиля Volume
        if (!globalVolume.profile.TryGet(out colorAdjustments))
        {
            Debug.LogError("ColorAdjustments not found in Volume profile!");
        }

        // Если Directional Light не задан, берём его с текущего объекта
        if (sunLight == null)
        {
            sunLight = GetComponent<Light>();
        }

        // Устанавливаем начальное смещение для звёзд
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
            // Обновляем позицию звёзд, следуя за игроком
            Vector3 newPosition = player.position + offset;
            newPosition.y = stars.transform.position.y; // Фиксируем высоту звёзд
            stars.transform.position = newPosition;

            // Сбрасываем вращение звёзд, чтобы они не крутились с камерой
            stars.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    void Update()
    {
        // Обновляем время суток
        timeOfDay += Time.deltaTime / dayDuration;
        if (timeOfDay > 1) timeOfDay -= 1;

        // Вращаем солнце по оси X (имитация движения солнца)
        float sunAngle = timeOfDay * 360f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 45f, 0f);

        // Обновляем интенсивность света
        float intensity = Mathf.Lerp(sunIntensityNight, sunIntensityDay, Mathf.Sin(timeOfDay * Mathf.PI * 2));
        sunLight.intensity = intensity;

        // Обновляем цвет неба и тумана через URP
        UpdateSkyAndFog();
    }

    void UpdateSkyAndFog()
    {
        // Обновляем цвет неба через ColorAdjustments (Tint)
        if (colorAdjustments != null)
        {
            colorAdjustments.colorFilter.value = skyColor.Evaluate(timeOfDay);
        }

        // Обновляем цвет тумана
        RenderSettings.fogColor = fogColor.Evaluate(timeOfDay);

        if (stars != null)
        {
            var emission = stars.emission;
            emission.enabled = (timeOfDay >= 0.0f && timeOfDay <= 0.3f) || (timeOfDay >= 0.7f && timeOfDay <= 1.0f); // Звёзды видны с 0-0.3 и 0.7-1
        }
    }
}