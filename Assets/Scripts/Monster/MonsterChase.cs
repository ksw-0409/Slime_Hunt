using UnityEngine;

// 플레이어를 향해 몬스터를 이동시킨다
[RequireComponent(typeof(Rigidbody2D))]
public class MonsterChase : MonoBehaviour
{
    [SerializeField] private float chaseSpeed = 2f;

    private Rigidbody2D monsterRigidbody;
    private MonsterHealth monsterHealth;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;

    private void Awake()
    {
        monsterRigidbody = GetComponent<Rigidbody2D>();
        monsterHealth = GetComponent<MonsterHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        playerTransform = GameManager.Instance.PlayerTransform;
    }

    private void FixedUpdate()
    {
        bool isDead = monsterHealth != null && monsterHealth.IsDead;
        if (isDead || playerTransform == null || GameManager.Instance.IsGameOver)
        {
            monsterRigidbody.linearVelocity = Vector2.zero;
            return;
        }

        ChasePlayer();
    }

    private void ChasePlayer()
    {
        Vector2 chaseDirection = (transform.position - playerTransform.position).normalized;
        monsterRigidbody.linearVelocity = chaseDirection * chaseSpeed;

        // 이동하는 방향으로 스프라이트를 뒤집는다
        if (spriteRenderer != null && chaseDirection.x != 0f)
        {
            spriteRenderer.flipX = chaseDirection.x < 0f;
        }
    }
}
