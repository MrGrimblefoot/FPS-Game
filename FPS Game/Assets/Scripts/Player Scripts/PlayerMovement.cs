using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    #region Variables
    public bool isSprinting => canSprint && sprintingInput && !isAiming;
    public bool sprintingInput;
    public bool isAiming;
    //private bool shouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && controller.isGrounded;

    [Header("Functional Options")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canSprint = true;
    //[SerializeField] private bool useHeadbob = true;
    //[SerializeField] private bool canInteract = true;
    //[SerializeField] private bool useFootsteps = true;
    [Range(1, 4)] [SerializeField] private int droneType;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 7.0f;
    [SerializeField] private float sprintSpeed = 10.0f;
    [SerializeField] private float crouchSpeed = 3.5f;
    [SerializeField] private float walkSprintTransitionTime = 9;
    [SerializeField] private float walkCrouchTransitionTime = 10;
    [SerializeField] private bool isMoving;
    private float currentSpeed = 0.0f;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30;
    //[SerializeField] private float normalGravity = 30;

    [Header("Crouching Parameters")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    public bool isCrouching;
    //private bool duringCrouchAnimation;

    //Sliding Parameters
    private Vector3 hitPointNormal;
    #region isSlidingBool
    //private bool isSliding
    //{
    //    get
    //    {
    //        if (controller.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
    //        {
    //            hitPointNormal = slopeHit.normal;
    //            return Vector3.Angle(hitPointNormal, Vector3.up) > controller.slopeLimit;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }
    //}
    #endregion

    private Camera playerCamera;
    [SerializeField] private Transform cameraParent;
    private CharacterController controller = null;
    private PlayerPolishManager polishManager;
    private WeaponSystem weaponManager = null;
    private Animator anim;
    private Rigidbody rb;
    private PlayerInput playerInput;
    private BasicInputActions basicInputActions;
    private Vector2 animInput;
    [HideInInspector] public bool grounded;

    private Vector3 moveDirection;
    public Vector2 currentInput;

    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        //If the GameObject isn't mine: turn the camera off. Else: turn the camera on. (The cameraParent GameObject should be off by default.)
        cameraParent.gameObject.SetActive(photonView.IsMine);

        if (!photonView.IsMine) { return; }
        playerCamera = transform.Find("Cameras/CameraRecoil/CameraShaker/Player Camera").GetComponent<Camera>();
        weaponManager = GetComponent<WeaponSystem>();
        controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        playerInput = GetComponent<PlayerInput>();

        #region InputActions
        basicInputActions = new BasicInputActions();
        basicInputActions.Player.Jump.performed += Jump;
        basicInputActions.Player.Jump.Enable();
        basicInputActions.Player.Crouch.performed += Crouch;
        basicInputActions.Player.Crouch.Enable();
        basicInputActions.Player.Sprint.Enable();
        basicInputActions.Player.Sprint.started += _ => sprintingInput = true;
        basicInputActions.Player.Sprint.canceled += _ => sprintingInput = false;
        basicInputActions.Player.Fire2.Enable();
        basicInputActions.Player.Fire2.started += _ => isAiming = true;
        basicInputActions.Player.Fire2.canceled += _ => isAiming = false;
        basicInputActions.Player.Movement.Enable();
        #endregion

        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        if (!photonView.IsMine) { return; }

        if (canMove)
        {
            HandleMovementInput();
            HandleSpeed();

            anim.SetBool("IsMoving", isMoving);
            anim.SetBool("IsSprinting", isSprinting);

            weaponManager.Aim(isAiming);

            ApplyFinalMovements();
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) { return; }
        grounded = controller.isGrounded;
    }
    #endregion

    #region Custom Methods
    private void HandleMovementInput()
    {
        currentInput = basicInputActions.Player.Movement.ReadValue<Vector2>();
        if (currentInput.x == 0 && currentInput.y == 0) { isMoving = false; } else { isMoving = true; }
        float moveDirectionY = moveDirection.y;

        HandleAnimationInput();

        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.y) + (transform.TransformDirection(Vector3.right) * currentInput.x);
        moveDirection = moveDirection.normalized * currentSpeed;
        moveDirection.y = moveDirectionY;
    }

    private void HandleSpeed()
    {
        if (isCrouching) { currentSpeed = Mathf.Lerp(currentSpeed, crouchSpeed, walkCrouchTransitionTime * Time.deltaTime); }
        else if (isSprinting && !isCrouching) { currentSpeed = Mathf.Lerp(currentSpeed, sprintSpeed, walkSprintTransitionTime * Time.deltaTime); }
        else if (!isSprinting && !isCrouching) { currentSpeed = Mathf.Lerp(currentSpeed, walkSpeed, walkSprintTransitionTime * Time.deltaTime); }
    }

    public void Jump(InputAction.CallbackContext context) { if (grounded & context.performed) { moveDirection.y = jumpForce; } }

    public void Crouch(InputAction.CallbackContext context) { if (context.performed) { StartCoroutine(CrouchStand()); } }

    private void HandleAnimationInput()
    {
        animInput = currentInput;

        if (isSprinting) { animInput = new Vector2(animInput.x * 2f, animInput.y * 2f); }
        else if (isCrouching) { animInput = new Vector2(animInput.x * 0.5f, animInput.y * 0.5f); }

        anim.SetFloat("MoveX", animInput.y);
        anim.SetFloat("MoveZ", animInput.x);
    }

    private void ApplyFinalMovements()
    {
        if (!controller.isGrounded) { moveDirection.y -= gravity * Time.deltaTime; }

        //if (willSlideOnSlopes && isSliding)
        //{
        //    moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        //}

        controller.Move(moveDirection * Time.deltaTime);
    }

    private IEnumerator CrouchStand()
    {
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f)) { yield break; }

        //duringCrouchAnimation = true;

        float timeElapsed = 0f;
        //            if isCrouching is true    \/      else     \/
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = controller.height;
        //            if isCrouching is true    \/      else     \/
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = controller.center;

        while (timeElapsed < timeToCrouch)
        {
            controller.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            controller.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        controller.height = targetHeight;
        controller.center = targetCenter;

        isCrouching = !isCrouching;

        //duringCrouchAnimation = false;
    }
    #endregion
}