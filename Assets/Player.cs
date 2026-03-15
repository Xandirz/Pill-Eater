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

    private Rigidbody2D rb;
    private Vector2 movement;

    private float walkTimer;
    private bool walkFrameToggle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

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
    }

    private void FixedUpdate()
    {
        rb.velocity = movement * moveSpeed;
    }

    private void HandleInput()
    {
        movement.x = 0f;
        movement.y = 0f;

        if (Input.GetKey(KeyCode.A))
            movement.x = -1f;
        if (Input.GetKey(KeyCode.D))
            movement.x = 1f;
        if (Input.GetKey(KeyCode.W))
            movement.y = 1f;
        if (Input.GetKey(KeyCode.S))
            movement.y = -1f;

        movement = movement.normalized;
    }

    private void HandleFlip()
    {
        if (movement.x < 0)
        {
            if (bodyRenderer != null)
                bodyRenderer.flipX = true;

            if (headRenderer != null)
                headRenderer.flipX = true;
        }
        else if (movement.x > 0)
        {
            if (bodyRenderer != null)
                bodyRenderer.flipX = false;

            if (headRenderer != null)
                headRenderer.flipX = false;
        }
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
}