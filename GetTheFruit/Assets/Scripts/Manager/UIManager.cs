using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Central UI controller that toggles Main‑Menu and Pause panels
/// and routes button callbacks.
/// Attach this to the UIRoot prefab.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _pausePanel;

    [Header("Fade (optional)")]
    [SerializeField] private CanvasGroup _fadeGroup;
    [SerializeField] private float _fadeDuration = 0.25f;

    [Header("Buttons (Main Menu)")]
    [SerializeField] private Button _btnStartGame;
    [SerializeField] private Button _btnQuitGame;

    [Header("Buttons (Pause)")]
    [SerializeField] private Button _btnQuitInPause;
    [SerializeField] private Button _btnReturnToMenu;

    private bool _isPaused;

    /* ================================================================== */
    /*  Unity lifecycle                                                   */
    /* ================================================================== */
    private void Awake()
    {
        // Wire button callbacks
        if (_btnStartGame != null) _btnStartGame.onClick.AddListener(OnStartGame);
        if (_btnQuitGame != null) _btnQuitGame.onClick.AddListener(OnQuitGame);
        if (_btnQuitInPause != null) _btnQuitInPause.onClick.AddListener(OnQuitGame);
        if (_btnReturnToMenu != null) _btnReturnToMenu.onClick.AddListener(OnReturnToMenu);

        ShowMainMenu();
    }

    private void Update()
    {
        // Pause toggle via ESC key
        if (Input.GetKeyDown(KeyCode.Escape) && SceneManager.GetActiveScene().name == "InGame")
        {
            if (_isPaused) OnResume();
            else ShowPause();
        }
    }

    /* ================================================================== */
    /*  Public API                                                        */
    /* ================================================================== */
    public void ShowMainMenu()
    {
        Time.timeScale = 1f;
        _isPaused = false;

        _mainMenuPanel.SetActive(true);
        _pausePanel.SetActive(false);
    }

    public void ShowPause()
    {
        _isPaused = true;
        Time.timeScale = 0f;

        _mainMenuPanel.SetActive(false);
        _pausePanel.SetActive(true);
    }

    /* ================================================================== */
    /*  Button handlers                                                   */
    /* ================================================================== */
    private void OnStartGame()
    {
        HideAll();
        SceneFlowManager.Instance.LoadInGame("InGame");
    }

    private void OnQuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnResume()
    {
        _pausePanel.SetActive(false);
        _isPaused = false;
        Time.timeScale = 1f;
    }

    private void OnReturnToMenu()
    {
        Time.timeScale = 1f;
        _isPaused = false;
        HideAll();
        SceneFlowManager.Instance.LoadMainMenu("MainMenu");
        ShowMainMenu();
    }

    /* ================================================================== */
    /*  Helpers                                                           */
    /* ================================================================== */
    private void HideAll()
    {
        _mainMenuPanel.SetActive(false);
        _pausePanel.SetActive(false);
    }

    /* Optional fade utility */
    public void FadeScreen(bool fadeIn) => StartCoroutine(FadeRoutine(fadeIn ? 1f : 0f));

    private System.Collections.IEnumerator FadeRoutine(float target)
    {
        if (_fadeGroup == null) yield break;

        float start = _fadeGroup.alpha;
        float timer = 0f;

        while (timer < _fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            _fadeGroup.alpha = Mathf.Lerp(start, target, timer / _fadeDuration);
            yield return null;
        }

        _fadeGroup.alpha = target;
    }
}
