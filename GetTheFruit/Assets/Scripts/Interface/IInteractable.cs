using UnityEngine;

/// <summary>
/// Generic interface for any object that can be activated by a player.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Called when the player presses its Interact key near this object.
    /// </summary>
    /// <param name="caller">Player GameObject requesting the interaction.</param>
    void Interact(GameObject caller);
}
