using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public float spawnProbability = 0.5f; // �����������, ��� �� ���� �������� ����� (0-1)
    public NavMeshSurface navMeshSurface; // ������ �� NavMeshSurface ��� ��������

    private Dictionary<Vector2Int, GameObject> spawnedZombies = new();

    // ��������, ����� ���� ����������� � �����
    public void TrySpawnZombie(Vector2Int chunkCoord, GameObject chunk)
    {
        // ���� ����� ��� ���� � ���� ����� - ������ �� ������
        if (spawnedZombies.ContainsKey(chunkCoord)) return;

        // ��������� �����������
        if (Random.value > spawnProbability) return;

        // ���������, ��� NavMesh ����� � ����� ������� ������:
        if (!IsNavMeshReady())
        {
            Debug.Log("NavMesh �� �����, ����� �� �������");
            return;
        }

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
            Debug.LogWarning("��� ����� ������ ��� ����� � �����");
            return;
        }

        // ������� �����
        GameObject zombie = Instantiate(zombiePrefab, spawnPoint.position, Quaternion.identity);
        spawnedZombies[chunkCoord] = zombie;
    }

    // ������� �����, ���� ���� �����������
    public void ClearZombiesInChunk(Vector2Int chunkCoord)
    {
        if (spawnedZombies.TryGetValue(chunkCoord, out var zombie))
        {
            Destroy(zombie);
            spawnedZombies.Remove(chunkCoord);
        }
    }

    // ������ ������� ��������, ��� NavMesh ��������
    bool IsNavMeshReady()
    {
        // ����� ��������� ������� NavMeshData
        return navMeshSurface.navMeshData != null;
    }
}
