using UnityEngine;

/// <summary>
/// Shared movement, gravity and ground‑check logic for all player characters.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public abstract class CharacterControllerBase : MonoBehaviour
{
    /* ------------------------------------------------------------------ */
    /*  Inspector                                                         */
    /* ------------------------------------------------------------------ */
    [Header("Movement Settings")]
    [SerializeField] protected float _moveSpeed = 6f;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck = null;
    [SerializeField] private float _groundRadius = 0.1f;
    [SerializeField] private LayerMask _groundMask;

    [Header("Refs")]
    [SerializeField] protected Animator _anim = null;

    /* ------------------------------------------------------------------ */
    /*  Cached components                                                 */
    /* ------------------------------------------------------------------ */
    protected Rigidbody2D _rb;

    /* ------------------------------------------------------------------ */
    /*  State                                                             */
    /* ------------------------------------------------------------------ */
    protected bool _isGrounded;
    private Vector3 _originalScale;   // stores prefab scale (e.g. 4,4,4)

    /* ------------------------------------------------------------------ */
    /*  Unity lifecycle                                                   */
    /* ------------------------------------------------------------------ */
    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (_anim == null ) 
            _anim = GetComponent<Animator>();

        _originalScale = transform.localScale;           // cache initial scale
    }

    protected virtual void Update()
    {
        UpdateGroundStatus();
    }

    /* ------------------------------------------------------------------ */
    /*  Public API                                                        */
    /* ------------------------------------------------------------------ */

    /// <summary>
    /// Moves the rigidbody horizontally.
    /// </summary>
    /// <param name="direction">X direction from ‑1 to 1.</param>
    public void Move(float direction)
    {
        Vector2 v = _rb.velocity;
        v.x = direction * _moveSpeed;
        _rb.velocity = v;

        if (direction != 0f)
        {
            // flip only the X sign, keep original Y,Z scale
            float sign = Mathf.Sign(direction);
            transform.localScale = new Vector3(sign * Mathf.Abs(_originalScale.x),
                                               _originalScale.y,
                                               _originalScale.z);
        }
    }

    /// <summary>
    /// Override if the character can jump.
    /// </summary>
    public virtual void TryJump() { }

    /* ------------------------------------------------------------------ */
    /*  Helpers                                                           */
    /* ------------------------------------------------------------------ */
    private void UpdateGroundStatus()
    {
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position,
                                              _groundRadius, _groundMask);
    }
}
