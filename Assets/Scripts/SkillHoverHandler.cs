using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SkillHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string skillName;
    public PlayerManager playerManager; // injecté par PlayerManager
    public TMP_Text messageText;       // injecté par PlayerManager

    public GameObject card;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (messageText == null)
        {
            Debug.LogWarning("[SkillHoverHandler] messageText is NULL");
            return;
        }

        if (playerManager == null)
        {
            Debug.LogWarning("[SkillHoverHandler] playerManager is NULL");
            return;
        }


        int value = playerManager.GetSkillValueFromCard(playerManager, skillName);
        messageText.SetText(value.ToString());
        Debug.Log($"[OnPointerEnter] skill: {skillName} → value: {value}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ClearMessage();
    }

    public void ClearMessage()
    {
        if (messageText != null)
            messageText.SetText("");
    }

}
