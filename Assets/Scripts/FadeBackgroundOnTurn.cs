using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeBackgroundOnTurn : MonoBehaviour
{
    public Image background1;
    public Image playerBG;
    public Image enemyBG;
    public float fadeDuration = 1f;

    private Coroutine fadeCoroutine;

    void Awake()
    {
        ResetBG();
    }

    public void SetTurn(bool myTurn)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeTurn(myTurn));
    }

    IEnumerator FadeTurn(bool myTurn)
    {
        float elapsed = 0f;

        float startAlphaPlayer = playerBG.color.a;
        float startAlphaEnemy = enemyBG.color.a;

        float targetAlphaPlayer = myTurn ? 1f : 0f;
        float targetAlphaEnemy = myTurn ? 0f : 1f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            elapsed = elapsed * 1.1f;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            SetAlpha(playerBG, Mathf.Lerp(startAlphaPlayer, targetAlphaPlayer, t));
            SetAlpha(enemyBG, Mathf.Lerp(startAlphaEnemy, targetAlphaEnemy, t));

            yield return null;
        }

        SetAlpha(playerBG, targetAlphaPlayer);
        SetAlpha(enemyBG, targetAlphaEnemy);
    }

    void SetAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    public void ResetBG()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        Debug.Log($"[FadeBG] reset bg by");

        SetAlpha(playerBG, 0f);
        SetAlpha(enemyBG, 0f);
        SetAlpha(background1, 1f);
    }
}
