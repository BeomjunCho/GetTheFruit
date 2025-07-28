using System;
using UnityEngine;

/// <summary>
/// ScriptableObject event channel to decouple interactables and mechanisms.
/// Supports bool payload (on/off).
/// </summary>
[CreateAssetMenu(fileName = "MechanismEventChannel",
                 menuName = "Puzzle/Event Channels/Mechanism")]
public class MechanismEventChannelSO : ScriptableObject
{
    /* ------------------------------------------------------------------ */
    /*  Event                                                             */
    /* ------------------------------------------------------------------ */

    /// <summary>Subscribers are invoked with the new state (true=on).</summary>
    public event Action<bool> OnEventRaised;

    /// <summary>
    /// Raise the event to all listeners.
    /// </summary>
    /// <param name="state">True for on, false for off.</param>
    public void Raise(bool state)
    {
        OnEventRaised?.Invoke(state);
    }
}
