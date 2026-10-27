using System.Collections.Generic;
using UnityEngine;

public enum BiomeType { City, Forest }
public enum CoinPattern { Line, JumpArc, RampArc }

public class EndlessTrackManager : MonoBehaviour
{
    [Header("Player & Track Dimensions")]
    public Transform playerTransform;
    public float tileLength = 23.8f;
    public int visibleTilesOnScreen = 7;
    public float laneDistance = 2.25f;

    [Header("Collectibles & Coin Settings")]
    public GameObject[] coinPrefabs;
    public float coinSpacingZ = 1.8f;
    public float arcCoinSpacingZ = 0.9f;
    public float groundCoinY = 0.8f;
    public float roofCoinY = 4.3f;
    public float rampCoinsOffsetZ = -16f;
    public float roofCoinsOffsetZ = -11f;
    public float jumpArcHeight = 2.2f;
    [Range(0f, 1f)] public float coinSpawnChance = 0.75f;

    [Header("Biome Ground Prefabs")]
    public GameObject cityGroundPrefab;
    public GameObject forestGroundPrefab;
    public GameObject tunnelPrefab;
    public int tilesBeforeTunnel = 10;

    [Header("City Obstacles (Short)")]
    public GameObject[] cityFullBlocks;
    public GameObject[] cityPassables;

    [Header("City Obstacles (Long / Elevated)")]
    [Tooltip("Trucks with ramps on the back")]
    public GameObject[] cityRampVehicles;
    [Tooltip("Flat-roof trucks without ramps to form the convoy bridge")]
    public GameObject[] cityLongVehicles;

    [Header("Sequence Settings (Dynamic Z-Spacing)")]
    public float firstObstacleStartZ = 18f;
    public float minGapBetweenSequences = 8f;
    public float maxGapBetweenSequences = 14f;

    [Tooltip("Spacing for normal cars and barriers")]
    public float minNormalStepZ = 14f;
    public float maxNormalStepZ = 18f;

    [Tooltip("Spacing strictly for long trucks and convoys to prevent clipping")]
    public float minLongStepZ = 20f;
    public float maxLongStepZ = 26f;

    [Range(0f, 1f)] public float breathingZoneChance = 0.05f;
    [Range(0f, 1f)] public float elevatedConvoyChance = 0.25f;

    private BiomeType currentBiome = BiomeType.City;
    private float nextTileSpawnZ = 0f;
    private float nextSequenceZ = 18f;
    private int tileCounter = 0;
    private int lastSafeLane = 1;

    private Queue<GameObject> activeGroundTiles = new Queue<GameObject>();
    private List<GameObject> activeObstaclesAndCoins = new List<GameObject>();

