using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Change cycliquement la couleur d'une Image UI en teintes pastel.
/// </summary>
[RequireComponent(typeof(Image))]
public class LightBarColorCycle : MonoBehaviour
{
    [Header("Cycle Couleur")]
    public float hueSpeed = 0.1f; // vitesse du cycle (0.1 = lent)
    [Range(0f, 1f)] public float saturation = 0.4f; // faible saturation = pastel
    [Range(0f, 1f)] public float brightness = 1f;   // intensité lumineuse

    private Image img;
    private float hue;

    void Start()
    {
        img = GetComponent<Image>();
        hue = Random.value; // démarre avec une couleur aléatoire
    }

    void Update()
    {
        hue += hueSpeed * Time.deltaTime;
        if (hue > 1f) hue -= 1f;

        Color pastelColor = Color.HSVToRGB(hue, saturation, brightness);
        img.color = pastelColor;
    }
}
