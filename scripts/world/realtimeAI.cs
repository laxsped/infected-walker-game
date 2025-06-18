using System.Collections;
using UnityEngine;
using Unity.AI.Navigation;

[RequireComponent(typeof(NavMeshSurface))]
public class DynamicNavMeshUpdater : MonoBehaviour
{
    private NavMeshSurface surface;

    void Awake()
    {
        surface = GetComponent<NavMeshSurface>();
    }

    void Start()
    {
        StartCoroutine(UpdateNavMeshRoutine());
    }

    IEnumerator UpdateNavMeshRoutine()
    {
        while (true)
        {
            if (surface.navMeshData != null)
                surface.UpdateNavMesh(surface.navMeshData); // Мягкое обновление
            else
                surface.BuildNavMesh(); // Если вдруг нет данных — сделай полный билд

            yield return new WaitForSeconds(3f); // Можно дольше, если надо
        }
    }
}
