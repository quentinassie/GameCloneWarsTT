using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class DragDrop : NetworkBehaviour
{
    private bool isDragging = false;
    private bool isDraggable = true;
    public GameObject canvas;
    public PlayerManager playerManager;
    private Transform startParent;
    private Vector2 startPosition;
    private Canvas mainCanvas;
    private RectTransform canvasRect;

    void Start()
    {
        canvas = GameObject.FindWithTag("MainCanvas");
        mainCanvas = canvas.GetComponent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        transform.localRotation = Quaternion.identity;
        if (!isOwned)
        {
            isDraggable = false;
        }
    }

    public void StartDrag()
    {
        if (!isDraggable) return;

        startParent = transform.parent;
        startPosition = transform.localPosition;

        transform.SetParent(canvas.transform, false);
        transform.SetAsLastSibling();

        isDragging = true;
    }

    public void EndDrag()
    {
        if (!isDraggable) return;
        isDragging = false;

        // Si aucune zone valide détectée, on remet à la position d'origine
        if (!IsPointerOverValidDropZone())
        {
            transform.localPosition = startPosition;
            transform.localRotation = Quaternion.identity;
            transform.SetParent(startParent, false);
        }
        else
        {
            // Si la carte est lâchée dans une zone valide, elle y reste
            transform.SetParent(startParent, false);
        }
    }

    public void PointerEnter()
    {
        if (!isOwned) return;
        transform.localScale = new Vector2(1.6f, 1.6f);
    }

    public void PointerExit()
    {
        if (!isOwned) return;
        transform.localScale = Vector2.one;
    }

    void Update()
    {
        if (!isDraggable) return;

        if (isDragging)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                Input.mousePosition,
                mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera,
                out Vector2 localPoint))
            {
                transform.localPosition = localPoint;
                GetComponent<RectTransform>().localRotation = Quaternion.identity;
            }
        }
    }

    private bool IsPointerOverValidDropZone()
    {
        // Vérifie si le pointeur est sur une zone UI avec le tag "DropZone"
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("DropZone"))
            {
                return true;
            }
        }
        return false;
    }

    public void AnimationZoom(float power)
    {
        StartCoroutine(ZoomEffect(power));
    }

    private IEnumerator ZoomEffect(float power)
    {
        RectTransform rect = GetComponent<RectTransform>();
        rect.localScale = new Vector3(power, power, 1f);

        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 targetScale = Vector3.one;
        Vector3 initialScale = rect.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rect.localScale = Vector3.Lerp(initialScale, targetScale, t);
            yield return null;
        }

        rect.localScale = targetScale;
    }
}