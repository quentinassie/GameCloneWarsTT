using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Applique une oscillation sinusoïdale directe sur la luminance d'une Image UI.
/// </summary>
[RequireComponent(typeof(Image))]
public class CardAreaBrightness : MonoBehaviour
{
    [Tooltip("Amplitude du facteur de luminance (autour de 1.0).")]
    public float brightnessAmplitude = 0.3f;

    [Tooltip("Vitesse de l'oscillation (Hz).")]
    public float oscillationSpeed = 1.0f;

    private Image uiImage;
    private Color baseColor;

    void Awake()
    {
        uiImage = GetComponent<Image>();
        baseColor = uiImage.color;
    }

    void Update()
    {
        float brightness = 0.8f + Mathf.Sin(Time.time * Mathf.PI * 2f * oscillationSpeed) * brightnessAmplitude;

        Color newColor = new Color(
            Mathf.Clamp01(baseColor.r * brightness),
            Mathf.Clamp01(baseColor.g * brightness),
            Mathf.Clamp01(baseColor.b * brightness),
            baseColor.a
        );

        uiImage.color = newColor;
    }
}
