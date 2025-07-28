using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Calculates total mass of colliders on this platform
/// and reports it to a PulleyController.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PulleyWeightSensor : MonoBehaviour
{
    [SerializeField] private PulleyController _controller;
    [SerializeField] private PulleySide _side;         // A or B
    [SerializeField] private LayerMask _weightMask;    // objects counted as weight

    private readonly List<Rigidbody2D> _bodies = new();
    private float _currentMass;

    /* ================================================================== */
    /*  Trigger callbacks                                                 */
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidLayer(other.gameObject.layer)) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        _bodies.Add(rb);
        _currentMass += rb.mass;
        _controller.ReportMass(_side, _currentMass);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsValidLayer(other.gameObject.layer)) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        if (_bodies.Remove(rb))
        {
            _currentMass -= rb.mass;
            _controller.ReportMass(_side, _currentMass);
        }
    }

    /* ================================================================== */
    /*  Helpers                                                           */
    private bool IsValidLayer(int layer)
    {
        return _weightMask == 0 || (_weightMask & (1 << layer)) != 0;
    }
}
