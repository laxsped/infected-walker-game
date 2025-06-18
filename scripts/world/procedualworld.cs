using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;

public class ChunkManager : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public int viewDistance = 1; // Оставляем 1 для оптимизации
    public int chunkSize = 20;
    public NavMeshSurface navMeshSurface;
    public ZombieSpawner zombieSpawner;

    private Dictionary<Vector2Int, GameObject> activeChunks = new();
    private Queue<GameObject> chunkPool = new(); // Пул чанков
    private Vector2Int currentPlayerChunk;
    private Transform player;
    private Vector2Int lastPlayerChunk; // Для отслеживания движения

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentPlayerChunk = GetChunkCoordFromPosition(player.position);
        lastPlayerChunk = currentPlayerChunk;
        StartCoroutine(InitializeChunks()); // Инициализируем чанки от игрока
        StartCoroutine(UpdateChunksRoutine()); // Следим за игроком
    }

    IEnumerator InitializeChunks()
    {
        yield return StartCoroutine(UpdateChunksSmoothly(true)); // Начальная генерация
    }

    IEnumerator UpdateChunksRoutine()
    {
        while (true)
        {
            Vector2Int newChunkCoord = GetChunkCoordFromPosition(player.position);
            if (newChunkCoord != currentPlayerChunk || newChunkCoord != lastPlayerChunk)
            {
                lastPlayerChunk = currentPlayerChunk;
                currentPlayerChunk = newChunkCoord;
                yield return StartCoroutine(UpdateChunksSmoothly(false, newChunkCoord));
            }
            yield return new WaitForSeconds(0.5f); // Увеличена до 0.5 секунды
        }
    }

    IEnumerator UpdateChunksSmoothly(bool isInitial, Vector2Int newPlayerChunk = default)
    {
        var newActiveChunks = new Dictionary<Vector2Int, GameObject>();
        List<Vector2Int> chunksToGenerate = new List<Vector2Int>();

        // Собираем координаты для генерации
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                Vector2Int chunkCoord = currentPlayerChunk + new Vector2Int(x, z);
                if (!activeChunks.ContainsKey(chunkCoord))
                {
                    chunksToGenerate.Add(chunkCoord);
                }
                else
                {
                    if (activeChunks[chunkCoord].activeSelf) // Сохраняем активные чанки
                    {
                        newActiveChunks[chunkCoord] = activeChunks[chunkCoord];
                    }
                    activeChunks.Remove(chunkCoord);
                }
            }
        }

        // Деактивируем старые чанки, которые не попали в новую зону
        foreach (var old in activeChunks)
        {
            old.Value.SetActive(false);
            chunkPool.Enqueue(old.Value);
        }

        // Сортируем чанки по направлению движения игрока (если не начальная генерация)
        if (!isInitial && chunksToGenerate.Count > 0)
        {
            Vector2 direction = new Vector2(newPlayerChunk.x - lastPlayerChunk.x, newPlayerChunk.y - lastPlayerChunk.y).normalized;
            chunksToGenerate.Sort((a, b) =>
            {
                Vector2 aDir = new Vector2(a.x - currentPlayerChunk.x, a.y - currentPlayerChunk.y);
                Vector2 bDir = new Vector2(b.x - currentPlayerChunk.x, b.y - currentPlayerChunk.y);
                float aDot = Vector2.Dot(aDir.normalized, direction);
                float bDot = Vector2.Dot(bDir.normalized, direction);
                return bDot.CompareTo(aDot); // Ближе к направлению — первее
            });
        }

        // Генерируем по одному чанку за итерацию с задержкой
        for (int i = 0; i < chunksToGenerate.Count; i++)
        {
            Vector2Int chunkCoord = chunksToGenerate[i];
            Vector3 worldPos = new Vector3(chunkCoord.x * chunkSize, 0, chunkCoord.y * chunkSize);
            GameObject prefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];

            GameObject newChunk;
            if (chunkPool.Count > 0)
            {
                newChunk = chunkPool.Dequeue();
                newChunk.transform.position = worldPos;
                newChunk.SetActive(true);
            }
            else
            {
                newChunk = Instantiate(prefab, worldPos, Quaternion.identity);
            }

            newActiveChunks[chunkCoord] = newChunk;

            // Пропускаем спавн зомби для начальной генерации
            if (!isInitial)
            {
                yield return StartCoroutine(SpawnZombieWithNavMeshCheck(chunkCoord, newChunk));
            }

            if (!isInitial) // Задержка только для обычного обновления
                yield return new WaitForSeconds(1.0f); // Оставляем 1 секунду
        }

        activeChunks = newActiveChunks;

        // Простая сборка NavMesh
        navMeshSurface.BuildNavMesh();
        yield return null;
    }

    IEnumerator SpawnZombieWithNavMeshCheck(Vector2Int chunkCoord, GameObject chunk)
    {
        yield return new WaitForSeconds(0.5f);
        if (navMeshSurface.navMeshData != null)
        {
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
                zombieSpawner.TrySpawnZombie(chunkCoord, chunk);
            }
            else
            {
                Debug.LogWarning("No ZombieSpawn point in chunk at " + chunkCoord);
            }
        }
        else
        {
            Debug.LogWarning("NavMesh not ready yet for chunk at " + chunkCoord);
        }
    }

    Vector2Int GetChunkCoordFromPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / chunkSize);
        int z = Mathf.FloorToInt(position.z / chunkSize);
        return new Vector2Int(x, z);
    }
}