using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Platform that moves along predefined waypoints while activated.
/// Movement is controlled via a MechanismEventChannel (e.g., Lever).
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MechanismBase
{
    /* ------------------------------------------------------------------ */
    /*  Inspector                                                         */
    /* ------------------------------------------------------------------ */
    [Header("Path Settings")]
    [SerializeField, Tooltip("Ordered list of waypoints in world space.")]
    private Transform[] _waypoints = null;

    [SerializeField, Tooltip("Units per second movement speed.")]
    private float _moveSpeed = 2f;

    [SerializeField, Tooltip("Wait time at each waypoint.")]
    private float _waitTime = 0.2f;

    [SerializeField, Tooltip("If true, platform reverses at end instead of looping.")]
    private bool _pingPong = true;

    [Header("Event Channel")]
    [SerializeField, Tooltip("Channel broadcast from Lever or WeightPad.")]
    private MechanismEventChannelSO _channel = null;

    /* ------------------------------------------------------------------ */
    /*  State                                                             */
    /* ------------------------------------------------------------------ */
    private int _currentIndex;
    private int _direction = 1;
    private Coroutine _moveRoutine;

    private readonly HashSet<Rigidbody2D> _passengers = new();
    private Vector3 _lastPosition;
    private Rigidbody2D _rb;

    /* ------------------------------------------------------------------ */
    /*  Unity lifecycle                                                   */
    /* ------------------------------------------------------------------ */
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;     
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void OnEnable()
    {
        if (_channel != null)
            _channel.OnEventRaised += Activate;
    }

    private void OnDisable()
    {
        if (_channel != null)
            _channel.OnEventRaised -= Activate;
    }

    /* ------------------------------------------------------------------ */
    /*  MechanismBase override                                            */
    /* ------------------------------------------------------------------ */
    protected override void OnActivate(bool on)
    {
        if (on)
        {
            if (_moveRoutine == null && _waypoints.Length >= 2)
                _moveRoutine = StartCoroutine(MoveRoutine());
        }
        else
        {
            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
                _moveRoutine = null;
            }
        }
    }

    /* ------------------------------------------------------------------ */
    /*  Movement coroutine                                                */
    /* ------------------------------------------------------------------ */
    private IEnumerator MoveRoutine()
    {
        if (_waypoints.Length < 2) yield break;

        _lastPosition = transform.position;

        while (true)
        {
            int nextIndex = _currentIndex + _direction;

            // Reached end of the waypoint array
            if (nextIndex >= _waypoints.Length || nextIndex < 0)
            {
                if (_pingPong)
                {
                    _direction *= -1;
                    nextIndex = _currentIndex + _direction;
                }
                else
                {
                    // Stop at the final waypoint when ping‑pong is off
                    _moveRoutine = null;
                    yield break;
                }
            }

            Vector3 start = transform.position;
            Vector3 end = _waypoints[nextIndex].position;
            float dist = Vector3.Distance(start, end);
            float t = 0f;

            while (t < 1f)
            {
                t += Time.fixedDeltaTime * (_moveSpeed / dist);

                Vector3 newPos = Vector3.Lerp(start, end, t);
                Vector3 delta = newPos - _lastPosition;

                _rb.MovePosition(newPos);

                /*foreach (Rigidbody2D rb in _passengers)
                    rb.MovePosition(rb.position + (Vector2)delta);*/

                _lastPosition = newPos;
                yield return new WaitForFixedUpdate();
            }

            _currentIndex = nextIndex;
            yield return new WaitForSeconds(_waitTime);
        }
    }

    /* ------------------------------------------------------------------ */
    /*  Passenger tracking                                                */
    /* ------------------------------------------------------------------ */
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.transform.CompareTag("Player") && !col.transform.CompareTag("Pushable"))
            return;

        Rigidbody2D rb = col.rigidbody;
        if (rb != null)
            _passengers.Add(rb);
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        Rigidbody2D rb = col.rigidbody;
        if (rb != null)
            _passengers.Remove(rb);
    }

#if UNITY_EDITOR
    /* ------------------------------------------------------------------ */
    /*  Gizmos                                                             */
    /* ------------------------------------------------------------------ */
    private void OnDrawGizmosSelected()
    {
        if (_waypoints == null || _waypoints.Length < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < _waypoints.Length - 1; i++)
            Gizmos.DrawLine(_waypoints[i].position, _waypoints[i + 1].position);
    }
#endif
}
