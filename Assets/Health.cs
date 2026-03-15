using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float popupHeightOffset = 0.5f;

    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        MessagePopUp.Create(
            transform.position + Vector3.up * popupHeightOffset,
            "-" + damage,
            MessagePopUp.Style.Error
        );

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        MessagePopUp.Create(
            transform.position + Vector3.up * popupHeightOffset,
            "+" + amount,
            MessagePopUp.Style.Info
        );
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}