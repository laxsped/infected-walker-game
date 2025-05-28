using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public float spawnProbability = 0.5f; // вероятность, что на чанк появится зомби (0-1)
    public NavMeshSurface navMeshSurface; // ссылка на NavMeshSurface для проверки

    private Dictionary<Vector2Int, GameObject> spawnedZombies = new();

    // Вызывать, когда чанк загружается и готов
    public void TrySpawnZombie(Vector2Int chunkCoord, GameObject chunk)
    {
        // Если зомби уже есть в этом чанке - ничего не делаем
        if (spawnedZombies.ContainsKey(chunkCoord)) return;

        // Проверяем вероятность
        if (Random.value > spawnProbability) return;

        // Проверяем, что NavMesh готов — самый простой способ:
        if (!IsNavMeshReady())
        {
            Debug.Log("NavMesh не готов, зомби не спавним");
            return;
        }

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
            Debug.LogWarning("Нет точки спавна для зомби в чанке");
            return;
        }

        // Спавним зомби
        GameObject zombie = Instantiate(zombiePrefab, spawnPoint.position, Quaternion.identity);
        spawnedZombies[chunkCoord] = zombie;
    }

    // Удаляем зомби, если чанк выгружается
    public void ClearZombiesInChunk(Vector2Int chunkCoord)
    {
        if (spawnedZombies.TryGetValue(chunkCoord, out var zombie))
        {
            Destroy(zombie);
            spawnedZombies.Remove(chunkCoord);
        }
    }

    // Пример простой проверки, что NavMesh построен
    bool IsNavMeshReady()
    {
        // Можно проверить наличие NavMeshData
        return navMeshSurface.navMeshData != null;
    }
}
