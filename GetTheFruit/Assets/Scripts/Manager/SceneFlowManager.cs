using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Centralized scene transition manager with simple fade support.
/// </summary>
public class SceneFlowManager : MonoBehaviour
{
    public static SceneFlowManager Instance { get; private set; }

    [Header("Fade")]
    [SerializeField] private CanvasGroup _fadeCanvas; // optional fade canvas
    [SerializeField] private float _fadeDuration = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    /* ------------------------------------------------------------------ */
    /*  Public API                                                        */
    /* ------------------------------------------------------------------ */
    public void LoadBoot(string sceneName = "Boot")
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    public void LoadMainMenu(string sceneName = "MainMenu")
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    public void LoadInGame(string sceneName = "InGame")
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    /* ------------------------------------------------------------------ */
    /*  Core coroutine                                                    */
    /* ------------------------------------------------------------------ */
    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        // Fade‑out
        yield return Fade(1f);

        // Async load
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;

        // Fade‑in
        yield return Fade(0f);
    }

    /* ------------------------------------------------------------------ */
    /*  Fade helper                                                       */
    /* ------------------------------------------------------------------ */
    private IEnumerator Fade(float targetAlpha)
    {
        if (_fadeCanvas == null) yield break;

        _fadeCanvas.blocksRaycasts = true;
        float startAlpha = _fadeCanvas.alpha;
        float timer = 0f;

        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / _fadeDuration;
            _fadeCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        _fadeCanvas.alpha = targetAlpha;
        _fadeCanvas.blocksRaycasts = targetAlpha > 0.5f;
    }
}
