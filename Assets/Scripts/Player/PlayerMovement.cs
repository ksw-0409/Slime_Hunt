using UnityEngine;
using UnityEngine.InputSystem;

// 키보드 입력으로 플레이어를 이동시킨다
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D playerRigidbody;
    private SpriteRenderer spriteRenderer;
    private float horizontalInput;
    private float verticalInput;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void ReadInput()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontalInput -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontalInput += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) verticalInput -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) verticalInput += 1f;
    }

    private void Move()
    {
        Vector2 moveDirection = new Vector2(horizontalInput, verticalInput);
        playerRigidbody.linearVelocity = moveDirection * moveSpeed;

        // 바라보는 방향으로 스프라이트를 뒤집는다
        if (spriteRenderer != null && horizontalInput != 0f)
        {
            spriteRenderer.flipX = horizontalInput < 0f;
        }
    }
}
