using UnityEngine;

// 일정 간격으로 맵 가장자리에 몬스터를 생성한다
public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float spawnAreaHalfWidth = 8.5f;
    [SerializeField] private float spawnAreaHalfHeight = 4.5f;

    private const int EdgeCount = 4;

    private float spawnTimer;

    private void Update()
    {
        if (GameManager.Instance.IsGameOver)
        {
            return;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer -= spawnInterval;
            SpawnMonster();
        }
    }

    private void SpawnMonster()
    {
        Instantiate(monsterPrefab, GetRandomEdgePosition(), Quaternion.identity);
    }

    // 위, 아래, 왼쪽, 오른쪽 가장자리 중 하나를 골라 그 변 위의 임의 지점을 돌려준다
    private Vector2 GetRandomEdgePosition()
    {
        int edgeIndex = Random.Range(0, EdgeCount);
        float randomX = Random.Range(-spawnAreaHalfWidth, spawnAreaHalfWidth);
        float randomY = Random.Range(-spawnAreaHalfHeight, spawnAreaHalfHeight);

        switch (edgeIndex)
        {
            case 0: return new Vector2(randomX, spawnAreaHalfHeight);
            case 1: return new Vector2(randomX, -spawnAreaHalfHeight);
            case 2: return new Vector2(-spawnAreaHalfWidth, randomY);
            default: return new Vector2(spawnAreaHalfWidth, randomY);
        }
    }
}
