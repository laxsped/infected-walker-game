using UnityEngine;

public class LogoWobble : MonoBehaviour
{
    public float amplitude = 5f;
    public float speed = 2f;
    private Vector3 startPos;

    void Start() => startPos = transform.localPosition;

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * speed) * amplitude;
        transform.localPosition = startPos + new Vector3(0, yOffset, 0);
    }
}
