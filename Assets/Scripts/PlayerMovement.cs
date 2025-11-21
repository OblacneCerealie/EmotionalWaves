using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    InputAction moveAction;
    InputAction jumpAction;
    InputAction lookAction;
    
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 0.4f;
    private Rigidbody rb;
    private bool isGrounded;
    private float verticalRotation = 0f;
    private Camera playerCamera;

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        lookAction = InputSystem.actions.FindAction("Look");
        
        
        rb = GetComponent<Rigidbody>();
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        // Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 lookValue = lookAction.ReadValue<Vector2>();
        float mouseX = lookValue.x * mouseSensitivity;
        float mouseY = lookValue.y * mouseSensitivity;
    
        
        // Rotate player left/right
        transform.Rotate(0, mouseX, 0);
        
        // Rotate camera up/down
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

        float horizontal = moveAction.ReadValue<Vector2>().x;
        float vertical = moveAction.ReadValue<Vector2>().y;

        // Move the character
        Vector3 movement = transform.right * horizontal + transform.forward * vertical;
        movement = movement.normalized * moveSpeed * Time.deltaTime;
        transform.position += movement;
        
        //Vector3 movement = new Vector3(horizontal, 0f, vertical) * moveSpeed * Time.deltaTime;
        //rb.MovePosition(transform.position + movement);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    void OnCollisionStay(Collision collision)
    {
        // Check if touching ground
        isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}