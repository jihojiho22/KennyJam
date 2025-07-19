using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BuildingSystem : MonoBehaviour, PlayerInput.IPlayerActions
{
    [Header("Building Settings")]
    [SerializeField] private GameObject[] buildablePrefabs;
    [SerializeField] private Material previewMaterial;
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private LayerMask obstacleLayer = 1;
    [SerializeField] private float maxRaycastDistance = 100f;
    [SerializeField] private float placementOffset = 0f;
    [SerializeField] private float rotationSpeed = 90f;
    
    [Header("Preview Settings")]
    [SerializeField] private bool showPreview = true;
    [SerializeField] private float previewOpacity = 0.6f;
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;
    
    [Header("Building Constraints")]
    [SerializeField] private float minDistanceFromPlayer = 1f;
    [SerializeField] private float maxDistanceFromPlayer = 20f;
    [SerializeField] private bool requireFlatSurface = false;
    [SerializeField] private float maxSurfaceAngle = 45f;
    
    private PlayerInput playerInput;
    private Camera playerCamera;
    private GameObject currentPreview;
    private GameObject selectedPrefab;
    private int currentPrefabIndex = 0;
    private bool isInBuildMode = false;
    private bool canPlace = false;
    private Vector3 placementPosition;
    private Quaternion placementRotation = Quaternion.identity;
    private List<Renderer> previewRenderers = new List<Renderer>();
    private List<Material> originalMaterials = new List<Material>();
    
    // Events
    public System.Action<GameObject, Vector3, Quaternion> OnBuildingPlaced;
    public System.Action OnBuildModeEntered;
    public System.Action OnBuildModeExited;
    
    void Awake()
    {
        playerInput = new PlayerInput();
        playerCamera = Camera.main;
        
        if (playerCamera == null)
        {
            Debug.LogError("No main camera found! Please tag your camera as 'MainCamera'");
        }
        
        if (buildablePrefabs.Length == 0)
        {
            Debug.LogWarning("No buildable prefabs assigned to BuildingSystem!");
        }
    }
    
    void OnEnable()
    {
        playerInput.Enable();
        playerInput.Player.SetCallbacks(this);
    }
    
    void OnDisable()
    {
        playerInput.Player.SetCallbacks(null);
        playerInput.Disable();
        ExitBuildMode();
    }
    
    void Update()
    {
        if (isInBuildMode && selectedPrefab != null)
        {
            UpdatePreview();
            HandleRotationInput();
        }
        
        HandleKeyboardInput();
    }
    
    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isInBuildMode)
            {
                TryPlaceBuilding();
            }
        }
    }
    
    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!isInBuildMode)
            {
                EnterBuildMode();
            }
            else
            {
                ExitBuildMode();
            }
        }
    }
    
    public void EnterBuildMode()
    {
        if (buildablePrefabs.Length == 0) return;
        
        isInBuildMode = true;
        selectedPrefab = buildablePrefabs[currentPrefabIndex];
        CreatePreview();
        OnBuildModeEntered?.Invoke();
        
        Debug.Log($"Entered build mode with prefab: {selectedPrefab.name}");
    }
    
    public void ExitBuildMode()
    {
        isInBuildMode = false;
        DestroyPreview();
        selectedPrefab = null;
        OnBuildModeExited?.Invoke();
        
        Debug.Log("Exited build mode");
    }
    
    public void CyclePrefab(int direction = 1)
    {
        if (buildablePrefabs.Length == 0) return;
        
        currentPrefabIndex = (currentPrefabIndex + direction + buildablePrefabs.Length) % buildablePrefabs.Length;
        
        if (isInBuildMode)
        {
            selectedPrefab = buildablePrefabs[currentPrefabIndex];
            DestroyPreview();
            CreatePreview();
        }
        
        Debug.Log($"Switched to prefab: {selectedPrefab.name}");
    }
    
    void CreatePreview()
    {
        if (selectedPrefab == null) return;
        
        DestroyPreview();
        
        currentPreview = Instantiate(selectedPrefab);
        currentPreview.name = $"{selectedPrefab.name}_Preview";
        
        // Store original materials and create preview materials
        previewRenderers.Clear();
        originalMaterials.Clear();
        
        Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            previewRenderers.Add(renderer);
            originalMaterials.Add(renderer.material);
            
            // Create preview material
            Material previewMat = new Material(previewMaterial);
            previewMat.color = new Color(previewMat.color.r, previewMat.color.g, previewMat.color.b, previewOpacity);
            renderer.material = previewMat;
        }
        
        // Completely remove all colliders from preview to prevent any collision interference
        Collider[] colliders = currentPreview.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            DestroyImmediate(collider);
        }
        
        // Disable any scripts that might interfere
        MonoBehaviour[] scripts = currentPreview.GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            script.enabled = false;
        }
        
        // Set preview to ignore all layers to be absolutely sure
        currentPreview.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        // Disable any Rigidbody components
        Rigidbody[] rigidbodies = currentPreview.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }
    
    void DestroyPreview()
    {
        if (currentPreview != null)
        {
            DestroyImmediate(currentPreview);
            currentPreview = null;
        }
        
        previewRenderers.Clear();
        originalMaterials.Clear();
    }
    
    void UpdatePreview()
    {
        if (currentPreview == null) return;
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        
        // Always disable preview during raycast to prevent interference
        currentPreview.SetActive(false);
        
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, groundLayer))
        {
            placementPosition = hit.point + Vector3.up * placementOffset;
            
            // Re-enable the preview and update its position
            currentPreview.SetActive(true);
            currentPreview.transform.position = placementPosition;
            currentPreview.transform.rotation = placementRotation;
            
            // Check if placement is valid
            canPlace = IsPlacementValid(placementPosition, placementRotation);
            
            // Update preview appearance
            UpdatePreviewAppearance();
        }
        else
        {
            // Re-enable the preview even if no ground was hit
            currentPreview.SetActive(true);
        }
    }
    
    void UpdatePreviewAppearance()
    {
        if (!showPreview) return;
        
        Color targetColor = canPlace ? validColor : invalidColor;
        targetColor.a = previewOpacity;
        
        foreach (Renderer renderer in previewRenderers)
        {
            if (renderer.material != null)
            {
                renderer.material.color = targetColor;
            }
        }
    }
    
    bool IsPlacementValid(Vector3 position, Quaternion rotation)
    {
        if (selectedPrefab == null) return false;
        
        // Check distance from player (more generous)
        float distanceFromPlayer = Vector3.Distance(transform.position, position);
        if (distanceFromPlayer < minDistanceFromPlayer || distanceFromPlayer > maxDistanceFromPlayer)
        {
            return false;
        }
        
        // Check surface angle if required (more generous)
        if (requireFlatSurface)
        {
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out hit, 5f, groundLayer))
            {
                float angle = Vector3.Angle(hit.normal, Vector3.up);
                if (angle > maxSurfaceAngle)
                {
                    return false;
                }
            }
        }
        
        // More generous obstacle detection - only check for major overlaps
        Collider[] prefabColliders = selectedPrefab.GetComponentsInChildren<Collider>();
        
        foreach (Collider prefabCollider in prefabColliders)
        {
            // Calculate the world bounds of the collider at the target position
            Bounds worldBounds = prefabCollider.bounds;
            Vector3 boundsCenter = position + rotation * (prefabCollider.bounds.center - prefabCollider.transform.position);
            
            // Create a new bounds at the target position with some tolerance
            Bounds targetBounds = new Bounds(boundsCenter, worldBounds.size * 0.8f); // 20% smaller for tolerance
            
            // Check for overlaps using the bounds, but be more lenient
            Collider[] overlaps = Physics.OverlapBox(
                targetBounds.center,
                targetBounds.extents,
                rotation,
                obstacleLayer
            );
            
            // Only fail if there's a significant overlap (more than 50% overlap)
            int significantOverlaps = 0;
            foreach (Collider overlap in overlaps)
            {
                // Check if the overlap is significant
                Bounds overlapBounds = overlap.bounds;
                float overlapVolume = CalculateOverlapVolume(targetBounds, overlapBounds);
                float targetVolume = targetBounds.size.x * targetBounds.size.y * targetBounds.size.z;
                
                if (overlapVolume > targetVolume * 0.5f) // 50% overlap threshold
                {
                    significantOverlaps++;
                }
            }
            
            if (significantOverlaps > 0)
            {
                return false;
            }
        }
        
        return true;
    }
    
    float CalculateOverlapVolume(Bounds bounds1, Bounds bounds2)
    {
        // Calculate intersection bounds
        Vector3 min = Vector3.Max(bounds1.min, bounds2.min);
        Vector3 max = Vector3.Min(bounds1.max, bounds2.max);
        
        // Check if there's an intersection
        if (min.x >= max.x || min.y >= max.y || min.z >= max.z)
        {
            return 0f;
        }
        
        // Calculate overlap volume
        Vector3 size = max - min;
        return size.x * size.y * size.z;
    }
    
    void TryPlaceBuilding()
    {
        if (!canPlace || selectedPrefab == null) return;
        
        // Instantiate the actual building
        GameObject placedBuilding = Instantiate(selectedPrefab, placementPosition, placementRotation);
        placedBuilding.name = $"{selectedPrefab.name}_{System.DateTime.Now.Ticks}";
        
        // Invoke the event
        OnBuildingPlaced?.Invoke(placedBuilding, placementPosition, placementRotation);
        
        Debug.Log($"Placed building: {placedBuilding.name} at {placementPosition}");
        
        // Exit build mode after placement
        ExitBuildMode();
    }
    
    void HandleRotationInput()
    {
        // Rotate with Q and E keys
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            placementRotation *= Quaternion.Euler(0, -rotationSpeed, 0);
        }
        else if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            placementRotation *= Quaternion.Euler(0, rotationSpeed, 0);
        }
        
        // Rotate with mouse wheel
        float scrollDelta = Mouse.current.scroll.ReadValue().y;
        if (scrollDelta != 0)
        {
            placementRotation *= Quaternion.Euler(0, scrollDelta * rotationSpeed * 0.1f, 0);
        }
    }
    
    void HandleKeyboardInput()
    {
        // Number keys 1-9 for quick prefab selection
        for (int i = 0; i < Mathf.Min(9, buildablePrefabs.Length); i++)
        {
            Key key = (Key)((int)Key.Digit1 + i);
            if (Keyboard.current[key].wasPressedThisFrame)
            {
                SetCurrentPrefab(i);
                if (!isInBuildMode)
                {
                    EnterBuildMode();
                }
                break;
            }
        }
        
        // Tab to cycle through prefabs
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            CyclePrefab();
        }
        
        // Escape to exit build mode
        if (Keyboard.current.escapeKey.wasPressedThisFrame && isInBuildMode)
        {
            ExitBuildMode();
        }
    }
    
    // Public methods for external control
    public void SetBuildablePrefabs(GameObject[] prefabs)
    {
        buildablePrefabs = prefabs;
        currentPrefabIndex = 0;
    }
    
    public void SetCurrentPrefab(int index)
    {
        if (index >= 0 && index < buildablePrefabs.Length)
        {
            currentPrefabIndex = index;
            if (isInBuildMode)
            {
                selectedPrefab = buildablePrefabs[currentPrefabIndex];
                DestroyPreview();
                CreatePreview();
            }
        }
    }
    
    public GameObject GetCurrentPrefab()
    {
        return selectedPrefab;
    }
    
    public bool IsInBuildMode()
    {
        return isInBuildMode;
    }
    
    public bool CanPlace()
    {
        return canPlace;
    }
    
    public Vector3 GetPlacementPosition()
    {
        return placementPosition;
    }
    
    // Gizmos for debugging
    void OnDrawGizmosSelected()
    {
        if (isInBuildMode && selectedPrefab != null)
        {
            // Draw placement area
            Gizmos.color = canPlace ? Color.green : Color.red;
            Gizmos.DrawWireSphere(placementPosition, 0.5f);
            
            // Draw distance constraints
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minDistanceFromPlayer);
            Gizmos.DrawWireSphere(transform.position, maxDistanceFromPlayer);
        }
    }
} 