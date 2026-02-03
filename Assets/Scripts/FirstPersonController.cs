using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private Transform cameraTransform;
    
    [Header("UI")]
    [SerializeField] private HandAnimationManager handAnimationManager;
    
    private CharacterController characterController;
    private float verticalVelocity;
    private bool controlEnabled = true;
    private bool isWalking;
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
        }
    }
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Update()
    {
        if (controlEnabled)
        {
            HandleMovement();
            HandleRotation();
        }
    }
    
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);
        Vector3 movement = moveDirection * moveSpeed;
        
        if (characterController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        
        movement.y = verticalVelocity;
        characterController.Move(movement * Time.deltaTime);
        
        UpdateWalkingState(horizontal, vertical);
    }
    
    private void UpdateWalkingState(float horizontal, float vertical)
    {
        if (handAnimationManager == null)
            return;
        
        bool hasMovement = Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f;
        
        if (hasMovement && !isWalking)
        {
            isWalking = true;
            handAnimationManager.StartWalking();
        }
        else if (!hasMovement && isWalking)
        {
            isWalking = false;
            handAnimationManager.StopWalking();
        }
    }
    
    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float horizontalRotation = mouseX * lookSensitivity;
        transform.Rotate(Vector3.up * horizontalRotation);
    }

    public void SetControlEnabled(bool enabled)
    {
        controlEnabled = enabled;
    }
}
