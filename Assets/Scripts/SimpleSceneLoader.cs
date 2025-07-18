using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneName = "";
    
    /// <summary>
    /// Loads the scene specified in the inspector
    /// </summary>
    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is empty! Please assign a scene name in the inspector.");
            return;
        }
        
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// Loads a scene with the specified name
    /// </summary>
    /// <param name="sceneToLoad">Name of the scene to load</param>
    public void LoadScene(string sceneToLoad)
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("Scene name is empty!");
            return;
        }
        
        Debug.Log($"Loading scene: {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
        //TODO: Add a loading screen
    }
} 