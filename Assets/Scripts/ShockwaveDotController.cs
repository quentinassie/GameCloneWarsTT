// Assets/Scripts/UI/FX/ShockwaveDotController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ShockwaveDotController : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private TMP_Text playerDot;
    [SerializeField] private TMP_Text enemyDot;

    [Header("Glow Colors (per DOT)")]
    [SerializeField] private Color32 playerGlowColor = new Color32(0, 180, 255, 128);
    [SerializeField] private Color32 enemyGlowColor = new Color32(255, 158, 0, 128);
    [SerializeField, Range(0f, 1f)] private float innerValue = 0.50f;
    [SerializeField, Range(0f, 1f)] private float outerValue = 0.18f;

    [Header("Isolation Scope")]
    [SerializeField] private Transform isolateRoot;
    [SerializeField] private bool isolateOthers = true;
    [SerializeField] private bool continuousIsolation = true;
    [SerializeField, Min(0.05f)] private float isolationInterval = 0.5f;

    [Header("Timing")]
    [SerializeField] private float flashInDuration = 0.12f;
    [SerializeField] private float shockDuration = 0.60f;
    [SerializeField] private float fadeOutDuration = 0.40f;
    [SerializeField] private bool useRealtime = true;

    [Header("Wave Params")]
    [SerializeField, Range(0f, 1f)] private float startOffset = 0f;
    [SerializeField, Range(0f, 1f)] private float endOffset = 1f;
    [SerializeField, Range(0.6f, 1f)] private float maxOffsetSafe = 0.90f;
    [SerializeField, Range(0f, 1f)] private float startPower = 0.30f;
    [SerializeField, Range(0f, 1f)] private float endPower = 0.18f;

    [Header("Other Texts – Face/Outline")]
    [Tooltip("Active un léger contour sur les autres TMP (Glow reste OFF).")]
    [SerializeField] private bool otherEnableOutline = true;
    [SerializeField, Range(0f, 0.2f)] private float otherOutlineThickness = 0.031f;
    [SerializeField, Range(0f, 1f)] private float otherOutlineSoftness = 0f;
    [SerializeField, Range(0f, 1f)] private float otherFaceSoftness = 0.20f;

    // TMP SDF props
    private static readonly int ID_GlowOffset = Shader.PropertyToID("_GlowOffset");
    private static readonly int ID_GlowPower = Shader.PropertyToID("_GlowPower");
    private static readonly int ID_GlowOuter = Shader.PropertyToID("_GlowOuter");
    private static readonly int ID_GlowInner = Shader.PropertyToID("_GlowInner");
    private static readonly int ID_GlowColor = Shader.PropertyToID("_GlowColor");

    private static readonly int ID_OutlineWidth = Shader.PropertyToID("_OutlineWidth");
    private static readonly int ID_OutlineSoft = Shader.PropertyToID("_OutlineSoftness");
    private static readonly int ID_OutlineColor = Shader.PropertyToID("_OutlineColor"); // (non utilisé ici)
    private static readonly int ID_FaceSoftness = Shader.PropertyToID("_FaceSoftness");
    private static readonly int ID_FaceDilate = Shader.PropertyToID("_FaceDilate");

    private static readonly int ID_UnderlaySoft = Shader.PropertyToID("_UnderlaySoftness");
    private static readonly int ID_UnderlayDil = Shader.PropertyToID("_UnderlayDilate");
    private static readonly int ID_UnderlayOffX = Shader.PropertyToID("_UnderlayOffsetX");
    private static readonly int ID_UnderlayOffY = Shader.PropertyToID("_UnderlayOffsetY");

    private Material _playerMatInstance;
    private Material _enemyMatInstance;
    private readonly HashSet<TMP_Text> _noGlowTexts = new();
    private readonly List<Material> _createdInstances = new();
    private Coroutine _coPlayer, _coEnemy, _coIsolation;

    private void Awake()
    {
        _playerMatInstance = PrepareDot(playerDot, playerGlowColor);
        _enemyMatInstance = PrepareDot(enemyDot, enemyGlowColor);

        if (isolateOthers)
        {
            IsolateGlowForOthers();
            if (continuousIsolation) _coIsolation = StartCoroutine(IsolationWatcher());
        }
    }

    private void OnDestroy()
    {
        if (_coIsolation != null) StopCoroutine(_coIsolation);
        if (_playerMatInstance) Destroy(_playerMatInstance);
        if (_enemyMatInstance) Destroy(_enemyMatInstance);
        foreach (var m in _createdInstances) if (m) Destroy(m);
        _createdInstances.Clear();
        _noGlowTexts.Clear();
    }

    // --- Public API ---
    public void TriggerPlayer()
    {
        if (!playerDot) return;
        if (_coPlayer != null) StopCoroutine(_coPlayer);
        _coPlayer = StartCoroutine(RunSequence(playerDot));
    }
    public void TriggerEnemy()
    {
        if (!enemyDot) return;
        if (_coEnemy != null) StopCoroutine(_coEnemy);
        _coEnemy = StartCoroutine(RunSequence(enemyDot));
    }
    public void TriggerBoth() { TriggerPlayer(); TriggerEnemy(); }

    // --- Core sequence ---
    private IEnumerator RunSequence(TMP_Text dot)
    {
        if (!dot) yield break;

        // flash-in
        float a = 0f;
        while (a < flashInDuration)
        {
            a += Delta();
            float k = Mathf.Clamp01(a / flashInDuration);
            dot.alpha = k;
            dot.SetVerticesDirty();
            yield return null;
        }
        dot.alpha = 1f; dot.SetVerticesDirty();

        // wave
        var mat = dot.fontMaterial;
        if (mat != null)
        {
            float t = 0f;
            float endClamped = Mathf.Min(endOffset, maxOffsetSafe);
            while (t < shockDuration)
            {
                t += Delta();
                float k = Mathf.Clamp01(t / shockDuration);
                float ease = EaseOutCubic(k);

                mat.SetFloat(ID_GlowOffset, Mathf.Lerp(startOffset, endClamped, ease));
                mat.SetFloat(ID_GlowPower, Mathf.Lerp(startPower, endPower, ease));
                dot.SetVerticesDirty();
                yield return null;
            }
            mat.SetFloat(ID_GlowOffset, endClamped);
            mat.SetFloat(ID_GlowPower, endPower);
            dot.SetVerticesDirty();
        }

        // fade-out
        float f = 0f;
        while (f < fadeOutDuration)
        {
            f += Delta();
            float k = Mathf.Clamp01(f / fadeOutDuration);
            dot.alpha = 1f - k;
            dot.SetVerticesDirty();
            yield return null;
        }
        dot.alpha = 0f; dot.SetVerticesDirty();
    }

    // --- Isolation watcher ---
    private IEnumerator IsolationWatcher()
    {
        while (true)
        {
            TryIsolateDelta();
            if (useRealtime) yield return new WaitForSecondsRealtime(isolationInterval);
            else yield return new WaitForSeconds(isolationInterval);
        }
    }

    private void TryIsolateDelta()
    {
        TMP_Text[] all = isolateRoot
            ? isolateRoot.GetComponentsInChildren<TMP_Text>(includeInactive: true)
            : FindObjectsOfType<TMP_Text>(includeInactive: true);

        foreach (var t in all)
        {
            if (!t || t == playerDot || t == enemyDot) continue;

            // si jamais un texte a (re)activé des effets -> re-noGlow + apply Face/Outline
            MakeNoGlowWithFaceOutline(t);
            _noGlowTexts.Add(t);
        }
    }

    private void IsolateGlowForOthers()
    {
        TMP_Text[] all = isolateRoot
            ? isolateRoot.GetComponentsInChildren<TMP_Text>(includeInactive: true)
            : FindObjectsOfType<TMP_Text>(includeInactive: true);

        foreach (var t in all)
        {
            if (!t || t == playerDot || t == enemyDot) continue;
            MakeNoGlowWithFaceOutline(t);
            _noGlowTexts.Add(t);
        }
    }

    private void MakeNoGlowWithFaceOutline(TMP_Text t)
    {
        var baseShared = t.fontSharedMaterial;
        if (baseShared == null) return;

        var inst = new Material(baseShared);
        inst.name = t.name + " (NoGlow+FaceOutline)";
        // Glow & Underlay OFF
        inst.DisableKeyword("GLOW_ON");
        inst.DisableKeyword("UNDERLAY_ON");
        SafeSetFloat(inst, ID_GlowOffset, 0f);
        SafeSetFloat(inst, ID_GlowPower, 0f);
        SafeSetFloat(inst, ID_GlowOuter, 0f);
        SafeSetFloat(inst, ID_GlowInner, 0f);
        SafeSetFloat(inst, ID_UnderlaySoft, 0f);
        SafeSetFloat(inst, ID_UnderlayDil, 0f);
        SafeSetFloat(inst, ID_UnderlayOffX, 0f);
        SafeSetFloat(inst, ID_UnderlayOffY, 0f);

        // Face softness (comme dans ton screenshot)
        SafeSetFloat(inst, ID_FaceSoftness, otherFaceSoftness);
        SafeSetFloat(inst, ID_FaceDilate, 0f); // on garde 0 par défaut

        // Outline (optionnel) – sans Glow
        if (otherEnableOutline && otherOutlineThickness > 0f)
        {
            inst.EnableKeyword("OUTLINE_ON");
            SafeSetFloat(inst, ID_OutlineWidth, otherOutlineThickness);
            SafeSetFloat(inst, ID_OutlineSoft, otherOutlineSoftness);
            // couleur: on laisse celle du preset; possible d’exposer un Color si besoin
        }
        else
        {
            inst.DisableKeyword("OUTLINE_ON");
            SafeSetFloat(inst, ID_OutlineWidth, 0f);
            SafeSetFloat(inst, ID_OutlineSoft, 0f);
        }

        t.fontMaterial = inst;
        t.extraPadding = false;
        if (t is TextMeshProUGUI ug) ug.maskable = true;

        t.SetVerticesDirty();
        _createdInstances.Add(inst);
    }

    // --- Prep DOT ---
    private Material PrepareDot(TMP_Text txt, Color32 glowColor)
    {
        if (!txt) return null;

        var inst = new Material(txt.fontSharedMaterial);
        inst.name = txt.name + " (DOT_Glow_ON)";
        txt.fontMaterial = inst;

        inst.EnableKeyword("GLOW_ON");
        inst.DisableKeyword("OUTLINE_ON");
        inst.DisableKeyword("UNDERLAY_ON");

        SafeSetFloat(inst, ID_OutlineWidth, 0f);
        SafeSetFloat(inst, ID_OutlineSoft, 0f);
        SafeSetFloat(inst, ID_FaceSoftness, 0f);
        SafeSetFloat(inst, ID_FaceDilate, 0f);
        SafeSetFloat(inst, ID_UnderlaySoft, 0f);
        SafeSetFloat(inst, ID_UnderlayDil, 0f);
        SafeSetFloat(inst, ID_UnderlayOffX, 0f);
        SafeSetFloat(inst, ID_UnderlayOffY, 0f);

        SafeSetFloat(inst, ID_GlowOffset, startOffset);
        SafeSetFloat(inst, ID_GlowPower, startPower);
        SafeSetFloat(inst, ID_GlowInner, innerValue);
        SafeSetFloat(inst, ID_GlowOuter, outerValue);
        if (inst.HasProperty(ID_GlowColor)) inst.SetColor(ID_GlowColor, glowColor);

        txt.extraPadding = true;                 // halo DOT non clippé
        if (txt is TextMeshProUGUI ugui) ugui.maskable = false;

        txt.alpha = 0f;
        txt.SetVerticesDirty();

        _createdInstances.Add(inst);
        return inst;
    }

    // --- Utils ---
    private static void SafeSetFloat(Material m, int id, float v) { if (m.HasProperty(id)) m.SetFloat(id, v); }
    private float Delta() => useRealtime ? Time.unscaledDeltaTime : Time.deltaTime;
    private static float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
}
