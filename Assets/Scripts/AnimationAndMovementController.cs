using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementController : MonoBehaviour
{
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;

    //Animation variable
    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;
    int isFallingHash;

    //Movement Vector
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentJumpMovement;
    Vector3 lastMousePosition;

    //Player's state
    bool isMovementPressed;
    bool isRunPressed;
    bool isJumpPressed;
    bool isGrounded;
    bool isFallingState;

    [Header("Jump and Gravity")]
    public float gravity = -9.81f;
    public float jumpHeight = 3.0f;
    float groundedGravity = -0.05f;

    [Header("Movement and Rotation")]
    public float moveSpeed = 5.0f;
    public float runMultiplier = 2.0f;

    [Range(0f, 1f)]
    public float rotationFactorPerFrame = 0.5f;
    [Range(50f, 150f)]
    public float mouseSensitivity = 100f;
    [Range(1f, 10f)]
    public float turnSensitivity = 5f;

    float run;
    float xRoatation = 0f;

    [Header("Ground check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Game Component")]
    public CameraManager cameraManager;
    public DialogueManager dialogueManager;
    public DOFcontroller dofController;
    public gameManager GameManager;
    public GameObject cameraAim;

    private void Awake()
    {
        playerInput = new PlayerInput { };
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
        isFallingHash = Animator.StringToHash("isFalling");

        playerInput.Player.Move.started += onMovementInput;
        playerInput.Player.Move.canceled += onMovementInput;
        playerInput.Player.Move.performed += onMovementInput;

        playerInput.Player.Sprint.started += onRun;
        playerInput.Player.Sprint.canceled += onRun;

        playerInput.Player.Jump.started += onJump;
        playerInput.Player.Jump.canceled += onJump;
    }

    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();

        isMovementPressed =
            currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void onJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }

    void handleMovement()
    {
        if (dofController.isFocusingObj) return;

        //Set falling state
        if (isGrounded)
        {
            currentJumpMovement.y = groundedGravity;
            isFallingState = false;
        }

        if (!isGrounded && currentJumpMovement.y < 0)
        {
            isFallingState = true;
        }

        //Run as default
        run = runMultiplier;

        //walk and jump reset runMultiplier to 1
        if (isJumpPressed || !isRunPressed)
        {
            run = 1.0f;
        }

        //Move player along Screen axis
        if (cameraManager.isBEV)
        {
            currentMovement.x = currentMovementInput.x;
            currentMovement.z = currentMovementInput.y;
        }
        else //Move toward direction that player is facing
        if (cameraManager.isFPS)
        {
            currentMovement =
                transform.right * currentMovementInput.x +
                transform.forward * currentMovementInput.y;
        }
        else if (cameraManager.isTPS)
        {
            currentMovement = transform.forward * currentMovementInput.y;
        }

        //Move player
        characterController
            .Move(currentMovement * run * moveSpeed * Time.deltaTime);

        //Check if player is jumping and apply upward force
        if (isJumpPressed && isGrounded)
        {
            currentJumpMovement.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        //Apply gravity to player
        currentJumpMovement.y += gravity * Time.deltaTime;

        //Apply jump
        characterController.Move(currentJumpMovement * Time.deltaTime);
    }

    void handleRotation()
    {
        Vector3 look = lastMousePosition - Input.mousePosition;

        //in BEV we rotate the entire body but camera isn't rotating
        if (cameraManager.isBEV)
        {
            Vector3 positionToLookAt;
            positionToLookAt.x = currentMovement.x;
            positionToLookAt.y = 0.0f;
            positionToLookAt.z = currentMovement.z;

            Quaternion currentRotation = transform.rotation;

            if (!isMovementPressed) return;

            Quaternion targetRotation =
                Quaternion.LookRotation(positionToLookAt);
            transform.rotation =
                Quaternion
                    .Slerp(currentRotation,
                    targetRotation,
                    rotationFactorPerFrame * Time.deltaTime);
        }
        else
        //In FPS/TPS we rotate the camera along with the player body
        {
            if (dofController.focusedObject != null && Input.GetMouseButton(1))
            {
                dofController.focusedObject.transform.rotation =
                    Quaternion
                        .Euler(dofController
                            .focusedObject
                            .transform
                            .rotation
                            .eulerAngles
                            .x +
                        (look.y * turnSensitivity * Time.deltaTime),
                        dofController
                            .focusedObject
                            .transform
                            .rotation
                            .eulerAngles
                            .y +
                        (look.x * turnSensitivity * Time.deltaTime),
                        0f);
            }
            else
            {
                //If cursor is showing ignore rotation input
                if (GameManager.showingCursor) return;

                float mouseX =
                    Input.GetAxis("Mouse X") *
                    mouseSensitivity *
                    Time.deltaTime;
                float mouseY =
                    Input.GetAxis("Mouse Y") *
                    mouseSensitivity *
                    Time.deltaTime;

                //X Axis
                transform.Rotate(Vector3.up * mouseX);

                //Y Axis
                xRoatation -= mouseY;

                //Check if player is in which state (FPS/TPS)
                if (cameraManager.isFPS)
                {
                    xRoatation = Mathf.Clamp(xRoatation, -80f, 80f);
                }
                else
                {
                    xRoatation = Mathf.Clamp(xRoatation, -135f, 135f);
                }

                cameraAim.transform.localRotation =
                    Quaternion.Euler(xRoatation, 0f, 0f);
            }

            lastMousePosition = Input.mousePosition;
        }
    }

    void handleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);
        bool isJumping = animator.GetBool(isJumpingHash);
        bool isFalling = animator.GetBool(isFallingHash);

        //Reset animation to idle if interacting with npc or object
        if (dialogueManager.onDialogue || dofController.isFocusingObj)
        {
            animator.SetBool(isWalkingHash, false);
            animator.SetBool(isRunningHash, false);
            animator.SetBool(isJumpingHash, false);
            animator.SetBool(isFallingHash, false);
            return;
        }

        //Set walking animation
        if (isMovementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
        }
        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        //Set running animation
        if ((isMovementPressed && isRunPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
        }
        else if ((!isMovementPressed || !isRunPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }

        //Set jumping animation
        if (isJumpPressed && !isJumping)
        {
            animator.SetBool(isJumpingHash, true);
        }
        else if (!isJumpPressed && isJumping)
        {
            animator.SetBool(isJumpingHash, false);
        }

        //Set falling animation
        if (isFallingState && !isFalling)
        {
            animator.SetBool(isFallingHash, true);
        }
        else if (!isFallingState && isFalling)
        {
            animator.SetBool(isFallingHash, false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        handleAnimation();
        
        if (GameManager.onPause || dialogueManager.onDialogue) return;

        //Check if player is on the ground
        isGrounded =
            Physics
                .CheckSphere(groundCheck.position, groundDistance, groundMask);

        handleMovement();
        handleRotation();
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        playerInput.Player.Disable();
    }
}
