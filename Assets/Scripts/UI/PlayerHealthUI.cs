using UnityEngine;
using UnityEngine.UI;

// 플레이어 체력을 체력바와 숫자로 표시한다
public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image healthTrailImage;
    [SerializeField] private Text healthText;
    [SerializeField] private Gradient healthColorGradient;
    [SerializeField] private float trailFollowSpeed = 0.5f;

    private void Update()
    {
        if (playerHealth == null)
        {
            return;
        }

        int currentHealth = playerHealth.CurrentHealth;
        int maxHealth = playerHealth.MaxHealth;
        float healthRatio = (float)currentHealth / maxHealth;

        healthFillImage.fillAmount = healthRatio;
        healthFillImage.color = healthColorGradient.Evaluate(healthRatio);
        healthText.text = $"{currentHealth} / {maxHealth}";

        // 잃은 체력만큼 잔상 바가 천천히 따라와 방금 받은 피해량이 보이게 한다 (게임오버로 시간이 멈춰도 움직이도록 unscaled 시간 사용)
        healthTrailImage.fillAmount = Mathf.MoveTowards(healthTrailImage.fillAmount, healthRatio, trailFollowSpeed * Time.unscaledDeltaTime);
    }
}
