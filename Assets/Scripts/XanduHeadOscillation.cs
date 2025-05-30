using UnityEngine;

public class XanduHeadOscillation : MonoBehaviour
{

    private float initialZ;
    private Vector3 initialPosition;

    void Start()
    {
        initialZ = transform.localRotation.eulerAngles.z;
        if (initialZ > 180f) initialZ -= 360f;

        initialPosition = transform.localPosition;
    }

    void Update()
    {
        float time = Time.time;
        float angleOffset = Mathf.Sin(time * 0.25f * Mathf.PI * 2f) * 10f;
        transform.localRotation = Quaternion.Euler(0f, 0f, initialZ + angleOffset);

        float posAngle = time * 0.5f * Mathf.PI * 2f;
        Vector3 offset = new Vector3(Mathf.Cos(posAngle), Mathf.Sin(posAngle), 0f) * 4f;
        transform.localPosition = initialPosition + offset;
    }
}
