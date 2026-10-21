using System.Collections.Generic;
using UnityEngine;

public enum BiomeType { City, Forest }

public class EndlessTrackManager : MonoBehaviour
{
    [Header("Player & Track Dimensions")]
    public Transform playerTransform;
    [Tooltip("Exact Z-length of your CityGround prefab (e.g., 23.8)")]
    public float tileLength = 23.8f;
    public int visibleTilesOnScreen = 7;
    public float laneDistance = 2.25f;

    [Header("Biome Ground Prefabs")]
    public GameObject cityGroundPrefab;
    public GameObject forestGroundPrefab;
    public GameObject tunnelPrefab;
    public int tilesBeforeTunnel = 10;

    [Header("City Obstacles (Cars, Trucks vs. Barriers, Cones)")]
    [Tooltip("Tall/Long obstacles that MUST be dodged by switching lanes (Cars, Trucks, Buses)")]
    public GameObject[] cityFullBlocks;
    [Tooltip("Obstacles that can be jumped over or slid under (Barriers, Cones, Low Fences)")]
    public GameObject[] cityPassables;

    [Header("Forest Obstacles")]
    public GameObject[] forestFullBlocks;
    public GameObject[] forestPassables;

    [Header("Collectibles")]
    public GameObject[] coinPrefabs;

    [Header("Subway Surfers Style Sequence Settings")]
    [Tooltip("Z distance where the very first obstacle appears in front of the player")]
    public float firstObstacleStartZ = 18f;
    [Tooltip("Short breathing gap between obstacle clusters")]
    public float minGapBetweenSequences = 8f;
    public float maxGapBetweenSequences = 14f;
    [Tooltip("Z distance between obstacles inside the same sequence")]
    public float minStepDistanceZ = 7f;
    public float maxStepDistanceZ = 11f;
    [Range(0f, 1f)]
    [Tooltip("Chance to add a slight breathing zone")]
    public float breathingZoneChance = 0.05f;

    private BiomeType currentBiome = BiomeType.City;
    private float nextTileSpawnZ = 0f;
    private float nextSequenceZ = 18f;
    private int tileCounter = 0;
    private int lastSafeLane = 1;

    private Queue<GameObject> activeGroundTiles = new Queue<GameObject>();
    private List<GameObject> activeObstaclesAndCoins = new List<GameObject>();

