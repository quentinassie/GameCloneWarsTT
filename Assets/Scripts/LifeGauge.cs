// Assets/Scripts/UI/DeckLifeMirrorGauge.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu("UI/Deck Life Mirror Gauge (Player+Enemy, 11 circles, alpha + thresholds + pulses)")]
[DisallowMultipleComponent]
public sealed class LifeGauge : MonoBehaviour
{
    // ---------- PLAYER ----------
    [Header("PLAYER — Circles (left → right, center at index 5)")]
    [SerializeField] private List<Image> playerCircles = new List<Image>(11);
    [Header("PLAYER — Center label (cards count)")]
    [SerializeField] private TMP_Text playerCenterCountText;
    [Header("PLAYER — Deck")]
    [Min(1)][SerializeField] private int playerInitialDeckCount = 6;
    [Min(0)][SerializeField] private int playerCurrentDeckCount = 6;
    [Header("PLAYER — Healthy accent (when green)")]
    [SerializeField] private bool playerEnableHealthyAccent = true;
    [SerializeField] private int playerHealthyAccentIndex = 6;
    [SerializeField] private Color playerHealthyAccentColor = new Color32(0x82, 0xFF, 0x00, 0xFF); // #82FF00

    // ---------- ENEMY ----------
    [Header("ENEMY — Circles (left → right, center at index 5)")]
    [SerializeField] private List<Image> enemyCircles = new List<Image>(11);
    [Header("ENEMY — Center label (cards count)")]
    [SerializeField] private TMP_Text enemyCenterCountText;
    [Header("ENEMY — Deck")]
    [Min(1)][SerializeField] private int enemyInitialDeckCount = 6;
    [Min(0)][SerializeField] private int enemyCurrentDeckCount = 6;
    [Header("ENEMY — Healthy accent (when green)")]
    [SerializeField] private bool enemyEnableHealthyAccent = true;
    [SerializeField] private int enemyHealthyAccentIndex = 6;
    [SerializeField] private Color enemyHealthyAccentColor = new Color32(0x82, 0xFF, 0x00, 0xFF);

    // ---------- Shared visuals ----------
    [Header("Visuals (alpha only)")]
    [Range(0f, 1f)][SerializeField] private float hiddenAlpha = 0f;
    [Range(0f, 1f)][SerializeField] private float visibleAlpha = 1f;

    [Header("Colors by threshold (ratio of initial)")]
    [SerializeField] private Color healthyColor = new Color32(0, 255, 0, 255);      // >= 3/6
    [SerializeField] private Color warningColor = new Color32(255, 165, 0, 255);    // < 3/6 and >= 2/6
    [SerializeField] private Color dangerColor = new Color32(255, 0, 0, 255);      // < 2/6
    [SerializeField] private Color overcapColor = new Color32(0x00, 0xFF, 0xAE, 0xFF); // #00FFAE (≥ 9/6)

    // ---------- Pulses (symétriques extrémité -> centre) ----------
    public enum PulseDirection { LeftToRight, RightToLeft }

    [Header("Pulse — Activation")]
    [SerializeField] private bool pulseEnabled = false;
    [SerializeField] private bool pulseAffectsHidden = false;

    [Header("Pulse — Rythme")]
    [Min(0f)][SerializeField] private float pulseIntervalSeconds = 1.50f;
    [Min(0f)][SerializeField] private float pulseStepDelay = 0.06f;

    [Header("Pulse — Courbe & Durées")]
    [Min(0f)][SerializeField] private float pulseRiseTime = 0.20f;
    [Min(0f)][SerializeField] private float pulseFallTime = 0.30f;
    [SerializeField] private AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Pulse — Teinte (HSV + blanc)")]
    [SerializeField, Range(0f, 1f)] private float pulseWhiteLerp = 0.35f; // part du blanc au pic
    [SerializeField, Range(0f, 1f)] private float pulseSatBoost = 0.10f; // +saturation (HSV)
    [SerializeField, Range(0f, 1.5f)] private float pulseValBoost = 0.35f; // +valeur (HSV)

    [Header("Pulse — Sens par côté")]
    [SerializeField] private PulseDirection playerPulseDirection = PulseDirection.LeftToRight;
    [SerializeField] private PulseDirection enemyPulseDirection = PulseDirection.RightToLeft;

    private Coroutine _coPlayerPulse;
    private Coroutine _coEnemyPulse;

    private const int Total = 11;
    private const int CenterIndex = 5;

    // Base colors persistantes par cercle
    private readonly Color[] _playerBaseColors = new Color[Total];
    private readonly Color[] _enemyBaseColors = new Color[Total];

    // Empêcher pulses concurrents sur un même Image
    private readonly Dictionary<Image, Coroutine> _pulseByImage = new Dictionary<Image, Coroutine>();

