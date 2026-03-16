using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

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
    [SerializeField] private Health health;
    [SerializeField] private WeaponController weaponController;

    private Rigidbody2D rb;
    private Vector2 movement;

    private float walkTimer;
    private bool walkFrameToggle;
    private bool isFacingLeft;

    private string lastStatsText;

    public Vector2 Movement => movement;
    public float MoveSpeed => moveSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (health == null)
            health = GetComponent<Health>();

        if (weaponController == null)
            weaponController = GetComponentInChildren<WeaponController>();

        if (bodyRenderer == null)
            Debug.LogError("Body Renderer is not assigned!", this);

        if (headRenderer == null)
            Debug.LogError("Head Renderer is not assigned!", this);
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
        rb.velocity = movement * moveSpeed;
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
        if (statsText == null)
            return;

        string fullText =
            $"HP: {(health != null ? $"{health.CurrentHealth}/{health.MaxHealth}" : "-")}\n" +
            $"Move Speed: {moveSpeed:0.##}\n" +
            $"Fire Rate: {(weaponController != null ? weaponController.ShotsPerSecond.ToString("0.##") : "-")}/sec\n" +
            $"Damage: {(weaponController != null ? weaponController.Damage.ToString() : "-")}\n" +
            $"Bullet Speed: {(weaponController != null ? weaponController.BulletSpeed.ToString("0.##") : "-")}";

        if (lastStatsText != fullText)
        {
            statsText.text = fullText;
            lastStatsText = fullText;
        }
    }
}