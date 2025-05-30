using UnityEngine;

public class XanduLegOscillation : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.05f; // amplitude du mouvement
    [SerializeField] private float frequency = 2f; // fréquence des oscillations
    [SerializeField] private Vector3 direction = Vector3.up; // direction du mouvement

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
