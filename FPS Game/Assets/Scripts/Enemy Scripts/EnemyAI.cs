using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private NavMeshAgent navMeshAgent;
    public Vector3 photonPos;
    [SerializeField] LayerMask whatIsGround, whatIsPlayer;
    [SerializeField] Transform player;
    [SerializeField] private Rigidbody rb;

    [SerializeField] private AISensor sensor;

    [Header("Patrol")]
    [SerializeField] private Vector3 walkPoint;
    bool walkPointSet;
    [SerializeField] private float walkPointRange;

    [Header("Attacking")]
    [SerializeField] private float timeBetweenAttacks;
    bool alreadyAttacked;

    [Header("States")]
    [SerializeField] private float sightRange, attackRange;
    [SerializeField] private bool playerInSightRange, playerInAttackRange;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            photonPos = (Vector3)stream.ReceiveNext();
        }
    }

    void Awake()
    {
        player = FindObjectOfType<CameraLook>().transform;
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.enabled = false;
        sensor = GetComponent<AISensor>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;

        photonPos = transform.position;

        Invoke("CheckIfOnNavmesh", 2f);
    }

    private void CheckIfOnNavmesh() { if (!navMeshAgent.isOnNavMesh) PhotonNetwork.Destroy(gameObject); }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) { transform.position = Vector3.Lerp(transform.position, photonPos, 0.1f); }
        if (!sensor.canSeePlayer && !sensor.canAttackPlayer) { Patroling(); }
        if (sensor.canSeePlayer && !sensor.canAttackPlayer) { Chase(); }
        if (sensor.canSeePlayer && sensor.canAttackPlayer) { Attack(); }

    }

    private void Patroling()
    {
        if (!walkPointSet) { SearchWalkPoint(); }

        if (walkPointSet) { navMeshAgent.SetDestination(walkPoint); }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if(distanceToWalkPoint.magnitude < 1f) { walkPointSet = false; }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if(Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround)) { walkPointSet = true; }
    }

    private void Chase()
    {
        navMeshAgent.SetDestination(player.position);

    }

    private void Attack()
    {

    }

}
