using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;
    public float jumpForce = 8f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 80f;

    [Header("Tilt Settings")]
    public float tiltAmount = 3f; // наклон влево/вправо при повороте
    public float tiltSmooth = 5f;

    [Header("Crouch")]
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    public float crouchTransitionSpeed = 5f;

    [Header("Stamina")]
    public float maxStamina = 5f;
    public float staminaDrain = 1.5f;
    public float staminaRecovery = 1f;
    public bool isRunning = false;

    public float stamina;
    [SerializeField] private StaminaBarsController staminaUI;

    private CharacterController controller;
    private float verticalLookRotation = 0f;
    private Vector3 velocity;

    private float currentSpeed;

    private float cameraTiltZ = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        stamina = maxStamina;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleCrouch();
        ApplyCameraTilt();
        if (staminaUI != null)
            staminaUI.SetStamina(stamina / maxStamina);
    }

    void HandleMovement()
    {
        bool isCrouching = Input.GetKey(KeyCode.LeftControl);
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

        // ¬ыбор скорости
        if (isCrouching)
            currentSpeed = crouchSpeed;
        else if (isRunning && stamina > 0f && isMoving)
        {
            currentSpeed = runSpeed;
            stamina -= staminaDrain * Time.deltaTime;
            if (stamina < 0f) stamina = 0f;
        }
        else
        {
            currentSpeed = walkSpeed;
            stamina += staminaRecovery * Time.deltaTime;
            if (stamina > maxStamina) stamina = maxStamina;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (controller.isGrounded)
        {
            velocity.y = -2f;
            if (Input.GetButtonDown("Jump") && !isCrouching)
                velocity.y = jumpForce;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);

        cameraTransform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, cameraTiltZ);

        transform.Rotate(Vector3.up * mouseX);

        // ќбновл€ем наклон по оси Z при движении мыши по X
        float targetTilt = -mouseX * tiltAmount;
        cameraTiltZ = Mathf.Lerp(cameraTiltZ, targetTilt, Time.deltaTime * tiltSmooth);
    }

    void HandleCrouch()
    {
        float targetHeight = Input.GetKey(KeyCode.LeftControl) ? crouchHeight : standHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
    }

    void ApplyCameraTilt()
    {
        // ѕримен€етс€ в HandleMouseLook, сюда перенесено просто дл€ структуры
        // Ќичего лишнего тут нет
    }
}
