using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BuildingSystem : MonoBehaviour, PlayerInput.IPlayerActions
{
    [Header("Building Settings")]
    [SerializeField] private GameObject[] buildablePrefabs;
    [SerializeField] private Material previewMaterial;
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private float maxRaycastDistance = 100f;
    [SerializeField] private float placementOffset = 0f;
    
    [Header("Building Constraints")]
    [SerializeField] private float minDistanceFromPlayer = 2f;
    [SerializeField] private float maxDistanceFromPlayer = 15f;
    [SerializeField] private bool requireFlatSurface = true;
    [SerializeField] private float maxSurfaceAngle = 25f;
    [SerializeField] private float minDistanceBetweenBuildings = 1f;
    
    [Header("Preview Settings")]
    [SerializeField] private float previewOpacity = 0.6f;
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;
    
    private PlayerInput playerInput;
    private Camera playerCamera;
    private GameObject currentPreview;
    private GameObject selectedPrefab;
    private int currentPrefabIndex = 0;
    private bool isInBuildMode = false;
    private bool canPlace = false;
    private Vector3 placementPosition;
    private Quaternion placementRotation = Quaternion.identity;
    private List<GameObject> placedBuildings = new List<GameObject>();
    
    public System.Action<GameObject, Vector3, Quaternion> OnBuildingPlaced;
    public System.Action OnBuildModeEntered;
    public System.Action OnBuildModeExited;
    
    void Awake()
    {
        playerInput = new PlayerInput();
        playerCamera = Camera.main;
    }
    
    void OnEnable()
    {
        playerInput.Enable();
        playerInput.Player.SetCallbacks(this);
    }
    
    void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.Player.SetCallbacks(null);
            playerInput.Disable();
        }
        ExitBuildMode();
    }
    
    void Update()
    {
        CleanupDestroyedBuildings();
        
        if (isInBuildMode && selectedPrefab != null)
        {
            UpdatePreview();
            HandleRotationInput();
        }
        
        HandleKeyboardInput();
    }
    
    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.performed && isInBuildMode)
        {
            TryPlaceBuilding();
        }
    }
    
    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.performed && !isInBuildMode)
        {
            // Check if clicking on enemy first
            GameObject enemy = GetEnemyUnderMouse();
            if (enemy != null)
            {
                // Enemy clicked, don't open building panel
                return;
            }
                
            BuildingUI buildingUI = FindFirstObjectByType<BuildingUI>();
            if (buildingUI != null)
            {
                if (buildingUI.IsBuildingPanelVisible())
                {
                    buildingUI.HideBuildingPanel();
                }
                else
                {
                    buildingUI.ShowBuildingPanel();
                }
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
    }
    
    public void StartBuildModeWithPrefab(GameObject prefab)
    {
        if (prefab == null) return;
        
        isInBuildMode = true;
        selectedPrefab = prefab;
        CreatePreview();
        OnBuildModeEntered?.Invoke();
    }
    
    public void ExitBuildMode()
    {
        isInBuildMode = false;
        DestroyPreview();
        selectedPrefab = null;
        OnBuildModeExited?.Invoke();
        
        BuildingUI buildingUI = FindFirstObjectByType<BuildingUI>();
        if (buildingUI != null)
        {
            buildingUI.HideBuildingPanel();
        }
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
    }
    
    void CreatePreview()
    {
        if (selectedPrefab == null) return;
        
        DestroyPreview();
        
        currentPreview = Instantiate(selectedPrefab);
        currentPreview.name = $"{selectedPrefab.name}_Preview";
        
        Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material previewMat = new Material(previewMaterial);
            previewMat.color = new Color(previewMat.color.r, previewMat.color.g, previewMat.color.b, previewOpacity);
            renderer.material = previewMat;
        }
        
        Collider[] colliders = currentPreview.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            DestroyImmediate(collider);
        }
        
        Rigidbody[] rigidbodies = currentPreview.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
            rb.useGravity = false;
        }
        
        MonoBehaviour[] scripts = currentPreview.GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            script.enabled = false;
        }
        
        SetLayerRecursively(currentPreview, LayerMask.NameToLayer("Ignore Raycast"));
        
        currentPreview.SetActive(false);
        currentPreview.SetActive(true);
    }
    
    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    
    void CleanupDestroyedBuildings()
    {
        placedBuildings.RemoveAll(building => building == null);
    }
    
    void DestroyPreview()
    {
        if (currentPreview != null)
        {
            DestroyImmediate(currentPreview);
            currentPreview = null;
        }
    }
    
    void UpdatePreview()
    {
        if (currentPreview == null) return;
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, groundLayer))
        {
            placementPosition = hit.point + Vector3.up * placementOffset;
            
            currentPreview.transform.position = placementPosition;
            currentPreview.transform.rotation = placementRotation;
            
            canPlace = IsPlacementValid(placementPosition, placementRotation);
            
            UpdatePreviewAppearance();
        }
        else
        {
            placementPosition = playerCamera.transform.position + playerCamera.transform.forward * 5f;
            currentPreview.transform.position = placementPosition;
            currentPreview.transform.rotation = placementRotation;
            
            canPlace = false;
            UpdatePreviewAppearance();
        }
    }
    
    void UpdatePreviewAppearance()
    {
        if (currentPreview == null) return;
        
        Color targetColor = canPlace ? validColor : invalidColor;
        targetColor.a = previewOpacity;
        
        Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
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
        
        float distanceFromPlayer = Vector3.Distance(transform.position, position);
        if (distanceFromPlayer < minDistanceFromPlayer || distanceFromPlayer > maxDistanceFromPlayer)
        {
            return false;
        }
        
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
        
        Collider[] prefabColliders = selectedPrefab.GetComponentsInChildren<Collider>();
        
        if (prefabColliders.Length == 0)
        {
            return true;
        }
        
        foreach (Collider prefabCollider in prefabColliders)
        {
            Vector3 buildingCenter = position + rotation * (prefabCollider.bounds.center - prefabCollider.transform.position);
            Vector3 buildingSize = prefabCollider.bounds.size;
            
            Collider[] objectsInArea = Physics.OverlapBox(
                buildingCenter,
                buildingSize * 0.5f,
                rotation
            );
            
            foreach (Collider obj in objectsInArea)
            {
                if (obj != null)
                {
                    if (obj.gameObject == currentPreview || obj.gameObject == gameObject || 
                        obj.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
                    {
                        continue;
                    }
                    
                    return false;
                }
            }
        }
        
        GameObject[] allTowers = GameObject.FindGameObjectsWithTag("Tower");
        foreach (GameObject tower in allTowers)
        {
            if (tower != null && tower != currentPreview)
            {
                float distance = Vector3.Distance(position, tower.transform.position);
                if (distance < 10f)
                {
                    return false;
                }
            }
        }
        
        if (minDistanceBetweenBuildings > 0)
        {
            GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Untagged");
            
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj != currentPreview && obj != gameObject && !placedBuildings.Contains(obj))
                {
                    float distance = Vector3.Distance(position, obj.transform.position);
                    if (distance < minDistanceBetweenBuildings)
                    {
                        return false;
                    }
                }
            }
        }
        
        return true;
    }
    
    void TryPlaceBuilding()
    {
        if (!canPlace || selectedPrefab == null) return;
        
        GameObject placedBuilding = Instantiate(selectedPrefab, placementPosition, placementRotation);
        placedBuilding.name = $"{selectedPrefab.name}_{System.DateTime.Now.Ticks}";
        
        placedBuildings.Add(placedBuilding);
        
        OnBuildingPlaced?.Invoke(placedBuilding, placementPosition, placementRotation);
        
        ExitBuildMode();
    }
    
    void HandleRotationInput()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            placementRotation *= Quaternion.Euler(0, -90, 0);
        }
        else if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            placementRotation *= Quaternion.Euler(0, 90, 0);
        }
        
        float scrollDelta = Mouse.current.scroll.ReadValue().y;
        if (scrollDelta != 0)
        {
            placementRotation *= Quaternion.Euler(0, scrollDelta * 15f, 0);
        }
    }
    
    void HandleKeyboardInput()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            CyclePrefab();
        }
        
        if (Keyboard.current.escapeKey.wasPressedThisFrame && isInBuildMode)
        {
            ExitBuildMode();
        }
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
    
    public GameObject[] GetBuildablePrefabs()
    {
        return buildablePrefabs;
    }
    

    
    GameObject GetEnemyUnderMouse()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                return hit.collider.gameObject;
            }
        }
        
        return null;
    }
    
    void OnDrawGizmosSelected()
    {
        if (isInBuildMode && selectedPrefab != null)
        {
            Gizmos.color = canPlace ? Color.green : Color.red;
            Gizmos.DrawWireSphere(placementPosition, 0.5f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minDistanceFromPlayer);
            Gizmos.DrawWireSphere(transform.position, maxDistanceFromPlayer);
        }
    }
} 