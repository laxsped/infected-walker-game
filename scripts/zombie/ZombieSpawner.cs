using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public float spawnProbability = 0.5f; // Вероятность, что на чанк появится зомби (0-1)
    public NavMeshSurface navMeshSurface; // Ссылка на NavMeshSurface для проверки

    private Dictionary<Vector2Int, GameObject> spawnedZombies = new();
    private Queue<(Vector2Int, GameObject)> spawnQueue = new(); // Очередь для спавна зомби
    private Queue<GameObject> zombiePool = new(); // Пул зомби
    private bool isSpawning = false; // Флаг, чтобы не запускать несколько корутин

    void Start()
    {
        // Запускаем корутину для спавна
        StartCoroutine(SpawnZombiesRoutine());
    }

    // Вызывать, когда чанк загружается и готов
    public void TrySpawnZombie(Vector2Int chunkCoord, GameObject chunk)
    {
        // Если зомби уже есть в этом чанке - ничего не делаем
        if (spawnedZombies.ContainsKey(chunkCoord)) return;

        // Проверяем вероятность
        if (Random.value > spawnProbability) return;

        // Добавляем в очередь на спавн
        spawnQueue.Enqueue((chunkCoord, chunk));
    }

    IEnumerator SpawnZombiesRoutine()
    {
        while (true)
        {
            if (spawnQueue.Count > 0 && !isSpawning)
            {
                isSpawning = true;
                var (chunkCoord, chunk) = spawnQueue.Dequeue();

                // Проверяем, что NavMesh готов
                yield return WaitForNavMesh();

                // Найдём точку спавна в чанке (метка "ZombieSpawn")
                Transform spawnPoint = null;
                foreach (var t in chunk.GetComponentsInChildren<Transform>())
                {
                    if (t.CompareTag("ZombieSpawn"))
                    {
                        spawnPoint = t;
                        break;
                    }
                }

                if (spawnPoint == null)
                {
                    Debug.LogWarning($"Нет точки спавна для зомби в чанке {chunkCoord}");
                    isSpawning = false;
                    continue;
                }

                // Спавним зомби
                GameObject zombie;
                if (zombiePool.Count > 0)
                {
                    zombie = zombiePool.Dequeue();
                    zombie.transform.position = spawnPoint.position;
                    zombie.transform.rotation = Quaternion.identity;
                    zombie.SetActive(true);
                }
                else
                {
                    zombie = Instantiate(zombiePrefab, spawnPoint.position, Quaternion.identity);
                }

                spawnedZombies[chunkCoord] = zombie;
                isSpawning = false;

                yield return null; // Ждём следующий кадр
            }
            else
            {
                yield return null; // Если очередь пуста, ждём кадр
            }
        }
    }

    // Удаляем зомби, если чанк выгружается
    public void ClearZombiesInChunk(Vector2Int chunkCoord)
    {
        if (spawnedZombies.TryGetValue(chunkCoord, out var zombie))
        {
            zombie.SetActive(false);
            zombiePool.Enqueue(zombie); // Добавляем в пул вместо уничтожения
            spawnedZombies.Remove(chunkCoord);
        }
    }

    // Ждём, пока NavMesh будет готов
    IEnumerator WaitForNavMesh()
    {
        int maxFramesToWait = 50; // Максимум 50 кадров (примерно 1 секунда на 60 FPS)
        for (int i = 0; i < maxFramesToWait; i++)
        {
            if (navMeshSurface.navMeshData != null)
            {
                yield break; // NavMesh готов
            }
            yield return null;
        }
        Debug.LogWarning("NavMesh не готов после ожидания, пропускаем спавн");
    }
}