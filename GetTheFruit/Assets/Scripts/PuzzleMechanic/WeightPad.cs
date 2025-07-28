using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Activates when the total mass of colliders on top meets the threshold.
/// Sends a bool event (true = on) via MechanismEventChannelSO.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WeightPad : MechanismBase
{
    [Header("Weight Settings")]
    [SerializeField] private float _requiredMass = 10f;
    [Tooltip("Layers counted as weight. Leave empty to accept any.")]
    [SerializeField] private LayerMask _weightMask;

    [Header("Events")]
    [SerializeField] private MechanismEventChannelSO _eventChannel;

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
        TrySetState();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsValidLayer(other.gameObject.layer)) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        if (_bodies.Remove(rb))
        {
            _currentMass -= rb.mass;
            TrySetState();
        }
    }

    /* ================================================================== */
    /*  Helpers                                                           */
    private bool IsValidLayer(int layer)
    {
        return _weightMask == 0 || (_weightMask & (1 << layer)) != 0;
    }

    private void TrySetState()
    {
        bool shouldBeActive = _currentMass >= _requiredMass;

        if (IsActive != shouldBeActive)
        {
            Activate(shouldBeActive);          // from MechanismBase
            _eventChannel?.Raise(shouldBeActive);
        }
    }

    /* ================================================================== */
    /*  Visual / SFX override                                             */
    protected override void OnActivate(bool on)
    {
        // TODO: play press animation / sound here if needed
    }
}
