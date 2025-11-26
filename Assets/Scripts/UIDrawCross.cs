// Assets/Scripts/UI/FX/UIDrawCrossGroups.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("FX/UI Draw Cross (Player+Enemy) - Alpha Only")]
[DisallowMultipleComponent]
public sealed class UIDrawCross : MonoBehaviour
{
    [Header("PLAYER lines (already placed)")]
    [SerializeField] private Image playerLine1;
    [SerializeField] private Image playerLine2;

    [Header("ENEMY lines (already placed)")]
    [SerializeField] private Image enemyLine1;
    [SerializeField] private Image enemyLine2;

    [Header("Appearance")]
    [SerializeField] private bool hideWhenIdle = true;

    [Header("Timing")]
    [SerializeField, Range(0.01f, 2f)] private float fadeInPerLine = 0.25f;
    [SerializeField, Range(0f, 1f)] private float delayBetweenLines = 0.10f;
    [SerializeField, Range(0f, 2f)] private float holdDuration = 0.25f;
    [SerializeField, Range(0f, 2f)] private float fadeOutDuration = 0.25f;

    private Coroutine _coPlayer, _coEnemy;

    private void Awake()
    {
        // Assure alpha=0 au démarrage si demandé
        if (hideWhenIdle)
        {
            SetAlpha(playerLine1, 0f); SetAlpha(playerLine2, 0f);
            SetAlpha(enemyLine1, 0f); SetAlpha(enemyLine2, 0f);
        }
    }

    // --------- Public API ----------
    public void PlayPlayer()
    {
        if (_coPlayer != null) StopCoroutine(_coPlayer);
        _coPlayer = StartCoroutine(CoPlayTwo(playerLine1, playerLine2));
    }

    public void PlayEnemy()
    {
        if (_coEnemy != null) StopCoroutine(_coEnemy);
        _coEnemy = StartCoroutine(CoPlayTwo(enemyLine1, enemyLine2));
    }

    public void PlayBoth()
    {
        PlayPlayer();
        PlayEnemy();
    }

    public void ResetAndHideAll()
    {
        if (_coPlayer != null) StopCoroutine(_coPlayer);
        if (_coEnemy != null) StopCoroutine(_coEnemy);
        _coPlayer = _coEnemy = null;

        SetAlpha(playerLine1, 0f); SetAlpha(playerLine2, 0f);
        SetAlpha(enemyLine1, 0f); SetAlpha(enemyLine2, 0f);
    }

    // --------- Core ----------
    private IEnumerator CoPlayTwo(Image line1, Image line2)
    {
        if (!line1 || !line2) yield break;

        // Reset alphas
        SetAlpha(line1, 0f); SetAlpha(line2, 0f);

        // 1) ligne 1 : fade-in
        yield return Fade(line1, 0f, 1f, fadeInPerLine);

        // 2) délai
        if (delayBetweenLines > 0f) yield return new WaitForSeconds(delayBetweenLines);

        // 3) ligne 2 : fade-in
        yield return Fade(line2, 0f, 1f, fadeInPerLine);

        // 4) hold
        if (holdDuration > 0f) yield return new WaitForSeconds(holdDuration);

        // 5) fade-out des deux ensemble
        if (fadeOutDuration > 0f)
        {
            float a1 = GetAlpha(line1);
            float a2 = GetAlpha(line2);
            float t = 0f;
            while (t < fadeOutDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / fadeOutDuration);
                SetAlpha(line1, Mathf.Lerp(a1, 0f, k));
                SetAlpha(line2, Mathf.Lerp(a2, 0f, k));
                yield return null;
            }
            SetAlpha(line1, 0f);
            SetAlpha(line2, 0f);
        }
    }

    private IEnumerator Fade(Image img, float from, float to, float duration)
    {
        if (!img) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            SetAlpha(img, Mathf.Lerp(from, to, k));
            yield return null;
        }
        SetAlpha(img, to);
    }

    // --------- Utils ----------
    private static void SetAlpha(Image img, float a)
    {
        if (!img) return;
        var c = img.color; c.a = a; img.color = c;
    }

    private static float GetAlpha(Image img)
    {
        return img ? img.color.a : 0f;
    }
}
