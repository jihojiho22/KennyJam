using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject buildingPanel;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI currentPrefabText;
    [SerializeField] private TextMeshProUGUI controlsText;
    [SerializeField] private Image statusIndicator;
    
    [Header("Status Colors")]
    [SerializeField] private Color validPlacementColor = Color.green;
    [SerializeField] private Color invalidPlacementColor = Color.red;
    [SerializeField] private Color neutralColor = Color.white;
    
    private BuildingSystem buildingSystem;
    
    void Start()
    {
        // Find the building system
        buildingSystem = FindFirstObjectByType<BuildingSystem>();
        
        if (buildingSystem == null)
        {
            Debug.LogError("BuildingUI: No BuildingSystem found in scene!");
            return;
        }
        
        // Subscribe to building system events
        buildingSystem.OnBuildModeEntered += OnBuildModeEntered;
        buildingSystem.OnBuildModeExited += OnBuildModeExited;
        buildingSystem.OnBuildingPlaced += OnBuildingPlaced;
        
        // Hide panel initially
        if (buildingPanel != null)
            buildingPanel.SetActive(false);
    }
    
    void Update()
    {
        if (buildingSystem != null && buildingSystem.IsInBuildMode())
        {
            UpdateUI();
        }
    }
    
    void UpdateUI()
    {
        // Update status text
        if (statusText != null)
        {
            if (buildingSystem.CanPlace())
            {
                statusText.text = "Ready to Place";
                statusText.color = validPlacementColor;
            }
            else
            {
                statusText.text = "Cannot Place Here";
                statusText.color = invalidPlacementColor;
            }
        }
        
        // Update current prefab text
        if (currentPrefabText != null)
        {
            GameObject currentPrefab = buildingSystem.GetCurrentPrefab();
            if (currentPrefab != null)
            {
                currentPrefabText.text = $"Current: {currentPrefab.name}";
            }
        }
        
        // Update status indicator
        if (statusIndicator != null)
        {
            statusIndicator.color = buildingSystem.CanPlace() ? validPlacementColor : invalidPlacementColor;
        }
    }
    
    void OnBuildModeEntered()
    {
        if (buildingPanel != null)
            buildingPanel.SetActive(true);
        
        if (controlsText != null)
        {
            controlsText.text = "Controls:\n" +
                               "• Left Click: Place Building\n" +
                               "• Right Click: Exit Build Mode\n" +
                               "• Q/E: Rotate Building\n" +
                               "• Mouse Wheel: Rotate Building\n" +
                               "• [1-9]: Select Different Prefab";
        }
    }
    
    void OnBuildModeExited()
    {
        if (buildingPanel != null)
            buildingPanel.SetActive(false);
    }
    
    void OnBuildingPlaced(GameObject building, Vector3 position, Quaternion rotation)
    {
        if (statusText != null)
        {
            statusText.text = $"Placed {building.name}!";
            statusText.color = validPlacementColor;
        }
    }
    
    void OnDestroy()
    {
        if (buildingSystem != null)
        {
            buildingSystem.OnBuildModeEntered -= OnBuildModeEntered;
            buildingSystem.OnBuildModeExited -= OnBuildModeExited;
            buildingSystem.OnBuildingPlaced -= OnBuildingPlaced;
        }
    }
    
    // Public methods for external UI control
    public void ShowBuildingPanel()
    {
        if (buildingPanel != null)
            buildingPanel.SetActive(true);
    }
    
    public void HideBuildingPanel()
    {
        if (buildingPanel != null)
            buildingPanel.SetActive(false);
    }
    
    public void SetStatusText(string text, Color color)
    {
        if (statusText != null)
        {
            statusText.text = text;
            statusText.color = color;
        }
    }
} 