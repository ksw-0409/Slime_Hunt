using UnityEngine;

// 몬스터 체력, 피해, 사망 처리
public class MonsterHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private Color deadColor = Color.gray;

    private int currentHealth;
    private bool isDead;
    private Rigidbody2D monsterRigidbody;
    private SpriteRenderer spriteRenderer;
    private Collider2D monsterCollider;

    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        monsterRigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        monsterCollider = GetComponent<Collider2D>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        if (currentHealth == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }
        isDead = true;

        monsterRigidbody.linearVelocity = Vector2.zero;
        spriteRenderer.color = deadColor;
        monsterCollider.enabled = false;
        GameManager.Instance.AddScore(scoreValue);
    }
}
