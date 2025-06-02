using UnityEngine;
using UnityEngine.UI;

public class Rotation79 : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(10f, 10f, 20f);

    public float pulseSpeed = 2f;
    [Range(0f, 1f)] public float minAlpha = 0.4f;
    [Range(0f, 1f)] public float maxAlpha = 0.8f;

    [Header("Blinking Effect")]
    public bool enableBlinking = true;
    public float blinkChancePerSecond = 0.1f;
    public float blinkDuration = 0.1f;

    private Image img;
    private RectTransform rectTransform;
    private Color baseColor;
    private float blinkTimer = 0f;
    private float nextBlinkTime = 0f;
    private bool isBlinking = false;

    private float angleX = 0f;
    private float angleY = 0f;
    private float angleZ = 0f;

    void Start()
    {
        img = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        baseColor = img.color;
        ScheduleNextBlink();
    }

    void Update()
    {
        angleX += rotationSpeed.x * Time.deltaTime;
        angleY += rotationSpeed.y * Time.deltaTime;
        angleZ += rotationSpeed.z * Time.deltaTime;

        angleX %= 360f;
        angleY %= 360f;
        angleZ %= 360f;

        Quaternion rotX = Quaternion.Euler(angleX, 0f, 0f);
        Quaternion rotY = Quaternion.Euler(0f, angleY, 0f);
        Quaternion rotZ = Quaternion.Euler(0f, 0f, angleZ);

        // Combinaison des rotations pour effet sphérique
        rectTransform.localRotation = rotZ * rotY * rotX;

        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);

        if (enableBlinking)
        {
            if (isBlinking)
            {
                blinkTimer -= Time.deltaTime;
                if (blinkTimer <= 0f)
                {
                    isBlinking = false;
                    ScheduleNextBlink();
                }
                else
                {
                    alpha = 0f;
                }
            }
            else if (Time.time >= nextBlinkTime)
            {
                isBlinking = true;
                blinkTimer = blinkDuration;
            }
        }

        img.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
    }

    void ScheduleNextBlink()
    {
        float interval = Random.Range(1f / blinkChancePerSecond, 3f / blinkChancePerSecond);
        nextBlinkTime = Time.time + interval;
    }
}
