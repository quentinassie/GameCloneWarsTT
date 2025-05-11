using System.Collections;
using UnityEngine;
using Mirror;

public class DragDrop : NetworkBehaviour
{
    private bool isDragging = false;
    private bool isDraggable = true;
    public GameObject canvas;
    public PlayerManager playerManager;
    private GameObject startParent;
    private Vector2 startPosition;

    void Start()
    {
        canvas = GameObject.FindWithTag("MainCanvas");
        transform.localRotation = Quaternion.identity;
        if (!isOwned)
        {
            isDraggable = false;
        }
    }


    public void StartDrag()
    {
        if (!isDraggable) return;

        startParent = transform.parent.gameObject;
        startPosition = transform.localPosition;


        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        isDragging = true;

    }

    public void EndDrag()
    {

        if (!isDraggable) return;
        isDragging = false;

        transform.localPosition = startPosition;
        transform.localRotation = Quaternion.identity;
        transform.SetParent(startParent.transform, false);

        //NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        //playerManager = networkIdentity.GetComponent<PlayerManager>();
        //playerManager.PlayCard()

    }


    public void PointerEnter()
    {
        if (!isOwned) return;

        //startParent = transform.parent.gameObject;
        startPosition = transform.localPosition;

        transform.localScale = new Vector2(1.3f, 1.3f);
        //transform.SetParent(canvas.transform, true);
        //transform.SetAsLastSibling();
    }


    public void PointerExit()
    {
        if (!isOwned) return;

        //transform.SetParent(startParent.transform, false);
        transform.localScale = Vector2.one;
        //transform.localPosition = startPosition;
    }


    void Update()
    {
        if (!isDraggable) return;

        if (isDragging)
        {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            GetComponent<RectTransform>().localRotation = Quaternion.identity;

        }
    }

}
