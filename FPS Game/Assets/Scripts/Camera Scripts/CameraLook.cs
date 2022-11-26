using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class CameraLook : MonoBehaviourPunCallbacks, IPunObservable
{
    public float fieldOfView;
    [SerializeField] private float lookSensitivity;
    private float currentLookSensitivity;
    [SerializeField] private float smoothing;
    [SerializeField] private float lookMax;
    [SerializeField] private float lookMin;

    private int weaponAngle;
    private Vector3 weaponPos;

    [SerializeField] private Transform weapon;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private WeaponSystem weaponController;

    public bool lockCursor = true;

    [SerializeField] private Transform playerCam;
    [SerializeField] private Transform weaponCam;

    private Vector2 smoothedVelocity;
    private Vector2 currentLookingPos;

    private PlayerInput playerInput;
    private BasicInputActions basicInputActions;
    Vector2 mouseInput;

    private void Start()
    {
        if (!photonView.IsMine) { return; }
        currentLookSensitivity = lookSensitivity;
        playerInput = GetComponent<PlayerInput>();

        #region InputActions
        basicInputActions = new BasicInputActions();
        basicInputActions.Player.MouseX.performed += ctx => mouseInput.x = ctx.ReadValue<float>();
        basicInputActions.Player.MouseX.Enable();
        basicInputActions.Player.MouseY.performed += ctx => mouseInput.y = ctx.ReadValue<float>();
        basicInputActions.Player.MouseY.Enable();
        #endregion
    }

    public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_message)
    {
        if (p_stream.IsWriting)
        {
            //p_stream.SendNext((int)(playerCam.localEulerAngles.y * 100f));
            p_stream.SendNext((int)(weapon.transform.localEulerAngles.x * 100f));
        }
        else
        {
            //aimAngle = (int)p_stream.ReceiveNext() / 100;
            weaponAngle = (int)p_stream.ReceiveNext() / 100;
        }
    }

    private void Update()
    {
        if (!photonView.IsMine)
        {
            RefreshMultiplayerStateForWeapon();
            return;
        }
        if (lockCursor == true)
        {
            //lock the cursor
            Cursor.lockState = CursorLockMode.Locked;
            //make it invisable
            Cursor.visible = false;
        }
        else
        {
            //lock the cursor
            Cursor.lockState = CursorLockMode.None; 
            //make it invisable
            Cursor.visible = true;
        }

        if (movement.isAiming) { currentLookSensitivity = lookSensitivity / weaponController.currentGunData.ADSSensitivityMultiplier; } else { currentLookSensitivity = lookSensitivity; }

        //calling the RotateCamera function
        RotateCamera();
    }

    void RotateCamera()
    {
        //storing input
        Vector2 inputValues = new Vector2(mouseInput.x, mouseInput.y);

        //smoothing
        inputValues = Vector2.Scale(inputValues, new Vector2(currentLookSensitivity * smoothing, currentLookSensitivity * smoothing));
        smoothedVelocity.x = Mathf.Lerp(smoothedVelocity.x, inputValues.x, 1f / smoothing);
        smoothedVelocity.y = Mathf.Lerp(smoothedVelocity.y, inputValues.y, 1f / smoothing);

        //taking current position and adding the new one
        currentLookingPos += smoothedVelocity;

        //restricting the camera movement
        currentLookingPos.y = Mathf.Clamp(currentLookingPos.y, lookMax, lookMin);

        //move the camera on the y axis
        playerCam.localRotation = Quaternion.AngleAxis(-currentLookingPos.y, Vector3.right);

        //move the camera on the x axis
        transform.localRotation = Quaternion.AngleAxis(currentLookingPos.x, Vector3.up);

        weapon.rotation = playerCam.rotation;
        weaponCam.rotation = playerCam.rotation;
    }

    void RefreshMultiplayerStateForWeapon()
    {
        float cacheEulY = weapon.localEulerAngles.y;

        Quaternion targetRotation = Quaternion.identity * Quaternion.AngleAxis(weaponAngle, Vector3.right);
        weapon.rotation = Quaternion.Slerp(weapon.rotation, targetRotation, Time.deltaTime * 8f);

        Vector3 finalRotation = weapon.localEulerAngles;
        finalRotation.y = cacheEulY;

        weapon.localEulerAngles = finalRotation;
    }
}
