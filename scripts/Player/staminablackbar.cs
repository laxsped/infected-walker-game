using UnityEngine;
using UnityEngine.UI;

public class StaminaBarsController : MonoBehaviour
{
    public RectTransform leftBar;
    public RectTransform rightBar;

    [Range(0f, 1f)] public float staminaRatio = 1f; // 1 = ������ �������, 0 = �����
    public float maxBarWidth = 300f;
    public float smoothSpeed = 5f;

    private float targetWidth = 0f;
    private float currentWidth = 0f;
    private bool wasExhausted = false;

    void Update()
    {
        if (staminaRatio < 0.3f) // ���� ������� ����
        {
            targetWidth = Mathf.Lerp(0, maxBarWidth, 1 - staminaRatio);
            wasExhausted = true;
        }
        else if (wasExhausted)
        {
            // ����� ������������ ��������������
            targetWidth = 0;
        }

        currentWidth = Mathf.Lerp(currentWidth, targetWidth, Time.deltaTime * smoothSpeed);
        UpdateBars(currentWidth);

        if (currentWidth < 1f) wasExhausted = false; // ����� ����� ����� ��������������
    }

    void UpdateBars(float width)
    {
        leftBar.sizeDelta = new Vector2(width, leftBar.sizeDelta.y);
        rightBar.sizeDelta = new Vector2(width, rightBar.sizeDelta.y);
    }

    // ����� �������� �����, �������� �� ������� �������� ������
    public void SetStamina(float ratio)
    {
        staminaRatio = Mathf.Clamp01(ratio);
    }
}