    private void OnValidate()
    {
        ClampAndWarn(playerCircles, "PLAYER");
        ClampAndWarn(enemyCircles, "ENEMY");

        playerInitialDeckCount = Mathf.Max(1, playerInitialDeckCount);
        enemyInitialDeckCount = Mathf.Max(1, enemyInitialDeckCount);

        playerCurrentDeckCount = Mathf.Clamp(playerCurrentDeckCount, 0, int.MaxValue);
        enemyCurrentDeckCount = Mathf.Clamp(enemyCurrentDeckCount, 0, int.MaxValue);

        playerHealthyAccentIndex = Mathf.Clamp(playerHealthyAccentIndex, 0, Total - 1);
        enemyHealthyAccentIndex = Mathf.Clamp(enemyHealthyAccentIndex, 0, Total - 1);

        EnsureBaseArraysInitialized();
        RefreshAll();
    }

    private void Awake()
    {
        playerInitialDeckCount = Mathf.Max(1, playerInitialDeckCount);
        enemyInitialDeckCount = Mathf.Max(1, enemyInitialDeckCount);

        playerCurrentDeckCount = Mathf.Max(playerCurrentDeckCount, 0);
        enemyCurrentDeckCount = Mathf.Max(enemyCurrentDeckCount, 0);

        playerHealthyAccentIndex = Mathf.Clamp(playerHealthyAccentIndex, 0, Total - 1);
        enemyHealthyAccentIndex = Mathf.Clamp(enemyHealthyAccentIndex, 0, Total - 1);

        EnsureBaseArraysInitialized();
        RefreshAll();
    }

    private void OnEnable()
    {
        if (pulseEnabled) StartPulses();
    }

    private void OnDisable()
    {
        StopPulses();
    }

    // ---------- PUBLIC API (PLAYER) ----------
    public void SetInitial(int initial)
    {
        playerInitialDeckCount = Mathf.Max(1, initial);
        playerCurrentDeckCount = Mathf.Max(playerCurrentDeckCount, 0);
        RefreshPlayer();
    }

    public void SetCurrent(int current)
    {
        playerCurrentDeckCount = Mathf.Max(current, 0);
        RefreshPlayer();
        // Option: TriggerOneShotPulseBoth();
    }

    public void DeltaCurrentPlayer(int delta) => SetCurrent(playerCurrentDeckCount + delta);
    public void DeltaCurrentEnemy(int delta) => SetEnemyCurrent(enemyCurrentDeckCount + delta);

    // ---------- PUBLIC API (ENEMY) ----------
    public void SetEnemyInitial(int initial)
    {
        enemyInitialDeckCount = Mathf.Max(1, initial);
        enemyCurrentDeckCount = Mathf.Max(enemyCurrentDeckCount, 0);
        RefreshEnemy();
    }

    public void SetEnemyCurrent(int current)
    {
        enemyCurrentDeckCount = Mathf.Max(current, 0);
        RefreshEnemy();
        // Option: TriggerOneShotPulseBoth();
    }

    // ---------- PUBLIC API (Pulses) ----------
    public void StartPulses()
    {
        pulseEnabled = true;

        if (_coPlayerPulse != null) StopCoroutine(_coPlayerPulse);
        if (_coEnemyPulse != null) StopCoroutine(_coEnemyPulse);

        if (playerCircles != null && playerCircles.Count == Total)
            _coPlayerPulse = StartCoroutine(DualPulseLoop(playerCircles, playerPulseDirection));
        if (enemyCircles != null && enemyCircles.Count == Total)
            _coEnemyPulse = StartCoroutine(DualPulseLoop(enemyCircles, enemyPulseDirection));
    }

    public void StopPulses()
    {
        pulseEnabled = false;

        if (_coPlayerPulse != null) { StopCoroutine(_coPlayerPulse); _coPlayerPulse = null; }
        if (_coEnemyPulse != null) { StopCoroutine(_coEnemyPulse); _coEnemyPulse = null; }

        foreach (var kv in _pulseByImage)
            if (kv.Value != null) StopCoroutine(kv.Value);
        _pulseByImage.Clear();
    }

    public void TriggerOneShotPulseBoth()
    {
        if (playerCircles != null && playerCircles.Count == Total)
            StartCoroutine(OneShotWave(playerCircles, playerPulseDirection));
        if (enemyCircles != null && enemyCircles.Count == Total)
            StartCoroutine(OneShotWave(enemyCircles, enemyPulseDirection));
    }

    // ---------- REFRESH ----------
    public void RefreshAll()
    {
        RefreshOne(playerCircles, playerCenterCountText,
                   playerInitialDeckCount, playerCurrentDeckCount,
                   playerEnableHealthyAccent, playerHealthyAccentIndex, playerHealthyAccentColor);

        RefreshOne(enemyCircles, enemyCenterCountText,
                   enemyInitialDeckCount, enemyCurrentDeckCount,
                   enemyEnableHealthyAccent, enemyHealthyAccentIndex, enemyHealthyAccentColor);
    }

