using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 externalVelocity;
    [SerializeField] private float externalVelocityDecay = 8f;

    [Header("Body Parts")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer headRenderer;

    [Header("Body Sprites")]
    [SerializeField] private Sprite idleBodySprite;
    [SerializeField] private Sprite walkBodySprite1;
    [SerializeField] private Sprite walkBodySprite2;

    [Header("Walk Animation")]
    [SerializeField] private float walkAnimationSpeed = 0.15f;

    [Header("Stats UI")]
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI statsTextSecondColumn;
    [SerializeField] private TextMeshProUGUI statsTextThirdColumn;
    [SerializeField] private Health health;
    [SerializeField] private WeaponController weaponController;
    [Header("Size")]
    [SerializeField] private float playerSize = 1f;
    [SerializeField] private float minPlayerSize = 0.5f;
    [SerializeField] private float maxPlayerSize = 5f;

    private Vector3 startScale;
    private Rigidbody2D rb;
    private Vector2 movement;

    private float walkTimer;
    private bool walkFrameToggle;
    private bool isFacingLeft;

    private string lastStatsText;
    private string lastStatsTextSecondColumn;
    private string lastStatsTextThirdColumn;

    public float PlayerSize => playerSize;
    public Vector2 Movement => movement;
    public float MoveSpeed => moveSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (health == null)
            health = GetComponent<Health>();

        if (weaponController == null)
            weaponController = GetComponentInChildren<WeaponController>();

        startScale = transform.localScale;

        if (bodyRenderer == null)
            Debug.LogError("Body Renderer is not assigned!", this);

        if (headRenderer == null)
            Debug.LogError("Head Renderer is not assigned!", this);

        ApplyPlayerSize();
    }

    private void Update()
    {
        HandleInput();
        HandleFlip();
        HandleWalkAnimation();
        UpdateStatsText();
    }

    private void FixedUpdate()
    {
        rb.velocity = movement * moveSpeed + externalVelocity;
        externalVelocity = Vector2.Lerp(externalVelocity, Vector2.zero, externalVelocityDecay * Time.fixedDeltaTime);
    }

    private void HandleInput()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;
    }

    private void HandleFlip()
    {
        if (movement.x < 0f && !isFacingLeft)
            SetFacing(true);
        else if (movement.x > 0f && isFacingLeft)
            SetFacing(false);
    }

    private void SetFacing(bool faceLeft)
    {
        isFacingLeft = faceLeft;

        if (bodyRenderer != null)
            bodyRenderer.flipX = faceLeft;

        if (headRenderer != null)
            headRenderer.flipX = faceLeft;
    }

    private void HandleWalkAnimation()
    {
        if (bodyRenderer == null)
            return;

        if (movement == Vector2.zero)
        {
            bodyRenderer.sprite = idleBodySprite;
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

        bodyRenderer.sprite = walkFrameToggle ? walkBodySprite1 : walkBodySprite2;
    }

    private void UpdateStatsText()
    {
        if (statsText == null && statsTextSecondColumn == null && statsTextThirdColumn == null)
            return;

        string firstColumnText =
            $"HP: {(health != null ? $"{health.CurrentHealth}/{health.MaxHealth}" : "-")}\n" +
            $"Move Speed: {moveSpeed:0.##}\n" +
            $"Fire Rate: {(weaponController != null ? weaponController.ShotsPerSecond.ToString("0.##") : "-")}/sec\n" +
            $"Damage: {(weaponController != null ? weaponController.Damage.ToString() : "-")}\n" +
            $"Bullet Speed: {(weaponController != null ? weaponController.BulletSpeed.ToString("0.##") : "-")}";

        string secondColumnText =
            $"Recoil: {(weaponController != null ? weaponController.RecoilForce.ToString("0.##") : "-")}\n" +
            $"Size: {playerSize:0.##}\n" +
            $"Poisonous: {(weaponController != null ? weaponController.Poisonous.ToString() : "-")}\n" +
            $"Explosion: {(weaponController != null ? weaponController.ExplosionChance.ToString() : "-")}\u00A0%" +
            $"Homing: {(weaponController != null ? weaponController.HomingChance.ToString() : "-")}\u00A0%";


        string thirdColumnText = "";

        if (statsText != null && lastStatsText != firstColumnText)
        {
            statsText.text = firstColumnText;
            lastStatsText = firstColumnText;
        }

        if (statsTextSecondColumn != null && lastStatsTextSecondColumn != secondColumnText)
        {
            statsTextSecondColumn.text = secondColumnText;
            lastStatsTextSecondColumn = secondColumnText;
        }

        if (statsTextThirdColumn != null)
        {
            statsTextThirdColumn.text = thirdColumnText;
        }
    }

    public void AddExternalVelocity(Vector2 velocity)
    {
        externalVelocity += velocity;
    }

    public void AddMoveSpeed(float amount)
    {
        moveSpeed += amount;

        if (moveSpeed < 3f)
            moveSpeed = 3f;
    }

    public void AddPlayerSize(float amount)
    {
        playerSize += amount;
        playerSize = Mathf.Clamp(playerSize, minPlayerSize, maxPlayerSize);
        ApplyPlayerSize();
    }

    private void ApplyPlayerSize()
    {
        transform.localScale = startScale * playerSize;
    }
}