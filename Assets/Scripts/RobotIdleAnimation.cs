using UnityEngine;

public class RobotIdleAnimation : MonoBehaviour
{
    public RectTransform torso;
    public RectTransform leftArm;
    public RectTransform rightArm;

    [Header("Shadows")]
    public RectTransform leftShadow;
    public RectTransform rightShadow;

    [Header("Torso")]
    public float torsoAmplitude = 2f;       // mouvement vertical
    public float torsoSpeed = 1f;

    [Header("Arms Movement")]
    public float armAmplitude = 3f;         // mouvement vertical léger
    public float armSpeed = 1.2f;
    public float armRotationAmplitude = 2f; // petit balancement en Z

    private Vector3 torsoInitialPos;
    private Vector3 leftArmInitialPos;
    private Vector3 rightArmInitialPos;

    private Vector3 leftShadowInitialPos;
    private Vector3 rightShadowInitialPos;

    void Start()
    {
        if (torso != null) torsoInitialPos = torso.anchoredPosition;
        if (leftArm != null) leftArmInitialPos = leftArm.anchoredPosition;
        if (rightArm != null) rightArmInitialPos = rightArm.anchoredPosition;

        if (leftShadow != null) leftShadowInitialPos = leftShadow.anchoredPosition;
        if (rightShadow != null) rightShadowInitialPos = rightShadow.anchoredPosition;
    }

    void Update()
    {
        float t = Time.time;

        // Torso : léger mouvement haut-bas
        if (torso != null)
        {
            torso.anchoredPosition = torsoInitialPos +
                new Vector3(0, Mathf.Sin(t * torsoSpeed) * torsoAmplitude, 0);
        }

        // Bras gauche
        if (leftArm != null)
        {
            Vector3 newPos = leftArmInitialPos +
                new Vector3(0, Mathf.Sin(t * armSpeed) * armAmplitude, 0);
            Quaternion newRot = Quaternion.Euler(0, 0,
                Mathf.Sin(t * armSpeed) * armRotationAmplitude - 15);

            leftArm.anchoredPosition = newPos;
            leftArm.localRotation = newRot;

            if (leftShadow != null)
            {
                leftShadow.anchoredPosition = leftShadowInitialPos +
                    (newPos - leftArmInitialPos);
                leftShadow.localRotation = Quaternion.Euler(0, 0,
                Mathf.Sin((t + 1f) * armSpeed) * armRotationAmplitude + 30);
            }
        }

        // Bras droit
        if (rightArm != null)
        {
            Vector3 newPos = rightArmInitialPos +
                new Vector3(0, Mathf.Sin((t + 1f) * armSpeed) * armAmplitude, 0);
            Quaternion newRot = Quaternion.Euler(0, 0,
                Mathf.Sin((t + 1f) * armSpeed) * armRotationAmplitude + 15);

            rightArm.anchoredPosition = newPos;
            rightArm.localRotation = newRot;

            if (rightShadow != null)
            {
                rightShadow.anchoredPosition = rightShadowInitialPos +
                    (newPos - rightArmInitialPos);
                rightShadow.localRotation = Quaternion.Euler(0, 0,
                Mathf.Sin((t + 1f) * armSpeed) * armRotationAmplitude - 15);
            }
        }
    }
}
