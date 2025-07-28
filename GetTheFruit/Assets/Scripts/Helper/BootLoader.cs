using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Entry point in the Boot scene. Instantiates manager prefabs,
/// ensures SceneFlowManager exists, then loads the Main Menu scene.
/// </summary>
public class BootLoader : MonoBehaviour
{
    [Header("Boot Settings")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    [Header("Manager Prefabs")]
    [Tooltip("Assign manager prefabs that should persist across scenes.")]
    [SerializeField] private List<GameObject> _managerPrefabs = new();

    private IEnumerator Start()
    {
        // Create a persistent root object
        GameObject root = new GameObject("ManagersRoot");
        DontDestroyOnLoad(root);

        // Instantiate each manager prefab under the root
        foreach (GameObject prefab in _managerPrefabs)
        {
            if (prefab == null) continue;
            GameObject instance = Instantiate(prefab, root.transform);
            instance.name = prefab.name; // remove "(Clone)" in hierarchy
        }

        // Ensure SceneFlowManager exists
        SceneFlowManager flow = root.GetComponentInChildren<SceneFlowManager>(true);
        if (flow == null)
            flow = root.AddComponent<SceneFlowManager>();

        // Wait one frame to let Awake() execute
        yield return null;

        // Load Main Menu asynchronously
        flow.LoadMainMenu(_mainMenuSceneName);
    }
}
