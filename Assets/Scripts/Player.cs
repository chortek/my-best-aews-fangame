using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Движение")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float gravity = -9.81f;
    public float damping = 10f;

    [Header("Выносливость")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 15f;
    public float staminaRegenDelay = 1f;
    private float currentStamina;
    private float regenDelayTimer = 0f;
    private bool isRunning = false;

    [Header("UI")]
    public Slider staminaSlider;         // <-- СЛАЙДЕР

    [Header("Камера")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;
    private float xRotation = 0f;

    [Header("Боббинг")]
    public float bobSpeed = 10f;
    public float bobAmount = 0.05f;
    private float bobTimer = 0f;

    [Header("Звук шагов")]
    public AudioClip footstepClip;
    public float stepInterval = 0.5f;
    private float stepTimer = 0f;

    private CharacterController controller;
    private AudioSource audioSource;
    private float verticalVelocity = 0f;
    private Transform cameraTransform;
    private Vector3 currentVelocity = Vector3.zero;
    private bool isMoving = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        cameraTransform = GetComponentInChildren<Camera>().transform;
        currentStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;

        // Настройка слайдера
        if (staminaSlider != null)
        {
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = maxStamina;
        }
    }

    void Update()
    {
        HandleStamina();
        HandleMovement();
        HandleCamera();
        HandleBob();
        HandleFootsteps();
        UpdateStaminaUI();
    }

    void HandleStamina()
    {
        isRunning = Input.GetKey(KeyCode.LeftShift) && isMoving && currentStamina > 0;

        if (isRunning)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            regenDelayTimer = staminaRegenDelay;
        }
        else
        {
            regenDelayTimer -= Time.deltaTime;
            if (regenDelayTimer <= 0f)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
            }
        }
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 moveInput = new Vector3(x, 0, z);
        isMoving = moveInput.magnitude > 0.1f;

        Vector3 targetMove = transform.right * x + transform.forward * z;
        float targetSpeed = (isRunning && currentStamina > 0) ? runSpeed : walkSpeed;

        if (isMoving)
        {
            currentVelocity = Vector3.Lerp(currentVelocity, targetMove * targetSpeed, Time.deltaTime * damping);
        }
        else
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, Time.deltaTime * damping);
        }

        verticalVelocity += gravity * Time.deltaTime;
        if (controller.isGrounded) verticalVelocity = -1f;

        Vector3 velocity = currentVelocity + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleBob()
    {
        if (!isMoving || !controller.isGrounded)
        {
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, new Vector3(0, 0.6f, 0), Time.deltaTime * 10f);
            return;
        }

        bobTimer += Time.deltaTime * bobSpeed * (isRunning ? 1.5f : 1f);
        float bobY = Mathf.Sin(bobTimer) * bobAmount;
        float bobX = Mathf.Cos(bobTimer * 0.5f) * bobAmount * 0.5f;

        cameraTransform.localPosition = new Vector3(bobX, 0.6f + bobY, 0f);
    }

    void HandleFootsteps()
    {
        if (!isMoving || !controller.isGrounded || footstepClip == null)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer += Time.deltaTime;
        float interval = isRunning ? stepInterval * 0.4f : stepInterval;

        if (stepTimer >= interval)
        {
            stepTimer = 0f;
            audioSource.PlayOneShot(footstepClip);
        }
    }

    void UpdateStaminaUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
        }
    }
}