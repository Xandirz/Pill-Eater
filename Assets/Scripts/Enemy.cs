using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float minDistanceToPlayer = 3f;
    [SerializeField] private float maxDistanceToPlayer = 5f;

    [Header("Enemy Separation")]
    [SerializeField] private float separationRadius = 1.5f;
    [SerializeField] private float separationStrength = 3.5f;
    [SerializeField] private float veryCloseDistance = 0.75f;
    [SerializeField] private float veryClosePushMultiplier = 2.5f;

    [Header("Orbit Around Player")]
    [SerializeField] private float orbitStrength = 0.75f;
    [SerializeField] private bool orbitClockwise = true;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite walkSprite1;
    [SerializeField] private Sprite walkSprite2;

    [Header("Walk Animation")]
    [SerializeField] private float walkAnimationSpeed = 0.15f;

    [Header("Weapon")]
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float weaponDistanceFromEnemy = 0.75f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float shootCooldown = 0.8f;
    [SerializeField] private float shootRange = 7f;
    [SerializeField] private int damage = 1;

    [Header("Summoning")]
    [SerializeField] private bool isSummoner = false;
    [SerializeField] private bool isSummon = false;
    [SerializeField] private GameObject summonPrefab;
    [SerializeField] private float summonCooldown = 1f;
    [SerializeField] private float summonDistance = 1.25f;
    [SerializeField] private List<Sprite> summonSprites = new();
    [SerializeField] private int maxSummonsOnScene = 10;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 aimDirection = Vector2.left;

    private float walkTimer;
    private bool walkFrameToggle;
    private bool isFacingLeft = true;
    private float shootTimer;
    private float summonTimer;

    public bool IsSummon => isSummon;
    public bool IsSummoner => isSummoner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        orbitClockwise = Random.value > 0.5f;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (spriteRenderer == null)
            Debug.LogError("Sprite Renderer is not assigned!", this);

        if (weaponTransform == null)
            Debug.LogError("Weapon Transform is not assigned!", this);

        if (bulletPrefab == null)
            Debug.LogError("Bullet Prefab is not assigned!", this);

        summonTimer = summonCooldown;
        
        ApplySummonSpriteIfNeeded();
    }
    private void ApplySummonSpriteIfNeeded()
    {
        if (!isSummon || spriteRenderer == null || summonSprites == null || summonSprites.Count == 0)
            return;

        Sprite randomSprite = summonSprites[Random.Range(0, summonSprites.Count)];

        idleSprite = randomSprite;
        walkSprite1 = randomSprite;
        walkSprite2 = randomSprite;
        spriteRenderer.sprite = randomSprite;
    }
    private void Update()
    {
        if (player == null)
            return;

        HandleAI();
        HandleFlip();
        HandleWalkAnimation();
        HandleWeaponAim();
        HandleShootCooldown();
        HandleShooting();
        HandleSummoning();
    }

    private void FixedUpdate()
    {
        rb.velocity = movement * moveSpeed;
    }

    private void HandleAI()
    {
        Vector2 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer > 0.001f)
            aimDirection = toPlayer.normalized;

        Vector2 distanceMove = Vector2.zero;

        if (distanceToPlayer > maxDistanceToPlayer)
            distanceMove = aimDirection;
        else if (distanceToPlayer < minDistanceToPlayer)
            distanceMove = -aimDirection;

        Vector2 separationMove = GetSeparationVector();
        Vector2 orbitMove = GetOrbitVector(distanceToPlayer);

        Vector2 finalMove = distanceMove + orbitMove + separationMove;

        if (finalMove.sqrMagnitude > 1f)
            finalMove.Normalize();

        movement = finalMove;
    }

    private Vector2 GetSeparationVector()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Vector2 separation = Vector2.zero;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == gameObject)
                continue;

            Vector2 offset = (Vector2)transform.position - (Vector2)enemy.transform.position;
            float distance = offset.magnitude;

            if (distance <= 0.001f)
                continue;

            if (distance < separationRadius)
            {
                float strength = 1f - (distance / separationRadius);
                float push = separationStrength * strength;

                if (distance < veryCloseDistance)
                    push *= veryClosePushMultiplier;

                separation += offset.normalized * push;
            }
        }

        return separation;
    }

    private Vector2 GetOrbitVector(float distanceToPlayer)
    {
        if (distanceToPlayer > maxDistanceToPlayer + 0.5f)
            return Vector2.zero;

        Vector2 tangent = orbitClockwise
            ? new Vector2(-aimDirection.y, aimDirection.x)
            : new Vector2(aimDirection.y, -aimDirection.x);

        return tangent * orbitStrength;
    }

    private void HandleFlip()
    {
        if (aimDirection.x < 0f && !isFacingLeft)
            SetFacing(true);
        else if (aimDirection.x > 0f && isFacingLeft)
            SetFacing(false);
    }

    private void SetFacing(bool faceLeft)
    {
        isFacingLeft = faceLeft;

        if (spriteRenderer != null)
            spriteRenderer.flipX = !faceLeft;
    }

    private void HandleWalkAnimation()
    {
        if (spriteRenderer == null)
            return;

        if (movement == Vector2.zero)
        {
            spriteRenderer.sprite = idleSprite;
            walkTimer = 0f;
            walkFrameToggle = false;
            return;
        }

        walkTimer += Time.deltaTime;

        if (walkTimer >= walkAnimationSpeed)
        {
            walkTimer = 0f;
            walkFrameToggle = !walkFrameToggle;
        }

        spriteRenderer.sprite = walkFrameToggle ? walkSprite1 : walkSprite2;
    }

    private void HandleWeaponAim()
    {
        if (weaponTransform == null)
            return;

        Vector2 weaponDirection = aimDirection.sqrMagnitude > 0.001f
            ? aimDirection.normalized
            : Vector2.left;

        weaponTransform.position = (Vector2)transform.position + weaponDirection * weaponDistanceFromEnemy;

        float angle = Mathf.Atan2(weaponDirection.y, weaponDirection.x) * Mathf.Rad2Deg;
        weaponTransform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void HandleShootCooldown()
    {
        if (shootTimer > 0f)
            shootTimer -= Time.deltaTime;
    }

    private void HandleShooting()
    {
        if (bulletPrefab == null || firePoint == null || player == null)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > shootRange)
            return;

        if (shootTimer > 0f)
            return;

        Shoot();
        shootTimer = shootCooldown;
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
            bulletComponent.Initialize(Bullet.BulletOwner.Enemy, damage);

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
            bulletRb.velocity = aimDirection * bulletSpeed;
    }

    private void HandleSummoning()
    {
        if (!isSummoner || summonPrefab == null)
            return;

        if (GetActiveSummonsCount() >= maxSummonsOnScene)
            return;

        summonTimer -= Time.deltaTime;

        if (summonTimer > 0f)
            return;

        summonTimer = summonCooldown;
        Summon();
    }
    private int GetActiveSummonsCount()
    {
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        int count = 0;

        foreach (Enemy enemy in allEnemies)
        {
            if (enemy != null && enemy.IsSummon)
                count++;
        }

        return count;
    }
    private void Summon()
    {
        Vector2 direction = aimDirection.sqrMagnitude > 0.001f
            ? aimDirection.normalized
            : Random.insideUnitCircle.normalized;

        if (direction.sqrMagnitude <= 0.001f)
            direction = Vector2.right;

        Vector3 spawnPosition = transform.position + (Vector3)(direction * summonDistance);

        GameObject summonObj = Instantiate(summonPrefab, spawnPosition, Quaternion.identity);

        Enemy summonEnemy = summonObj.GetComponent<Enemy>();
        if (summonEnemy != null)
            summonEnemy.SetAsSummon();

        Health summonHealth = summonObj.GetComponent<Health>();
        if (summonHealth != null)
            summonHealth.FullHeal();
    }

    public void SetAsSummon()
    {
        isSummon = true;
        isSummoner = false;
        ApplySummonSpriteIfNeeded();
    }
}