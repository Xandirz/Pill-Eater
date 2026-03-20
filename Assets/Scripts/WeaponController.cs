using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSize = 1f;
    [Header("Position")]
    [SerializeField] private float distanceFromPlayer = 0.75f;

    public Health PlayerHealth;
    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 12f;
    [SerializeField] private float shootCooldown = 0.15f;
    [SerializeField] private int damage = 1;
    [SerializeField] private int poisonous = 0;
    [SerializeField] private int projectilesPerShot = 1;
    [SerializeField] private int maxProjectilesPerShot = 6;
    [SerializeField] private float projectileSideSpacing = 0.22f;
    [SerializeField] private float recoilForce = 0f;

    [Header("Special Chances")]
    [SerializeField] private int explosionChance = 0;
    [SerializeField] private int homingChance = 0;

    private Vector2 aimDirection = Vector2.right;
    private float shootTimer;

    public float RecoilForce => recoilForce;
    public float BulletSpeed => bulletSpeed;
    public float ShootCooldown => shootCooldown;
    public float BulletSize => bulletSize;
    public float ShotsPerSecond => shootCooldown > 0f ? 1f / shootCooldown : 0f;
    public int Damage => damage;
    public int Poisonous => poisonous;
    public int ProjectilesPerShot => projectilesPerShot;
    public int MaxProjectilesPerShot => maxProjectilesPerShot;
    public int ExplosionChance => explosionChance;
    public int HomingChance => homingChance;
    public GameObject BulletPrefab => bulletPrefab;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (player == null)
            Debug.LogError("Player is not assigned!", this);

        if (mainCamera == null)
            Debug.LogError("Main Camera is not assigned!", this);

        if (firePoint == null)
            Debug.LogError("Fire Point is not assigned!", this);

        if (bulletPrefab == null)
            Debug.LogError("Bullet Prefab is not assigned!", this);

        projectilesPerShot = Mathf.Clamp(projectilesPerShot, 1, maxProjectilesPerShot);
        explosionChance = Mathf.Clamp(explosionChance, 0, 100);
        homingChance = Mathf.Clamp(homingChance, 0, 100);
        poisonous = Mathf.Max(0, poisonous);
        damage = Mathf.Max(1, damage);
        bulletSpeed = Mathf.Max(3f, bulletSpeed);
        bulletSize = Mathf.Clamp(bulletSize, 1f, 3f);
    }

    private void Update()
    {
        HandleAim();
        HandleShootCooldown();
        HandleShoot();
    }

    private void HandleAim()
    {
        if (player == null || mainCamera == null)
            return;

        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;

        Vector2 direction = mouseWorldPosition - player.position;

        if (direction.sqrMagnitude > 0.0001f)
            aimDirection = direction.normalized;

        transform.position = (Vector2)player.position + aimDirection * distanceFromPlayer;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void HandleShootCooldown()
    {
        if (shootTimer > 0f)
            shootTimer -= Time.deltaTime;
    }

    private void HandleShoot()
    {
        if (!Input.GetMouseButton(0))
            return;

        if (shootTimer > 0f)
            return;

        Shoot();
        shootTimer = shootCooldown;
    }

    private void Shoot()
    {
        if (PlayerHealth.currentHealth <= 0)
        {
            return;
        }
        if (bulletPrefab == null || firePoint == null)
            return;

        int projectileCount = Mathf.Clamp(projectilesPerShot, 1, maxProjectilesPerShot);

        Vector2 forward = aimDirection.sqrMagnitude > 0.0001f ? aimDirection.normalized : Vector2.right;
        Vector2 side = new Vector2(-forward.y, forward.x);

        float centerOffset = (projectileCount - 1) * 0.5f;

        for (int i = 0; i < projectileCount; i++)
        {
            float lateralIndex = i - centerOffset;
            Vector3 spawnPosition = firePoint.position + (Vector3)(side * lateralIndex * projectileSideSpacing);

            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, firePoint.rotation);

            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bool isHoming = Random.Range(0, 100) < homingChance;

                bulletComponent.Initialize(
                    Bullet.BulletOwner.Player,
                    damage,
                    poisonous,
                    isHoming,
                    bulletSpeed,
                    bulletSize
                );
            }

            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
                bulletRb.velocity = forward * bulletSpeed;
        }

        if (Mathf.Abs(recoilForce) > 0.001f)
            ApplyRecoil();
    }
    public void AddBulletSize(float amount)
    {
        bulletSize += amount;
        bulletSize = Mathf.Clamp(bulletSize, 1f, 3f);
    }
    public void AddDamage(int amount)
    {
        damage += amount;

        if (damage < 1)
            damage = 1;
    }

    public void AddPoisonous(int amount)
    {
        poisonous += amount;

        if (poisonous < 0)
            poisonous = 0;
    }

    public void AddProjectilesPerShot(int amount)
    {
        projectilesPerShot += amount;
        projectilesPerShot = Mathf.Clamp(projectilesPerShot, 1, maxProjectilesPerShot);
    }

    public void AddBulletSpeed(float amount)
    {
        bulletSpeed += amount;

        if (bulletSpeed < 3f)
            bulletSpeed = 3f;
    }

    public void AddRecoilForce(float amount)
    {
        recoilForce += amount;
    }

    public void AddExplosionChance(int amount)
    {
        explosionChance += amount;
        explosionChance = Mathf.Clamp(explosionChance, 0, 100);
    }

    public void AddHomingChance(int amount)
    {
        homingChance += amount;
        homingChance = Mathf.Clamp(homingChance, 0, 100);
    }

    private void ApplyRecoil()
    {
        if (player == null)
            return;

        Player playerComponent = player.GetComponent<Player>();
        if (playerComponent == null)
            return;

        Vector2 recoilDirection = -aimDirection.normalized;
        playerComponent.AddExternalVelocity(recoilDirection * recoilForce);
    }

    public void AddShotsPerSecond(float amount)
    {
        float currentShotsPerSecond = shootCooldown > 0f ? 1f / shootCooldown : 0f;
        currentShotsPerSecond += amount;

        if (currentShotsPerSecond < 0.5f)
            currentShotsPerSecond = 0.5f;

        shootCooldown = 1f / currentShotsPerSecond;
    }
}