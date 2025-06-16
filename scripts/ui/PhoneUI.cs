using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhoneShopUI : MonoBehaviour
{
    [Header("Phone Elements")]
    public Animator phoneAnimator;
    public GameObject phoneCanvas;
    public AudioSource audioSource;
    public AudioClip clickSound;

    [Header("Texts")]
    public Text itemText;
    public Text descriptionText;
    public Text sellerMessageText;

    [Header("Shop Data")]
    public string[] sellerMessages;
    public string[] itemNames = { "COMPASS 70", "SHOTGUN 210" };
    public string[] itemDescriptions = {
        "Points to the nearest supply house. Breaks after time.",
        "Devastating, but costs a LOT of humanity per shot."
    };
    public GameObject shotgunPickupPrefab; // assign в инспекторе
    public Transform playerTransform; // ссылка на игрока (можно задать в инспекторе)

    [Header("Delays")]
    public float canvasActivationDelay = 0.3f;

    private int currentIndex = 0;
    private bool isPhoneActive = false;
    private bool isShotgunPurchased = false;

    void Start()
    {
        phoneCanvas.SetActive(false);
        descriptionText.gameObject.SetActive(false);
        sellerMessageText.text = "";
    }

    public void OnPhoneEquipped()
    {
        // Вызов из WeaponManager, когда телефон становится активным
        if (!isPhoneActive)
        {
            StartCoroutine(OpenPhone());
        }
    }

    public void OnPhoneUnequipped()
    {
        // Можно вызывать из WeaponManager, когда оружие меняется
        isPhoneActive = false;
        phoneCanvas.SetActive(false);
        descriptionText.gameObject.SetActive(false);
        sellerMessageText.text = "";
    }

    IEnumerator OpenPhone()
    {
        isPhoneActive = true;
        phoneAnimator.SetTrigger("PressEnter"); // Визуал включения
        yield return new WaitForSeconds(canvasActivationDelay);
        phoneCanvas.SetActive(true);
        descriptionText.gameObject.SetActive(true);
        UpdateUI();
        sellerMessageText.text = sellerMessages[Random.Range(0, sellerMessages.Length)];
    }

    void Update()
    {
        if (!isPhoneActive) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentIndex = (currentIndex - 1 + itemNames.Length) % itemNames.Length;
            phoneAnimator.SetTrigger("PressQ");
            PlayClick();
            UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentIndex = (currentIndex + 1) % itemNames.Length;
            phoneAnimator.SetTrigger("PressE");
            PlayClick();
            UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            phoneAnimator.SetTrigger("PressEnter");
            PlayClick();
            ConfirmSelection();
        }
    }

    void UpdateUI()
    {
        itemText.text = itemNames[currentIndex];
        descriptionText.text = itemDescriptions[currentIndex];
    }

    void ConfirmSelection()
    {
        Debug.Log("ConfirmSelection called, currentIndex = " + currentIndex);

        switch (currentIndex)
        {
            case 0:
                Debug.Log("Куплен компас — заглушка");
                break;

            case 1: // Дробовик
                if (isShotgunPurchased)
                {
                    sellerMessageText.text = "You already bought it!";
                    Debug.Log("Shotgun already purchased!");
                    return;
                }

                HumanitySystem humanity = FindObjectOfType<HumanitySystem>();
                if (humanity == null)
                {
                    Debug.LogError("HumanitySystem not found!");
                    sellerMessageText.text = "Error: No humanity system!";
                    return;
                }

                if (humanity != null && humanity.SpendHumanity(210))
                {
                    isShotgunPurchased = true;
                    sellerMessageText.text = "Destroy them!";
                    Debug.Log("Shotgun purchased! Spawning...");

                    if (shotgunPickupPrefab != null)
                    {
                        Vector3 spawnPosition = playerTransform.position + Vector3.up * 5f;
                        Debug.Log("Spawning shotgun at: " + spawnPosition);
                        GameObject droppedShotgun = Instantiate(shotgunPickupPrefab, spawnPosition, Quaternion.identity);
                        if (droppedShotgun != null)
                        {
                            droppedShotgun.SetActive(true); // Явно активируем объект
                            Rigidbody rb = droppedShotgun.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.useGravity = true;
                                rb.AddForce(Vector3.down * 5f); // Даём толчок вниз
                                Debug.Log("Shotgun spawned with ID: " + droppedShotgun.GetInstanceID() + ", Active: " + droppedShotgun.activeSelf);
                            }
                            else
                            {
                                Debug.LogError("Rigidbody not found on shotgun prefab!");
                            }
                        }
                        else
                        {
                            Debug.LogError("Failed to instantiate shotgun!");
                        }
                    }
                    else
                    {
                        Debug.LogError("shotgunPickupPrefab not assigned!");
                    }
                }
                else
                {
                    sellerMessageText.text = "Not enough humanity!";
                    Debug.Log("Not enough humanity to buy shotgun.");
                }
                break;
        }
    }

    public void SpawnShotgunDrop()
    {
        Vector3 spawnPos = playerTransform.position + Vector3.up * 5f; // 5 метров над игроком
        Instantiate(shotgunPickupPrefab, spawnPos, Quaternion.identity);
    }

    void PlayClick()
    {
        if (audioSource && clickSound)
            audioSource.PlayOneShot(clickSound);
    }
}
