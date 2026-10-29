
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject coinPrefab;

    public int coinCount = 5;

    public float startX = -3f;
    public float spacing = 1f;
    public float spawnY = -2.2f;

    void Start()
    {
        if (coinPrefab == null)
        {
            Debug.LogError("Coin Prefab не назначен!");
            return;
        }

        for (int i = 0; i < coinCount; i++)
        {
            float x = startX + i * spacing;
            float y = spawnY;

            // Монету над Spike поднимаем выше
            if (Mathf.Abs(x - 3f) < 0.1f)
            {
                y = -1f;
            }

            Instantiate(
                coinPrefab,
                new Vector3(x, y, 0f),
                Quaternion.identity
            );
        }
    }
}
