using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles move, jump, wall‑stick, wall‑climb and wall‑detach for SlimeWoman (P1).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SlimeWomanController : CharacterControllerBase
{
    /* ------------------------------------------------------------------ */
    /*  Inspector                                                         */
    /* ------------------------------------------------------------------ */
    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 10f;

    [Header("Wall Stick / Climb")]
    [SerializeField] private Transform _wallCheck;
    [SerializeField] private float _wallRadius = 0.1f;
    [SerializeField] private LayerMask _wallMask;
    [SerializeField] private float _climbSpeed = 4f;
    [SerializeField] private float _detachHoldTime = 1f;
    [SerializeField] private float _pushOffForce = 4f;
    [SerializeField] private float _reattachDelay = 0.25f;

    /* ------------------------------------------------------------------ */
    /*  State                                                             */
    /* ------------------------------------------------------------------ */
    private PlayerControls.SlimeWomanActions _a;
    private bool _onWall;
    private float _detachTimer;
    private float _reattachTimer;
    private int _wallSide;          // -1 = left wall, 1 = right wall

    /* ------------------------------------------------------------------ */
    /*  Unity lifecycle                                                   */
    /* ------------------------------------------------------------------ */
    protected override void Awake()
    {
        base.Awake();
        _a = GameInputManager.Instance.Controls.SlimeWoman;
    }

    private void FixedUpdate()
    {
        if (_onWall)
        {
            // Climb up/down along the wall
            float vertical = _a.Move.ReadValue<Vector2>().y;
            _rb.velocity = new Vector2(0f, vertical * _climbSpeed);
        }
        else
        {
            // Normal ground/air horizontal move
            float horizontal = _a.Move.ReadValue<Vector2>().x;
            Move(horizontal);
        }
    }

    protected override void Update()
    {
        base.Update();
        HandleWallStick();

        // Jump key should work only when NOT on a wall
        if (!_onWall && _a.Jump.WasPressedThisFrame())
            TryJump();

        // re‑attach delay timer
        if (_reattachTimer > 0f) _reattachTimer -= Time.deltaTime;
    }

    /* ------------------------------------------------------------------ */
    /*  Jump logic                                                        */
    /* ------------------------------------------------------------------ */
    public override void TryJump()
    {
        if (_isGrounded)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);
        }
        else if (_onWall)
        {
            // Wall jump away from surface
            float dir = -_wallSide;                // opposite to wall
            _rb.velocity = new Vector2(dir * _jumpForce * 0.75f, _jumpForce);
            DetachFromWall(true);
        }
    }

    /* ------------------------------------------------------------------ */
    /*  Wall‑stick / detach                                               */
    /* ------------------------------------------------------------------ */
    private void HandleWallStick()
    {

        // Skip stick detection while in cooldown
        if (_reattachTimer > 0f)
        {
            _onWall = false;
            return;
        }

        // Check wall presence only if airborne (no ground)
        bool hit = !_isGrounded && Physics2D.OverlapCircle(_wallCheck.position,
                                                           _wallRadius, _wallMask);
        if (hit)
        {
            // Determine side
            _wallSide = _wallCheck.position.x > transform.position.x ? 1 : -1;
            _onWall = true;
            _rb.gravityScale = 0f;
            _rb.velocity = Vector2.zero;

            // Face the wall
            transform.localScale = new Vector3(_wallSide * Mathf.Abs(transform.localScale.x),
                                               transform.localScale.y,
                                               transform.localScale.z);

            // Hold‑to‑detach
            if (_a.WallDetach.IsPressed())
            {
                _detachTimer += Time.deltaTime;
                if (_detachTimer >= _detachHoldTime)
                    DetachFromWall(false);
            }
            else
            {
                _detachTimer = 0f;
            }
        }
        else
        {
            // Not on wall
            _onWall = false;
            _rb.gravityScale = 1f;
            _detachTimer = 0f;
        }
    }

    /// <summary>
    /// Leaves the wall, optionally with a push‑off impulse.
    /// </summary>
    /// <param name="withJumpImpulse">true when wall‑jump already applied.</param>
    private void DetachFromWall(bool withJumpImpulse)
    {
        _onWall = false;
        _rb.gravityScale = 1f;
        _detachTimer = 0f;
        _reattachTimer = _reattachDelay;

        if (!withJumpImpulse)
        {
            // Horizontal push away from wall so collider no longer overlaps
            _rb.velocity = new Vector2(-_wallSide * _pushOffForce, 0.5f);
        }
    }



#if UNITY_EDITOR
    /* ------------------------------------------------------------------ */
    /*  Gizmos                                                             */
    /* ------------------------------------------------------------------ */
    private void OnDrawGizmosSelected()
    {
        if (_wallCheck != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_wallCheck.position, _wallRadius);
        }
    }
#endif
}
