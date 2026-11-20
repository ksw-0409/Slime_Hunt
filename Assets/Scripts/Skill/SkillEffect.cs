using UnityEngine;

// 생성된 위치를 중심으로 범위 안의 몬스터에게 피해를 준다
public class SkillEffect : MonoBehaviour
{
    [SerializeField] private int skillDamage = 10;
    [SerializeField] private float damageRadius = 2.5f;

    private void Start()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, damageRadius);
        foreach (Collider2D hitCollider in hitColliders)
        {
            MonsterHealth monsterHealth = hitCollider.GetComponent<MonsterHealth>();
            if (monsterHealth != null)
            {
                monsterHealth.TakeDamage(skillDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
