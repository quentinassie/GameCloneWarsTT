using System.Collections;
using UnityEngine;

public class DroidSuspension : MonoBehaviour
{
    private float dz = 0f;
    private float max = 1.05f;
    private float localTime;
    private Coroutine currentSuspension;


    private void Start()
    {
        currentSuspension = StartCoroutine(Suspension());
    }

    private IEnumerator Suspension()
    {
        while (true)
        {
            while (1f + dz < max)
            {
                localTime = Time.deltaTime * 0.07f;
                dz += localTime;
                transform.localScale = new Vector3(1f + dz, 1f + dz, 1f + dz);
                yield return null;
            }

            while (1f + dz > 1f)
            {
                localTime = Time.deltaTime * 0.07f;
                dz -= localTime;
                transform.localScale = new Vector3(1f + dz, 1f + dz, 1f + dz);
                yield return null;
            }
        }
    }
}
