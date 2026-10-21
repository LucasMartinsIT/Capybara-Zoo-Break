using System.Collections.Generic;
using UnityEngine;

public class CityTileDecorator : MonoBehaviour
{
    [Header("Sidewalk Sockets (Hydrants, Poles, Benches, Soda)")]
    public Transform[] sidewalkPropSockets;
    public GameObject[] sidewalkPropPrefabs;
    [Range(0f, 1f)] public float propSpawnChance = 0.6f;

    [Header("Grass Lot Sockets (Houses, Shops, Trees)")]
    public Transform[] buildingLotSockets;
    public GameObject[] buildingAndTreePrefabs;
    [Range(0f, 1f)] public float buildingSpawnChance = 0.85f;

    [Header("Building Position Randomizer")]
    [Tooltip("Minimum and maximum distance a building can be pushed back away from the road")]
    public float minSetbackFromRoad = 0f;
    public float maxSetbackFromRoad = 2.5f;
    [Tooltip("Subtle random offset along the Z axis to break the grid pattern")]
    public float maxAlongRoadJitter = 0.5f;

    [Header("Sidewalk Boundary Protection")]
    [Tooltip("Prevents large buildings from overlapping onto the sidewalk")]
    public bool clampToSidewalkEdge = true;
    [Tooltip("Minimum X distance from the center of the road (X=0) where the grass area starts")]
    public float safeGrassEdgeX = 5.2f;

    private List<GameObject> spawnedDecorations = new List<GameObject>();

    private void OnEnable()
    {
        // Wait a frame or check if ObjectPooler is ready before decorating
        if (ObjectPooler.Instance != null)
        {
            DecorateTile();
        }
    }

    private void OnDisable()
    {
        ClearDecorations();
    }

    public void DecorateTile()
    {
        ClearDecorations();

        // 1. Spawn Buildings, Houses, and Trees on the Grass Sockets with random setback
        if (buildingAndTreePrefabs != null && buildingAndTreePrefabs.Length > 0)
        {
            foreach (Transform socket in buildingLotSockets)
            {
                if (socket != null && Random.value <= buildingSpawnChance)
                {
                    GameObject prefab = buildingAndTreePrefabs[Random.Range(0, buildingAndTreePrefabs.Length)];
                    SpawnBuildingAtSocket(prefab, socket);
                }
            }
        }

        // 2. Spawn Streetlights, Hydrants, Benches, and Signs on the Sidewalk Sockets
        if (sidewalkPropPrefabs != null && sidewalkPropPrefabs.Length > 0)
        {
            foreach (Transform socket in sidewalkPropSockets)
            {
                if (socket != null && Random.value <= propSpawnChance)
                {
                    GameObject prefab = sidewalkPropPrefabs[Random.Range(0, sidewalkPropPrefabs.Length)];
                    SpawnDecorationAtSocket(prefab, socket);
                }
            }
        }
    }

    private void SpawnBuildingAtSocket(GameObject prefab, Transform socket)
    {
        if (prefab == null || ObjectPooler.Instance == null) return;

        GameObject instance = ObjectPooler.Instance.GetPooledItem(prefab);
        if (instance != null)
        {
            float setback = Random.Range(minSetbackFromRoad, maxSetbackFromRoad);
            float jitterZ = Random.Range(-maxAlongRoadJitter, maxAlongRoadJitter);

            // Determine if socket is on the left side (X < 0) or right side (X > 0) of the road
            float sideSign = Mathf.Sign(socket.position.x);

            Vector3 targetPos = socket.position;
            targetPos.x += sideSign * setback;
            targetPos.z += jitterZ;

            instance.transform.position = targetPos;
            instance.transform.rotation = socket.rotation;
            instance.SetActive(true);

            if (clampToSidewalkEdge)
            {
                EnforceSidewalkBoundary(instance, sideSign);
            }

            spawnedDecorations.Add(instance);
        }
    }

    private void EnforceSidewalkBoundary(GameObject building, float sideSign)
    {
        MeshRenderer[] renderers = building.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0) return;

        Bounds totalBounds = new Bounds(building.transform.position, Vector3.zero);
        bool hasValidRenderer = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            // Ignore grass planes if they are still part of the building prefab
            if (renderers[i].gameObject.name.StartsWith("Grass")) continue;

            if (!hasValidRenderer)
            {
                totalBounds = renderers[i].bounds;
                hasValidRenderer = true;
            }
            else
            {
                totalBounds.Encapsulate(renderers[i].bounds);
            }
        }

        if (!hasValidRenderer) return;

        if (sideSign < 0f) // Left side of the road (negative X)
        {
            float limitX = -safeGrassEdgeX;
            if (totalBounds.max.x > limitX)
            {
                float overlap = totalBounds.max.x - limitX;
                building.transform.position -= new Vector3(overlap, 0f, 0f);
            }
        }
        else // Right side of the road (positive X)
        {
            float limitX = safeGrassEdgeX;
            if (totalBounds.min.x < limitX)
            {
                float overlap = limitX - totalBounds.min.x;
                building.transform.position += new Vector3(overlap, 0f, 0f);
            }
        }
    }

    private void SpawnDecorationAtSocket(GameObject prefab, Transform socket)
    {
        if (prefab == null || ObjectPooler.Instance == null) return;

        GameObject instance = ObjectPooler.Instance.GetPooledItem(prefab);
        if (instance != null)
        {
            instance.transform.position = socket.position;
            instance.transform.rotation = socket.rotation;
            instance.SetActive(true);
            spawnedDecorations.Add(instance);
        }
    }

    private void ClearDecorations()
    {
        for (int i = 0; i < spawnedDecorations.Count; i++)
        {
            if (spawnedDecorations[i] != null)
            {
                spawnedDecorations[i].SetActive(false);
            }
        }
        spawnedDecorations.Clear();
    }
}