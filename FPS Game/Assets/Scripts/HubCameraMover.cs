using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HubCameraMover : MonoBehaviour
{
    private PlayerInput playerInput;
    private BasicInputActions basicInputActions;
    [SerializeField] private LayerMask selectable;
    Vector3 mousePos;
    Vector3 targetPos;
    Quaternion targetRot;
    Vector3 startPos;
    Quaternion startRot;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    public GameObject testCube;
    public Camera cam;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        #region InputActions
        basicInputActions = new BasicInputActions();
        basicInputActions.Base.Move.performed += CheckForInput;
        basicInputActions.Player.MouseX.Enable();
        basicInputActions.Base.MousePosition.Enable();
        mousePos = basicInputActions.Base.MousePosition.ReadValue<Vector2>();
        #endregion

        targetPos = startPos;
        targetRot = startRot;
    }

    void Update()
    {
        Vector3.Slerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
        Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);

        Debug.Log(cam.ScreenToWorldPoint(mousePos));

        testCube.transform.position = Camera.main.ScreenToWorldPoint(mousePos);
    }

    void CheckForInput(InputAction.CallbackContext context)
    {
        Collider[] colliders = Physics.OverlapSphere(Camera.main.ScreenToWorldPoint(mousePos), 5, selectable);
        foreach (var collider in colliders)
        {
            if(collider.tag != "")
            {
                return;
            }
            else
            {
                targetPos = collider.gameObject.transform.position;
                targetRot = collider.gameObject.transform.rotation;
            }
        }
    }
}
