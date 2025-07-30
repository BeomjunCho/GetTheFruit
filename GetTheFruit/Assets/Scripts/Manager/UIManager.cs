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

    public static UIManager Instance { get; private set; }   // singleton accessor

    [Header("Panels")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private GameObject _howToPlayPanel;

    [Header("Fade (optional)")]
    [SerializeField] private CanvasGroup _fadeGroup;
    [SerializeField] private float _fadeDuration = 0.25f;

    [Header("Buttons (Main Menu)")]
    [SerializeField] private Button _btnStartGame;
    [SerializeField] private Button _btnQuitGame;
    [SerializeField] private Button _btnOpenHowToPlay;

    [Header("Buttons (Pause)")]
    [SerializeField] private Button _btnQuitInPause;
    [SerializeField] private Button _btnReturnToMenu;

    [Header("Buttons (Win Panel)")]
    [SerializeField] private Button _btnQuitInWin;
    [SerializeField] private Button _btnReturnToMenuInWin;

    [Header("Buttons (HowToPlay)")]
    [SerializeField] private Button _btnCloseHowToPlay;

    private bool _isPaused;

    /* ================================================================== */
    /*  Unity lifecycle                                                   */
    /* ================================================================== */
    private void Awake()
    {
        Instance = this;

        // Wire button callbacks
        if (_btnStartGame != null) _btnStartGame.onClick.AddListener(OnStartGame);
        if (_btnQuitGame != null) _btnQuitGame.onClick.AddListener(OnQuitGame);
        if (_btnQuitInPause != null) _btnQuitInPause.onClick.AddListener(OnQuitGame);
        if (_btnReturnToMenu != null) _btnReturnToMenu.onClick.AddListener(OnReturnToMenu);
        if (_btnReturnToMenuInWin != null) _btnReturnToMenuInWin.onClick.AddListener(OnReturnToMenu);
        if (_btnQuitInWin != null) _btnQuitInWin.onClick.AddListener(OnQuitGame);
        if (_btnOpenHowToPlay != null) _btnOpenHowToPlay.onClick.AddListener(ShowHowToPlay);
        if (_btnCloseHowToPlay != null) _btnCloseHowToPlay.onClick.AddListener(OnCloseHowToPlay);

        _howToPlayPanel.SetActive(false);

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

    public void ShowWinScreen()
    {
        Time.timeScale = 0f;          // freeze gameplay
        _isPaused = false;

        _mainMenuPanel.SetActive(false);
        _pausePanel.SetActive(false);
        _winPanel.SetActive(true);
    }

    /* ================================================================== */
    /*  How‑To‑Play logic                                                 */
    /* ================================================================== */
    public void ShowHowToPlay()
    {
        HideAll();                    
        _howToPlayPanel.SetActive(true);
    }

    private void OnCloseHowToPlay()
    {
        _howToPlayPanel.SetActive(false);
        ShowMainMenu();                
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
        // reset checkpoint before unloading level
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.ResetCheckpoint();

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
        _winPanel.SetActive(false);
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
