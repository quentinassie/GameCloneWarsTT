using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HexLineFill : MonoBehaviour
{
    [Header("Bas")]
    [SerializeField] private Image middleG_Bas;
    [SerializeField] private Image middleD_Bas;

    [Header("Milieu")]
    [SerializeField] private Image middlePart;

    [Header("Haut")]
    [SerializeField] private Image middleG_Haut;
    [SerializeField] private Image middleD_Haut;

    [SerializeField] private float segmentDuration = 0.5f;
    [SerializeField] private float pauseBetween = 0.1f;

    private Coroutine fillRoutine;

    public void StartHexFillSequence()
    {
        if (fillRoutine != null)
            StopCoroutine(fillRoutine);

        fillRoutine = StartCoroutine(FillSequence());
    }

    private IEnumerator FillSequence()
    {
        // Reset tous
        ResetFill(middleG_Bas, middleD_Bas, middlePart, middleG_Haut, middleD_Haut);

        // 1. Bas → G puis D
        yield return FillPart(middleG_Bas, segmentDuration);
        yield return FillPart(middleD_Bas, segmentDuration);
        yield return new WaitForSeconds(pauseBetween);

        // 2. Partie centrale (pointes)
        yield return FillPart(middlePart, segmentDuration);
        yield return new WaitForSeconds(pauseBetween);

        // 3. Haut → G puis D
        yield return FillPart(middleG_Haut, segmentDuration);
        yield return FillPart(middleD_Haut, segmentDuration);
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

    private void ResetFill(params Image[] images)
    {
        foreach (var img in images)
        {
            img.fillAmount = 0f;
        }
    }
}
