// Assets/Scripts/UI/ShipUIAngleMotion.cs
using UnityEngine;

/// <summary>
/// Mouvement UI: déplacement linéaire d'un RectTransform selon un angle (radians, cercle trigo).
/// Conçu pour Canvas Screen Space - Overlay. Utilise Time.unscaledDeltaTime.
/// </summary>
[DisallowMultipleComponent]
public sealed class ShipAnim3 : MonoBehaviour
{
    [Header("Target (UI)")]
    [SerializeField] private RectTransform ship; // auto-assign si manquant

    [Header("Direction")]
    [Tooltip("Angle en radians (cercle trigo, 0=+X, PI/2=+Y). Ex: 2.67 ≈ 153°, haut-gauche.")]
    [SerializeField] private float angleRadians = 2.67f;
    [Tooltip("Inverse le sens (utile pour tests).")]
    [SerializeField] private bool invertDirection = false;

    [Header("Motion")]
    [Min(0f)]
    [Tooltip("Vitesse linéaire (px/s) en espace parent.")]
    [SerializeField] private float speed = 240f;

    [Header("Start Position (Local XY)")]
    [Tooltip("Position locale de départ (placez en bas-droite).")]
    [SerializeField] private Vector2 startLocalPos = new Vector2(420f, -300f);

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

    /// <summary> Arrête la boucle de mouvement. </summary>
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
            SetLocalXY(ship, startLocalPos);

            // direction unité depuis angle (radians)
            Vector2 dir = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
            if (dir.sqrMagnitude > 1e-10f) dir.Normalize();
            if (invertDirection) dir = -dir;

            float t = 0f;
            while (t < moveDurationSeconds)
            {
                float dt = Time.unscaledDeltaTime;
                t += dt;

                Vector2 p = GetLocalXY(ship);
                p += dir * (speed * dt);
                SetLocalXY(ship, p);

                yield return null;
            }

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
