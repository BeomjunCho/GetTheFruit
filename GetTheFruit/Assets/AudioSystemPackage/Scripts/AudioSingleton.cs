using UnityEngine;

/// <summary>
/// Generic singleton base that survives scene loads
/// and destroys duplicate instances.
/// Skips DontDestroyOnLoad if the root object is already
/// inside the hidden DontDestroyOnLoad scene.
/// </summary>
public class AudioSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    /// <summary>Global access point.</summary>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name, typeof(T));
                    _instance = obj.GetComponent<T>();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Checks whether the root object is already placed
    /// in Unity's special DontDestroyOnLoad scene.
    /// </summary>
    /// <returns>True if persistent already.</returns>
    private bool IsAlreadyPersistent()
    {
        // Unity assigns buildIndex -1 to the hidden scene that stores
        // objects marked with DontDestroyOnLoad.
        return transform.root.gameObject.scene.buildIndex == -1;
    }

    /// <summary>
    /// Duplicate‑safe Awake.
    /// </summary>
    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;

        // Only move to persistent scene if not already there
        if (!IsAlreadyPersistent())
            DontDestroyOnLoad(transform.root.gameObject); // use root to avoid duplicates
    }
}
