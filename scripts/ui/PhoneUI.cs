﻿using UnityEngine;
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
                    sellerMessageText.text = "you already bought it!";
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
                    sellerMessageText.text = "destroy them!";
                    Debug.Log("Shotgun purchased! Activating in inventory...");

                    // Активируем дробовик в WeaponManager
                    WeaponManager.Instance.isShotgunPurchased = true;
                    WeaponManager.Instance.EquipWeapon(3); // Переключаем на дробовик
                }
                else
                {
                    sellerMessageText.text = "not enough humanity.";
                    Debug.Log("Not enough humanity to buy shotgun.");
                }
                break;
        }
    }

    void PlayClick()
    {
        if (audioSource && clickSound)
            audioSource.PlayOneShot(clickSound);
    }
}
