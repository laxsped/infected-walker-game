using UnityEngine;
using UnityEngine.UI;

public class HumanitySystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text humanityText; // Подключи сюда текст в Canvas

    [Header("Settings")]
    [SerializeField] private int initialHumanity = 0;
    [SerializeField] private int xorKey = 777; // Любое случайное число, которое будет "шифровать" значение

    private int encryptedHumanity = 0;

    // Геттер и сеттер для очков с XOR-шифрованием
    public int Humanity
    {
        get => encryptedHumanity ^ xorKey;
        private set => encryptedHumanity = value ^ xorKey;
    }

    void Start()
    {
        Humanity = initialHumanity;
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) // Нажми K для добавления 100 человечности
        {
            AddHumanity(100);
            Debug.Log("Добавлено 100 человечности для теста!");
        }
    }

    public void AddHumanity(int amount)
    {
        Humanity += amount;
        UpdateUI();
    }

    public bool SpendHumanity(int amount)
    {
        if (Humanity >= amount)
        {
            Humanity -= amount;
            UpdateUI();
            return true;
        }
        else
        {
            Debug.Log("Недостаточно человечности!");
            return false;
        }
    }

    private void UpdateUI()
    {
        if (humanityText != null)
            humanityText.text = $"HUMANITY {Humanity}";
    }
}
