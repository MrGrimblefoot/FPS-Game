using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
//using Photon.Pun;

public class Sway : MonoBehaviour/*PunCallbacks*/
{
    #region Variables
    [SerializeField] private float swayIntensity;
    [SerializeField] private float smooth;

    public bool isMine;

    private Quaternion originRotation;
    public WeaponSystem gun;

    float targetXMouse = 0f;
    float targetYMouse = 0f;

    private PlayerInput playerInput;
    private BasicInputActions basicInputActions;
    #endregion

    #region MonoBehaviour Callbacks
    private void Start()
    {
        //if (!GetComponentInParent<PhotonView>().IsMine) { return; }
        originRotation = transform.localRotation;
        gun = GetComponentInParent<WeaponSystem>();

        #region InputActions
        if (isMine)
        {
            basicInputActions = new BasicInputActions();
            basicInputActions.Player.MouseX.performed += ctx => targetXMouse = ctx.ReadValue<float>();
            basicInputActions.Player.MouseX.Enable();
            basicInputActions.Player.MouseY.performed += ctx => targetYMouse = ctx.ReadValue<float>();
            basicInputActions.Player.MouseY.Enable();
        }
        #endregion

    }
    void Update()
    {
        //if (!GetComponentInParent<PhotonView>().IsMine) { return; }
        UpdateSway();
    }
    #endregion

    #region Private Methods
    private void UpdateSway()
    {
        //calculate target rotation
        Quaternion tempXAdj = Quaternion.AngleAxis((swayIntensity / 10) * targetXMouse, Vector3.up);
        Quaternion tempYAdj = Quaternion.AngleAxis((swayIntensity / 10) * targetYMouse, Vector3.right);
        Quaternion tempZAdj = Quaternion.AngleAxis((swayIntensity / 10) * targetYMouse, Vector3.right);
        Quaternion targetRotation = originRotation * tempXAdj * tempYAdj * tempZAdj;

        //rotate towards target rotation
        if (!gun.aiming) { transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * smooth); }
        else { transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * gun.currentGunData.rotationalKickbackReturnSpeed / gun.currentGunData.ADSSwayMultiplier); }
    }
    #endregion
}
