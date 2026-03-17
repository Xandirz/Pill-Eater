using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PillPickup : MonoBehaviour
{
    private const int EffectCount = 7;

    private int positiveEffect;
    private int negativeEffect;

    private bool consumed;

    public void Initialize(int positive, int negative)
    {
        positiveEffect = positive;
        negativeEffect = negative;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed) return;
        if (!other.CompareTag("Player")) return;

        Player player = other.GetComponent<Player>();
        if (player == null)
            player = other.GetComponentInParent<Player>();

        Health health = other.GetComponent<Health>();
        if (health == null)
            health = other.GetComponentInParent<Health>();

        WeaponController weapon = other.GetComponentInChildren<WeaponController>();
        if (weapon == null)
            weapon = other.GetComponentInParent<WeaponController>();

        if (player == null || health == null || weapon == null)
            return;

        int finalPositiveEffect = ResolveValidEffect(positiveEffect, true, player, health, weapon);
        int finalNegativeEffect = ResolveValidEffect(negativeEffect, false, player, health, weapon);

        string positiveText = ApplyEffect(finalPositiveEffect, true, player, health, weapon);
        string negativeText = ApplyEffect(finalNegativeEffect, false, player, health, weapon);

        string combinedPopup = "";

        if (!string.IsNullOrEmpty(positiveText))
            combinedPopup += $"<color=#59FF7A>{positiveText}</color>";

        if (!string.IsNullOrEmpty(negativeText))
        {
            if (!string.IsNullOrEmpty(combinedPopup))
                combinedPopup += "\n";

            combinedPopup += $"<color=#FF5A5A>{negativeText}</color>";
        }

        if (!string.IsNullOrEmpty(combinedPopup))
        {
            MessagePopUp.CreateRaw(
                transform.position + Vector3.up * 1f,
                combinedPopup
            );
        }

        consumed = true;
        Destroy(gameObject);
    }

    private int ResolveValidEffect(int effect, bool positive, Player player, Health health, WeaponController weapon)
    {
        if (IsEffectAllowed(effect, positive, player, health, weapon))
            return effect;

        int[] validEffects = new int[EffectCount];
        int validCount = 0;

        for (int i = 0; i < EffectCount; i++)
        {
            if (IsEffectAllowed(i, positive, player, health, weapon))
            {
                validEffects[validCount] = i;
                validCount++;
            }
        }

        if (validCount == 0)
            return effect;

        return validEffects[Random.Range(0, validCount)];
    }

    private bool IsEffectAllowed(int effect, bool positive, Player player, Health health, WeaponController weapon)
    {
        switch (effect)
        {
            case 0: // Heal / Damage HP
            {
                if (positive)
                    return health.CurrentHealth < health.MaxHealth;

                return health.CurrentHealth >= 11;
            }

            case 1: // Damage
            {
                if (positive)
                    return true;

                return weapon.Damage > 0;
            }

            case 2: // Move Speed
            {
                if (positive)
                    return true;

                return player.MoveSpeed > 3f;
            }

            case 3: // Fire Rate
            {
                if (positive)
                    return true;

                return weapon.ShootCooldown > 0.5f;
            }

            case 4: // Bullet Speed
            {
                if (positive)
                    return true;

                return weapon.BulletSpeed >= 3f;
            }
            case 5: // Recoil
            {
                return true;
            }

            case 6: // Player Size
            {
                if (positive)
                    return player.PlayerSize < 5f;

                return player.PlayerSize > 0.75f;
            }
        }

        return false;
    }

    private string ApplyEffect(int effect, bool positive, Player player, Health health, WeaponController weapon)
    {
        switch (effect)
        {
            case 0: // Heal / Damage HP
            {
                if (positive)
                {
                    int healAmount = Random.Range(1, 11);
                    health.Heal(healAmount);
                    return $"+{healAmount} HP";
                }
                else
                {
                    int damageAmount = Random.Range(1, 11);
                    health.TakeDamage(damageAmount);
                    return $"-{damageAmount} HP";
                }
            }

            case 1: // Damage
            {
                int amount = positive ? Random.Range(1, 4) : Random.Range(1, 3);

                if (positive)
                {
                    weapon.AddDamage(amount);
                    return $"+{amount} Damage";
                }
                else
                {
                    weapon.AddDamage(-amount);
                    return $"-{amount} Damage";
                }
            }

            case 2: // Move Speed
            {
                int amount = positive ? Random.Range(1, 4) : Random.Range(1, 3);

                if (positive)
                {
                    player.AddMoveSpeed(amount);
                    return $"+{amount} Move Speed";
                }
                else
                {
                    player.AddMoveSpeed(-amount);
                    return $"-{amount} Move Speed";
                }
            }

            case 3: // Fire Rate
            {
                int amount = positive ? Random.Range(1, 4) : Random.Range(1, 3);

                if (positive)
                {
                    weapon.AddShotsPerSecond(amount);
                    return $"+{amount} Fire Rate";
                }
                else
                {
                    weapon.AddShotsPerSecond(-amount);
                    return $"-{amount} Fire Rate";
                }
            }

            case 4: // Bullet Speed
            {
                int amount = positive ? Random.Range(1, 4) : Random.Range(1, 3);

                if (positive)
                {
                    weapon.AddBulletSpeed(amount);
                    return $"+{amount} Bullet Speed";
                }
                else
                {
                    weapon.AddBulletSpeed(-amount);
                    return $"-{amount} Bullet Speed";
                }
            }
            
            case 5: // Recoil
            {
                float amount = Random.Range(1, 5);

                if (positive)
                {
                    weapon.AddRecoilForce(amount);
                    return $"+{amount:0.##} Recoil";
                }
                else
                {
                    weapon.AddRecoilForce(-amount);
                    return $"-{amount:0.##} Recoil";
                }
            }

            case 6: // Player Size
            {
                float amount = positive ? Random.Range(0.1f, 0.35f) : Random.Range(0.1f, 0.25f);

                if (positive)
                {
                    player.AddPlayerSize(amount);
                    return $"+{amount:0.##} Size";
                }
                else
                {
                    player.AddPlayerSize(-amount);
                    return $"-{amount:0.##} Size";
                }
            }
        }

        return null;
    }
}