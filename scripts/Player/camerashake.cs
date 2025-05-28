using UnityEngine;

public class MenuCameraShake : MonoBehaviour
{
    public float shakeAmount = 0.1f;
    public float shakeSpeed = 1f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offsetX = Mathf.PerlinNoise(Time.time * shakeSpeed, 0) * 2 - 1;
        float offsetY = Mathf.PerlinNoise(0, Time.time * shakeSpeed) * 2 - 1;
        Vector3 shake = new Vector3(offsetX, offsetY, 0) * shakeAmount;
        transform.position = startPos + shake;
    }
}
