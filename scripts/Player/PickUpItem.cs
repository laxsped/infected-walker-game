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
                Debug.Log("Нашёл объект: " + hit.name + ", тег: " + hit.tag);
                if (hit.CompareTag("Pickup"))
                {
                    Debug.Log("Подбираю " + hit.name);
                    WeaponManager.Instance.EquipWeaponByName(hit.name);
                    Destroy(hit.gameObject);
                    break;
                }
            }
        }
    }
}
