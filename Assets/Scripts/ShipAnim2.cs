// Assets/Scripts/UI/ShipUIVerticalWithLeftDrift.cs
using UnityEngine;

/// <summary>
/// Mouvement UI: monte en Y (bas -> haut) avec un drift gauche en X ajustable.
/// Conçu pour RectTransform sous Canvas (Screen Space - Overlay).
/// </summary>
[DisallowMultipleComponent]
public sealed class ShipAnim2 : MonoBehaviour
{
    [Header("Target (UI)")]
    [SerializeField] private RectTransform ship; // Assigné auto si manquant

    [Header("Motion (px/s)")]
    [Min(0f)]
    [Tooltip("Vitesse ascendante (Y+), en px/s, espace parent.")]
    [SerializeField] private float speedY = 240f;

    [Min(0f)]
    [Tooltip("Drift vers la gauche (X-), en px/s. 0 = aucun drift.")]
    [SerializeField] private float leftDriftX = 30f;

    [Header("Start Position (Local XY)")]
    [Tooltip("Position locale de départ (placez en bas de l'écran).")]
    [SerializeField] private Vector2 startLocalPos = new Vector2(0f, -300f);

    [Header("Timing")]
    [Min(0f)]
    [Tooltip("Délai avant le premier départ (s).")]
    [SerializeField] private float startDelaySeconds = 0f;

    [Min(0.1f)]
    [Tooltip("Durée d'une traversée active (s).")]
    [SerializeField] private float moveDurationSeconds = 6f;

    [Min(0.1f)]
    [Tooltip("Période de répétition (s).")]
    [SerializeField] private float periodSeconds = 60f;

    [Header("Start/Play")]
    [Tooltip("Lancer automatiquement à OnEnable.")]
    [SerializeField] private bool playOnEnable = true;

    private Coroutine _loop;

    private void Reset()
    {
        if (!ship) ship = GetComponent<RectTransform>();
    }

    private void Awake()
    {
        if (!ship) ship = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (playOnEnable) Play();
    }

    private void OnDisable()
    {
        Stop();
    }

    /// <summary> Démarre/relance la boucle de mouvement. </summary>
    public void Play()
    {
        if (!ship) return;
        if (_loop != null) StopCoroutine(_loop);
        _loop = StartCoroutine(RunLoop());
    }

    /// <summary> Arrête la boucle. </summary>
    public void Stop()
    {
        if (_loop != null) { StopCoroutine(_loop); _loop = null; }
    }

    private System.Collections.IEnumerator RunLoop()
    {
        if (startDelaySeconds > 0f)
            yield return new WaitForSecondsRealtime(startDelaySeconds);

        while (true)
        {
            // Reset départ (XY uniquement)
            SetLocalXY(ship, startLocalPos);

            float t = 0f;
            while (t < moveDurationSeconds)
            {
                float dt = Time.unscaledDeltaTime;
                t += dt;

                // Avance en Y+ (haut) et drift en X- (gauche)
                Vector2 p = GetLocalXY(ship);
                p.y += speedY * dt;
                if (leftDriftX > 0f) p.x -= leftDriftX * dt;
                SetLocalXY(ship, p);

                yield return null;
            }

            // Respecte la période globale
            float wait = Mathf.Max(0f, periodSeconds - moveDurationSeconds);
            if (wait > 0f) yield return new WaitForSecondsRealtime(wait);
        }
    }

    // Helpers: manipule XY en espace parent, conserve Z.
    private static Vector2 GetLocalXY(RectTransform rt)
    {
        var lp = rt.localPosition;
        return new Vector2(lp.x, lp.y);
    }

    private static void SetLocalXY(RectTransform rt, Vector2 xy)
    {
        var lp = rt.localPosition;
        rt.localPosition = new Vector3(xy.x, xy.y, lp.z);
    }
}
