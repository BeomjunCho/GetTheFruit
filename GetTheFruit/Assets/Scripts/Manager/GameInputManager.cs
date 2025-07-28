using UnityEngine;

/// <summary>
/// Wraps the PlayerControls asset and exposes typed accessors.
/// </summary>
public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance { get; private set; }
    public PlayerControls Controls { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Controls = new PlayerControls();
        Controls.Enable();
        DontDestroyOnLoad(gameObject);
    }
}
