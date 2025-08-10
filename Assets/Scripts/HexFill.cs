using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HexFill : MonoBehaviour
{
    [Header("Bas (non utilisé dans ce mode)")]
    [SerializeField] private Image middleG_Bas;
    [SerializeField] private Image middleD_Bas;

    [Header("Milieu (non utilisé dans ce mode)")]
    [SerializeField] private Image middlePart;

    [Header("Haut (seule partie animée)")]
    [SerializeField] private Image middleG_Haut;
    [SerializeField] private Image middleD_Haut;

    [Header("Durées")]
    [SerializeField] private float segmentDuration = 0.5f; // durée pour le remplissage de la partie haute

    private Coroutine fillRoutine;

    // On garde les couleurs d'origine uniquement pour la partie haute
    private Color originalColor_HautG;
    private Color originalColor_HautD;

    void Awake()
    {
        if (middleG_Haut != null) originalColor_HautG = middleG_Haut.color;
        if (middleD_Haut != null) originalColor_HautD = middleD_Haut.color;
    }

    /// <summary>
    /// Lance le remplissage uniquement sur la partie haute, avec la couleur passée.
    /// </summary>
    public void StartHexFillSequence(Color32 target)
    {
        if (fillRoutine != null)
            StopCoroutine(fillRoutine);

        fillRoutine = StartCoroutine(FillSequence(target));
    }

    private IEnumerator FillSequence(Color target)
    {
        // Reset uniquement la partie haute
        ResetFill(middleG_Haut, middleD_Haut);
        ApplyColorTop(target);

        // Remplissage haut avec easing
        yield return FillPartsSimultaneously(middleG_Haut, middleD_Haut, segmentDuration, EaseOut);

        // Restaure les couleurs originales de la partie haute
        RestoreOriginalColorsTop();
    }

    // ---------- Anim helpers ----------

    private IEnumerator FillPart(Image img, float duration)
    {
        if (img == null) yield break;

        float elapsed = 0f;
        img.fillAmount = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            img.fillAmount = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        img.fillAmount = 1f;
    }

    private IEnumerator FillPartsSimultaneously(Image imgA, Image imgB, float duration, System.Func<float, float> easing = null)
    {
        if (imgA == null && imgB == null) yield break;

        float elapsed = 0f;
        if (imgA != null) imgA.fillAmount = 0f;
        if (imgB != null) imgB.fillAmount = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float amount = easing != null ? easing(t) : t;

            if (imgA != null) imgA.fillAmount = amount;
            if (imgB != null) imgB.fillAmount = amount;

            yield return null;
        }

        if (imgA != null) imgA.fillAmount = 1f;
        if (imgB != null) imgB.fillAmount = 1f;
    }

    private float EaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - t, 5f); // Quintic ease-out
    }

    // ---------- Utils ----------

    private void ResetFill(params Image[] images)
    {
        foreach (var img in images)
        {
            if (img != null) img.fillAmount = 0f;
        }
    }

    private void ApplyColorTop(Color color)
    {
        if (middleG_Haut != null) middleG_Haut.color = color;
        if (middleD_Haut != null) middleD_Haut.color = color;
    }

    private void RestoreOriginalColorsTop()
    {
        if (middleG_Haut != null) middleG_Haut.color = originalColor_HautG;
        if (middleD_Haut != null) middleD_Haut.color = originalColor_HautD;
    }
}
