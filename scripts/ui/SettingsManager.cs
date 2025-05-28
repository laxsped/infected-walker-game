using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider sensitivitySlider;
    public Slider fovSlider;
    public PlayerMovement playerMovement;
    public Camera playerCamera;

    void Start()
    {
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        fovSlider.onValueChanged.AddListener(SetFOV);

        sensitivitySlider.value = playerMovement.mouseSensitivity;
        fovSlider.value = playerCamera.fieldOfView;
    }

    void SetSensitivity(float value)
    {
        playerMovement.mouseSensitivity = value;
    }

    void SetFOV(float value)
    {
        playerCamera.fieldOfView = value;
    }
}
