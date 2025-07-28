using UnityEngine;

/// <summary>
/// Smoothly follows the midpoint of two targets with a vertical offset.
/// Optional world space bounds prevent showing areas outside the level.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    /* ------------------------------------------------------------------ */
    /*  Inspector                                                         */
    /* ------------------------------------------------------------------ */
    [Header("Targets")]
    [SerializeField] private Transform _targetA = null;
    [SerializeField] private Transform _targetB = null;

    [Header("Follow Settings")]
    [SerializeField, Tooltip("Units per second the camera moves.")]
    private float _followSpeed = 5f;

    [SerializeField, Tooltip("Vertical offset added to the midpoint.")]
    private float _verticalOffset = 2f;

    [Header("Level Bounds (optional)")]
    [SerializeField] private bool _useBounds = false;

    [SerializeField] private Vector2 _minBounds;
    [SerializeField] private Vector2 _maxBounds;

    /* ------------------------------------------------------------------ */
    /*  Cached components                                                 */
    /* ------------------------------------------------------------------ */
    private Camera _cam;

    /* ------------------------------------------------------------------ */
    /*  Unity lifecycle                                                   */
    /* ------------------------------------------------------------------ */
    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (_targetA == null || _targetB == null) return;

        /* ------------------------- Follow logic ------------------------ */
        Vector3 mid = (_targetA.position + _targetB.position) * 0.5f;
        mid.y += _verticalOffset;                   // lift above midpoint
        Vector3 desired = new Vector3(mid.x, mid.y, transform.position.z);

        Vector3 smoothed = Vector3.Lerp(transform.position, desired,
                                        _followSpeed * Time.deltaTime);

        /* --------------------------- Bounds --------------------------- */
        if (_useBounds)
        {
            float vertExtent = _cam.orthographicSize;
            float horzExtent = vertExtent * _cam.aspect;

            smoothed.x = Mathf.Clamp(smoothed.x,
                                     _minBounds.x + horzExtent,
                                     _maxBounds.x - horzExtent);

            smoothed.y = Mathf.Clamp(smoothed.y,
                                     _minBounds.y + vertExtent,
                                     _maxBounds.y - vertExtent);
        }

        transform.position = smoothed;
    }

#if UNITY_EDITOR
    /* ------------------------------------------------------------------ */
    /*  Gizmos for bounds                                                 */
    /* ------------------------------------------------------------------ */
    private void OnDrawGizmosSelected()
    {
        if (!_useBounds) return;

        Gizmos.color = Color.yellow;
        Vector3 min = new Vector3(_minBounds.x, _minBounds.y, 0f);
        Vector3 max = new Vector3(_maxBounds.x, _maxBounds.y, 0f);
        Vector3 center = (min + max) * 0.5f;
        Vector3 size = max - min;
        Gizmos.DrawWireCube(center, size);
    }
#endif
}