    public void RefreshPlayer()
    {
        RefreshOne(playerCircles, playerCenterCountText,
                   playerInitialDeckCount, playerCurrentDeckCount,
                   playerEnableHealthyAccent, playerHealthyAccentIndex, playerHealthyAccentColor);
    }

    public void RefreshEnemy()
    {
        RefreshOne(enemyCircles, enemyCenterCountText,
                   enemyInitialDeckCount, enemyCurrentDeckCount,
                   enemyEnableHealthyAccent, enemyHealthyAccentIndex, enemyHealthyAccentColor);
    }

    // ---------- Core (one side) ----------
    private void RefreshOne(
        List<Image> circles,
        TMP_Text centerText,
        int initialCount,
        int currentCount,
        bool enableAccent,
        int accentIndex,
        Color accentColor)
    {
        if (circles == null || circles.Count != Total) return;

        float ratioRaw = initialCount > 0 ? (float)currentCount / initialCount : 0f;
        float ratio = Mathf.Clamp01(ratioRaw);

        int segments = Mathf.CeilToInt(ratio * 6f);
        segments = Mathf.Clamp(segments, 1, 6);

        SetCircleAlpha(circles, CenterIndex, true);
        int pairsToShow = segments - 1;
        for (int i = 1; i <= 5; i++)
        {
            bool show = i <= pairsToShow;
            SetCircleAlpha(circles, CenterIndex - i, show);
            SetCircleAlpha(circles, CenterIndex + i, show);
        }

        Color baseColor;
        if (ratioRaw >= (9f / 6f)) baseColor = overcapColor;
        else if (ratio >= (3f / 6f)) baseColor = healthyColor;
        else if (ratio < (2f / 6f)) baseColor = dangerColor;
        else baseColor = warningColor;

        ApplyColorToAll(circles, baseColor);

        bool isHealthy = (ratio >= (3f / 6f)) && (ratioRaw < (9f / 6f));
        if (enableAccent && isHealthy)
            TintSingleKeepAlpha(circles, accentIndex, accentColor);

        if (centerText != null)
            centerText.SetText(currentCount.ToString());
    }

    // ---------- Pulses (implémentation) ----------
    private IEnumerator DualPulseLoop(List<Image> circles, PulseDirection direction)
    {
        var waitInterval = new WaitForSeconds(pulseIntervalSeconds);

        int[] order = new int[Total];
        for (int i = 0; i < Total; i++) order[i] = i;

        while (true)
        {
            yield return waitInterval;

            if (circles == null || circles.Count != Total) continue;

            System.Func<int, int> map = (i) =>
                direction == PulseDirection.LeftToRight ? order[i] : order[Total - 1 - i];

            for (int step = 0; step < 5; step++)
            {
                int left = map(step);
                int right = map(Total - 1 - step);

                StartPulseOnImage(circles[left]);
                StartPulseOnImage(circles[right]);

                yield return new WaitForSeconds(pulseStepDelay);
            }

            int center = map(CenterIndex);
            yield return StartCoroutine(PulseOne(circles[center]));
        }
    }

    private IEnumerator OneShotWave(List<Image> circles, PulseDirection direction)
    {
        if (circles == null || circles.Count != Total) yield break;

        System.Func<int, int> map = (i) =>
            direction == PulseDirection.LeftToRight ? i : (Total - 1 - i);

        for (int step = 0; step < 5; step++)
        {
            int left = map(step);
            int right = map(Total - 1 - step);

            StartPulseOnImage(circles[left]);
            StartPulseOnImage(circles[right]);

            yield return new WaitForSeconds(pulseStepDelay);
        }

        int center = map(CenterIndex);
        yield return StartCoroutine(PulseOne(circles[center]));
    }

    private Coroutine StartPulseOnImage(Image img)
    {
        if (img == null) return null;

        if (_pulseByImage.TryGetValue(img, out var running) && running != null)
            StopCoroutine(running); // éviter un reset tardif vers une ancienne base

        var co = StartCoroutine(PulseOne(img));
        _pulseByImage[img] = co;
        return co;
    }

