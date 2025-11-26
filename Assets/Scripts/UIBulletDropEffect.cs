// Assets/Scripts/UIBulletDropEffect.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("FX/UI Bullet Drop Effect (No EventSystem)")]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public sealed class UIBulletDropEffect : MonoBehaviour
{
    [Header("Bubble")]
    public Color bubbleColor = new Color(0.75f, 0.95f, 1f, 0.9f);
    [Range(8f, 256f)] public float bubbleMaxSize = 56f;
    [Range(0.05f, 1.2f)] public float bubbleDuration = 0.45f;
    public Sprite bubbleSprite; // si null -> prend l'Image hôte

    [Header("Ring")]
    public Color ringColor = new Color(0.75f, 0.95f, 1f, 0.5f);
    [Range(8f, 512f)] public float ringMaxSize = 128f;
    [Range(0.05f, 1.2f)] public float ringDuration = 0.55f;
    public Sprite ringSprite; // si null -> bubbleSprite

    [Header("Behavior")]
    [Range(0, 5)] public int bubblesPerClick = 1;
    [Range(0f, 32f)] public float spawnJitter = 6f;
    [Range(0f, 32f)] public float risePixels = 8f;

    [Header("Hierarchy")]
    public RectTransform parentOverride; // par défaut: ce RectTransform

    private RectTransform _self;
    private Image _selfImage;
    private Canvas _canvas;
    private Camera _eventCam; // null en Overlay

    // --- Auto-destruction locale, robuste même si le spawner est désactivé ---
    private sealed class AutoKillAfter : MonoBehaviour
    {
        public Transform requiredParent;
        public float seconds = 0.5f;
        public bool useRealtime = true;

        private float _t0;
        public void Init(Transform parent, float sec, bool realtime)
        {
            requiredParent = parent;
            seconds = sec;
            useRealtime = realtime;
            _t0 = useRealtime ? Time.realtimeSinceStartup : Time.time;
        }

        private void OnEnable()
        {
            // sécurité si ajouté sans Init
            if (_t0 <= 0f) _t0 = useRealtime ? Time.realtimeSinceStartup : Time.time;
        }

        private void Update()
        {
            float now = useRealtime ? Time.realtimeSinceStartup : Time.time;
            if (now - _t0 < seconds) return;

            // WHY: ne détruire que si toujours enfant du parent "fluid"
            if (requiredParent == null || transform.parent == requiredParent)
            {
                Destroy(gameObject);
            }
            else
            {
                // a été réparenté: on n'impose plus le kill
                Destroy(this);
            }
        }
    }

    private void Awake()
    {
        _self = GetComponent<RectTransform>();
        _selfImage = GetComponent<Image>();
        _canvas = GetComponentInParent<Canvas>();

        if (_canvas != null)
            _eventCam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

        if (_selfImage != null) _selfImage.raycastTarget = true;

        if (bubbleSprite == null && _selfImage != null) bubbleSprite = _selfImage.sprite;
        if (ringSprite == null) ringSprite = bubbleSprite;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryManualHit(Input.mousePosition);

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
                TryManualHit(t.position);
        }
    }

    private void TryManualHit(Vector2 screenPos)
    {
        RectTransform parent = parentOverride != null ? parentOverride : _self;
        if (RectTransformUtility.RectangleContainsScreenPoint(_self, screenPos, _eventCam) &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPos, _eventCam, out Vector2 local))
        {
            SpawnAtLocal(parent, local);
        }
    }

    private void SpawnAtLocal(RectTransform parent, Vector2 local)
    {
        int count = bubblesPerClick < 1 ? 1 : bubblesPerClick;
        for (int i = 0; i < count; i++)
        {
            Vector2 p = local + (Vector2)Random.insideUnitCircle * spawnJitter;
            SpawnBubble(parent, p);
            SpawnRing(parent, p);
        }
    }

    private void SpawnBubble(RectTransform parent, Vector2 localPos)
    {
        GameObject go = new GameObject("Bubble", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchoredPosition = localPos;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = Vector2.one * (bubbleMaxSize * 0.35f);
        rt.localScale = Vector3.one * 0.6f;

        Image img = go.GetComponent<Image>();
        img.sprite = bubbleSprite;
        img.raycastTarget = false;
        img.color = bubbleColor;

        StartCoroutine(AnimBubble(img, rt));
    }

    private void SpawnRing(RectTransform parent, Vector2 localPos)
    {
        GameObject go = new GameObject("BubbleRing", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchoredPosition = localPos;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = Vector2.one * (bubbleMaxSize * 0.25f);
        rt.localScale = Vector3.one * 0.4f;

        Image img = go.GetComponent<Image>();
        img.sprite = ringSprite;
        img.raycastTarget = false;
        img.color = ringColor;

        // Anim
        StartCoroutine(AnimRing(img, rt));

        // Watchdog autonome sur l'objet (0.5s non-scalé)
        var watchdog = go.AddComponent<AutoKillAfter>();
        watchdog.Init(parent, 0.5f, true);
    }

    private IEnumerator AnimBubble(Image img, RectTransform rt)
    {
        float t = 0f;
        float dur = Mathf.Max(0.02f, bubbleDuration);
        float startScale = 0.6f;
        float peakScale = 1.1f;
        float endScale = 1.0f;

        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, risePixels);

        Color c0 = bubbleColor; c0.a = bubbleColor.a;
        Color c1 = bubbleColor; c1.a = bubbleColor.a * 0.6f;
        Color c2 = bubbleColor; c2.a = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            float ease = Mathf.SmoothStep(0f, 1f, k);

            float s = (k < 0.35f)
                ? Mathf.Lerp(startScale, peakScale, k / 0.35f)
                : Mathf.Lerp(peakScale, endScale, (k - 0.35f) / 0.65f);

            rt.localScale = Vector3.one * s;
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, ease);

            Color c = (k < 0.7f)
                ? Color.Lerp(c0, c1, k / 0.7f)
                : Color.Lerp(c1, c2, (k - 0.7f) / 0.3f);

            img.color = c;
            rt.sizeDelta = Vector2.one * Mathf.Lerp(bubbleMaxSize * 0.4f, bubbleMaxSize, ease);
            yield return null;
        }
        if (rt != null) Object.Destroy(rt.gameObject);
    }

    private IEnumerator AnimRing(Image img, RectTransform rt)
    {
        float t = 0f;
        float dur = Mathf.Max(0.02f, ringDuration);
        Color c0 = ringColor; c0.a = ringColor.a;
        Color c1 = ringColor; c1.a = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            float ease = 1f - Mathf.Pow(1f - k, 2f);

            rt.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.8f, ease);
            img.color = Color.Lerp(c0, c1, ease);
            rt.sizeDelta = Vector2.one * Mathf.Lerp(bubbleMaxSize * 0.4f, ringMaxSize, ease);

            yield return null;
        }
        if (rt != null) Object.Destroy(rt.gameObject);
    }
}
