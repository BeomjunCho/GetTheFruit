using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Detects nearby interactables and triggers them when player presses the key.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class InteractionHandler : MonoBehaviour
{
    [SerializeField] private float _radius = 0.5f;
    [SerializeField] private LayerMask _interactableMask;
    [SerializeField] private int _playerIndex = 0;   // 0 = SlimeWoman, 1 = RockMan

    private readonly List<IInteractable> _overlaps = new();
    private InputAction _interact;

    /* ------------------------------------------------------------------ */
    /*  Unity lifecycle                                                   */
    /* ------------------------------------------------------------------ */
    private void Awake()
    {
        // Cache the appropriate "Interact" action at runtime, so the code does
        // not depend on the generated property name.
        _interact = GameInputManager.Instance.Controls.FindAction(
            _playerIndex == 0 ? "SlimeWoman/Interact" : "RockMan/Interact",
            throwIfNotFound: true);
    }

    private void Update()
    {
        if (_interact.WasPressedThisFrame())
        {
            IInteractable target = GetClosestInteractable();
            target?.Interact(gameObject);
        }
    }

    /* ------------------------------------------------------------------ */
    /*  Helpers                                                           */
    /* ------------------------------------------------------------------ */
    private IInteractable GetClosestInteractable()
    {
        _overlaps.Clear();
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, _radius, _interactableMask);
        foreach (var col in cols)
            if (col.TryGetComponent(out IInteractable ia)) _overlaps.Add(ia);

        if (_overlaps.Count == 0) return null;

        _overlaps.Sort((a, b) =>
            Vector2.Distance(((Component)a).transform.position, transform.position)
            .CompareTo(Vector2.Distance(((Component)b).transform.position, transform.position)));

        return _overlaps[0];
    }
}
