using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using YamlDotNet.Serialization;

class LevelData {
    public uint BlockSize { get; set; }
    public uint BlockScale { get; set; }
    public Point Size { get; set; }
    public Point Player { get; set; }
    public List<List<List<uint>>> Layers { get; set; }
}

public class EnvironmentGenerator : MonoBehaviour {
    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject rockModel;
    [SerializeField] private GameObject treeModel;
    [SerializeField] private TextAsset levelAsset;

    private Dictionary<uint, GameObject> GetModelMap() {
        return new Dictionary<uint, GameObject>() {
            { 1, treeModel },
            { 2, rockModel }
        };
    }

    private LevelData ParseLevelData() {
        var deserializer = new Deserializer();
        return deserializer.Deserialize<LevelData>(levelAsset.text);
    }

    private void CreatePlayer(LevelData levelData) {
        var x = levelData.Player.X * levelData.BlockSize;
        var z = levelData.Player.Y * levelData.BlockSize;
        playerModel.transform.position = new Vector3(x, 0, z);
    }

    private GameObject CreateFloor(LevelData levelData) {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        var x = levelData.Size.X * levelData.BlockSize / 2;
        var y = levelData.Size.Y * levelData.BlockSize / 2;
        floor.transform.position = new Vector3(x, -1, y);
        floor.transform.localScale = new Vector3(levelData.Size.X, 1, levelData.Size.Y);
        return floor;
    }

    private Object CreateEntity(LevelData levelData, Dictionary<uint, GameObject> modelMap, int layer, int x, int y) {
        var modelNumber = levelData.Layers[layer][x][y];

        if (modelMap.TryGetValue(modelNumber, out var model) == false) return null;

        var clone = Instantiate(
            model,
            new Vector3(x * levelData.BlockSize, 0, y * levelData.BlockSize),
            model.transform.rotation
        );

        clone.transform.localScale = new Vector3(levelData.BlockScale, levelData.BlockScale, levelData.BlockScale);

        return clone;
    }

    private void CreateEnvironment(LevelData levelData) {
        var modelMap = GetModelMap();

        for (var i = 0; i < levelData.Layers.Count; i++) {
            var layer = levelData.Layers[i];

            for (var j = 0; j < layer.Count; j++) {
                var row = layer[j];

                for (var k = 0; k < row.Count; k++) {
                    CreateEntity(levelData, modelMap, i, j, k);
                }
            }
        }
    }

    void Start() {
        var levelData = ParseLevelData();
        CreateFloor(levelData);
        CreateEnvironment(levelData);
        CreatePlayer(levelData);
    }

    void Update() {
    }
}