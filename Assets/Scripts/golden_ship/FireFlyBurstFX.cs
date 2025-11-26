// Assets/Scripts/UI/FireflyBurstFX.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class FireFlyBurstFX : MonoBehaviour
{
    [Header("Hierarchy")]
    [SerializeField] private RectTransform fxParent;   // parent UI des FX (même Canvas / layer)
    [SerializeField] private Canvas canvas;            // Canvas (Screen Space Overlay/Camera)

    [Header("Particle Appearance")]
    [SerializeField] private Sprite fireflySprite;
    [SerializeField] private Material particleMaterial;          // optionnel (ex: additif)
    [SerializeField] private Gradient colorOverLifetime = DefaultGradient();
    [SerializeField] private Vector2 sizeRange = new Vector2(7f, 16f);
    [SerializeField] private AnimationCurve scaleCurve = DefaultScaleCurve(); // 0..1 → scale

    [Header("Emission")]
    [Min(1)][SerializeField] private int minCount = 8;
    [Min(1)][SerializeField] private int maxCount = 14;

    [Header("Lifetime (≤ 1s)")]
    [SerializeField] private Vector2 lifetimeRange = new Vector2(0.60f, 0.95f); // clampé à 1.0 max
    [SerializeField] private AnimationCurve fadeCurve = DefaultFadeCurve();      // 0..1 -> alpha

    [Header("Motion")]
    [SerializeField] private Vector2 speedRange = new Vector2(160f, 260f);  // px/s
    [SerializeField] private float drag = 2.2f;                             // amortissement (1/s)
    [SerializeField] private Vector2 gravity = new Vector2(0f, 0f);         // petite attraction (px/s²)
    [SerializeField] private float jitterAmplitude = 22f;                   // px/s (bruit de direction)
    [SerializeField] private float jitterFrequency = 9f;                    // Hz

    [Header("Spin")]
    [SerializeField] private Vector2 spinDegPerSecRange = new Vector2(-360f, 360f); // rotation aléatoire

    [Header("Shockwave (optional)")]
    [SerializeField] private bool spawnShockwave = true;
    [SerializeField] private Sprite shockwaveSprite;           // cercle doux
    [SerializeField] private float shockwaveDuration = 0.22f;
    [SerializeField] private Vector2 shockwaveSizeRange = new Vector2(24f, 120f); // de .. à (px)
    [SerializeField] private AnimationCurve shockwaveCurve = DefaultShockwaveCurve();
    [SerializeField] private Color shockwaveColor = new Color(1f, 1f, 1f, 0.65f);
    [SerializeField] private Material shockwaveMaterial;       // optionnel (additif)

    // Pool
    private readonly List<Image> _pool = new List<Image>(64);
    private readonly HashSet<Image> _inUse = new HashSet<Image>();

    private void Reset()
    {
        if (!fxParent) fxParent = transform as RectTransform;
        if (!canvas) canvas = GetComponentInParent<Canvas>();
    }

    /// <summary>Crée un burst sous la souris (screen space).</summary>
    public void SpawnBurstAtScreenPos(Vector2 screenPos)
    {
        if (!fxParent || !canvas || fireflySprite == null) return;

        Camera cam = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
            cam = canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(fxParent, screenPos, cam, out Vector2 localPoint))
            return;

        SpawnBurstAtLocalPoint(localPoint);
    }

    /// <summary>Crée un burst au point local de fxParent.</summary>
    public void SpawnBurstAtLocalPoint(Vector2 localPoint)
    {
        int count = Mathf.Clamp(Random.Range(minCount, maxCount + 1), 1, 999);

        // Shockwave central (optionnel)
        if (spawnShockwave && shockwaveSprite != null)
            StartCoroutine(PlayShockwave(localPoint));

        // Particules radiales
        for (int i = 0; i < count; i++)
        {
            var img = GetImage(fireflySprite, particleMaterial);
            var rt = (RectTransform)img.transform;
            rt.SetParent(fxParent, false);
            rt.anchoredPosition = localPoint;

            float size = Random.Range(sizeRange.x, sizeRange.y);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);

            // Couleur initiale (Alpha sera modulé par fadeCurve)
            Color c = colorOverLifetime.Evaluate(0f);
            img.color = c;
            img.raycastTarget = false;

            // Direction radiale 360°
            float ang = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)).normalized;

            float speed = Random.Range(speedRange.x, speedRange.y);

            // Spin aléatoire
            float spin = Random.Range(spinDegPerSecRange.x, spinDegPerSecRange.y);

            // Lifetime
            float life = Mathf.Clamp(Random.Range(lifetimeRange.x, lifetimeRange.y), 0.01f, 1.0f);

            // Seed pour jitter
            float seed = Random.value * 1000f;

            StartCoroutine(PlayOne(img, localPoint, dir * speed, spin, life, seed));
        }
    }

    private IEnumerator PlayOne(Image img, Vector2 startPos, Vector2 velocityInit, float spinDegPerSec, float life, float seed)
    {
        _inUse.Add(img);
        var rt = (RectTransform)img.transform;

        float t = 0f;
        Color startCol = img.color;
        float startRot = rt.localEulerAngles.z;

        Vector2 pos = startPos;
        Vector2 vel = velocityInit;

        while (t < life)
        {
            float dt = Time.deltaTime;
            t += dt;
            float k = Mathf.Clamp01(t / life);

            // Jitter (Perlin) – petit bruit directionnel
            Vector2 jitter = Vector2.zero;
            if (jitterAmplitude > 0f && jitterFrequency > 0f)
            {
                float tt = seed + t * jitterFrequency;
                float nx = Mathf.PerlinNoise(tt, 0.37f) * 2f - 1f;
                float ny = Mathf.PerlinNoise(0.59f, tt) * 2f - 1f;
                jitter = new Vector2(nx, ny) * jitterAmplitude;
            }

            // Accélération: gravité + amortissement (drag ~ vel)
            Vector2 acc = gravity - vel * drag + jitter;

            vel += acc * dt;
            pos += vel * dt;

            rt.anchoredPosition = pos;

            // Spin
            rt.localRotation = Quaternion.Euler(0f, 0f, startRot + spinDegPerSec * t);

            // Scale over lifetime
            float scaleK = (scaleCurve != null) ? Mathf.Max(0f, scaleCurve.Evaluate(k)) : 1f;
            rt.localScale = Vector3.one * scaleK;

            // Couleur + fade
            Color col = colorOverLifetime.Evaluate(k);
            float alphaMul = (fadeCurve != null) ? Mathf.Clamp01(fadeCurve.Evaluate(k)) : 1f;
            col.a *= alphaMul;
            img.color = col;

            yield return null;
        }

        Release(img);
    }

    private IEnumerator PlayShockwave(Vector2 localPoint)
    {
        var img = GetImage(shockwaveSprite, shockwaveMaterial);
        var rt = (RectTransform)img.transform;
        rt.SetParent(fxParent, false);
        rt.anchoredPosition = localPoint;
        img.raycastTarget = false;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, shockwaveDuration);
        Color baseCol = shockwaveColor;

        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);

            // Radius/scale animé
            float s = Mathf.Lerp(shockwaveSizeRange.x, shockwaveSizeRange.y, shockwaveCurve.Evaluate(k));
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, s);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, s);

            // Fade out
            Color c = baseCol;
            c.a *= 1f - k;
            img.color = c;

            yield return null;
        }

        Release(img);
    }

    // ---------- Pool helpers ----------
    private Image GetImage(Sprite sprite, Material mat)
    {
        // réutilise une Image inactive si possible
        for (int i = 0; i < _pool.Count; i++)
        {
            if (!_pool[i].gameObject.activeSelf)
            {
                var img = _pool[i];
                img.gameObject.SetActive(true);
                img.sprite = sprite;
                img.material = mat ? mat : null;
                return img;
            }
        }
        var go = new GameObject("FX_Image", typeof(RectTransform), typeof(Image));
        var newImg = go.GetComponent<Image>();
        newImg.sprite = sprite;
        newImg.material = mat ? mat : null;
        _pool.Add(newImg);
        return newImg;
    }

    private void Release(Image img)
    {
        if (!img) return;
        _inUse.Remove(img);
        img.gameObject.SetActive(false);
        img.transform.localScale = Vector3.one; // hygiene
        img.color = Color.white;
    }

    // ---------- Defaults ----------
    private static Gradient DefaultGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.95f, 0.6f), 0f), new GradientColorKey(new Color(1f, 0.6f, 0.2f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        return g;
    }

    private static AnimationCurve DefaultScaleCurve()
        => new AnimationCurve(new Keyframe(0f, 0.6f, 2.5f, 2.5f), new Keyframe(0.15f, 1.2f, 0f, 0f), new Keyframe(1f, 0.4f, -1.8f, -1.8f));

    private static AnimationCurve DefaultFadeCurve()
        => AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private static AnimationCurve DefaultShockwaveCurve()
        => new AnimationCurve(new Keyframe(0f, 0f, 0f, 3f), new Keyframe(1f, 1f, 0f, 0f));
}
