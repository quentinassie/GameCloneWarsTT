using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HoloTableDisplay : MonoBehaviour
{
    [SerializeField] private GameObject fluid;
    [SerializeField] private float transitionDuration = 0.5f;

    private const string ShaderColorProperty = "_ColorTint";
    private Coroutine currentAnimation;
    private Material fluidMaterial;

    void Start()
    {
        EnsureMaterial();
    }

    public void SetHoloTable(bool myTurn)
    {
        if (fluid == null) return;
        EnsureMaterial();

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        if (!fluid.activeSelf)
            fluid.SetActive(true);

        if (myTurn)
        {
            // Blanc pur (#FFFFFF) avec alpha 1
            if (!ColorUtility.TryParseHtmlString("#FFFFFF", out var targetWhite))
                targetWhite = Color.white; // fallback

            currentAnimation = StartCoroutine(AnimateShaderColorTo(targetWhite, transitionDuration));
        }
        else
        {
            // Tour adverse : fade-out
            currentAnimation = StartCoroutine(FadeOutThenDisable());
        }
    }


    public void ShowResultColor()
    {
        if (fluid == null) return;
        EnsureMaterial();

        // Vert résultat
        if (!ColorUtility.TryParseHtmlString("#9EF096", out var green))
            green = new Color(0.62f, 0.94f, 0.59f, 1f); // fallback

        if (fluid.activeSelf)
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(AnimateShaderColorTo(green, 0.5f));
        }
        else
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            fluid.SetActive(true);

            // Fade-in depuis alpha 0 vers vert plein
            var transparentGreen = new Color(green.r, green.g, green.b, 0f);
            fluidMaterial.SetColor(ShaderColorProperty, transparentGreen);
            currentAnimation = StartCoroutine(FadeInToColor(green, 0.5f));
        }
    }

    // ---------- Helpers ----------

    private void EnsureMaterial()
    {
        if (fluidMaterial != null) return;
        if (fluid == null) return;

        var image = fluid.GetComponent<Image>();
        if (image != null)
            fluidMaterial = image.material;
        else
            Debug.LogWarning("Image component not found on fluid.");
    }

    private IEnumerator FadeInToColor(Color targetColor, float duration)
    {
        float elapsed = 0f;
        var startColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
        var endColor = new Color(targetColor.r, targetColor.g, targetColor.b, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var lerped = Color.Lerp(startColor, endColor, elapsed / duration);
            fluidMaterial.SetColor(ShaderColorProperty, lerped);
            yield return null;
        }
        fluidMaterial.SetColor(ShaderColorProperty, endColor);
    }

    private IEnumerator AnimateShaderColorTo(Color targetColor, float duration)
    {
        EnsureMaterial();

        var startColor = fluidMaterial.GetColor(ShaderColorProperty);
        float elapsed = 0f;

        // Si l’objet vient d’être activé avec alpha 0, on assure une base non-nulle
        if (startColor.a <= 0f)
            startColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var lerped = Color.Lerp(startColor, targetColor, elapsed / duration);
            fluidMaterial.SetColor(ShaderColorProperty, lerped);
            yield return null;
        }
        fluidMaterial.SetColor(ShaderColorProperty, targetColor);
    }

    private IEnumerator FadeOutThenDisable()
    {
        EnsureMaterial();

        var startColor = fluidMaterial.GetColor(ShaderColorProperty);
        var endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            var lerped = Color.Lerp(startColor, endColor, elapsed / transitionDuration);
            fluidMaterial.SetColor(ShaderColorProperty, lerped);
            yield return null;
        }

        fluidMaterial.SetColor(ShaderColorProperty, endColor);
        fluid.SetActive(false);
    }
}
