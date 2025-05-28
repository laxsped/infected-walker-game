using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;

public class ChunkManager : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public int viewDistance = 2;
    public int chunkSize = 20;
    public NavMeshSurface navMeshSurface;
    public ZombieSpawner zombieSpawner;

    private Dictionary<Vector2Int, GameObject> activeChunks = new();
    private Queue<GameObject> chunkPool = new(); // Пул чанков
    private Vector2Int currentPlayerChunk;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentPlayerChunk = GetChunkCoordFromPosition(player.position);
        StartCoroutine(UpdateChunksSmoothly());  // загружаем сразу
        StartCoroutine(UpdateChunksRoutine());   // потом начинаем следить
    }

    IEnumerator UpdateChunksRoutine()
    {
        while (true)
        {
            Vector2Int newChunkCoord = GetChunkCoordFromPosition(player.position);
            if (newChunkCoord != currentPlayerChunk)
            {
                currentPlayerChunk = newChunkCoord;
                yield return StartCoroutine(UpdateChunksSmoothly());
            }

            yield return new WaitForSeconds(0.3f); // не чаще 3 раз в секунду
        }
    }

    IEnumerator UpdateChunksSmoothly()
    {
        var newActiveChunks = new Dictionary<Vector2Int, GameObject>();

        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                Vector2Int chunkCoord = currentPlayerChunk + new Vector2Int(x, z);

                if (activeChunks.TryGetValue(chunkCoord, out GameObject chunk))
                {
                    newActiveChunks[chunkCoord] = chunk;
                    activeChunks.Remove(chunkCoord);
                }
                else
                {
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

                    zombieSpawner.TrySpawnZombie(chunkCoord, newChunk);

                    yield return null; // один кадр на каждый чанк
                }
            }
        }

        // Деактивируем старые чанки
        foreach (var old in activeChunks)
        {
            old.Value.SetActive(false);
            chunkPool.Enqueue(old.Value);
        }

        activeChunks = newActiveChunks;

        // Строим NavMesh и ждём, пока оно сделается
        yield return BuildNavMeshAsync();
    }

    IEnumerator BuildNavMeshAsync()
    {
        navMeshSurface.BuildNavMesh();
        // Тут, к сожалению, нет встроенного await,
        // но можно подождать несколько кадров, чтобы NavMesh успел обновиться
        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator BuildNavMeshDelayed()
    {
        yield return new WaitForSeconds(0.5f); // небольшая пауза, чтобы сгладить
        navMeshSurface.BuildNavMesh();
    }

    Vector2Int GetChunkCoordFromPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / chunkSize);
        int z = Mathf.FloorToInt(position.z / chunkSize);
        return new Vector2Int(x, z);
    }
}
