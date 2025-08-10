using UnityEngine;
using TMPro;
using System.Collections;

public class SpeechBubbleUI : MonoBehaviour
{
    public TMP_Text bubbleText;
    public float displayDuration = 2f;

    [Header("Effet flottant")]
    public float floatAmplitude = 0.2f;   // Hauteur du flottement
    public float floatSpeed = 2f;         // Vitesse du flottement

    [Header("Effet écriture")]
    public float firstPartSpeed = 0.05f;  // Vitesse écriture première phrase
    public float secondPartSpeed = 0.05f; // Vitesse écriture du skill
    public float pauseBetweenParts = 2f;  // Pause entre les deux parties

    private CanvasGroup canvasGroup;
    private bool isShowing;
    private Vector3 initialPosition;
    private Coroutine typingCoroutine;

    void Start()
    {
        initialPosition = transform.position;

        // Ajoute CanvasGroup si absent
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
    }

    void Update()
    {
        // Effet flottant
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = initialPosition + new Vector3(0, newY, 0);
    }

    public void ShowMessage(string playerName, string skill)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeMessage(playerName, skill));
    }

    private IEnumerator TypeMessage(string playerName, string skill)
    {
        // Affiche et reset alpha
        canvasGroup.alpha = 1;
        isShowing = true;
        bubbleText.text = "";

        string firstPart = $"!!??@***\nLe joueur {playerName} a choisi ....";
        string secondPart = $"\n\n{skill} !";

        // --- Première partie (jusqu’à “....”) ---
        foreach (char c in firstPart)
        {
            bubbleText.text += c;
            yield return new WaitForSeconds(firstPartSpeed);
        }

        // Pause avant la deuxième partie
        yield return new WaitForSeconds(pauseBetweenParts);

        // --- Deuxième partie (le skill) ---
        foreach (char c in secondPart)
        {
            bubbleText.text += c;
            yield return new WaitForSeconds(secondPartSpeed);
        }

        // Attend avant de cacher
        yield return new WaitForSeconds(displayDuration);

        HideMessage();
    }

    public void HideMessage()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float fadeTime = 0.5f;
        float startAlpha = canvasGroup.alpha;
        float t = 0;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, t / fadeTime);
            yield return null;
        }

        canvasGroup.alpha = 0;
        isShowing = false;
        bubbleText.text = "";
    }
}