    private void Start()
    {
        // Force a sensible minimum start distance even if a negative value was set in the Inspector
        nextSequenceZ = Mathf.Max(15f, firstObstacleStartZ);

        for (int i = 0; i < visibleTilesOnScreen; i++)
        {
            // Allow obstacles right from the first tile (since nextSequenceZ already protects the player's start spot)
            SpawnNextGroundTile(true);
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        if (playerTransform.position.z - tileLength > nextTileSpawnZ - (visibleTilesOnScreen * tileLength))
        {
            RecycleOldestGroundTile();
            SpawnNextGroundTile(true);
        }

        CleanupPassedItems();
    }

    private void SpawnNextGroundTile(bool allowObstacles)
    {
        tileCounter++;
        GameObject tileToSpawn = null;

        if (tunnelPrefab != null && tileCounter >= tilesBeforeTunnel)
        {
            tileCounter = 0;
            tileToSpawn = ObjectPooler.Instance.GetPooledItem(tunnelPrefab);
            RollNextBiome();
            allowObstacles = false;
        }
        else
        {
            GameObject targetPrefab = (currentBiome == BiomeType.City || forestGroundPrefab == null)
                ? cityGroundPrefab
                : forestGroundPrefab;

            tileToSpawn = ObjectPooler.Instance.GetPooledItem(targetPrefab);
        }

        if (tileToSpawn != null)
        {
            tileToSpawn.transform.position = new Vector3(0f, 0f, nextTileSpawnZ);
            tileToSpawn.transform.rotation = Quaternion.identity;
            tileToSpawn.SetActive(true);
            activeGroundTiles.Enqueue(tileToSpawn);
        }

        float tileEndZ = nextTileSpawnZ + tileLength;

        while (nextSequenceZ < tileEndZ)
        {
            if (allowObstacles)
            {
                float sequenceLength = SpawnSubwaySurfersSequence(nextSequenceZ);
                float gap = Random.Range(minGapBetweenSequences, maxGapBetweenSequences);

                if (Random.value < breathingZoneChance)
                {
                    gap += Random.Range(8f, 14f);
                }

                nextSequenceZ += sequenceLength + gap;
            }
            else
            {
                nextSequenceZ = tileEndZ + 5f;
            }
        }

        nextTileSpawnZ += tileLength;
    }

    private float SpawnSubwaySurfersSequence(float startZ)
    {
        GameObject[] fullBlocks = (currentBiome == BiomeType.City) ? cityFullBlocks : forestFullBlocks;
        GameObject[] passables = (currentBiome == BiomeType.City) ? cityPassables : forestPassables;

        bool hasFullBlocks = fullBlocks != null && fullBlocks.Length > 0;
        bool hasPassables = passables != null && passables.Length > 0;

        if (!hasFullBlocks && !hasPassables) return 10f;

        // Each sequence has 2 to 4 close-quarters staggered steps
        int stepsInSequence = Random.Range(2, 5);
        float currentZ = startZ;

        for (int step = 0; step < stepsInSequence; step++)
        {
            lastSafeLane = GetNextReachableSafeLane(lastSafeLane);

            // Pick a guaranteed non-safe lane to ensure at least 1 hazard ALWAYS spawns on this step
            List<int> hazardLanes = new List<int>();
            for (int l = 0; l < 3; l++)
            {
                if (l != lastSafeLane) hazardLanes.Add(l);
            }

            int guaranteedLane = hazardLanes[Random.Range(0, hazardLanes.Count)];
            int secondaryLane = hazardLanes[0] == guaranteedLane ? hazardLanes[1] : hazardLanes[0];

            // 1. Spawn on Guaranteed Hazard Lane (with random Z offset so it doesn't look like a grid)
            float zOffset1 = Random.Range(-2.5f, 2.5f);
            GameObject primaryPrefab = (hasFullBlocks && Random.value < 0.65f)
                ? GetRandomPrefab(fullBlocks)
                : GetRandomPrefab(passables);

            SpawnObstacleInLane(primaryPrefab, guaranteedLane, currentZ + zOffset1);

            // 35% chance to place a warning cone/trap 4.5m in front of a full block car
            if (primaryPrefab != null && hasFullBlocks && hasPassables && System.Array.IndexOf(fullBlocks, primaryPrefab) >= 0 && Random.value < 0.35f)
            {
                SpawnObstacleInLane(GetRandomPrefab(passables), guaranteedLane, currentZ + zOffset1 - 4.5f);
            }

            // 2. Spawn on Secondary Hazard Lane (75% chance, pushed forward/back in Z so they are staggered!)
            if (Random.value < 0.75f)
            {
                // Intentionally offset Z from the primary obstacle so they form a diagonal rather than a flat line
                float zOffset2 = (zOffset1 >= 0f) ? Random.Range(-3.5f, -1.0f) : Random.Range(1.0f, 3.5f);
                GameObject secondaryPrefab = (hasFullBlocks && Random.value < 0.5f)
                    ? GetRandomPrefab(fullBlocks)
                    : GetRandomPrefab(passables);

                SpawnObstacleInLane(secondaryPrefab, secondaryLane, currentZ + zOffset2);
            }

            // 3. Optional challenge in the Safe Lane (30% chance for a jumpable barrier/cone)
            if (hasPassables && Random.value < 0.30f)
            {
                float safeZOffset = Random.Range(-1.5f, 2.0f);
                SpawnObstacleInLane(GetRandomPrefab(passables), lastSafeLane, currentZ + safeZOffset);
            }

            currentZ += Random.Range(minStepDistanceZ, maxStepDistanceZ);
        }

        return currentZ - startZ;
    }

    private int GetNextReachableSafeLane(int currentSafe)
    {
        if (currentSafe == 0) return Random.Range(0, 2);
        if (currentSafe == 2) return Random.Range(1, 3);
        return Random.Range(0, 3);
    }

    private void SpawnObstacleInLane(GameObject prefab, int lane, float zPos)
    {
        if (prefab == null || ObjectPooler.Instance == null) return;

        float xPos = (lane - 1) * laneDistance;
        Vector3 spawnPos = new Vector3(xPos, 0f, zPos);

        GameObject item = ObjectPooler.Instance.GetPooledItem(prefab);
        if (item != null)
        {
            item.transform.position = spawnPos;
            item.transform.rotation = Quaternion.identity;
            item.SetActive(true);
            activeObstaclesAndCoins.Add(item);
        }
    }

    private GameObject GetRandomPrefab(GameObject[] array)
    {
        if (array == null || array.Length == 0) return null;
        return array[Random.Range(0, array.Length)];
    }

    private void RecycleOldestGroundTile()
    {
        if (activeGroundTiles.Count > 0)
        {
            GameObject oldTile = activeGroundTiles.Dequeue();
            if (oldTile != null) oldTile.SetActive(false);
        }
    }

    private void CleanupPassedItems()
    {
        float destroyZThreshold = playerTransform.position.z - 18f;

        for (int i = activeObstaclesAndCoins.Count - 1; i >= 0; i--)
        {
            GameObject item = activeObstaclesAndCoins[i];
            if (item == null || !item.activeInHierarchy || item.transform.position.z < destroyZThreshold)
            {
                if (item != null) item.SetActive(false);
                activeObstaclesAndCoins.RemoveAt(i);
            }
        }
    }

    private void RollNextBiome()
    {
        currentBiome = (Random.value > 0.5f) ? BiomeType.City : BiomeType.Forest;
    }
}