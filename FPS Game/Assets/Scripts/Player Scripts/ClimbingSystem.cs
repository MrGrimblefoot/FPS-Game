using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ClimbingSystem : MonoBehaviourPunCallbacks
{
    [Header("References")]
    public Transform orientation;
    public Rigidbody rb;
    public LayerMask whatIsWall;
    private PlayerMovement movement = null;

    public bool isClimbing;

    [Header("Climbing")]
    public float climbSpeed;
    public float maxClimbTime;
    [SerializeField] private float climbTimer;

    [Header("ClimbJumping")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;
    public KeyCode jumpKey = KeyCode.Space;
    public int maxClimbJumps;
    private int climbJumpsLeft;

    [Header("Detection")]
    public float detectionLength;
    public float sphereCastRadius;
    private float wallLookAngle;
    public float maxWallLookAngle;

    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    public float minWallNormalAngleChange;

    [Header("Exiting")]
    public bool exitingWall;
    public float exitWallCooldown;
    private float exitWallTimer;

    private void Awake()
    {
        if (!photonView.IsMine) { return; }
        movement = GetComponent<PlayerMovement>();
        climbJumpsLeft = maxClimbJumps;
    }

    private void Update()
    {
        WallCheck();
        StateMachine();

        if (isClimbing && !exitingWall) ClimbingMovement();
    }

    private void StateMachine()
    {
        // State 1 - Climbing
        if (wallFront && Input.GetKey(KeyCode.W) && wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if (!isClimbing && climbTimer > 0) StartClimbing();

            // timer
            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer < 0) StopClimbing();
        }

        // State 2 - Exiting
        else if (exitingWall)
        {
            if (isClimbing) StopClimbing();

            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer < 0) exitingWall = false;
        }

        // State 3 - None
        else
        {
            if (isClimbing) StopClimbing();
        }

        if (wallFront && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0) ClimbJump();
    }

    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, whatIsWall);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        bool newWall = frontWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if ((wallFront && newWall) || movement.grounded)
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = maxClimbJumps;
        }
    }

    private void StartClimbing()
    {
        isClimbing = true;
        //movement.climbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;

        /// idea - camera fov change
    }

    private void ClimbingMovement()
    {
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);

        /// idea - sound effect
    }

    private void StopClimbing()
    {
        isClimbing = false;
        //movement.climbing = false;

        /// idea - particle effect
        /// idea - sound effect
    }

    private void ClimbJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallCooldown;

        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }
}
