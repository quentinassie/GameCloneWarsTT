using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HexSequence : MonoBehaviour
{
    [SerializeField] private float delayBetweenHexes = 0.1f;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float fadeOutMultiplier = 1.5f;
    [SerializeField] private Color fillColor = Color.white;

    public void StartHexFillSequence()
    {
        StartCoroutine(FillHexesCoroutine());
    }

    private IEnumerator FillHexesCoroutine()
    {
        Image[] hexes = GetComponentsInChildren<Image>(includeInactive: true);

        for (int i = 0; i < hexes.Length; i++)
        {
            Image hex = hexes[i];
            if (hex == null) continue;

            Color startColor = fillColor;
            startColor.a = 0;
            Color endColor = fillColor;
            endColor.a = 1;

            hex.color = startColor;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                hex.color = Color.Lerp(startColor, endColor, elapsed / fadeDuration);
                yield return null;
            }

            hex.color = endColor;

            StartCoroutine(FadeOutHex(hex));
            yield return new WaitForSeconds(delayBetweenHexes);
        }
    }

    private IEnumerator FadeOutHex(Image hex)
    {
        Color startColor = hex.color;
        Color endColor = hex.color;
        endColor.a = 0;

        float duration = fadeDuration * fadeOutMultiplier;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            hex.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
        }

        hex.color = endColor;
    }
}
