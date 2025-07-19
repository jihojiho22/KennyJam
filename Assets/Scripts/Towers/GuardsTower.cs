using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuardsTower : MonoBehaviour
{
    [Header("Summoning Settings")]
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private Transform[] summonPositions;
    [SerializeField] private float summonInterval = 5f;
    [SerializeField] private float minDistanceBetweenGuards = 2f;
    [SerializeField] private int maxGuardsPerPosition = 1; // Set to 0 for unlimited guards per position
    
    [Header("Guard Settings")]
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private float spawnHeightOffset = 0.5f;
    [SerializeField] private bool autoStartSummoning = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color gizmoColor = Color.blue;
    
    private List<GameObject> activeGuards = new List<GameObject>();
    private Dictionary<Transform, List<GameObject>> guardsAtPositions = new Dictionary<Transform, List<GameObject>>();
    private Coroutine summoningCoroutine;
    private bool isSummoning = false;
    
    void Start()
    {
        InitializeGuardsDictionary();
        
        if (autoStartSummoning)
        {
            StartSummoning();
        }
    }
    
    void InitializeGuardsDictionary()
    {
        guardsAtPositions.Clear();
        foreach (Transform position in summonPositions)
        {
            if (position != null)
            {
                guardsAtPositions[position] = new List<GameObject>();
            }
        }
    }
    
    public void StartSummoning()
    {
        if (isSummoning) return;
        
        isSummoning = true;
        summoningCoroutine = StartCoroutine(SummoningRoutine());
    }
    
    public void StopSummoning()
    {
        if (!isSummoning) return;
        
        isSummoning = false;
        if (summoningCoroutine != null)
        {
            StopCoroutine(summoningCoroutine);
            summoningCoroutine = null;
        }
    }
    
    IEnumerator SummoningRoutine()
    {
        while (isSummoning)
        {
            yield return new WaitForSeconds(summonInterval);
            
            if (isSummoning) 
            {
                TrySummonGuard();
            }
        }
    }
    
    void TrySummonGuard()
    {
        if (characterPrefab == null || summonPositions.Length == 0)
        {
            Debug.LogWarning("GuardsTower: Character prefab or summon positions not set!");
            return;
        }
        
        Transform bestPosition = FindBestSummonPosition();
        if (bestPosition != null)
        {
            Vector3 baseSpawnPosition = GetSpawnPosition(bestPosition);
            Vector3 validSpawnPosition = FindValidSpawnPosition(baseSpawnPosition);
            SummonGuard(validSpawnPosition, bestPosition);
        }
    }
    
    Transform FindBestSummonPosition()
    {
        // Find a random position that has space for more guards
        List<Transform> availablePositions = new List<Transform>();
        
        foreach (Transform position in summonPositions)
        {
            if (position == null) continue;
            
            int guardsAtThisPosition = guardsAtPositions[position].Count;
            if (maxGuardsPerPosition == 0 || guardsAtThisPosition < maxGuardsPerPosition)
            {
                availablePositions.Add(position);
            }
        }
        
        // If no positions have space, find the position with the least guards
        if (availablePositions.Count == 0)
        {
            Transform bestPosition = null;
            int minGuards = int.MaxValue;
            
            foreach (Transform position in summonPositions)
            {
                if (position == null) continue;
                
                int guardsAtThisPosition = guardsAtPositions[position].Count;
                if (guardsAtThisPosition < minGuards)
                {
                    minGuards = guardsAtThisPosition;
                    bestPosition = position;
                }
            }
            
            return bestPosition;
        }
        
        // Return a random available position
        return availablePositions[Random.Range(0, availablePositions.Count)];
    }
    
    Vector3 GetSpawnPosition(Transform position)
    {
        Vector3 basePosition = position.position;
        
        // Try to find ground level
        RaycastHit hit;
        if (Physics.Raycast(basePosition + Vector3.up * 10f, Vector3.down, out hit, 20f, groundLayer))
        {
            return hit.point + Vector3.up * spawnHeightOffset;
        }
        
        return basePosition + Vector3.up * spawnHeightOffset;
    }
    
    Vector3 FindValidSpawnPosition(Vector3 basePosition)
    {
        // First try the base position
        if (IsValidSpawnPosition(basePosition))
        {
            return basePosition;
        }
        
        // If base position is invalid, try positions in expanding circles around it
        float baseRadius = minDistanceBetweenGuards;
        int maxRings = 3; // Try up to 3 rings of positions
        int positionsPerRing = 8; // 8 positions per ring
        
        for (int ring = 1; ring <= maxRings; ring++)
        {
            float radius = baseRadius * ring;
            
            for (int i = 0; i < positionsPerRing; i++)
            {
                float angle = (360f / positionsPerRing) * i;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    0,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                );
                
                Vector3 testPosition = basePosition + offset;
                
                // Find ground level for the test position
                RaycastHit hit;
                if (Physics.Raycast(testPosition + Vector3.up * 10f, Vector3.down, out hit, 20f, groundLayer))
                {
                    testPosition = hit.point + Vector3.up * spawnHeightOffset;
                }
                else
                {
                    testPosition += Vector3.up * spawnHeightOffset;
                }
                
                if (IsValidSpawnPosition(testPosition))
                {
                    Debug.Log($"GuardsTower: Found valid position at ring {ring}, angle {angle}°, distance {radius}");
                    return testPosition;
                }
            }
        }
        
        // If no valid position found, try a random position far from the base
        Vector3 randomOffset = Random.insideUnitSphere * (minDistanceBetweenGuards * 2f);
        randomOffset.y = 0; // Keep it horizontal
        Vector3 fallbackPosition = basePosition + randomOffset;
        
        // Find ground level for the fallback position
        RaycastHit fallbackHit;
        if (Physics.Raycast(fallbackPosition + Vector3.up * 10f, Vector3.down, out fallbackHit, 20f, groundLayer))
        {
            fallbackPosition = fallbackHit.point + Vector3.up * spawnHeightOffset;
        }
        else
        {
            fallbackPosition += Vector3.up * spawnHeightOffset;
        }
        
        Debug.LogWarning($"GuardsTower: No valid position found, using fallback position at {fallbackPosition}");
        return fallbackPosition;
    }
    
    bool IsValidSpawnPosition(Vector3 position)
    {
        // Check if position is too close to existing guards
        foreach (GameObject guard in activeGuards)
        {
            if (guard != null)
            {
                // Use horizontal distance only (ignore Y difference)
                Vector3 guardPos = guard.transform.position;
                Vector3 horizontalPos = new Vector3(position.x, guardPos.y, position.z);
                float horizontalDistance = Vector3.Distance(horizontalPos, guardPos);
                
                if (horizontalDistance < minDistanceBetweenGuards)
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    void SummonGuard(Vector3 position, Transform assignedPosition)
    {
        GameObject guard = Instantiate(characterPrefab, position, Quaternion.identity);
        guard.name = $"Guard_{assignedPosition.name}_{System.DateTime.Now.Ticks}";
        
       
        activeGuards.Add(guard);
        guardsAtPositions[assignedPosition].Add(guard);
        
       
        SetupGuardBehavior(guard, assignedPosition);
        
        Debug.Log($"GuardsTower: Summoned guard at {position}");
    }
    
    void SetupGuardBehavior(GameObject guard, Transform assignedPosition)
    {

        StartCoroutine(MonitorGuard(guard, assignedPosition));
    }
    
    IEnumerator MonitorGuard(GameObject guard, Transform assignedPosition)
    {
        while (guard != null)
        {
            yield return new WaitForSeconds(1f);
        }
        
        // Guard was destroyed, remove from tracking
        activeGuards.Remove(guard);
        guardsAtPositions[assignedPosition].Remove(guard);
    }
    
    public void ForceSummonAtPosition(int positionIndex)
    {
        if (positionIndex >= 0 && positionIndex < summonPositions.Length)
        {
            Transform position = summonPositions[positionIndex];
            if (position != null)
            {
                Vector3 baseSpawnPosition = GetSpawnPosition(position);
                Vector3 validSpawnPosition = FindValidSpawnPosition(baseSpawnPosition);
                SummonGuard(validSpawnPosition, position);
            }
        }
    }
    
    public void ClearAllGuards()
    {
        foreach (GameObject guard in activeGuards)
        {
            if (guard != null)
            {
                DestroyImmediate(guard);
            }
        }
        
        activeGuards.Clear();
        InitializeGuardsDictionary();
    }
    
    public int GetActiveGuardCount()
    {
        return activeGuards.Count;
    }
    
    public bool IsSummoning()
    {
        return isSummoning;
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        Gizmos.color = gizmoColor;
        
        // Draw summon positions
        if (summonPositions != null)
        {
            foreach (Transform position in summonPositions)
            {
                if (position != null)
                {
                    Gizmos.DrawWireSphere(position.position, 0.5f);
                    Gizmos.DrawLine(position.position, position.position + Vector3.up * 2f);
                }
            }
        }
        
        // Draw active guards
        Gizmos.color = Color.green;
        foreach (GameObject guard in activeGuards)
        {
            if (guard != null)
            {
                Gizmos.DrawWireSphere(guard.transform.position, minDistanceBetweenGuards);
            }
        }
    }
    
    void OnValidate()
    {
        // Ensure positive values
        summonInterval = Mathf.Max(0.1f, summonInterval);
        minDistanceBetweenGuards = Mathf.Max(0.1f, minDistanceBetweenGuards);
        maxGuardsPerPosition = Mathf.Max(0, maxGuardsPerPosition); // 0 means unlimited
    }
} 