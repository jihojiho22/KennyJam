using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    [SerializeField] private int minimumSpawnTime = 10;
    [SerializeField] private int maximumSpawnTime = 15;
    [SerializeField] private string[] enemyLabels;
    private List<GameObject> models = new();
    private GameObject parent;

    private float lastSpawnTime;
    
    GameObject SpawnEnemy() {
        lastSpawnTime = Time.time;
        
        var model = models[Random.Range(0, models.Count)];
        
        Debug.Log($"Spawning enemy {model.name}");
        
        var enemy =  Instantiate(
            model,
            transform.position,
            model.transform.rotation
        );
        
        enemy.transform.parent = parent.transform;

        return enemy;
    }

    void Start() {
        var parentName = "EnemySpawner Enemies";
        parent = GameObject.Find(parentName) ?? new GameObject(parentName);

        lastSpawnTime = Time.deltaTime;
        
        models.Clear();

        models = AssetDatabase
            .FindAssetGUIDs("l:Enemy")
            .Where(guid => enemyLabels.Length == 0 || AssetDatabase.GetLabels(guid).Intersect(enemyLabels).Any())
            .Select(AssetDatabase.LoadAssetByGUID<GameObject>)
            .OrderBy(go => go.name)
            .ToList();
    }

    void Update() {
        if (Time.time - lastSpawnTime > Random.Range(minimumSpawnTime, maximumSpawnTime)) {
            SpawnEnemy();
        }
    }
}