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
    [SerializeField] private float weaponDistanceFromEnemy = 0.5f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float shootCooldown = 0.8f;
    [SerializeField] private float shootRange = 7f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 aimDirection = Vector2.left;

    private float walkTimer;
    private bool walkFrameToggle;
    private bool isFacingLeft = true;
    private float shootTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        orbitClockwise = Random.value > 0.5f;

        if (player == null)
            Debug.LogError("Player is not assigned!", this);

        if (spriteRenderer == null)
            Debug.LogError("Sprite Renderer is not assigned!", this);

        if (weaponTransform == null)
            Debug.LogError("Weapon Transform is not assigned!", this);

        if (firePoint == null)
            Debug.LogError("Fire Point is not assigned!", this);

        if (bulletPrefab == null)
            Debug.LogError("Bullet Prefab is not assigned!", this);
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
        {
            distanceMove = aimDirection;
        }
        else if (distanceToPlayer < minDistanceToPlayer)
        {
            distanceMove = -aimDirection;
        }

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

        Vector2 tangent;

        if (orbitClockwise)
            tangent = new Vector2(-aimDirection.y, aimDirection.x);
        else
            tangent = new Vector2(aimDirection.y, -aimDirection.x);

        return tangent * orbitStrength;
    }

    private void HandleFlip()
    {
        if (aimDirection.x < 0f && !isFacingLeft)
        {
            SetFacing(true);
        }
        else if (aimDirection.x > 0f && isFacingLeft)
        {
            SetFacing(false);
        }
    }

    private void SetFacing(bool faceLeft)
    {
        isFacingLeft = faceLeft;

        // Спрайт врага по умолчанию смотрит влево
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

        Vector2 weaponDirection = aimDirection.sqrMagnitude > 0.001f ? aimDirection.normalized : Vector2.left;

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
        {
            bulletComponent.Initialize(Bullet.BulletOwner.Enemy);
        }

        SpriteRenderer bulletSprite = bullet.GetComponentInChildren<SpriteRenderer>();
        if (bulletSprite != null)
        {
            bulletSprite.color = Color.red;
        }

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.velocity = aimDirection * bulletSpeed;
        }
    }
}