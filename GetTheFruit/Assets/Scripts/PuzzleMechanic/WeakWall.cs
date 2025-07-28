using UnityEngine;

/// <summary>
/// Breakable wall that is destroyed only by RockMan's punch.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WeakWall : MonoBehaviour
{
    /* ------------------------------------------------------------------ */
    /*  Inspector                                                         */
    /* ------------------------------------------------------------------ */
    [Header("Break Settings")]
    [SerializeField, Tooltip("Optional particle prefab spawned on break.")]
    private GameObject _breakEffect = null;

    [SerializeField, Tooltip("Seconds to wait before destroying the wall.")]
    private float _destroyDelay = 0.05f;

    /* ------------------------------------------------------------------ */
    /*  Public API                                                        */
    /* ------------------------------------------------------------------ */

    /// <summary>
    /// Called by RockManController when the punch ray hits this wall.
    /// </summary>
    public void Break()
    {
        // Prevent multiple calls
        if (!gameObject.activeSelf) return;

        // Spawn particle effect at the wall's position
        if (_breakEffect != null)
            Instantiate(_breakEffect, transform.position, Quaternion.identity);

        // Disable visual & collision immediately
        Collider2D col = GetComponent<Collider2D>();
        col.enabled = false;

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        // Schedule final destruction
        Destroy(gameObject, _destroyDelay);
    }
}
