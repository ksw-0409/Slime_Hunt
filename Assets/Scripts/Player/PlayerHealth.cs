using UnityEngine;

// 플레이어 체력, 몬스터 접촉 피해, 사망 처리
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int contactDamage = 10;

    private int currentHealth;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<MonsterHealth>() != null)
        {
            TakeDamage(contactDamage);
        }
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
        isDead = true;
        GameManager.Instance.GameOver();
    }
}
