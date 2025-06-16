using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance;

    public GameObject[] weapons;
    public bool isShotgunPurchased = false; // дробовик под индексом 3
    private int currentWeaponIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        EquipWeapon(2); // старт с ножа

        // 🧱 Спрятать дробовик в начале
        if (!isShotgunPurchased) weapons[3].SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) EquipWeapon(3);
    }

    public void EquipWeapon(int index)
    {
        // 🔐 Проверка: дробовик ещё не куплен?
        if (index == 3 && !isShotgunPurchased)
        {
            Debug.Log("Дробовик ещё не куплен!");
            return;
        }

        for (int i = 0; i < weapons.Length; i++)
            weapons[i].SetActive(i == index);

        currentWeaponIndex = index;

        // Телефон включён?
        if (index == 1)
        {
            FindObjectOfType<PhoneShopUI>()?.OnPhoneEquipped();
        }
        else
        {
            FindObjectOfType<PhoneShopUI>()?.OnPhoneUnequipped();
        }
    }


    // 🔥 Новый метод для выбора оружия по имени
    public void EquipWeaponByName(string weaponName)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].name == weaponName)
            {
                EquipWeapon(i);
                return;
            }
        }

        Debug.LogWarning("Оружие с именем '" + weaponName + "' не найдено.");
    }
}
