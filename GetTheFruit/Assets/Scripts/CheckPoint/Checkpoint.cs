using UnityEngine;

/// <summary>
/// Trigger zone that registers itself as the active checkpoint when the player enters.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject _activateEffect;

    private bool _isActivated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isActivated) return;
        if (!other.CompareTag("Player")) return;

        _isActivated = true;
        RespawnManager.Instance.RegisterCheckpoint(transform.position);

        if (_activateEffect != null)
            _activateEffect.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (_activateEffect.activeSelf)
        {
            _activateEffect.SetActive(false);
        }
    }
}
