using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public float spawnProbability = 0.8f; // Шанс 80%
    public NavMeshSurface navMeshSurface; // Ссылка на NavMeshSurface
    private Transform player; // Добавляем ссылку на игрока

    private Dictionary<Vector2Int, GameObject> spawnedZombies = new();
    private Queue<(Vector2Int, GameObject)> spawnQueue = new();
    private Queue<GameObject> zombiePool = new();
    private bool isSpawning = false;

    void Start()
    {
        // Находим игрока
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(SpawnZombiesRoutine());
    }

    public void TrySpawnZombie(Vector2Int chunkCoord, GameObject chunk)
    {
        if (spawnedZombies.ContainsKey(chunkCoord)) return;
        if (Random.value > spawnProbability) return;
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

                yield return WaitForNavMesh();

                Transform spawnPoint = null;
                foreach (var t in chunk.GetComponentsInChildren<Transform>())
                {
                    if (t.CompareTag("ZombieSpawn"))
                    {
                        spawnPoint = t;
                        break;
                    }
                }

                if (spawnPoint != null)
                {
                    UnityEngine.AI.NavMeshHit hit;
                    if (UnityEngine.AI.NavMesh.SamplePosition(spawnPoint.position, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        GameObject zombie;
                        if (zombiePool.Count > 0)
                        {
                            zombie = zombiePool.Dequeue();
                            zombie.transform.position = hit.position;
                            zombie.SetActive(true);
                        }
                        else
                        {
                            zombie = Instantiate(zombiePrefab, hit.position, Quaternion.identity);
                        }

                        // Оптимизация AI: отключаем, если зомби слишком далеко
                        ZombieAI ai = zombie.GetComponent<ZombieAI>();
                        if (ai != null && Vector3.Distance(player.position, hit.position) > 50f)
                        {
                            ai.enabled = false; // Выключаем AI для дальних зомби
                        }

                        spawnedZombies[chunkCoord] = zombie;
                    }
                }

                isSpawning = false;
                yield return null;
            }
            else
            {
                yield return null;
            }
        }
    }

    public void ClearZombiesInChunk(Vector2Int chunkCoord)
    {
        if (spawnedZombies.TryGetValue(chunkCoord, out var zombie))
        {
            zombie.SetActive(false);
            zombiePool.Enqueue(zombie);
            spawnedZombies.Remove(chunkCoord);
        }
    }

    IEnumerator WaitForNavMesh()
    {
        int maxFramesToWait = 100; // Увеличено до 100 кадров
        for (int i = 0; i < maxFramesToWait; i++)
        {
            if (navMeshSurface.navMeshData != null)
            {
                yield break; // NavMesh готов
            }
            yield return null;
        }
        Debug.LogWarning("NavMesh не готов после ожидания");
    }
}