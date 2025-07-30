using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Heavy character: jumps, punches weak walls and pushes heavy objects.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RockManController : CharacterControllerBase
{
    /* ------------------------------------------------------------------ */
    /*  Inspector                                                         */
    /* ------------------------------------------------------------------ */
    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 10f;

    [Header("Punch Settings")]
    [SerializeField] private float _punchRange = 0.6f;
    [SerializeField] private LayerMask _weakWallMask;

    /* ------------------------------------------------------------------ */
    /*  Animator                                                          */
    /* ------------------------------------------------------------------ */
    private static readonly int SpeedHash = Animator.StringToHash("speed");
    private static readonly int GroundHash = Animator.StringToHash("isGrounded");
    private static readonly int PunchHash = Animator.StringToHash("punch");
    private static readonly int JumpHash = Animator.StringToHash("jump");
    private static readonly int InteractHash = Animator.StringToHash("interact");

    /* ------------------------------------------------------------------ */
    /*  SFX state                                                         */
    /* ------------------------------------------------------------------ */
    private int _lastFootstepIndex = -1;
    private bool _wasGrounded;

    /* ------------------------------------------------------------------ */
    /*  SFX helpers                                                       */
    /* ------------------------------------------------------------------ */
    private void PlayFootstepSfx()
    {
        if (!_isGrounded) return;

        int idx;
        do { idx = Random.Range(1, 5); } while (idx == _lastFootstepIndex);
        _lastFootstepIndex = idx;

        AudioManager.Instance.Play2dSfx($"RockManFoot_{idx:D2}", 0.4f);
    }

    private void PlayJumpSfx() => AudioManager.Instance.Play2dSfx("RockManJump_01", 0.5f);
    private void PlayLandSfx() => AudioManager.Instance.Play2dSfx("RockManLand_01", 0.5f);
    private void PlayPunchSfx() => AudioManager.Instance.Play2dSfx("RockManPunch_01");

    /* ------------------------------------------------------------------ */
    /*  Cached input actions                                              */
    /* ------------------------------------------------------------------ */
    private PlayerControls.RockManActions _a;

    /* ------------------------------------------------------------------ */
    /*  Unity lifecycle                                                   */
    /* ------------------------------------------------------------------ */
    protected override void Awake()
    {
        base.Awake();
        _a = GameInputManager.Instance.Controls.RockMan;
    }

    private void FixedUpdate()
    {
        Move(_a.Move.ReadValue<Vector2>().x);
    }

    protected override void Update()
    {
        base.Update();

        if (_a.Jump.WasPressedThisFrame())
            TryJump();

        if (_a.Punch.WasPressedThisFrame())
            Punch();

        /* Optional interact animation trigger */
        if (_a.Interact != null && _a.Interact.WasPressedThisFrame())
            _anim.SetTrigger(InteractHash);

        if (!_wasGrounded && _isGrounded)
            PlayLandSfx();

        _wasGrounded = _isGrounded;

        SyncAnimator();
    }

    /* ------------------------------------------------------------------ */
    /*  CharacterControllerBase override                                  */
    /* ------------------------------------------------------------------ */
    public override void TryJump()
    {
        if (_isGrounded)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);
            _anim.SetTrigger(JumpHash);
            PlayJumpSfx();
        }
    }

    /* ------------------------------------------------------------------ */
    /*  Punch logic                                                       */
    /* ------------------------------------------------------------------ */
    private void Punch()
    {
        /* Play punch animation immediately */
        _anim.SetTrigger(PunchHash);
        PlayPunchSfx();

        float sign = Mathf.Sign(transform.localScale.x);
        Vector2 origin = (Vector2)transform.position + Vector2.right * sign * _punchRange * 0.5f;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.right * sign,
            _punchRange,
            _weakWallMask);

        if (hit.collider != null && hit.collider.TryGetComponent(out WeakWall wall))
        {
            wall.Break();
        }
    }

    /* ------------------------------------------------------------------ */
    /*  Push logic                                                        */
    /* ------------------------------------------------------------------ */
    private void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Pushable"))
        {
            float dir = Mathf.Sign(col.transform.position.x - transform.position.x);
            col.rigidbody.AddForce(Vector2.right * dir * _moveSpeed * 2f, ForceMode2D.Force);
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
}
