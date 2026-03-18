using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpecialPillPickup : MonoBehaviour
{
    private bool consumed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed || !other.CompareTag("Player"))
            return;

        WeaponController weapon = other.GetComponentInChildren<WeaponController>();
        if (weapon == null)
            weapon = other.GetComponentInParent<WeaponController>();

        if (weapon == null)
            return;

        weapon.AddDamage(1);

        MessagePopUp.CreateRaw(
            transform.position + Vector3.up,
            "<color=#B16CFF>+1 Damage</color>"
        );

        consumed = true;
        Destroy(gameObject);
    }
}