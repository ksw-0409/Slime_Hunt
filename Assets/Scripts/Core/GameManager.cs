using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// 점수, 게임오버, 재시작을 관리하는 싱글톤
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject gameOverPanel;

    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }
    public Transform PlayerTransform => playerTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 재시작 직후에도 게임 시간이 멈춰 있지 않도록 항상 되돌린다
        Time.timeScale = 1f;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (IsGameOver && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }

    public void AddScore(int amount)
    {
        if (IsGameOver)
        {
            return;
        }
        Score += amount;
    }

    public void GameOver()
    {
        if (IsGameOver)
        {
            return;
        }
        IsGameOver = true;
        Time.timeScale = 0f;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
