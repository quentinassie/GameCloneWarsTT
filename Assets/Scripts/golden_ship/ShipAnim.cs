// Assets/Scripts/UI/ShipBalloonSimpleLoop2D.cs
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ShipBalloonSimpleLoop2D : MonoBehaviour
{
    [Header("Hierarchy (UI)")]
    [SerializeField] private RectTransform groupRoot; // parent qui bouge (déjà dans le Canvas)
    [SerializeField] private RectTransform balloon;   // RectTransform qui contient l'image + TMP glow
    [SerializeField] private Canvas canvas;           // Canvas racine (Screen Space Overlay)

    [Header("Click FX (optional)")]
    [SerializeField] private BalloonClickGlow clickFx; // ← assigner si tu utilises le clic

    [Header("Start & Direction")]
    [SerializeField] private Vector2 startAnchoredPos = new Vector2(-250f, 90f);
    [SerializeField] private bool useObjectRotation = true;
    [SerializeField] private float angleZDeg = 12.1f;

    [Header("Motion")]
    [Min(0f)][SerializeField] private float speed = 240f;
    [Min(0f)][SerializeField] private float repeatDelay = 1.0f;
    [Min(0f)][SerializeField] private float margin = 40f;

    [Header("Breathing (balloon)")]
    [Range(0f, 0.5f)][SerializeField] private float scaleAmplitude = 0.06f;
    [Min(0f)][SerializeField] private float scaleFrequency = 0.75f;

    [Header("Swing Z — Balloon (pivot = bout de tige)")]
    [SerializeField] private RectTransform swingBalloon;       // ex: text_glow (parent de l'image ballon)
    [SerializeField, Range(0f, 30f)] private float swingBalloonAmpDeg = 4f;
    [SerializeField, Min(0f)] private float swingBalloonFreqHz = 1.1f;
    [SerializeField] private float swingBalloonPhase = 0f;     // radians

    [Header("Swing Z — Rod/Ship subgroup (pivot = arrière)")]
    [SerializeField] private RectTransform swingRodGroup;      // sous-groupe à l'arrière du vaisseau
    [SerializeField, Range(0f, 30f)] private float swingRodAmpDeg = 2.5f;
    [SerializeField, Min(0f)] private float swingRodFreqHz = 0.9f;
    [SerializeField] private float swingRodPhase = 0.6f;       // radians

    [SerializeField] private bool playOnAwake = true;

    // runtime
    private Coroutine _loop;
    private Vector3 _balloonBaseScale = Vector3.one;
    private float _swingBalloonBaseZ, _swingRodBaseZ;

    private void Reset()
    {
        if (!groupRoot) groupRoot = GetComponent<RectTransform>();
        if (!canvas) canvas = GetComponentInParent<Canvas>();
    }

    private void Awake()
    {
        if (!groupRoot) groupRoot = GetComponent<RectTransform>();
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        if (balloon) _balloonBaseScale = balloon.localScale;

        // Capturer Z de base
        if (swingBalloon) _swingBalloonBaseZ = swingBalloon.localEulerAngles.z;
        if (swingRodGroup) _swingRodBaseZ = swingRodGroup.localEulerAngles.z;
    }

    private void OnEnable()
    {
        if (playOnAwake) Play();
    }

    private void OnDisable()
    {
        Stop();
    }

    public void Play()
    {
        if (!groupRoot || !canvas) return;
        if (_loop != null) StopCoroutine(_loop);
        _loop = StartCoroutine(RunLoop());
    }

    public void Stop()
    {
        if (_loop != null) { StopCoroutine(_loop); _loop = null; }
        if (balloon) balloon.localScale = _balloonBaseScale;
        if (swingBalloon) SetLocalZ(swingBalloon, _swingBalloonBaseZ);
        if (swingRodGroup) SetLocalZ(swingRodGroup, _swingRodBaseZ);
        if (clickFx) clickFx.ResetVisuals(); // ← reset FX clic
    }

    public void SetAngle(float zDeg) { angleZDeg = zDeg; useObjectRotation = false; }
    public void SetSpeed(float pxPerSec) { speed = Mathf.Max(0f, pxPerSec); }

    private System.Collections.IEnumerator RunLoop()
    {
        var canvasRect = canvas.transform as RectTransform;

        while (true)
        {
            // Reset départ + reset FX clic au début de chaque run
            groupRoot.anchoredPosition = startAnchoredPos;
            if (clickFx) clickFx.ResetVisuals(); // ← reset FX clic

            Vector2 dir = ComputeDir2D();

            while (true)
            {
                float dt = Time.deltaTime;

                // Move XY uniquement
                Vector2 pos = groupRoot.anchoredPosition;
                pos += dir * (speed * dt);
                groupRoot.anchoredPosition = pos;

                // Breathing (scale) sur le balloon
                if (balloon && scaleAmplitude > 0f)
                {
                    float s = 1f + Mathf.Sin(2f * Mathf.PI * scaleFrequency * Time.time) * scaleAmplitude;
                    balloon.localScale = _balloonBaseScale * s;
                }

                // Swing Z (balloon)
                if (swingBalloon && swingBalloonAmpDeg > 0f && swingBalloonFreqHz > 0f)
                {
                    float dz = Mathf.Sin(2f * Mathf.PI * swingBalloonFreqHz * Time.time + swingBalloonPhase) * swingBalloonAmpDeg;
                    SetLocalZ(swingBalloon, _swingBalloonBaseZ + dz);
                }

                // Swing Z (rod/ship subgroup)
                if (swingRodGroup && swingRodAmpDeg > 0f && swingRodFreqHz > 0f)
                {
                    float dz = Mathf.Sin(2f * Mathf.PI * swingRodFreqHz * Time.time + swingRodPhase) * swingRodAmpDeg;
                    SetLocalZ(swingRodGroup, _swingRodBaseZ + dz);
                }

                // Sorti du Canvas ?
                if (IsFullyOutside(canvasRect, groupRoot, margin)) break;

                yield return null;
            }

            // Reset entre runs
            if (balloon) balloon.localScale = _balloonBaseScale;
            if (swingBalloon) SetLocalZ(swingBalloon, _swingBalloonBaseZ);
            if (swingRodGroup) SetLocalZ(swingRodGroup, _swingRodBaseZ);

            if (repeatDelay > 0f) yield return new WaitForSeconds(repeatDelay);
            else yield return null;
        }
    }

    private static void SetLocalZ(Transform t, float zDeg)
    {
        var e = t.localEulerAngles;
        e.z = zDeg;
        t.localEulerAngles = e;
    }

    private Vector2 ComputeDir2D()
    {
        if (useObjectRotation)
        {
            Vector3 r = groupRoot.right;
            Vector2 d = new Vector2(r.x, r.y);
            return d.sqrMagnitude > 1e-6f ? d.normalized : Vector2.right;
        }
        else
        {
            float rad = angleZDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }

    private static bool IsFullyOutside(RectTransform canvasRect, RectTransform target, float marginPx)
    {
        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, target);
        Rect c = canvasRect.rect;
        Rect expandedCanvas = new Rect(c.xMin - marginPx, c.yMin - marginPx,
                                       c.width + 2f * marginPx, c.height + 2f * marginPx);
        Rect b = new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
        return !expandedCanvas.Overlaps(b);
    }
}
