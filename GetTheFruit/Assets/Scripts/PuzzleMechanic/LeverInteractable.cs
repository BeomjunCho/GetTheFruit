using UnityEngine;

/// <summary>
/// Two‑way lever that toggles its state every time a player interacts with it.
/// Publishes the new state through a MechanismEventChannelSO.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LeverInteractable : MonoBehaviour, IInteractable
{
    /* ------------------------------------------------------------------ */
    /*  Inspector                                                         */
    /* ------------------------------------------------------------------ */
    [Header("Event Channel")]
    [SerializeField, Tooltip("Broadcast channel to notify connected mechanisms.")]
    private MechanismEventChannelSO _channel = null;

    [Header("Start State")]
    [SerializeField, Tooltip("Initial ON state of the lever.")]
    private bool _startOn = false;

    [Header("Visuals (optional)")]
    [SerializeField] private Transform _handle = null;              // child object
    [SerializeField] private Vector3 _onRotation = new Vector3(0, 0, -45);
    [SerializeField] private Vector3 _offRotation = new Vector3(0, 0, 45);

    [Header("Settings")]
    [SerializeField, Tooltip("Seconds before lever can be toggled again.")]
    private float _cooldown = 0.2f;

    public bool finalGate = false;

    /* ------------------------------------------------------------------ */
    /*  State                                                             */
    /* ------------------------------------------------------------------ */
    private bool _isOn;
    private float _nextToggleTime;

    /* ------------------------------------------------------------------ */
    /*  Unity lifecycle                                                   */
    /* ------------------------------------------------------------------ */
    private void Awake()
    {
        _isOn = _startOn;
        UpdateVisual();

        if (_channel != null)
            _channel.Raise(_isOn);
    }

    /* ------------------------------------------------------------------ */
    /*  IInteractable implementation                                      */
    /* ------------------------------------------------------------------ */
    public void Interact(GameObject caller)
    {
        if (Time.time < _nextToggleTime) return;    // cooldown guard
        ToggleLever();
        _nextToggleTime = Time.time + _cooldown;
        if (finalGate)
        {
            RespawnManager.Instance?.SetFinalGateOpen(true);
        }
    }

    /* ------------------------------------------------------------------ */
    /*  Internal                                                          */
    /* ------------------------------------------------------------------ */
    private void ToggleLever()
    {
        _isOn = !_isOn;
        AudioManager.Instance.Play3dSfx("LeverPull_01", this.transform);
        // Notify all listeners
        if (_channel != null)
            _channel.Raise(_isOn);

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (_handle == null) return;

        _handle.localEulerAngles = _isOn ? _onRotation : _offRotation;
    }

#if UNITY_EDITOR
    /* ------------------------------------------------------------------ */
    /*  Gizmos                                                            */
    /* ------------------------------------------------------------------ */
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _isOn ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
#endif
}