    private void Start()
    {
        nextSequenceZ = Mathf.Max(15f, firstObstacleStartZ);
        for (int i = 0; i < visibleTilesOnScreen; i++)
        {
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
                ? cityGroundPrefab : forestGroundPrefab;
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
                if (Random.value < breathingZoneChance) gap += Random.Range(8f, 14f);
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
        bool hasFullBlocks = cityFullBlocks != null && cityFullBlocks.Length > 0;
        bool hasPassables = cityPassables != null && cityPassables.Length > 0;
        bool hasRamps = cityRampVehicles != null && cityRampVehicles.Length > 0;
        bool hasLongVehicles = cityLongVehicles != null && cityLongVehicles.Length > 0;

        if (!hasFullBlocks) return 10f;

        int stepsInSequence = Random.Range(2, 5);
        float currentZ = startZ;

        bool isElevatedConvoy = hasRamps && hasLongVehicles && (Random.value < elevatedConvoyChance);

        // Lock a single lane for the entire elevated convoy so flat-roof trucks stay aligned behind the ramp truck
        if (isElevatedConvoy)
        {
            lastSafeLane = GetNextReachableSafeLane(lastSafeLane);
        }

        for (int step = 0; step < stepsInSequence; step++)
        {
            float stepDistanceZ = isElevatedConvoy
                ? Random.Range(minLongStepZ, maxLongStepZ)
                : Random.Range(minNormalStepZ, maxNormalStepZ);

            // Calculate maximum coins that fit inside this step without overlapping into the next step
            int maxCoinsForStep = Mathf.Max(3, Mathf.FloorToInt((stepDistanceZ - 3.5f) / coinSpacingZ));

            if (isElevatedConvoy)
            {
                // Push currentZ forward on step 0 so the backward-extending wooden ramp doesn't clip previous cars
                if (step == 0)
                {
                    currentZ += 12f;
                }

                // Step 0 spawns the ramp truck; subsequent steps spawn flat-roof trucks in the exact same lane
                GameObject roofPrefab = (step == 0) ? GetRandomPrefab(cityRampVehicles) : GetRandomPrefab(cityLongVehicles);
                SpawnObstacleInLane(roofPrefab, lastSafeLane, currentZ);

                if (step == 0)
                {
                    // Diagonal coin trail aligned with the rear wooden ramp using rampCoinsOffsetZ
                    SpawnCoinTrail(lastSafeLane, currentZ + rampCoinsOffsetZ, 5, groundCoinY, CoinPattern.RampArc);
                    // Flat coin trail along the roof of the first ramp truck using roofCoinsOffsetZ
                    SpawnCoinTrail(lastSafeLane, currentZ + roofCoinsOffsetZ, 6, roofCoinY, CoinPattern.Line);
                }
                else
                {
                    // Flat coin trail strictly limited to 6-8 coins so they stay inside the container roof
                    int roofCoinCount = Random.Range(6, 9);
                    SpawnCoinTrail(lastSafeLane, currentZ + roofCoinsOffsetZ, roofCoinCount, roofCoinY, CoinPattern.Line);
                }

                // Spawn regular hazards in adjacent ground lanes aligned with the truck body
                for (int l = 0; l < 3; l++)
                {
                    if (l != lastSafeLane && Random.value < 0.5f)
                    {
                        SpawnObstacleInLane(GetRandomPrefab(cityFullBlocks), l, currentZ - 6f + Random.Range(-2f, 2f));
                    }
                }
            }
            else
            {
                // Pick next reachable safe lane for ground navigation
                lastSafeLane = GetNextReachableSafeLane(lastSafeLane);

                List<int> hazardLanes = new List<int>();
                for (int l = 0; l < 3; l++)
                {
                    if (l != lastSafeLane) hazardLanes.Add(l);
                }

                int guaranteedLane = hazardLanes[Random.Range(0, hazardLanes.Count)];
                float zOffset1 = Random.Range(0f, 3.0f);

                // Check if primary hazard is a jumpable barrier
                bool primaryIsPassable = hasPassables && Random.value < 0.35f;
                GameObject primaryPrefab = primaryIsPassable ? GetRandomPrefab(cityPassables) : GetRandomPrefab(cityFullBlocks);
                SpawnObstacleInLane(primaryPrefab, guaranteedLane, currentZ + zOffset1);

                // Secondary hazard lane
                if (Random.value < 0.5f)
                {
                    int secondaryLane = hazardLanes[0] == guaranteedLane ? hazardLanes[1] : hazardLanes[0];
                    float zOffset2 = Random.Range(0f, 3.0f);
                    GameObject secondaryPrefab = (hasPassables && Random.value < 0.4f) ? GetRandomPrefab(cityPassables) : GetRandomPrefab(cityFullBlocks);
                    SpawnObstacleInLane(secondaryPrefab, secondaryLane, currentZ + zOffset2);
                }

                // 30% chance to spawn a jumpable barrier in the safe lane with a centered JumpArc coin trail
                bool spawnedPassableInSafeLane = false;
                if (hasPassables && Random.value < 0.30f)
                {
                    float barrierZ = currentZ + 2.0f;
                    SpawnObstacleInLane(GetRandomPrefab(cityPassables), lastSafeLane, barrierZ);
                    spawnedPassableInSafeLane = true;

                    // Center the 5-coin jump parabola directly over barrierZ using the shorter arcCoinSpacingZ
                    float arcStartZ = barrierZ - (2f * arcCoinSpacingZ);
                    SpawnCoinTrail(lastSafeLane, arcStartZ, 5, groundCoinY, CoinPattern.JumpArc);
                }

                // Otherwise, spawn a clean ground line of coins fitted to the current step length
                if (!spawnedPassableInSafeLane && Random.value < coinSpawnChance)
                {
                    int coinCount = Random.Range(5, maxCoinsForStep + 1);
                    SpawnCoinTrail(lastSafeLane, currentZ + 1.0f, coinCount, groundCoinY, CoinPattern.Line);
                }
            }

            currentZ += stepDistanceZ;
        }

        return currentZ - startZ;
    }

    private void SpawnCoinTrail(int lane, float startZ, int coinCount, float startY, CoinPattern pattern)
    {
        if (coinPrefabs == null || coinPrefabs.Length == 0 || ObjectPooler.Instance == null) return;

        float xPos = (lane - 1) * laneDistance;

        // Use tighter spacing for JumpArc so the jump parabola is compact
        float activeSpacingZ = (pattern == CoinPattern.JumpArc) ? arcCoinSpacingZ : coinSpacingZ;

        for (int i = 0; i < coinCount; i++)
        {
            float currentZ = startZ + (i * activeSpacingZ);
            float currentY = startY;

            if (pattern == CoinPattern.JumpArc)
            {
                // Parabolic jump arc using Sine wave (peaks exactly at the middle coin)
                float t = (coinCount > 1) ? (float)i / (coinCount - 1) : 0.5f;
                currentY = startY + (Mathf.Sin(t * Mathf.PI) * jumpArcHeight);
            }
            else if (pattern == CoinPattern.RampArc)
            {
                // Linearly climbs from ground height to truck roof height
                float t = (coinCount > 1) ? (float)i / (coinCount - 1) : 1f;
                currentY = Mathf.Lerp(groundCoinY, roofCoinY, t);
            }

            Vector3 spawnPos = new Vector3(xPos, currentY, currentZ);
            GameObject coin = ObjectPooler.Instance.GetPooledItem(coinPrefabs[0]);

            if (coin != null)
            {
                coin.transform.position = spawnPos;
                coin.transform.rotation = Quaternion.identity;
                coin.SetActive(true);
                activeObstaclesAndCoins.Add(coin);
            }
        }
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
        // Increased to 35f so long trucks aren't recycled while the player is still on their rear ramp/roof
        float destroyZThreshold = playerTransform.position.z - 35f;
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