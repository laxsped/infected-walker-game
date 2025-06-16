using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public float pickupRange = 3f;
    public KeyCode pickupKey = KeyCode.F;

    void Update()
    {
        if (Input.GetKeyDown(pickupKey))
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);
            foreach (var hit in hits)
            {
                Debug.Log("Found object: " + hit.name + ", tag: " + hit.tag);
                if (hit.CompareTag("Pickup") && hit.name.Contains("ShotgunPickup"))
                {
                    Debug.Log("Picking up Shotgun!");
                    WeaponManager.Instance.EquipWeapon(3); // Переключаем на дробовик
                    Destroy(hit.gameObject);
                    break;
                }
            }
        }
    }
}