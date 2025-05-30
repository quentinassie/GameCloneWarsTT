using UnityEngine;

public class HoloWave : MonoBehaviour
{
    public float waveSpeed = 2f;
    public float waveStrength = 0.03f;
    private Material holoMaterial;
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * waveSpeed) * waveStrength;
        transform.localPosition = initialPosition + new Vector3(0, offset, 0);
    }
}
