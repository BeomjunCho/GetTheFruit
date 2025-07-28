using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Spawns both player characters at designated spawn points
/// once the InGame scene is fully loaded.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string _inGameSceneName = "InGame";

    [Header("Player Prefabs")]
    [SerializeField] private GameObject _playerAPrefab; // RockMan
    [SerializeField] private GameObject _playerBPrefab; // SlimeWoman

    private GameObject _playerAInstance;
    private GameObject _playerBInstance;

    /* ================================================================== */
    /*  Unity lifecycle                                                   */
    /* ================================================================== */
    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /* ================================================================== */
    /*  Scene callback                                                    */
    /* ================================================================== */
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != _inGameSceneName) return;

        SpawnPlayers();
    }

    /* ================================================================== */
    /*  Spawning                                                          */
    /* ================================================================== */
    private void SpawnPlayers()
    {
        // Prevent double spawn
        if (_playerAInstance != null) Destroy(_playerAInstance);
        if (_playerBInstance != null) Destroy(_playerBInstance);

        Transform spawnA = GameObject.FindWithTag("SpawnA")?.transform;
        Transform spawnB = GameObject.FindWithTag("SpawnB")?.transform;

        if (spawnA == null || spawnB == null)
        {
            Debug.LogError("Spawn points with tags 'SpawnA' and 'SpawnB' not found.");
            return;
        }

        _playerAInstance = Instantiate(_playerAPrefab, spawnA.position, spawnA.rotation);
        _playerBInstance = Instantiate(_playerBPrefab, spawnB.position, spawnB.rotation);
    }
}
