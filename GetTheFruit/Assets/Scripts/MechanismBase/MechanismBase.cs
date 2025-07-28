using UnityEngine;

/// <summary>
/// Abstract base class for all puzzle mechanisms (doors, platforms, etc.).
/// Provides common Activate API and state handling.
/// </summary>
public abstract class MechanismBase : MonoBehaviour
{
    /// <summary>True when the mechanism is currently active/on.</summary>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Called by interactables or event channels to change the mechanism state.
    /// </summary>
    /// <param name="on">True to activate, false to deactivate.</param>
    public void Activate(bool on)
    {
        if (IsActive == on) return;     // ignore redundant calls
        IsActive = on;
        OnActivate(on);
    }

    /// <summary>
    /// Implement specific behavior (move, open, etc.) in derived classes.
    /// </summary>
    /// <param name="on">Target state.</param>
    protected abstract void OnActivate(bool on);
}
