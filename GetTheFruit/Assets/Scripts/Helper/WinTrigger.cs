using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Calls UIManager.ShowWinScreen once both players are inside the trigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WinTrigger : MonoBehaviour
{
    [Tooltip("Tag used by all player root objects.")]
    [SerializeField] private string _playerTag = "Player";
    [Tooltip("Number of players required to trigger win.")]
    [SerializeField] private int _requiredPlayers = 2;

    private readonly HashSet<GameObject> _inside = new();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(_playerTag)) return;

        _inside.Add(other.gameObject);

        if (_inside.Count >= _requiredPlayers)
        {
            UIManager.Instance?.ShowWinScreen();
            enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(_playerTag)) return;
        _inside.Remove(other.gameObject);
    }
}
