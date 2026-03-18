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

        Health health = other.GetComponent<Health>();
        if (health == null)
            health = other.GetComponentInParent<Health>();

        if (weapon == null || health == null)
            return;

        weapon.AddDamage(1);
        health.Heal(10);

        MessagePopUp.CreateRaw(
            transform.position + Vector3.up,
            "<color=#B16CFF>+1 Damage\n+10 HP</color>"
        );

        consumed = true;
        Destroy(gameObject);
    }
}