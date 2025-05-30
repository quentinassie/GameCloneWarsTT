using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DroidArmLook : MonoBehaviour
{
    [SerializeField] private float amplitude = 15f;
    [SerializeField] private bool isMyTurn = false;
    [SerializeField] private bool begin = true;
    [SerializeField] private Image armShadedImage;
    [SerializeField] private float fadeDuration = 0.05f;

    private float angle = 0f;
    private Coroutine currentRotation;
    private Coroutine fadeRoutine;

    private void Update()
    {
        if (begin) return;

        begin = true;

        if (currentRotation != null)
            StopCoroutine(currentRotation);

        float target = isMyTurn ? -amplitude : amplitude;
        currentRotation = StartCoroutine(RotateTo(target));
    }

    private IEnumerator RotateTo(float targetAngle)
    {
        float angle = transform.eulerAngles.z;
        if (angle > 180f) angle -= 360f;

        bool fadeStarted = false;

        while (Mathf.Abs(angle - targetAngle) > 0.1f)
        {
            float delta = Time.deltaTime * 150f;
            angle = Mathf.MoveTowards(angle, targetAngle, delta);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // Déclencher le fade vers la fin de la rotation
            if (!fadeStarted && Mathf.Abs(angle - targetAngle) < 8f)
            {
                float targetAlpha = isMyTurn ? 0f : 1f;

                if (fadeRoutine != null)
                    StopCoroutine(fadeRoutine);

                fadeRoutine = StartCoroutine(FadeImage(armShadedImage, targetAlpha, fadeDuration));
                fadeStarted = true;
            }

            yield return null;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
    }


    private IEnumerator FadeImage(Image img, float targetAlpha, float duration)
    {
        if (img == null) yield break;

        float startAlpha = img.color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            time = time * 0.8f;
            float t = Mathf.Clamp01(time / duration);
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            Color c = img.color;
            c.a = newAlpha;
            img.color = c;

            yield return null;
        }

        Color finalColor = img.color;
        finalColor.a = targetAlpha;
        img.color = finalColor;
    }

    public void SetMyTurn(bool myTurn)
    {
        isMyTurn = myTurn;
        begin = false;

    }
}