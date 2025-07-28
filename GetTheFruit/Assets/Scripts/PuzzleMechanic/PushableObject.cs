using UnityEngine;

/// <summary>
/// Heavy block that moves only when pushed by RockMan.
/// Any other collision is damped to zero horizontal velocity.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PushableObject : MonoBehaviour
{
    /* ------------------------------------------------------------------ */
    /*  Inspector                                                         */
    /* ------------------------------------------------------------------ */
    [Header("Physics Settings")]
    [SerializeField] private float _mass = 20f;
    [SerializeField] private float _idleDrag = 5f;
    [SerializeField] private float _moveDrag = 1f;
    [SerializeField] private float _maxSpeed = 5f;

    [Header("Ground Check (optional)")]
    [SerializeField] private Transform _groundCheck = null;
    [SerializeField] private float _groundRadius = 0.05f;
    [SerializeField] private LayerMask _groundMask;

    /* ------------------------------------------------------------------ */
    /*  Cached components                                                 */
    /* ------------------------------------------------------------------ */
    private Rigidbody2D _rb;

    /* ------------------------------------------------------------------ */
    /*  State                                                             */
    /* ------------------------------------------------------------------ */
    private bool _isBeingPushed;


    /* ------------------------------------------------------------------ */
    /*  Cached original constraint                                         */
    /* ------------------------------------------------------------------ */
    private RigidbodyConstraints2D _defaultConstraints;

    /* ------------------------------------------------------------------ */
    /*  Unity lifecycle                                                   */
    /* ------------------------------------------------------------------ */
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.mass = _mass;
        _rb.freezeRotation = true;

        _defaultConstraints = _rb.constraints;   // usually FreezeRotation
        _rb.constraints |= RigidbodyConstraints2D.FreezePositionX; // lock X by default

        gameObject.tag = "Pushable";
    }

    /* ------------------------------------------------------------------ */
    /*  FixedUpdate                                                       */
    /* ------------------------------------------------------------------ */
    private void FixedUpdate()
    {
        ClampHorizontalSpeed();
        AdaptDrag();
        _isBeingPushed = false; // reset flag each physics step
    }

    /* ------------------------------------------------------------------ */
    /*  Helpers                                                           */
    /* ------------------------------------------------------------------ */
    private void ClampHorizontalSpeed()
    {
        Vector2 v = _rb.velocity;
        v.x = Mathf.Clamp(v.x, -_maxSpeed, _maxSpeed);
        _rb.velocity = v;
    }
    private void AdaptDrag()
    {
        if (_isBeingPushed)
        {
            // unlock X so RockMan can move the box
            _rb.constraints = _defaultConstraints;   // only FreezeRotation
            _rb.drag = _moveDrag;
        }
        else
        {
            // lock X to eliminate micro pushes from SlimeWoman or physics resolution
            _rb.constraints = _defaultConstraints | RigidbodyConstraints2D.FreezePositionX;
            _rb.drag = _idleDrag;
        }
    }

    private bool IsGrounded()
    {
        if (_groundCheck == null) return true;
        return Physics2D.OverlapCircle(_groundCheck.position,
                                       _groundRadius, _groundMask);
    }

    /* ------------------------------------------------------------------ */
    /*  Collision with RockMan                                            */
    /* ------------------------------------------------------------------ */
    private void OnCollisionStay2D(Collision2D col)
    {
        if (!IsGrounded()) return;
        if (!col.transform.CompareTag("Player")) return;

        // allow push only when RockMan is the collider
        if (col.transform.TryGetComponent<RockManController>(out _))
            _isBeingPushed = true;
    }
}
