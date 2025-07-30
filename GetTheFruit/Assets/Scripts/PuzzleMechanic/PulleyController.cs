using System.Collections;
using UnityEngine;

/// <summary>
/// Controls a two‑way pulley: the heavier side moves down,
/// the lighter side moves up, both clamped by moveDistance.
/// </summary>
public enum PulleySide { A, B }

public class PulleyController : MechanismBase
{
    [Header("Platform References")]
    [SerializeField] private Transform _platformA;
    [SerializeField] private Transform _platformB;

    [Header("Movement Settings")]
    [SerializeField] private float _moveDistance = 3f;     // max offset from start
    [SerializeField] private float _moveSpeed = 2f;        // units per second
    [Tooltip("Mass difference below this value is treated as balanced.")]
    [SerializeField] private float _deadZone = 0.1f;

    [SerializeField, Tooltip("Seconds to wait before reacting to a mass change.")]
    private float _evaluationDelay = 0.5f;
    private Coroutine _delayRoutine;

    private Vector2 _startA;
    private Vector2 _startB;
    private float _massA;
    private float _massB;
    private Coroutine _moveRoutine;

    /* ================================================================== */
    /*  Unity lifecycle                                                   */
    private void Awake()
    {
        _startA = _platformA.position;
        _startB = _platformB.position;
    }

    private void OnDisable()
    {
        if (_delayRoutine != null)
        {
            StopCoroutine(_delayRoutine);
            _delayRoutine = null;
        }
        AudioManager.Instance.Stop3dSound("PulleyMoving_01");
    }

    /* ================================================================== */
    /*  Public API called by sensors                                      */
    public void ReportMass(PulleySide side, float mass)
    {
        if (side == PulleySide.A) _massA = mass;
        else _massB = mass;

        if (!gameObject.activeInHierarchy) return;

        if (_delayRoutine != null)
            StopCoroutine(_delayRoutine);

        _delayRoutine = StartCoroutine(DelayedEvaluate());
    }

    /* ================================================================== */
    /*  Core logic                                                        */
    private void EvaluateMass()
    {
        float diff = _massA - _massB;

        Vector2 dir; // movement direction for platform A (B is opposite)

        if (Mathf.Abs(diff) <= _deadZone)
            dir = Vector2.zero;                // balanced → back
        else
            dir = diff > 0 ? Vector2.down      // A heavier
                           : Vector2.up;       // B heavier

        StartMove(dir);
    }


    private void StartMove(Vector2 dir)
    {
        if (!isActiveAndEnabled) return;

        if (_moveRoutine != null) StopCoroutine(_moveRoutine);

        AudioManager.Instance.Play3dLoop("PulleyMoving_01", transform);

        _moveRoutine = StartCoroutine(MoveRoutine(dir));
    }

    private IEnumerator MoveRoutine(Vector2 dir)
    {
        while (true)
        {
            if (dir == Vector2.zero)
            {
                // Return both platforms to start positions
                bool doneA = MoveTowards(_platformA, _startA);
                bool doneB = MoveTowards(_platformB, _startB);
                
                if (doneA && doneB)
                {
                    AudioManager.Instance.Stop3dSound("PulleyMoving_01");
                    yield break;      
                }
            }
            else
            {
                // Desired target offset for A = ±moveDistance
                Vector2 targetA = _startA + dir * _moveDistance;
                Vector2 targetB = _startB - dir * _moveDistance;

                bool doneA = MoveTowards(_platformA, targetA);
                bool doneB = MoveTowards(_platformB, targetB);
                if (doneA && doneB)
                {
                    AudioManager.Instance.Stop3dSound("PulleyMoving_01");
                    yield break;
                }
            }

            yield return null;
        }
    }

    private IEnumerator DelayedEvaluate()
    {
        yield return new WaitForSeconds(_evaluationDelay);
        EvaluateMass();
    }

    private bool MoveTowards(Transform t, Vector2 target)
    {
        t.position = Vector2.MoveTowards(
            t.position, target, _moveSpeed * Time.deltaTime);

        return (Vector2)t.position == target;
    }

    /* ================================================================== */
    /*  Optional visual / SFX                                             */
    protected override void OnActivate(bool on) { /* not used here */ }
}
