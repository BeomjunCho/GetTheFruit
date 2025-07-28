using UnityEngine;

/// <summary>
/// Keeps two targets on-screen by centering between them and
/// dynamically adjusting orthographic size. Optional world bounds supported.
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
    [SerializeField] private float _followSpeed = 5f;
    //[SerializeField] private float _verticalOffset = 2f;

    [Header("Zoom Settings")]
    [SerializeField, Tooltip("Extra space between target and screen edge.")]
    private float _screenEdgeBuffer = 1f;
    [SerializeField] private float _zoomSpeed = 5f;
    [SerializeField] private float _minSize = 5f;
    [SerializeField] private float _maxSize = 20f;

    [Header("Level Bounds (optional)")]
    [SerializeField] private bool _useBounds = false;
    [SerializeField] private Vector2 _minBounds;
    [SerializeField] private Vector2 _maxBounds;

    [Header("Dynamic Offset")]
    [SerializeField, Tooltip("Max vertical offset when targets are very close.")]
    private float _nearOffset = 3f;
    [SerializeField, Tooltip("Distance at which offset becomes zero.")]
    private float _offsetFadeDistance = 10f;   // targets > this -> no offset

    private bool _snapNextFrame;             

    public void SnapImmediately() => _snapNextFrame = true;   

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

        UpdatePosition();
        UpdateZoom();
        ClampToBounds();
    }

    /* ------------------------------------------------------------------ */
    /*  Private helpers                                                   */
    /* ------------------------------------------------------------------ */

    private void UpdatePosition()
    {
        Vector3 mid = (_targetA.position + _targetB.position) * 0.5f;

        // 1. Distance between targets
        float dist = Vector2.Distance(_targetA.position, _targetB.position);

        // 2. Fade factor: 1 when close, 0 when far
        float t = Mathf.Clamp01(dist / _offsetFadeDistance);   // 0 -> close, 1 -> far
        float dynOffset = Mathf.Lerp(_nearOffset, 0f, t);      // close -> nearOffset, far -> 0

        mid.y += dynOffset;    // apply

        Vector3 desired = new Vector3(mid.x, mid.y, transform.position.z);
        if (_snapNextFrame)
            transform.position = desired;
        else
            transform.position = Vector3.Lerp(transform.position, desired,
                                              _followSpeed * Time.deltaTime);
    }

    /// <summary>Calculates required size so both targets stay visible.</summary>
    private void UpdateZoom()
    {
        Vector3 diff = _targetA.position - _targetB.position;

        float sizeX = Mathf.Abs(diff.x) * 0.5f / _cam.aspect;
        float sizeY = Mathf.Abs(diff.y) * 0.5f;

        // compensate for SAME dynamic offset
        float dist = Vector2.Distance(_targetA.position, _targetB.position);
        float t = Mathf.Clamp01(dist / _offsetFadeDistance);
        float dynOffset = Mathf.Lerp(_nearOffset, 0f, t);
        sizeY += Mathf.Abs(dynOffset);

        float required = Mathf.Clamp(Mathf.Max(sizeX, sizeY) + _screenEdgeBuffer,
                              _minSize, _maxSize);

        if (_snapNextFrame)
        {
            _cam.orthographicSize = required;
            _snapNextFrame = false; 
        }
        else
        {
            _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize,
                                               required,
                                               _zoomSpeed * Time.deltaTime);
        }
    }

    /// <summary>Clamps camera inside predefined world bounds.</summary>
    private void ClampToBounds()
    {
        if (!_useBounds) return;

        float vertExtent = _cam.orthographicSize;
        float horzExtent = vertExtent * _cam.aspect;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x,
                            _minBounds.x + horzExtent,
                            _maxBounds.x - horzExtent);
        pos.y = Mathf.Clamp(pos.y,
                            _minBounds.y + vertExtent,
                            _maxBounds.y - vertExtent);

        transform.position = pos;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!_useBounds) return;

        Gizmos.color = Color.yellow;
        Vector3 min = new Vector3(_minBounds.x, _minBounds.y, 0f);
        Vector3 max = new Vector3(_maxBounds.x, _maxBounds.y, 0f);
        Gizmos.DrawWireCube((min + max) * 0.5f, max - min);
    }
#endif
}
