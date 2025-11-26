// Assets/Scripts/UI/TMPForcePreset.cs
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public sealed class KeepGlowAlive : MonoBehaviour
{
    [Header("TMP & Preset")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private Material preset;

    [Header("Options")]
    [SerializeField] private bool applyEveryFrame = true;
    [SerializeField] private bool warnOnChange = true;

    private Material _lastApplied;
    private bool _suspended; // quand vrai: on n’écrase plus le matériau

    private void Reset() { text = GetComponent<TMP_Text>(); }

    private void Awake()
    {
        if (!text) text = GetComponent<TMP_Text>();
        ApplyPresetNow();
    }

    private void OnEnable() { ApplyPresetNow(); }

    private void LateUpdate()
    {
        if (_suspended || !applyEveryFrame || !text || !preset) return;

        if (!ReferenceEquals(text.fontSharedMaterial, preset) ||
            text.materialForRendering.shader != preset.shader)
        {
            if (warnOnChange && _lastApplied && !ReferenceEquals(_lastApplied, text.fontSharedMaterial))
            {
                // Debug.LogWarning($"[KeepGlowAlive] Material remplacé sur '{text.name}'. Réapplication du preset '{preset.name}'.");
            }
            ApplyPresetNow();
        }
    }

    public void ApplyPresetNow()
    {
        if (!text || !preset) return;
        text.fontSharedMaterial = preset;
        text.material = preset;
        text.UpdateMeshPadding();
        text.SetMaterialDirty();
        _lastApplied = preset;
    }

    /// <summary>Bloque / débloque le relock du preset (utilisé par un effet runtime).</summary>
    public void SuspendLock(bool suspend) => _suspended = suspend;

    /// <summary>Pratique pour savoir si le lock est suspendu.</summary>
    public bool IsSuspended => _suspended;
}
