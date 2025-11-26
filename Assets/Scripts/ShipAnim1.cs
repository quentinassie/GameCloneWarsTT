// Assets/Scripts/UI/ShipUILinearPass.cs
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ShipAnim1 : MonoBehaviour
{
    public enum MovementAxis { Right, Up }

    [Header("Target (UI)")]
    [SerializeField] private RectTransform ship;       // ← ce GameObject (SHIP) avec tes images en enfants

    [Header("Direction")]
    [SerializeField] private bool useObjectRotation = true; // prend ship.eulerAngles.z au moment du départ
    [SerializeField] private float angleZDeg = 0f;          // si !useObjectRotation
    [SerializeField] private MovementAxis axis = MovementAxis.Right; // visuel orienté Right ou Up ?
    [SerializeField] private bool invertDirection = false;  // inverse le sens si besoin

    [Header("Motion")]
    [Min(0f)][SerializeField] private float speed = 240f;   // px/s (en espace parent)
    [SerializeField] private Vector2 startLocalPos = new Vector2(-250f, 90f);

    [Header("Timing")]
    [Min(0f)][SerializeField] private float startDelaySeconds = 0f;     // phase (avant 1er départ)
    [Min(0.1f)][SerializeField] private float moveDurationSeconds = 6f; // temps de traversée actif
    [Min(0.1f)][SerializeField] private float periodSeconds = 60f;      // répétition (toutes les X s)

    [Header("Start/Play")]
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

    public void Play()
    {
        if (!ship) return;
        if (_loop != null) StopCoroutine(_loop);
        _loop = StartCoroutine(RunLoop());
    }

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
            // reset au départ (XY uniquement)
            SetLocalXY(ship, startLocalPos);

            // direction depuis l’angle Z
            Vector2 dir = ComputeDir2D();
            if (invertDirection) dir = -dir;

            // mouvement pendant moveDurationSeconds
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

            // attente pour respecter la période
            float wait = Mathf.Max(0f, periodSeconds - moveDurationSeconds);
            if (wait > 0f) yield return new WaitForSecondsRealtime(wait);
        }
    }

    private Vector2 ComputeDir2D()
    {
        // angle utilisé
        float z = useObjectRotation ? ship.eulerAngles.z : angleZDeg;
        float rad = z * Mathf.Deg2Rad;

        // Axe Right = (cos, sin). Axe Up = rotation +90° => (-sin, cos).
        Vector2 d = (axis == MovementAxis.Right)
            ? new Vector2(Mathf.Cos(rad), Mathf.Sin(rad))
            : new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));

        return d.sqrMagnitude > 1e-8f ? d.normalized : Vector2.right;
    }

    // Helpers: on évite anchoredPosition, on bouge en espace parent (fiable)
    private static Vector2 GetLocalXY(RectTransform rt)
    {
        var lp = rt.localPosition;
        return new Vector2(lp.x, lp.y);
    }
    private static void SetLocalXY(RectTransform rt, Vector2 xy)
    {
        var lp = rt.localPosition;
        rt.localPosition = new Vector3(xy.x, xy.y, lp.z); // conserve Z
    }
}
