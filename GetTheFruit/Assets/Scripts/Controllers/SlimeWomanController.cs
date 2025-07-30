using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles move, jump, wall‑stick/climb/detach and interact actions for
/// SlimeWoman (P1) while driving the Animator (Idle, Walk, Jump, Stick,
/// Detach, Interact).
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
    [SerializeField] private Transform _wallCheck = null;
    [SerializeField] private float _wallRadius = 0.1f;
    [SerializeField] private LayerMask _wallMask = default;
    [SerializeField] private float _climbSpeed = 4f;
    [SerializeField] private float _detachHoldTime = 1f;
    [SerializeField] private float _pushOffForce = 4f;
    [SerializeField] private float _reattachDelay = 0.25f;

    /* ------------------------------------------------------------------ */
    /*  Animator                                                          */
    /* ------------------------------------------------------------------ */
    private static readonly int SpeedHash = Animator.StringToHash("speed");
    private static readonly int GroundHash = Animator.StringToHash("isGrounded");
    private static readonly int JumpHash = Animator.StringToHash("jump");
    private static readonly int StickHash = Animator.StringToHash("stick");
    private static readonly int DetachHash = Animator.StringToHash("detach");
    private static readonly int InteractHash = Animator.StringToHash("interact");


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
            /* Vertical climb along wall */
            float vertical = _a.Move.ReadValue<Vector2>().y;
            _rb.velocity = new Vector2(0f, vertical * _climbSpeed);
        }
        else
        {
            /* Normal horizontal move */
            float horizontal = _a.Move.ReadValue<Vector2>().x;
            Move(horizontal);
        }
    }

    protected override void Update()
    {
        base.Update();
        HandleWallStick();

        /* Jump is allowed only when not sticking */
        if (!_onWall && _a.Jump.WasPressedThisFrame())
            TryJump();

        /* Optional interact trigger */
        if (_a.Interact != null && _a.Interact.WasPressedThisFrame())
            _anim.SetTrigger(InteractHash);

        if (_reattachTimer > 0f)
            _reattachTimer -= Time.deltaTime;

        SyncAnimator();
    }

    /* ------------------------------------------------------------------ */
    /*  Jump logic                                                        */
    /* ------------------------------------------------------------------ */
    /// <inheritdoc/>
    public override void TryJump()
    {
        if (_isGrounded)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);
            _anim.SetTrigger(JumpHash);
        }
        else if (_onWall)
        {
            /* Wall jump away from surface */
            float dir = -_wallSide;
            _rb.velocity = new Vector2(dir * _jumpForce * 0.75f, _jumpForce);
            _anim.SetTrigger(JumpHash);
            DetachFromWall(true);
        }
    }

    /* ------------------------------------------------------------------ */
    /*  Wall‑stick / detach                                               */
    /* ------------------------------------------------------------------ */
    private void HandleWallStick()
    {
        /* Skip detection while cooldown active */
        if (_reattachTimer > 0f)
        {
            _onWall = false;
            return;
        }

        /* Detect wall only in air */
        bool hit = !_isGrounded && Physics2D.OverlapCircle(
                       _wallCheck.position, _wallRadius, _wallMask);

        if (hit)
        {
            /* Newly attached? trigger stick anim */
            if (!_onWall)
                _anim.SetTrigger(StickHash);

            _wallSide = _wallCheck.position.x > transform.position.x ? 1 : -1;
            _onWall = true;
            _rb.gravityScale = 0f;
            _rb.velocity = Vector2.zero;

            /* Face the wall */
            transform.localScale = new Vector3(
                _wallSide * Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z);

            /* Hold‑to‑detach timer */
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
            /* Lost wall contact */
            if (_onWall)
                DetachFromWall(false);
        }
    }

    /// <summary>
    /// Leaves the wall, optionally after a jump impulse.
    /// </summary>
    /// <param name="withJumpImpulse">true if a wall‑jump was already applied.</param>
    private void DetachFromWall(bool withJumpImpulse)
    {
        _onWall = false;
        _rb.gravityScale = 1f;
        _detachTimer = 0f;
        _reattachTimer = _reattachDelay;

        _anim.SetTrigger(DetachHash);

        if (!withJumpImpulse)
        {
            /* Push a bit away from wall */
            _rb.velocity = new Vector2(-_wallSide * _pushOffForce, 0.5f);
        }
    }

    /* ------------------------------------------------------------------ */
    /*  Animator sync                                                     */
    /* ------------------------------------------------------------------ */
    /// <summary>
    /// Writes movement state into Animator every frame.
    /// </summary>
    private void SyncAnimator()
    {
        _anim.SetFloat(SpeedHash, Mathf.Abs(_rb.velocity.x));
        _anim.SetBool(GroundHash, _isGrounded);
    }

#if UNITY_EDITOR
    /* ------------------------------------------------------------------ */
    /*  Gizmos                                                            */
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
