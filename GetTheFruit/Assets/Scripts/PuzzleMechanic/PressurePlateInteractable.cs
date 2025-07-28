using UnityEngine;

/// <summary>
/// Activates while at least one valid collider stays on the plate.
/// Ideal for player‑triggered switches.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PressurePlateInteractable : MechanismBase
{
    [Header("Trigger Settings")]
    [Tooltip("Layers allowed to press the plate.")]
    [SerializeField] private LayerMask _triggerMask;

    [Header("Events")]
    [SerializeField] private MechanismEventChannelSO _eventChannel;

    private int _pressCount;

    /* ================================================================== */
    /*  Trigger callbacks                                                 */
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidLayer(other.gameObject.layer)) return;

        _pressCount++;
        if (_pressCount == 1)
        {
            Activate(true);
            _eventChannel?.Raise(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsValidLayer(other.gameObject.layer)) return;

        _pressCount = Mathf.Max(0, _pressCount - 1);
        if (_pressCount == 0)
        {
            Activate(false);
            _eventChannel?.Raise(false);
        }
    }

    /* ================================================================== */
    /*  Helpers                                                           */
    private bool IsValidLayer(int layer)
    {
        return _triggerMask == 0 || (_triggerMask & (1 << layer)) != 0;
    }

    /* ================================================================== */
    /*  Visual / SFX override                                             */
    protected override void OnActivate(bool on)
    {
        // TODO: plate depress / release animation or sound
    }
}
