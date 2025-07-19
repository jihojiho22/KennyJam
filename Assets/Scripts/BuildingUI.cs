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
        buildingSystem = FindFirstObjectByType<BuildingSystem>();
        
        if (buildingSystem == null)
        {
            Debug.LogError("BuildingUI: No BuildingSystem found in scene!");
            return;
        }
        
        buildingSystem.OnBuildModeEntered += OnBuildModeEntered;
        buildingSystem.OnBuildModeExited += OnBuildModeExited;
        buildingSystem.OnBuildingPlaced += OnBuildingPlaced;
        
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
                               "• Mouse Wheel: Rotate Building";
        }
    }
    
    void OnBuildModeExited()
    {
        if (buildingPanel != null)
            buildingPanel.SetActive(false);
    }
    
    void OnBuildingPlaced(GameObject building, Vector3 position, Quaternion rotation)
    {
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
    
    public void StartBuildWithPrefab(GameObject prefab)
    {
        if (buildingSystem != null && prefab != null)
        {
            buildingSystem.StartBuildModeWithPrefab(prefab);
        }
    }
    
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
    
    public bool IsBuildingPanelVisible()
    {
        return buildingPanel != null && buildingPanel.activeSelf;
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