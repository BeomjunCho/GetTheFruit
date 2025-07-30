using System.Collections;
using UnityEngine;

/// <summary>
/// Breakable wall destroyed only by RockMan's punch.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WeakWall : MonoBehaviour
{
    /* ------------------------------------------------------------------ */
    /*  Inspector                                                         */
    /* ------------------------------------------------------------------ */
    [Header("Break Settings")]
    [SerializeField, Tooltip("Optional particle prefab spawned on break.")]
    private GameObject _breakEffect = null;

    [SerializeField, Tooltip("Delay (seconds) before SFX + disappear.")]
    private float _destroyDelay = 1f;

    /* ------------------------------------------------------------------ */
    /*  State                                                             */
    /* ------------------------------------------------------------------ */
    private bool _broken;

    /* ------------------------------------------------------------------ */
    /*  Public API                                                        */
    /* ------------------------------------------------------------------ */
    public void Break()
    {
        if (_broken) return;
        _broken = true;

        /* Disable collision right away so gameplay continues *//*
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;*/

        StartCoroutine(BreakRoutine());
    }

    /* ------------------------------------------------------------------ */
    /*  Coroutine                                                         */
    /* ------------------------------------------------------------------ */
    private IEnumerator BreakRoutine()
    {
        /* wait before visual disappearance */
        yield return new WaitForSeconds(_destroyDelay);

        /* play SFX */
        AudioManager.Instance.Play2dSfx("WeakWallBroken_01");

        /* spawn particles (optional) */
        if (_breakEffect != null)
            Instantiate(_breakEffect, transform.position, Quaternion.identity);

        /* hide visuals */
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        /* destroy GameObject immediately after */
        Destroy(gameObject);
    }
}
