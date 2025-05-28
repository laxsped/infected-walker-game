using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // если будет перезапуск или экран смерти

public class InfectionSystem : MonoBehaviour
{
    [Header("Infection Settings")]
    public float infection = 0f;
    public float maxInfection = 100f;
    public float passiveInfectionRate = 1f; // в % за 10 сек
    public float timeBetweenPassiveInfection = 10f;

    [Header("UI")]
    public Slider infectionSlider;

    private float timer;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= timeBetweenPassiveInfection)
        {
            AddInfection(passiveInfectionRate);
            timer = 0f;
        }
    }

    public void AddInfection(float amount)
    {
        infection += amount;
        infection = Mathf.Clamp(infection, 0, maxInfection);
        UpdateUI();

        if (infection >= maxInfection)
        {
            GameOver();
        }
    }

    void UpdateUI()
    {
        if (infectionSlider != null)
        {
            infectionSlider.value = infection / maxInfection;
        }
    }

    void GameOver()
    {
        Debug.Log(" Заражение достигло 100%! Game Over.");
        // Здесь можешь сделать переход на экран смерти или перезапуск
        // SceneManager.LoadScene("GameOverScene");
    }
}
