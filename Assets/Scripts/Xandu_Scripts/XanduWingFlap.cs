using UnityEngine;

public class XanduWingFlap : MonoBehaviour
{
    [SerializeField] private Transform wingLeft;
    [SerializeField] private Transform wingRight;
    [SerializeField] private Transform wingLeftBase;

    [SerializeField] private Vector3 wingScaleVariation = new Vector3(0.15f, 0.25f, 0f);
    [SerializeField] private float positionAmplitude = 0.02f;

    private float initialLeftZ;
    private float initialRightZ;
    private Vector3 initialLeftScale;
    private Vector3 initialRightScale;
    private Vector3 initialRightPosition;
    private Vector3 wingLeftLocalOffset; // offset par rapport au base
    private float offsetLeft;
    private float offsetRight;

    void Start()
    {
        initialLeftZ = wingLeft.localRotation.eulerAngles.z;
        if (initialLeftZ > 180f) initialLeftZ -= 360f;

        initialRightZ = wingRight.localRotation.eulerAngles.z;
        if (initialRightZ > 180f) initialRightZ -= 360f;

        initialLeftScale = wingLeft.localScale;
        initialRightScale = wingRight.localScale;

        initialRightPosition = wingRight.localPosition;
        wingLeftLocalOffset = wingLeft.localPosition; // position relative au bras

        offsetLeft = Random.Range(0f, Mathf.PI * 2f);
        offsetRight = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float time = Time.time;
        float sine = (Mathf.Sin(time * 1.5f * Mathf.PI * 2) + 1f) * 0.5f;
        float flapAngleRight = sine * 10f;
        float flapAngleLeft = -sine * 10f;

        // Rotation
        wingLeft.localRotation = Quaternion.Euler(0f, 0f, initialLeftZ + flapAngleLeft - 10f);
        wingRight.localRotation = Quaternion.Euler(0f, 0f, initialRightZ + flapAngleRight + 10f);

        // Déformation
        float sineLeft = (Mathf.Sin(time * Mathf.PI * 2 + offsetLeft) + 1f) * 0.5f;
        float sineRight = (Mathf.Sin(time * Mathf.PI * 2 + offsetRight) + 1f) * 0.5f;

        float scaleXLeft = 1f - sineLeft * wingScaleVariation.x;
        float scaleYLeft = 1f + sineLeft * wingScaleVariation.y;
        float scaleXRight = 1f - sineRight * wingScaleVariation.x;
        float scaleYRight = 1f + sineRight * wingScaleVariation.y;

        wingLeft.localScale = new Vector3(initialLeftScale.x * scaleXLeft, initialLeftScale.y * scaleYLeft, initialLeftScale.z);
        wingRight.localScale = new Vector3(initialRightScale.x * scaleXRight, initialRightScale.y * scaleYRight, initialRightScale.z);

        // Position relative à la base (fixe)
        float posAngle = time * 0.5f * Mathf.PI * 2;
        Vector3 circleOffset = new Vector3(Mathf.Cos(posAngle), Mathf.Sin(posAngle), 0f) * 4f;

        wingLeft.localPosition = wingLeftLocalOffset + circleOffset;
        wingRight.localPosition = initialRightPosition + circleOffset;
    }
}
