using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class EnemyManager : MonoBehaviour, IPunObservable
{
    EnemyAnimatorManager animatorManager;
    EnemyHealthManager healthManager;

    public State currentState;
    public GameObject currentTarget;
    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public Rigidbody rb;

    public bool isPreformingAction;
    public bool isInteracting;
    public float rotationSpeed = 25f;
    public float moveSpeed = 5f;
    public float maximumAttackRange = 2f;

    [Header("AI Settings")]
    public float detectionRadius = 20;
    //the higher, and lower, respectively these angles are, the greater detection Field of View (basically like eye sight)
    public float maximumDetectionAngle = 50;
    public float minimumDetectionAngle = -50;

    public float currentRecoveryTime = 0;

    [SerializeField] private EnemyAttackDamageHandler[] attackDamageHandler;

    public Vector3 pos;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            pos = (Vector3)stream.ReceiveNext();
        }
    }

    #region MonoBehaviour Callbacks
    void Awake()
    {
        animatorManager = GetComponent<EnemyAnimatorManager>();
        healthManager = GetComponent<EnemyHealthManager>();
        navMeshAgent = GetComponentInChildren<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        navMeshAgent.enabled = false;
        rb.isKinematic = false;
        pos = transform.position;
    }

    void Update()
    {
        //if (this.enabled) { Debug.Log("Enemy Manager is Running!"); }

        HandleRecoveryTimer();

        isInteracting = animatorManager.anim.GetBool("IsInteracting");

        if (!PhotonNetwork.IsMasterClient) { transform.position = Vector3.Lerp(transform.position, pos, 0.1f); }
    }

    private void FixedUpdate()
    {
        HandleStateMachine();
    }
    #endregion

    #region Private Methods
    private void HandleStateMachine()
    {
        if(currentState != null)
        {
            State nextState = currentState.Tick(this, healthManager, animatorManager);

            if(nextState != null)
            {
                SwitchToNextState(nextState);
            }
        }
    }

    private void SwitchToNextState(State state)
    {
        currentState = state;
    }

    private void HandleRecoveryTimer()
    {
        if(currentRecoveryTime > 0) { currentRecoveryTime -= Time.deltaTime; }

        if (isPreformingAction)
        {
            if(currentRecoveryTime <= 0) { isPreformingAction = false;}
        }
    }

    public void ResetAttackColliders()
    {
        for (int i = 0; i < attackDamageHandler.Length; i++)
        {
            attackDamageHandler[i].hasDamaged = false;
        }
    }
    #endregion
}
