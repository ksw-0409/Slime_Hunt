using UnityEngine;
using UnityEngine.InputSystem;

// 스킬 입력, 쿨타임, 스킬 이펙트 생성
public class PlayerSkill : MonoBehaviour
{
    [SerializeField] private GameObject skillEffectPrefab;
    [SerializeField] private float skillCooldown = 1.5f;

    private float remainingCooldown;

    public float RemainingCooldown => remainingCooldown;

    private void Update()
    {
        if (remainingCooldown > 0f)
        {
            remainingCooldown -= Time.deltaTime;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (keyboard.spaceKey.wasPressedThisFrame && remainingCooldown <= 0f)
        {
            UseSkill();
        }
    }

    private void UseSkill()
    {
        remainingCooldown = skillCooldown;
        Instantiate(skillEffectPrefab, transform.position, Quaternion.identity);
    }
}
