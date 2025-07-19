using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using YamlDotNet.Serialization;

class LevelData {
    public uint BlockSize { get; set; }
    public uint BlockScale { get; set; }
    public Point Player { get; set; }
    public List<List<List<uint>>> Layers { get; set; }
}

public class EnvironmentGenerator : MonoBehaviour {
    [SerializeField] private GameObject[] models;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private TextAsset levelAsset;

    private LevelData ParseLevelData() {
        var deserializer = new Deserializer();
        return deserializer.Deserialize<LevelData>(levelAsset.text);
    }

    private void CreatePlayer(LevelData levelData) {
        var x = levelData.Player.X * levelData.BlockSize;
        var z = levelData.Player.Y * levelData.BlockSize;
        playerModel.transform.position = new Vector3(x, 3, z);
    }

    private Object CreateEntity(LevelData levelData, int layer, int x, int y) {
        // 0 means empty space, so we add 1 to the model number
        var modelNumber = levelData.Layers[layer][x][y];
        
        if (modelNumber == 0 || modelNumber > models.Length) return null;
        
        // Subtract 1 to get the index of the model since 0 represents an empty value
        modelNumber--;

        var model = models[modelNumber];

        var clone = Instantiate(
            model,
            new Vector3(x * levelData.BlockSize, 0, y * levelData.BlockSize),
            model.transform.rotation
        );

        clone.transform.localScale = new Vector3(levelData.BlockScale, levelData.BlockScale, levelData.BlockScale);
        
        return clone;
    }

    private void CreateEnvironment(LevelData levelData) {
        var sceneryGameObject = new GameObject("Scenery");
        
        for (var i = 0; i < levelData.Layers.Count; i++) {
            var layer = levelData.Layers[i];
            var layerGameObject = new GameObject($"Layer {i}") {
                transform = {
                    parent = sceneryGameObject.transform
                }
            };
            
            var layerName = i == 0 ? "Ground" : "Scenery";
            layerGameObject.layer = LayerMask.NameToLayer(layerName);

            for (var j = 0; j < layer.Count; j++) {
                var row = layer[j];
                
                for (var k = 0; k < row.Count; k++) {
                    var entity = CreateEntity(levelData, i, j, k);
                    
                    if (entity == null) continue;
                    
                    entity.GameObject().transform.parent = layerGameObject.transform;
                }
            }
        }
    }

    void Start() {
        var levelData = ParseLevelData();
        CreateEnvironment(levelData);
        CreatePlayer(levelData);
    }

    void Update() {
    }
}