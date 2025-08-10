using UnityEngine;

public class XanduLegOscillation : MonoBehaviour
{
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        float time = Time.time;
        float posAngle = time * 0.5f * Mathf.PI * 2f;
        Vector3 offset = new Vector3(Mathf.Cos(posAngle), Mathf.Sin(posAngle), 0f) * 4f;
        transform.localPosition = initialPosition + offset;
    }
}
