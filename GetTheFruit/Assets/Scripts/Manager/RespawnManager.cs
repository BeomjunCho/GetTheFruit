using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Stores the last checkpoint position and respawns the whole player group
/// when any registered player dies.
/// </summary>
public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float _respawnDelay = 0.5f;

    private readonly List<PlayerRespawnHandler> _players = new();
    private Vector3 _lastCheckpointPos;
    private bool _hasCheckpoint;
    private bool _isRespawning;

    public bool HasCheckpoint => _hasCheckpoint;
    public Vector3 LastCheckpointPos => _lastCheckpointPos;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedSceneRespawn());
        if (Camera.main.TryGetComponent(out CameraController cam))
            cam.SnapImmediately();
    }

    private IEnumerator DelayedSceneRespawn()
    {
        yield return null; 
        if (!HasCheckpoint) yield break;
        foreach (var p in _players)
            p.OnRespawn(_lastCheckpointPos);
    }

    /* ------------------------------------------------------------------ */
    /*  Registration                                                      */
    public void RegisterPlayer(PlayerRespawnHandler player)
    {
        if (!_players.Contains(player))
            _players.Add(player);
    }

    public void UnregisterPlayer(PlayerRespawnHandler player)
    {
        _players.Remove(player);
    }

    /* ------------------------------------------------------------------ */
    /*  Checkpoint                                                        */
    public void RegisterCheckpoint(Vector3 position)
    {
        _lastCheckpointPos = position;
        _hasCheckpoint = true;
    }

    /* ------------------------------------------------------------------ */
    /*  Group respawn                                                     */
    public void RequestGroupRespawn()
    {
        if (_isRespawning || !_hasCheckpoint) return;
        StartCoroutine(GroupRespawnRoutine());
    }

    /// <summary>Clears saved checkpoint so next level starts fresh.</summary>
    public void ResetCheckpoint()
    {
        _hasCheckpoint = false;
        _lastCheckpointPos = Vector3.zero;
    }

    private System.Collections.IEnumerator GroupRespawnRoutine()
    {
        _isRespawning = true;

        // Phase 1: mark all players dead
        foreach (var p in _players)
            p.OnDeathStart();

        yield return new WaitForSeconds(_respawnDelay);

        // Phase 2: respawn every player at the checkpoint
        foreach (var p in _players)
            p.OnRespawn(_lastCheckpointPos);

        _isRespawning = false;
    }
}
