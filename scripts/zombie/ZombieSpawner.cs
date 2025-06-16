using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public float spawnProbability = 0.5f; // �����������, ��� �� ���� �������� ����� (0-1)
    public NavMeshSurface navMeshSurface; // ������ �� NavMeshSurface ��� ��������

    private Dictionary<Vector2Int, GameObject> spawnedZombies = new();
    private Queue<(Vector2Int, GameObject)> spawnQueue = new(); // ������� ��� ������ �����
    private Queue<GameObject> zombiePool = new(); // ��� �����
    private bool isSpawning = false; // ����, ����� �� ��������� ��������� �������

    void Start()
    {
        // ��������� �������� ��� ������
        StartCoroutine(SpawnZombiesRoutine());
    }

    // ��������, ����� ���� ����������� � �����
    public void TrySpawnZombie(Vector2Int chunkCoord, GameObject chunk)
    {
        // ���� ����� ��� ���� � ���� ����� - ������ �� ������
        if (spawnedZombies.ContainsKey(chunkCoord)) return;

        // ��������� �����������
        if (Random.value > spawnProbability) return;

        // ��������� � ������� �� �����
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

                // ���������, ��� NavMesh �����
                yield return WaitForNavMesh();

                // ����� ����� ������ � ����� (����� "ZombieSpawn")
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
                    Debug.LogWarning($"��� ����� ������ ��� ����� � ����� {chunkCoord}");
                    isSpawning = false;
                    continue;
                }

                // ������� �����
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

                yield return null; // ��� ��������� ����
            }
            else
            {
                yield return null; // ���� ������� �����, ��� ����
            }
        }
    }

    // ������� �����, ���� ���� �����������
    public void ClearZombiesInChunk(Vector2Int chunkCoord)
    {
        if (spawnedZombies.TryGetValue(chunkCoord, out var zombie))
        {
            zombie.SetActive(false);
            zombiePool.Enqueue(zombie); // ��������� � ��� ������ �����������
            spawnedZombies.Remove(chunkCoord);
        }
    }

    // ���, ���� NavMesh ����� �����
    IEnumerator WaitForNavMesh()
    {
        int maxFramesToWait = 50; // �������� 50 ������ (�������� 1 ������� �� 60 FPS)
        for (int i = 0; i < maxFramesToWait; i++)
        {
            if (navMeshSurface.navMeshData != null)
            {
                yield break; // NavMesh �����
            }
            yield return null;
        }
        Debug.LogWarning("NavMesh �� ����� ����� ��������, ���������� �����");
    }
}