using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HexFill : MonoBehaviour
{
    [Header("Bas")]
    [SerializeField] private Image middleG_Bas;
    [SerializeField] private Image middleD_Bas;

    [Header("Milieu")]
    [SerializeField] private Image middlePart;

    [Header("Haut")]
    [SerializeField] private Image middleG_Haut;
    [SerializeField] private Image middleD_Haut;

    [Header("Durées")]
    [SerializeField] private float segmentDuration = 0.5f;
    [SerializeField] private float pauseBetween = 0.1f;
    [SerializeField] private float holdDuration = 1.0f;

    private Coroutine fillRoutine;



    private Color[] originalColors;

    void Awake()
    {
        originalColors = new Color[5];
        originalColors[0] = middleG_Bas.color;
        originalColors[1] = middleD_Bas.color;
        originalColors[2] = middlePart.color;
        originalColors[3] = middleG_Haut.color;
        originalColors[4] = middleD_Haut.color;
    }

    public void StartHexFillSequence(Color32 target)
    {
        if (fillRoutine != null)
            StopCoroutine(fillRoutine);

        fillRoutine = StartCoroutine(FillSequence(target));
    }

    private IEnumerator FillSequence(Color target)
    {
        ResetFill(middleG_Bas, middleD_Bas, middlePart, middleG_Haut, middleD_Haut);
        ApplyColor(target);

        yield return FillPartsSimultaneously(middleG_Bas, middleD_Bas, segmentDuration * 0.5f);
        yield return FillPart(middlePart, segmentDuration);

        // Haut avec easing
        yield return FillPartsSimultaneously(middleG_Haut, middleD_Haut, 1.0f, EaseOut);



        RestoreOriginalColors();
    }

    private IEnumerator FillPart(Image img, float duration)
    {
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

    private IEnumerator FillPartReverse(Image img, float duration)
    {
        float elapsed = 0f;
        img.fillAmount = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            img.fillAmount = Mathf.Clamp01(1f - (elapsed / duration));
            yield return null;
        }

        img.fillAmount = 0f;
    }

    private IEnumerator FillPartsSimultaneously(Image imgA, Image imgB, float duration, System.Func<float, float> easing = null)
    {
        float elapsed = 0f;
        imgA.fillAmount = 0f;
        imgB.fillAmount = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float amount = easing != null ? easing(t) : t;
            imgA.fillAmount = amount;
            imgB.fillAmount = amount;
            yield return null;
        }

        imgA.fillAmount = 1f;
        imgB.fillAmount = 1f;
    }

    private IEnumerator FillPartsSimultaneouslyReverse(Image imgA, Image imgB, float duration)
    {
        float elapsed = 0f;
        imgA.fillAmount = 1f;
        imgB.fillAmount = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float amount = Mathf.Clamp01(1f - (elapsed / duration));
            imgA.fillAmount = amount;
            imgB.fillAmount = amount;
            yield return null;
        }

        imgA.fillAmount = 0f;
        imgB.fillAmount = 0f;
    }
    private float EaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - t, 5f); // Quintic ease-out
    }


    private void ResetFill(params Image[] images)
    {
        foreach (var img in images)
            img.fillAmount = 0f;
    }

    private void ApplyColor(Color color)
    {
        middleG_Bas.color = color;
        middleD_Bas.color = color;
        middlePart.color = color;
        middleG_Haut.color = color;
        middleD_Haut.color = color;
    }

    private void RestoreOriginalColors()
    {
        middleG_Bas.color = originalColors[0];
        middleD_Bas.color = originalColors[1];
        middlePart.color = originalColors[2];
        middleG_Haut.color = originalColors[3];
        middleD_Haut.color = originalColors[4];
    }
}