using UnityEngine;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class EnvironmentGenerator : MonoBehaviour {
    [SerializeField] private GameObject rockModel;
    [SerializeField] private GameObject treeModel;
    [SerializeField] private GameObject floorPlane;
    [SerializeField] private TextAsset levelAsset;

    void Start() {
        var modelMap = new Dictionary<char, GameObject>() {
            {'1', treeModel},
            {'2', rockModel}
        };
        
        string levelData = Regex.Replace(levelAsset.text,"[^\\d]","");
        Vector3 floorSize = floorPlane.GetComponent<Renderer>().bounds.size;

        for (var i = 0; i < levelData.Length; i++) {
            var c = levelData[i];
            
            if(modelMap.ContainsKey(c) == false) continue;
            
            var x = i % 10;
            var z = (int)Math.Floor((float)i / 10.0);
            var model = modelMap[c];
            
            var clone = Instantiate(model,
                new Vector3((x * 10) - (floorSize.x / 2), 0, (z * 10) - (floorSize.z / 2)),
                model.transform.rotation
            );
        
            clone.transform.localScale = new Vector3(10, 10, 10);
        }
    }

    void Update() {
        
    }
}
