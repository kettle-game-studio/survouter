using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public InputActionAsset actions;
    public Camera playerCamera;
    public float mouseSpeed = 1;
    public Vector2 lookLimits = new Vector2(-60, 60);
    public float walkSpeed = 2;
    public float runSpeed = 6;
    public float jumpHeight = 2;
    public float jumpAngle = 45;
    public float maxInteractDistance = 1.5f;
    public float dialogRecordTtl = 3;
    public float dialogRecordWriteTime = 0.5f;
    public AudioClip walk;
    public AudioClip jump;
    public AudioSource walkSource;
    public AudioSource jumpSource;

    InputAction lookAction;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction sprintAction;
    InputAction attackAction;
    InputAction interactAction;
    float verticalLookAngle = 0;
    Rigidbody rigidBody;
    bool canJump;
    float JumpVelocity => Mathf.Sqrt(2 * Physics.gravity.magnitude * jumpHeight);
    InputActionMap playerInputMap;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        rigidBody = GetComponent<Rigidbody>();
        playerInputMap = actions.FindActionMap("Player");
        playerInputMap.Enable();
        lookAction = playerInputMap.FindAction("Look");
        moveAction = playerInputMap.FindAction("Move");
        jumpAction = playerInputMap.FindAction("Jump");
        sprintAction = playerInputMap.FindAction("Sprint");
        attackAction = playerInputMap.FindAction("Attack");
        interactAction = playerInputMap.FindAction("Interact");

        walkSource.clip = walk;
        jumpSource.clip = jump;
    }

    void Update()
    {
        Vector2 lookValue = lookAction.ReadValue<Vector2>();
        lookValue = lookValue * mouseSpeed;
        verticalLookAngle = Mathf.Clamp(verticalLookAngle + lookValue.y, lookLimits.x, lookLimits.y);
        playerCamera.transform.localRotation = Quaternion.AngleAxis(verticalLookAngle, Vector3.left);
        transform.rotation = Quaternion.AngleAxis(lookValue.x, Vector3.up) * transform.rotation;

        bool attackButton = attackAction.WasPressedThisFrame();
        bool interactButton = interactAction.WasPressedThisFrame();

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out var hitInfo, maxInteractDistance))
        {
            // Interactinator
            // interactinator = hitInfo.collider.GetComponentInParent<Interactinator>();
            // if (interactinator != null && ThingInArm == null)
            // {
            //     tooltipText.text = $"[E] {interactinator.tooltip}";
            // }
        }

        if (interactButton)
        {
            // Interact
        }
        else if (attackButton)
        {
            // Attack
        }
    }

    void FixedUpdate()
    {
        rigidBody.useGravity = !canJump;

        Vector3 resultVelocity = Vector3.up * rigidBody.linearVelocity.y;

        float speed = sprintAction.IsPressed() ? runSpeed : walkSpeed;
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        moveValue *= speed;

        if (moveValue.magnitude > 0.5 && canJump)
        {
            if (!walkSource.isPlaying)
                walkSource.Play();
        }
        else
        {
            walkSource.Stop();
        }
        walkSource.pitch = sprintAction.IsPressed() ? 2f : 1f;

        Vector3 forwardVector = transform.rotation * Vector3.forward;
        Vector3 rightVector = transform.rotation * Vector3.right;
        resultVelocity += forwardVector * moveValue.y + rightVector * moveValue.x;

        if (canJump && jumpAction.IsPressed())
        {
            resultVelocity.y = JumpVelocity;
            jumpSource.Play();
        }

        rigidBody.linearVelocity = resultVelocity;
        canJump = false;
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (var contact in collision.contacts)
            if (Vector3.Dot(contact.normal, Vector3.up) > Mathf.Cos(jumpAngle * Mathf.Deg2Rad))
            {
                canJump = true;
            }
    }

    public void LockInput(float atTime)
    {
        StartCoroutine(LockInputCoroutine(atTime));
    }

    IEnumerator LockInputCoroutine(float atTime)
    {
        moveAction.Disable();
        jumpAction.Disable();
        attackAction.Disable();
        interactAction.Disable();
        yield return new WaitForSeconds(atTime);
        moveAction.Enable();
        jumpAction.Enable();
        attackAction.Enable();
        interactAction.Enable();
        playerInputMap.Enable();
    }
}

