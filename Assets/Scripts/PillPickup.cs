using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
[RequireComponent(typeof(Collider2D))]
public class PillPickup : MonoBehaviour
{
    private enum EffectType
    {
        Health = 0,
        Damage = 1,
        MoveSpeed = 2,
        FireRate = 3,
        BulletSpeed = 4,
        Recoil = 5,
        PlayerSize = 6,
        Poisonous = 7,
        ProjectilesPerShot = 8,
        ExplosionChance = 9,
        HomingChance = 10,
        BulletSize = 11,
        MaxHealth = 12
    }
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<Sprite> pillSprites = new();
    private EffectType positiveEffect;
    private EffectType negativeEffect;
    private bool consumed;

    public void Initialize(int positive, int negative)
    {
        positiveEffect = ClampEffect(positive);
        negativeEffect = ClampEffect(negative);
    }

    public void InitializeRandom()
    {
        Player player = FindAnyObjectByType<Player>();
        if (player == null)
        {
            positiveEffect = EffectType.Damage;
            negativeEffect = EffectType.MoveSpeed;
            return;
        }

        Health health = player.GetComponent<Health>();
        WeaponController weapon = player.GetComponentInChildren<WeaponController>();

        if (health == null || weapon == null)
        {
            positiveEffect = EffectType.Damage;
            negativeEffect = EffectType.MoveSpeed;
            return;
        }

        positiveEffect = GetRandomValidEffect(true, player, health, weapon, null);
        negativeEffect = GetRandomValidEffect(false, player, health, weapon, positiveEffect);
    }

