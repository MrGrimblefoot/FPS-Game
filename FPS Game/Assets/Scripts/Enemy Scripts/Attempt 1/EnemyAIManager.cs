using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class EnemyAIManager : MonoBehaviourPunCallbacks, IPunObservable
{
    EnemyAnimatorManager animatorManager;
    EnemyHealthManager healthManager;

    [Tooltip("1, Ambush State/Roam. 2, Chase State. 3, Combat Stance State. 4, Attack State. 5, Return State.")]
    [Range(1, 5)]
    [SerializeField] private int currentState;
    [SerializeField] private GameObject currentTarget;
    [SerializeField] private bool isWanderer;

    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public Rigidbody rb;

    [Space]
    [SerializeField] private bool isPreformingAction;
    [SerializeField] private bool isInteracting;

    [Header("Basic AI Settings")]
    [SerializeField] private float rotationSpeed = 25f;
    [SerializeField] private float detectionRadius = 20;
    [Tooltip("The higher, and lower, respectively these angles are, the greater detection Field of View (basically like eye sight)")]
    [SerializeField] private float maximumDetectionAngle = 50;
    [Tooltip("The higher, and lower, respectively these angles are, the greater detection Field of View (basically like eye sight)")]
    [SerializeField] private float minimumDetectionAngle = -50;
    [SerializeField] private float currentRecoveryTime = 0;

    [Header("Ambush State Variables")]
    [SerializeField] private bool isSleeping;
    [SerializeField] private string waitingAnimation;
    [SerializeField] private string rouseAnimation;
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private float detectionCheckDelayTimer;
    [SerializeField] private float startDetectionCheckDelayTimer;

    [Header("Wander Variables")]
    [SerializeField] private float wanderRadius;
    [SerializeField] private bool start;
    [SerializeField] private LayerMask whatInteruptsWander;

    [Header("Chase State Variables")]
    [SerializeField] private float maximumChaseRange;

    [Header("Attack State Variables")]
    [SerializeField] private EnemyAttackAction[] enemyAttacks;
    [SerializeField] private EnemyAttackDamageHandler[] attackDamageHandler;
    [SerializeField] private EnemyAttackAction currentAttack;
    [SerializeField] private float maximumAttackRange = 2f;

    [Header("Return State Variables")]
    [SerializeField] private float maximumStopRange = 0.5f;
    [SerializeField] private Transform returnPoint;

    [HideInInspector]
    public Vector3 pos;

    public Vector3 randomDestination;
    public GameObject randomDestinationPoint;

    RaycastHit hit;

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
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        navMeshAgent.enabled = false;
        rb.isKinematic = false;
        pos = transform.position;

        Invoke("Check", 2f);
    }

    private void Check()
    {
        if (!navMeshAgent.isOnNavMesh) PhotonNetwork.Destroy(gameObject);
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

    #region Custom Methods
    private void HandleStateMachine()
    {
        switch (currentState)
        {
            case 1:
                if (isWanderer) { WanderState(); }
                else { AmbushState(); }
                break;
            case 2:
                ChaseState();
                break;
            case 3:
                CombatStanceState();
                break;
            case 4:
                AttackState();
                break;
            case 5:
                if (isWanderer) { currentState = 1; }
                else { ReturnState(); }
                break;
        }
    }

    private void AmbushState() //Current State 1
    {
        if (isSleeping && isInteracting == false)
        {
            animatorManager.PlayTargetAnimation(waitingAnimation, true);
        }

        navMeshAgent.enabled = false;

        HandlePlayerDetection(2, 1);
    }

    private void WanderState() //Current State 1
    {
        if (isPreformingAction)
        {
            animatorManager.anim.SetFloat("Vertical", 0, 0.1f, Time.deltaTime);
            currentState = 2;
        }

        navMeshAgent.enabled = true;

        #region Start
        if (start == false)
        {
            Vector3 relativeDirection = transform.InverseTransformDirection(navMeshAgent.desiredVelocity);
            Vector3 targetVelocity = rb.velocity;
            navMeshAgent.enabled = true;
            navMeshAgent.SetDestination(RandomNavMeshLocation());
            rb.velocity = targetVelocity;
            transform.rotation = navMeshAgent.transform.rotation;
            animatorManager.anim.SetFloat("Vertical", 0.5f);
            start = true;
        }
        #endregion

        if (currentTarget == null)
        {
            if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z), transform.forward, out hit, 2, whatInteruptsWander))
            {
                if (hit.transform.gameObject.layer == 0)
                {
                    Debug.Log("Something's in the way! Rerouting!");
                    navMeshAgent.SetDestination(RandomNavMeshLocation());
                    animatorManager.anim.SetFloat("Vertical", 0.5f);
                }
            }

            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                navMeshAgent.SetDestination(RandomNavMeshLocation());
                animatorManager.anim.SetFloat("Vertical", 0.5f);
            }
            else
            {
                Vector3 direction = randomDestination - transform.position;
                direction.y = 0;
                direction.Normalize();

                if (direction == Vector3.zero) { direction = transform.forward; }

                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed / Time.deltaTime);

                navMeshAgent.transform.localPosition = Vector3.zero;
                navMeshAgent.transform.localRotation = Quaternion.identity;
            }

            HandlePlayerDetection(2, 1);
        }
    }

    private void ChaseState() //Current State 2
    {
        if (isPreformingAction)
        {
            animatorManager.anim.SetFloat("Vertical", 0, 0.1f, Time.deltaTime);
            currentState = 2;
        }

        if(currentTarget != null)
        {
            navMeshAgent.enabled = true;

            Vector3 targetDirection = currentTarget.transform.position - transform.position;
            float distanceFromTarget = Vector3.Distance(currentTarget.transform.position, transform.position);

            if (distanceFromTarget > maximumAttackRange)
            {
                animatorManager.anim.SetFloat("Vertical", 1);
            }

            HandleRotateTowardsTarget();
            navMeshAgent.transform.localPosition = Vector3.zero;
            navMeshAgent.transform.localRotation = Quaternion.identity;

            if (distanceFromTarget <= maximumAttackRange) { currentState = 3; }
            else if (distanceFromTarget >= maximumAttackRange && distanceFromTarget <= maximumChaseRange) { currentState = 2; }
            else if (distanceFromTarget > maximumChaseRange)
            {
                if (isWanderer)
                {
                    currentTarget = null;
                    navMeshAgent.SetDestination(RandomNavMeshLocation());
                    animatorManager.anim.SetFloat("Vertical", 0.5f);
                }
                else { currentTarget = returnPoint.gameObject; }
                currentState = 5;
            }            
        }
        else { currentState = 5; }
    }

    private void CombatStanceState() //Current State 3
    {
        float distanceFromTarget = Vector3.Distance(currentTarget.transform.position,transform.position);
        //potentially circle player ot walk around them

        HandleRotateTowardsTarget();

        if (isPreformingAction) { animatorManager.anim.SetFloat("Vertical", 0, 0.1f, Time.deltaTime); navMeshAgent.enabled = false; }
        else { navMeshAgent.enabled = true; }

        if (currentRecoveryTime <= 0 && distanceFromTarget <= maximumAttackRange) { currentState = 4; }
        else if (distanceFromTarget > maximumAttackRange) { currentState = 2; }
        else { currentState = 3; }
    }

    private void AttackState() //Current State 4
    {
        Vector3 targetDirection = currentTarget.transform.position - transform.position;
        float viewableAngle = Vector3.Angle(targetDirection, transform.forward);
        float distanceFromTarget = Vector3.Distance(currentTarget.transform.position, transform.position);

        HandleRotateTowardsTarget();

        if (isPreformingAction) { currentState = 4; }

        if (currentAttack != null)
        {
            //If we are too close to the enemy to perform the our current attack, get a new attack
            if (distanceFromTarget < currentAttack.minimumDistanceNeededToAttack) { currentState = 4; }
            //If we are close enough to attack, then continue
            else if (distanceFromTarget < currentAttack.maximumDistanceNeededToAttack)
            {
                //If oue enemy is within our attack's viewable angle, we attack
                if (viewableAngle <= currentAttack.maximumAttackAngle && viewableAngle >= currentAttack.minimumAttackAngle)
                {
                    if (currentRecoveryTime <= 0 && !isPreformingAction)
                    {
                        animatorManager.anim.SetFloat("Vertical", 0, 0.1f, Time.deltaTime);
                        animatorManager.anim.SetFloat("Horizontal", 0, 0.1f, Time.deltaTime);
                        animatorManager.PlayTargetAnimation(currentAttack.actionAnimation, true);
                        isPreformingAction = true;
                        currentRecoveryTime = currentAttack.recoveryTime;
                        currentAttack = null;
                        currentState = 3;
                    }
                }
            }
        }
        else { GetNewAttack(); }

        currentState = 3;
    }

    private void ReturnState() //Current State 5
    {
        Vector3 returnDirection = returnPoint.position - transform.position;
        float distanceFromTarget = Vector3.Distance(returnPoint.position, transform.position);

        if (distanceFromTarget > maximumStopRange)
        {
            animatorManager.anim.SetFloat("Vertical", 0.5f, 0.1f, Time.deltaTime);
        }

        HandleRotateTowardsTarget();
        navMeshAgent.transform.localPosition = Vector3.zero;
        navMeshAgent.transform.localRotation = Quaternion.identity;

        if (detectionCheckDelayTimer > 0) { detectionCheckDelayTimer -= Time.deltaTime; }
        if (detectionCheckDelayTimer <= 0)
        {
            //This is to check how many times the script is checking for the player every second.
            //Debug.Log(gameObject.name + " is checking for target!");
            #region HandleTargetDetection
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayer);

            for (int i = 0; i < colliders.Length; i++)
            {
                PlayerPolishManager player = colliders[i].transform.GetComponent<PlayerPolishManager>();

                if (player != null)
                {
                    Vector3 targetDirection = player.transform.position - transform.position;
                    float viewableAngle = Vector3.Angle(targetDirection, transform.forward);

                    if (viewableAngle > minimumDetectionAngle && viewableAngle < maximumDetectionAngle)
                    {
                        currentTarget = player.gameObject;
                        isSleeping = false;
                        animatorManager.PlayTargetAnimation(rouseAnimation, true);
                    }
                }
            }
            #endregion
            detectionCheckDelayTimer = startDetectionCheckDelayTimer;
        }

        #region HandleStateChange
        if (currentTarget != null) { currentState = 2; }
        #endregion

        if (distanceFromTarget <= maximumStopRange)
        {
            animatorManager.anim.SetFloat("Vertical", 0);
            navMeshAgent.enabled = false;
            animatorManager.PlayTargetAnimation(waitingAnimation, true);
            currentTarget = null;
            currentState = 1;
        }
        else { currentState = 5; }
    }

    private void GetNewAttack()
    {
        Vector3 targetsDirection = currentTarget.transform.position - transform.position;
        float viewableAngle = Vector3.Angle(targetsDirection, transform.forward);
        float distanceFromTarget = Vector3.Distance(currentTarget.transform.position, transform.position);

        int maxScore = 0;
        for (int i = 0; i < enemyAttacks.Length; i++)
        {
            EnemyAttackAction enemyAttackAction = enemyAttacks[i];

            if (distanceFromTarget <= enemyAttackAction.maximumDistanceNeededToAttack && distanceFromTarget >= enemyAttackAction.minimumDistanceNeededToAttack)
            {
                if (viewableAngle <= enemyAttackAction.maximumAttackAngle && viewableAngle >= enemyAttackAction.minimumAttackAngle)
                {
                    maxScore += enemyAttackAction.attackScore;
                }
            }
        }

        int randomValue = Random.Range(0, maxScore);
        int temporaryScore = 0;

        for (int i = 0; i < enemyAttacks.Length; i++)
        {
            EnemyAttackAction enemyAttackAction = enemyAttacks[i];

            if (distanceFromTarget <= enemyAttackAction.maximumDistanceNeededToAttack && distanceFromTarget >= enemyAttackAction.minimumDistanceNeededToAttack)
            {
                if (viewableAngle <= enemyAttackAction.maximumAttackAngle && viewableAngle >= enemyAttackAction.minimumAttackAngle)
                {
                    if (currentAttack != null) { return; }
                    temporaryScore += enemyAttackAction.attackScore;

                    if (temporaryScore > randomValue) { currentAttack = enemyAttackAction; }
                }
            }
        }
    }

    private void HandleRotateTowardsTarget()
    { 
        //Rotate manually
        if (isPreformingAction)
        {
            Debug.Log("I'm Rotating Manually!");
            Vector3 direction = currentTarget.transform.position - transform.position;
            direction.y = 0;
            direction.Normalize();

            if (direction == Vector3.zero) { direction = transform.forward; }

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            navMeshAgent.enabled = false;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed / Time.deltaTime);
        }
        //Rotate with pathfinding (navmesh)
        else
        {
            Debug.Log("NavMesh Rotating Me!");
            Vector3 relativeDirection = transform.InverseTransformDirection(navMeshAgent.desiredVelocity);
            Vector3 targetVelocity = rb.velocity;

            navMeshAgent.enabled = true;
            if(currentTarget != null) { navMeshAgent.SetDestination(currentTarget.transform.position); }
            rb.velocity = targetVelocity;
            transform.rotation = Quaternion.Slerp(transform.rotation, navMeshAgent.transform.rotation, rotationSpeed / Time.deltaTime);
        }
    }

    private void HandlePlayerDetection(int stateToSwitchTo, int thisState)
    {
        //The timer stuff is to save on performance. It really helps!
        if (detectionCheckDelayTimer > 0) { detectionCheckDelayTimer -= Time.deltaTime; }
        if (detectionCheckDelayTimer <= 0)
        {
            //This is to check how many times the script is checking for the player every second.
            //Debug.Log(gameObject.name + " is checking for target!");
            #region HandleTargetDetection
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayer);

            for (int i = 0; i < colliders.Length; i++)
            {
                PlayerPolishManager player = colliders[i].transform.GetComponent<PlayerPolishManager>();

                if (player != null)
                {
                    Vector3 targetDirection = player.transform.position - transform.position;
                    float viewableAngle = Vector3.Angle(targetDirection, transform.forward);

                    if (viewableAngle > minimumDetectionAngle && viewableAngle < maximumDetectionAngle)
                    {
                        currentTarget = player.gameObject;
                        isSleeping = false;
                        animatorManager.PlayTargetAnimation(rouseAnimation, true);
                    }
                }
            }
            #endregion
            detectionCheckDelayTimer = startDetectionCheckDelayTimer;
        }

        #region HandleStateChange
        if (currentTarget != null) { currentState = stateToSwitchTo; }
        else { currentState = thisState; }
        #endregion

    }

    public Vector3 RandomNavMeshLocation()
    {
        Vector3 finalPos = Vector3.zero;
        Vector3 randomPos = Random.insideUnitSphere * wanderRadius;
        randomPos += transform.position;
        if(NavMesh.SamplePosition(randomPos, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            finalPos = hit.position;
        }
        else { Debug.Log("My random destination is not on NavMesh."); RandomNavMeshLocation(); }

        //Debug.Log(finalPos);
        randomDestination = finalPos;
        return finalPos;
    }

    private void HandleRecoveryTimer()
    {
        if (currentRecoveryTime > 0) { currentRecoveryTime -= Time.deltaTime; }

        if (isPreformingAction)
        {
            if (currentRecoveryTime <= 0) { isPreformingAction = false; }
        }
    }

    public void ResetAttackColliders()
    {
        for (int i = 0; i < attackDamageHandler.Length; i++)
        {
            attackDamageHandler[i].hasDamaged = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 direction = transform.TransformDirection(Vector3.forward) * 2;
        Gizmos.DrawRay(new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z), direction);
    }
    #endregion
}