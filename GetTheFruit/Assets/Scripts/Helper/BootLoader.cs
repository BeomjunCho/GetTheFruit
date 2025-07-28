using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entry point in the Boot scene. Creates a ManagersRoot that persists
/// across scenes and instantiates all manager prefabs under it.
/// </summary>
public class BootLoader : MonoBehaviour
{
    [Header("Boot Settings")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    [Header("Manager Prefabs")]
    [SerializeField] private List<GameObject> _managerPrefabs = new();

    private IEnumerator Start()
    {
        // 1) Create a persistent root object
        GameObject root = new GameObject("ManagersRoot");
        DontDestroyOnLoad(root);

        // 2) Instantiate every manager prefab as a child of the root
        foreach (GameObject prefab in _managerPrefabs)
        {
            if (prefab == null) continue;
            GameObject instance = Instantiate(prefab, root.transform);
            instance.name = prefab.name; // remove "(Clone)"
        }

        // 3) Guarantee a SceneFlowManager exists
        SceneFlowManager flow = root.GetComponentInChildren<SceneFlowManager>(true);
        if (flow == null)
        {
            GameObject go = new GameObject("SceneFlowManager");   // create GO
            go.transform.SetParent(root.transform, false);        // set parent (void)
            flow = go.AddComponent<SceneFlowManager>();            // get component
        }

        // 4) Wait one frame to let manager Awake() methods finish
        yield return null;

        // 5) Load Main Menu
        flow.LoadMainMenu(_mainMenuSceneName);
    }
}
