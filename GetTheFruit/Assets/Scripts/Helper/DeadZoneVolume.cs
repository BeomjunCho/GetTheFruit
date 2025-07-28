using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Kill zone: any player entering this trigger is killed and respawned.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DeadZoneVolume : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("Optional VFX spawned on death.")]
    private GameObject _deathEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (_deathEffect != null)
            Instantiate(_deathEffect, other.transform.position, Quaternion.identity);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