    private EffectType ClampEffect(int value)
    {
        int min = 0;
        int max = System.Enum.GetValues(typeof(EffectType)).Length - 1;
        value = Mathf.Clamp(value, min, max);
        return (EffectType)value;
    }
    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        SetRandomSprite();
    }private void SetRandomSprite()
    {
        if (spriteRenderer == null || pillSprites == null || pillSprites.Count == 0)
            return;

        spriteRenderer.sprite = pillSprites[Random.Range(0, pillSprites.Count)];
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed || !other.CompareTag("Player"))
            return;

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

        positiveEffect = EnsureValidEffect(positiveEffect, true, player, health, weapon, null);
        negativeEffect = EnsureValidEffect(negativeEffect, false, player, health, weapon, positiveEffect);

            player.ChangeHeadRandomly();
        string positiveText = ApplyEffect(positiveEffect, true, player, health, weapon);
        string negativeText = ApplyEffect(negativeEffect, false, player, health, weapon);

        string combinedPopup = string.Empty;

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
                transform.position + Vector3.up,
                combinedPopup
            );
        }

        consumed = true;
        Destroy(gameObject);
    }

    private EffectType EnsureValidEffect(
        EffectType effect,
        bool positive,
        Player player,
        Health health,
        WeaponController weapon,
        EffectType? forbiddenEffect)
    {
        if (IsEffectAllowed(effect, positive, player, health, weapon, forbiddenEffect))
            return effect;

        return GetRandomValidEffect(positive, player, health, weapon, forbiddenEffect);
    }

    private EffectType GetRandomValidEffect(
        bool positive,
        Player player,
        Health health,
        WeaponController weapon,
        EffectType? forbiddenEffect)
    {
        List<EffectType> validEffects = new List<EffectType>();

        foreach (EffectType effect in System.Enum.GetValues(typeof(EffectType)))
        {
            if (IsEffectAllowed(effect, positive, player, health, weapon, forbiddenEffect))
                validEffects.Add(effect);
        }

        if (validEffects.Count == 0)
        {
            foreach (EffectType effect in System.Enum.GetValues(typeof(EffectType)))
            {
                if (forbiddenEffect.HasValue && effect == forbiddenEffect.Value)
                    continue;

                validEffects.Add(effect);
            }
        }

        return validEffects[Random.Range(0, validEffects.Count)];
    }

    private bool IsEffectAllowed(
        EffectType effect,
        bool positive,
        Player player,
        Health health,
        WeaponController weapon,
        EffectType? forbiddenEffect)
    {
        if (forbiddenEffect.HasValue && effect == forbiddenEffect.Value)
            return false;

        switch (effect)
        {
            case EffectType.Health:
            {
                if (positive)
                    return health.CurrentHealth < health.MaxHealth;

                return health.CurrentHealth > 1;
            }

            case EffectType.Damage:
            {
                if (positive)
                    return true;

                return weapon.Damage > 2;
            }

            case EffectType.MoveSpeed:
            {
                if (positive)
                    return true;

                return player.MoveSpeed > 5f;
            }

            case EffectType.FireRate:
            {
                if (positive)
                    return true;

                return weapon.ShotsPerSecond > 2f;
            }

            case EffectType.BulletSpeed:
            {
                if (positive)
                    return true;

                return weapon.BulletSpeed > 3f;
            }

            case EffectType.Recoil:
            {
                return true;
            }

            case EffectType.PlayerSize:
            {
                if (positive)
                    return player.PlayerSize < 3f;

                return player.PlayerSize > 0.25f;
            }

            case EffectType.Poisonous:
            {
                if (positive)
                    return true;

                return weapon.Poisonous > 1;
            }

            case EffectType.ProjectilesPerShot:
            {
                if (positive)
                    return weapon.ProjectilesPerShot < weapon.MaxProjectilesPerShot;

                return weapon.ProjectilesPerShot > 1;
            }
            
            case EffectType.ExplosionChance:
            {
                if (positive)
                    return weapon.ExplosionChance < 50;

                return weapon.ExplosionChance > 0;
            }

            case EffectType.HomingChance:
            {
                if (positive)
                    return weapon.HomingChance < 50;

                return weapon.HomingChance > 0;
            }
            case EffectType.BulletSize:
            {
                if (positive)
                    return weapon.BulletSize < 3f;

                return weapon.BulletSize > 0.5f;
            }

            case EffectType.MaxHealth:
            {
                if (positive)
                    return health.MaxHealth < 100;

                return health.MaxHealth > 50;
            }
        }

        return false;
    }

    private string ApplyEffect(
        EffectType effect,
        bool positive,
        Player player,
        Health health,
        WeaponController weapon)
    {
        switch (effect)
        {
            case EffectType.Health:
            {
                if (positive)
                {
                    int missingHealth = health.MaxHealth - health.CurrentHealth;
                    if (missingHealth <= 0)
                        return null;

                    int healAmount = Random.Range(1, Mathf.Min(10, missingHealth) + 1);
                    health.Heal(healAmount);
                    return $"+{healAmount} HP";
                }
                else
                {
                    int maxSafeDamage = health.CurrentHealth - 1;
                    if (maxSafeDamage <= 0)
                        return null;

                    int damageAmount = Random.Range(1, Mathf.Min(10, maxSafeDamage) + 1);
                    health.TakeDamage(damageAmount);
                    return $"-{damageAmount} HP";
                }
            }

            case EffectType.Damage:
            {
                int amount = positive ? Random.Range(1, 4) : Random.Range(1, 3);

                if (positive)
                {
                    weapon.AddDamage(amount);
                    return $"+{amount} Damage";
                }
                else
                {
                    int realAmount = Mathf.Min(amount, weapon.Damage - 1);
                    if (realAmount <= 0)
                        return null;

                    weapon.AddDamage(-realAmount);
                    return $"-{realAmount} Damage";
                }
            }

            case EffectType.MoveSpeed:
            {
                int amount = positive ? Random.Range(1, 4) : Random.Range(1, 3);

                if (positive)
                {
                    player.AddMoveSpeed(amount);
                    return $"+{amount} Move Speed";
                }
                else
                {
                    float maxReduction = player.MoveSpeed -5f;
                    int realAmount = Mathf.Min(amount, Mathf.FloorToInt(maxReduction));
                    if (realAmount <= 0)
                        return null;

                    player.AddMoveSpeed(-realAmount);
                    return $"-{realAmount} Move Speed";
                }
            }

            case EffectType.FireRate:
            {
                int amount = positive ? Random.Range(1, 4) : Random.Range(1, 3);

                if (positive)
                {
                    weapon.AddShotsPerSecond(amount);
                    return $"+{amount} Fire Rate";
                }
                else
                {
                    float maxReduction = weapon.ShotsPerSecond - 2f;
                    int realAmount = Mathf.Min(amount, Mathf.FloorToInt(maxReduction));
                    if (realAmount <= 0)
                        return null;

                    weapon.AddShotsPerSecond(-realAmount);
                    return $"-{realAmount} Fire Rate";
                }
            }

            case EffectType.BulletSpeed:
            {
                int amount = positive ? Random.Range(1, 4) : Random.Range(1, 3);

                if (positive)
                {
                    weapon.AddBulletSpeed(amount);
                    return $"+{amount} Bullet Speed";
                }
                else
                {
                    float maxReduction = weapon.BulletSpeed - 3f;
                    int realAmount = Mathf.Min(amount, Mathf.FloorToInt(maxReduction));
                    if (realAmount <= 0)
                        return null;

                    weapon.AddBulletSpeed(-realAmount);
                    return $"-{realAmount} Bullet Speed";
                }
            }

            case EffectType.Recoil:
            {
                float amount = Random.Range(1f, 5f);

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

            case EffectType.PlayerSize:
            {
                float[] sizeSteps = { 0.25f, 0.5f, 0.75f, 1f };
                float amount = sizeSteps[Random.Range(0, sizeSteps.Length)];

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

            case EffectType.Poisonous:
            {
                int amount = positive ? Random.Range(1, 4) : Random.Range(1, 3);

                if (positive)
                {
                    weapon.AddPoisonous(amount);
                    return $"+{amount} Poisonous";
                }
                else
                {
                    int realAmount = Mathf.Min(amount, weapon.Poisonous);
                    if (realAmount <= 0)
                        return null;

                    weapon.AddPoisonous(-realAmount);
                    return $"-{realAmount} Poisonous";
                }
            }

            case EffectType.ProjectilesPerShot:
            {
                int amount = 1;

                if (positive)
                {
                    if (weapon.ProjectilesPerShot >= weapon.MaxProjectilesPerShot)
                        return null;

                    weapon.AddProjectilesPerShot(amount);
                    return $"+{amount} Projectiles";
                }
                else
                {
                    if (weapon.ProjectilesPerShot <= 1)
                        return null;

                    weapon.AddProjectilesPerShot(-amount);
                    return $"-{amount} Projectiles";
                }
            }
            
            case EffectType.ExplosionChance:
            {
                int amount = positive ? Random.Range(5, 16) : Random.Range(5, 11);

                if (positive)
                {
                    int realAmount = Mathf.Min(amount, 100 - weapon.ExplosionChance);
                    if (realAmount <= 0)
                        return null;

                    weapon.AddExplosionChance(realAmount);
                    return $"+{realAmount}% Explosion";
                }
                else
                {
                    int realAmount = Mathf.Min(amount, weapon.ExplosionChance);
                    if (realAmount <= 0)
                        return null;

                    weapon.AddExplosionChance(-realAmount);
                    return $"-{realAmount}% Explosion";
                }
            }

            case EffectType.HomingChance:
            {
                int amount = positive ? Random.Range(5, 16) : Random.Range(5, 11);

                if (positive)
                {
                    int realAmount = Mathf.Min(amount, 100 - weapon.HomingChance);
                    if (realAmount <= 0)
                        return null;

                    weapon.AddHomingChance(realAmount);
                    return $"+{realAmount}% Homing";
                }
                else
                {
                    int realAmount = Mathf.Min(amount, weapon.HomingChance);
                    if (realAmount <= 0)
                        return null;

                    weapon.AddHomingChance(-realAmount);
                    return $"-{realAmount}% Homing";
                }
            }
            case EffectType.BulletSize:
            {
                float amount = 0.25f;

                if (positive)
                {
                    float realAmount = Mathf.Min(amount, 3f - weapon.BulletSize);
                    if (realAmount <= 0f)
                        return null;

                    weapon.AddBulletSize(realAmount);
                    return $"+{realAmount:0.##} Bullet Size";
                }
                else
                {
                    float realAmount = Mathf.Min(amount, weapon.BulletSize - 1f);
                    if (realAmount <= 0f)
                        return null;

                    weapon.AddBulletSize(-realAmount);
                    return $"-{realAmount:0.##} Bullet Size";
                }
            }

            case EffectType.MaxHealth:
            {
                int[] steps = { 5, 10, 15, 20 };
                int amount = steps[Random.Range(0, steps.Length)];

                if (positive)
                {
                    int realAmount = Mathf.Min(amount, 100 - health.MaxHealth);
                    if (realAmount <= 0)
                        return null;

                    health.AddMaxHealth(realAmount);
                    health.FullHeal();
                    return $"+{realAmount} Max HP\nFull Heal";
                }
                else
                {
                    int realAmount = Mathf.Min(amount, health.MaxHealth - 20);
                    if (realAmount <= 0)
                        return null;

                    health.AddMaxHealth(-realAmount);
                    return $"-{realAmount} Max HP";
                }
            }
        }

        return null;
    }
}