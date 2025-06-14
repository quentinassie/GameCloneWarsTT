using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fait osciller la luminance d'une image UI (Canvas Overlay) sans modifier la transparence.
/// Idéal pour un style Clone Wars avec effets lumineux.
/// </summary>
[RequireComponent(typeof(Image))]
public class CardAreaBrightness : MonoBehaviour
{
    [Tooltip("Vitesse de l'oscillation.")]
    public float oscillationSpeed = 0.5f;

    [Tooltip("Facteur de luminance minimum (1 = normal).")]
    public float minBrightness = 0.7f;

    [Tooltip("Facteur de luminance maximum (1 = normal).")]
    public float maxBrightness = 1.3f;

    private Image uiImage;
    private Color baseColor;

    void Awake()
    {
        uiImage = GetComponent<Image>();
        baseColor = uiImage.color;
    }

    void Update()
    {
        float brightness = Mathf.Lerp(minBrightness, maxBrightness, (Mathf.Sin(Time.time * oscillationSpeed) + 1f) / 2f);
        Color newColor = new Color(
            Mathf.Clamp01(baseColor.r * brightness),
            Mathf.Clamp01(baseColor.g * brightness),
            Mathf.Clamp01(baseColor.b * brightness),
            baseColor.a
        );
        uiImage.color = newColor;
    }
}
