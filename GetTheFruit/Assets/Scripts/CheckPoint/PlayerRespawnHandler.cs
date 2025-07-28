using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles death and respawn for a single player.
/// Real respawn is coordinated by RespawnManager.
/// </summary>
public class PlayerRespawnHandler : MonoBehaviour
{
    private bool _isDead;

    /* ------------------------------------------------------------------ */
    /*  Unity lifecycle                                                   */
    private void Start()
    {
        // Register after RespawnManager Awake has run
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.RegisterPlayer(this);
        else
            Debug.LogError("RespawnManager instance not found in scene.");
    }

    private void OnDestroy()
    {
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.UnregisterPlayer(this);
    }

    private void OnEnable()
    {
        GameInputManager.Instance.Controls.Global.Reset.performed += OnReset;
    }

    private void OnDisable()
    {
        GameInputManager.Instance.Controls.Global.Reset.performed -= OnReset;
    }

    private void OnReset(InputAction.CallbackContext ctx)
    {
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    /* ------------------------------------------------------------------ */
    /*  Public API                                                        */
    public void Die()
    {
        if (_isDead) return;
       
        RespawnManager.Instance.RequestGroupRespawn();
    }

    /* ------------------------------------------------------------------ */
    /*  Called by RespawnManager                                          */
    public void OnDeathStart()
    {
        if (_isDead) return;

        _isDead = true;
        gameObject.SetActive(false); // hide for now
    }

    public void OnRespawn(Vector3 pos)
    {
        transform.position = pos;
        gameObject.SetActive(true);
        _isDead = false;
    }
}
