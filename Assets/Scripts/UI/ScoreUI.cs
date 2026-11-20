using UnityEngine;
using UnityEngine.UI;

// 현재 점수를 표시하고, 점수가 오르면 숫자를 잠깐 키웠다가 되돌린다
public class ScoreUI : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private float popScale = 1.3f;
    [SerializeField] private float popRecoverSpeed = 2f;

    private int displayedScore = -1;

    private void Update()
    {
        int currentScore = GameManager.Instance.Score;
        if (currentScore != displayedScore)
        {
            // 처음 표시할 때는 키우지 않는다
            if (displayedScore >= 0)
            {
                scoreText.rectTransform.localScale = Vector3.one * popScale;
            }
            displayedScore = currentScore;
            scoreText.text = currentScore.ToString();
        }

        scoreText.rectTransform.localScale = Vector3.MoveTowards(scoreText.rectTransform.localScale, Vector3.one, popRecoverSpeed * Time.unscaledDeltaTime);
    }
}
