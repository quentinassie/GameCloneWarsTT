// Assets/Scripts/UI/BalloonClickGlow.cs  (ajout: appel du FX lucioles)
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public sealed class BalloonClickGlow : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image balloonImage;
    [SerializeField] private TMP_Text glowTMP;
    [SerializeField] private Button button;
    [SerializeField] private bool autoBindButton = true;

    [Header("Preset Lock (optional)")]
    [SerializeField] private KeepGlowAlive presetLock;

    [Header("Glow shader (TMP SDF)")]
    [SerializeField] private string glowKeyword = "GLOW_ON";
    [SerializeField] private string glowPowerProp = "_GlowPower"; // fallback

    [Header("Auto-restore")]
    [Min(0f)][SerializeField] private float autoRestoreSeconds = 1.2f;

    [Header("Click FX (fireflies)")]
    [SerializeField] private FireFlyBurstFX fireflyFX; // ← drag & drop ton spawner

    private Material _runtimeMat;
    private float _initGlowPower = 1f;
    private Color _initBalloonColor;
    private Coroutine _restoreCo;

    private void Reset()
    {
        if (!balloonImage) balloonImage = GetComponent<Image>();
        if (!glowTMP) glowTMP = GetComponentInChildren<TMP_Text>(true);
        if (!button) button = GetComponent<Button>();
    }

    private void Awake()
    {
        if (!balloonImage) balloonImage = GetComponent<Image>();
        if (!glowTMP) glowTMP = GetComponentInChildren<TMP_Text>(true);
        if (!button && autoBindButton) button = GetComponent<Button>();
        if (button && autoBindButton) button.onClick.AddListener(OnButtonClick);

        if (glowTMP)
        {
            var shared = glowTMP.fontSharedMaterial;
            if (shared != null)
            {
                _runtimeMat = new Material(shared) { name = shared.name + " (BalloonClickGlow)" };
                if (!string.IsNullOrEmpty(glowPowerProp) && shared.HasProperty(glowPowerProp))
                    _initGlowPower = shared.GetFloat(glowPowerProp);
            }
        }

        if (balloonImage) _initBalloonColor = balloonImage.color;
    }

    public void OnButtonClick()
    {
        if (!glowTMP || _runtimeMat == null) return;

        // 1) FX lucioles à la position du curseur
        if (fireflyFX) fireflyFX.SpawnBurstAtScreenPos(Input.mousePosition);

        // 2) suspend preset lock et bascule mat runtime
        if (presetLock) presetLock.SuspendLock(true);
        glowTMP.fontMaterial = _runtimeMat;
        glowTMP.SetMaterialDirty();

        // 3) cache l’image du ballon
        if (balloonImage)
        {
            var c = balloonImage.color; c.a = 0f; balloonImage.color = c;
        }

        // 4) coupe le glow instant (keyword ou fallback power)
        bool didKeyword = false;
        if (!string.IsNullOrEmpty(glowKeyword))
        {
            _runtimeMat.DisableKeyword(glowKeyword);
            didKeyword = true;
        }
        if (!didKeyword && !string.IsNullOrEmpty(glowPowerProp) && _runtimeMat.HasProperty(glowPowerProp))
        {
            _runtimeMat.SetFloat(glowPowerProp, 0f);
        }
        glowTMP.UpdateMeshPadding();
        glowTMP.SetMaterialDirty();

        // 5) auto-restore
        if (autoRestoreSeconds > 0f)
        {
            if (_restoreCo != null) StopCoroutine(_restoreCo);
            _restoreCo = StartCoroutine(DelayedRestore(autoRestoreSeconds));
        }
    }

    public void ResetVisuals()
    {
        if (balloonImage) balloonImage.color = _initBalloonColor;

        if (_runtimeMat != null && glowTMP)
        {
            if (!string.IsNullOrEmpty(glowKeyword))
                _runtimeMat.EnableKeyword(glowKeyword);
            else if (!string.IsNullOrEmpty(glowPowerProp) && _runtimeMat.HasProperty(glowPowerProp))
                _runtimeMat.SetFloat(glowPowerProp, _initGlowPower);

            glowTMP.UpdateMeshPadding();
            glowTMP.SetMaterialDirty();
        }

        if (_restoreCo != null) { StopCoroutine(_restoreCo); _restoreCo = null; }

        if (presetLock && presetLock.IsSuspended)
        {
            presetLock.SuspendLock(false);
            presetLock.ApplyPresetNow();
        }
    }

    private System.Collections.IEnumerator DelayedRestore(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetVisuals();
    }
}