    private IEnumerator PulseOne(Image img)
    {
        if (img == null) yield break;

        if (!pulseAffectsHidden && img.color.a <= hiddenAlpha + 0.0001f)
            yield break;

        float t = 0f;
        while (t < pulseRiseTime)
        {
            t += Time.deltaTime;
            float k = (pulseRiseTime <= 0f) ? 1f : Mathf.Clamp01(t / pulseRiseTime);
            float e = pulseCurve != null ? pulseCurve.Evaluate(k) : k;

            var baseCol = GetCurrentBaseColor(img);
            img.color = BrightenHSVTowardsWhite(baseCol, e, pulseWhiteLerp, pulseSatBoost, pulseValBoost);
            yield return null;
        }

        t = 0f;
        while (t < pulseFallTime)
        {
            t += Time.deltaTime;
            float k = (pulseFallTime <= 0f) ? 1f : Mathf.Clamp01(t / pulseFallTime);
            float e = pulseCurve != null ? pulseCurve.Evaluate(1f - k) : (1f - k);

            var baseCol = GetCurrentBaseColor(img);
            img.color = BrightenHSVTowardsWhite(baseCol, e, pulseWhiteLerp, pulseSatBoost, pulseValBoost);
            yield return null;
        }

        img.color = GetCurrentBaseColor(img);

        if (_pulseByImage.ContainsKey(img))
            _pulseByImage[img] = null;
    }

    private static Color BrightenHSVTowardsWhite(Color baseCol, float strength, float whiteLerp, float satBoost, float valBoost)
    {
        strength = Mathf.Clamp01(strength);

        Color.RGBToHSV(baseCol, out float h, out float s, out float v);
        float sTarget = Mathf.Clamp01(s + satBoost);
        float vTarget = Mathf.Clamp01(v + valBoost);

        float sOut = Mathf.Lerp(s, sTarget, strength);
        float vOut = Mathf.Lerp(v, vTarget, strength);
        Color hsvBoosted = Color.HSVToRGB(h, sOut, vOut);
        hsvBoosted.a = baseCol.a;

        Color towardsWhite = Color.Lerp(hsvBoosted, Color.white, whiteLerp * strength);
        towardsWhite.a = baseCol.a;

        return towardsWhite;
    }

    // ---------- Helpers ----------
    private void EnsureBaseArraysInitialized()
    {
        InitBaseArrayFromImages(playerCircles, _playerBaseColors);
        InitBaseArrayFromImages(enemyCircles, _enemyBaseColors);
    }

    private void InitBaseArrayFromImages(List<Image> circles, Color[] baseArray)
    {
        if (circles == null || baseArray == null || circles.Count != Total) return;
        for (int i = 0; i < Total; i++)
        {
            var img = circles[i];
            baseArray[i] = img ? img.color : Color.white;
        }
    }

    private void ClampAndWarn(List<Image> list, string label)
    {
        if (list != null && list.Count != Total)
            Debug.LogWarning($"[LifeGauge/{label}] Need exactly {Total} images, got {list.Count}.");
    }

    private void SetCircleAlpha(List<Image> list, int index, bool visible)
    {
        if (index < 0 || index >= Total) return;
        var img = list[index];
        if (!img) return;
        var c = img.color;
        c.a = visible ? visibleAlpha : hiddenAlpha;
        img.color = c;
    }

    private void ApplyColorToAll(List<Image> list, Color baseColor)
    {
        var baseArray = GetBaseArrayForList(list);
        for (int i = 0; i < list.Count; i++)
        {
            var img = list[i];
            if (!img) continue;

            var c = img.color; // conserve alpha
            c.r = baseColor.r; c.g = baseColor.g; c.b = baseColor.b;
            img.color = c;

            if (baseArray != null)
            {
                var b = c; b.a = 1f; // stocker la base en opaque
                baseArray[i] = b;
            }
        }
    }

    private void TintSingleKeepAlpha(List<Image> list, int index, Color tint)
    {
        if (index < 0 || index >= Total) return;
        var img = list[index];
        if (!img) return;

        var c = img.color; // conserve alpha
        c.r = tint.r; c.g = tint.g; c.b = tint.b;
        img.color = c;

        var baseArray = GetBaseArrayForList(list);
        if (baseArray != null)
        {
            var b = c; b.a = 1f;
            baseArray[index] = b;
        }
    }

    private Color[] GetBaseArrayForList(List<Image> list)
    {
        if (list == playerCircles) return _playerBaseColors;
        if (list == enemyCircles) return _enemyBaseColors;
        return null;
    }

    private bool TryGetIndexAndBase(Image img, out int idx, out Color baseColor)
    {
        idx = -1; baseColor = Color.white;

        if (playerCircles != null)
        {
            idx = playerCircles.IndexOf(img);
            if (idx >= 0) { baseColor = WithAlpha(_playerBaseColors[idx], img.color.a); return true; }
        }
        if (enemyCircles != null)
        {
            idx = enemyCircles.IndexOf(img);
            if (idx >= 0) { baseColor = WithAlpha(_enemyBaseColors[idx], img.color.a); return true; }
        }
        return false;
    }

    private Color GetCurrentBaseColor(Image img)
    {
        if (TryGetIndexAndBase(img, out _, out var baseCol)) return baseCol;
        return img.color;
    }

    private static Color WithAlpha(Color c, float a) { c.a = a; return c; }
}
