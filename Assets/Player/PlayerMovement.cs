using System;
using PurrNet;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    //[SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpForce = 1f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.2f;

    [Header("Dash Settings")]
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashCooldown = 1f;
    private float lastDashTime;
    private float dashTimer;
    private Vector3 dashVelocity;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 0.7f;
    [SerializeField] private float standHeight = 1.5f;
    [SerializeField] private float crouchSpeed = 4f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    //private SyncVar<bool> isCrouching = new(false);
    private bool isCrouching = false;

    //Movement mechanics
    private Vector3 externalVelocity;
    private float boostTimer;

    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private NetworkAnimator animator;
    
    private CharacterController characterController;
    private Vector3 velocity;
    private float verticalRotation = 0f;

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();
        enabled = isOwner;
        if (!isOwner)
        {
            Destroy(playerCamera.gameObject);
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        bool isGrounded = IsGrounded();
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        //speed boost
        if(boostTimer > 0)
        {
            characterController.Move(externalVelocity * Time.deltaTime);
            externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, 5f * Time.deltaTime);
            boostTimer -= Time.deltaTime;
        }

        //float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        float currentSpeed = isCrouching ? crouchSpeed : moveSpeed;
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        //jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
        //crouch
        if(Input.GetKey(KeyCode.LeftControl)){isCrouching=true;}
        else{isCrouching = false;}
        HandleCrouch();
        //dash
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash();
        }
        if (dashTimer > 0)
        {
            characterController.Move(dashVelocity * Time.deltaTime);
            dashTimer -= Time.deltaTime;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        //Animations
        animator.SetFloat("Forward", vertical);
        animator.SetFloat("Sideways", horizontal);
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.03f, Vector3.down, groundCheckDistance);
    }

    private void Dash()
    {
        if(Time.time < lastDashTime + dashCooldown)
            return;

        lastDashTime = Time.time;

        Vector3 dashDirection = transform.forward;
        dashDirection.y = 0f;
        dashDirection.Normalize();

        dashVelocity = dashDirection * (dashDistance / dashDuration);
        dashTimer = dashDuration;
    }

    private void HandleCrouch()
    {
        float targetHeight = isCrouching ? crouchHeight : standHeight;

        characterController.height = Mathf.Lerp(
            characterController.height,
            targetHeight,
            Time.deltaTime * crouchTransitionSpeed
        );

        //player center
        characterController.center = new Vector3(
            0,
            characterController.height / 2f,
            0
        );

        //cam height
        playerCamera.transform.localPosition = new Vector3(
            playerCamera.transform.localPosition.x,
            characterController.height - 0.2f,
            playerCamera.transform.localPosition.z
        );

        //weaponholder height
        weaponHolder.localPosition = new Vector3(
            weaponHolder.localPosition.x,
            characterController.height - 1.8f,
            weaponHolder.localPosition.z
        );
    }

    public void ApplyBoost(Vector3 force, float duration)
    {
        externalVelocity = force;
        boostTimer = duration;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.03f, Vector3.down * groundCheckDistance);
    }
#endif
}