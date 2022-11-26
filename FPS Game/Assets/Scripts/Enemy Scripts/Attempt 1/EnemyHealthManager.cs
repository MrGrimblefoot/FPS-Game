using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class EnemyHealthManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public List<Collider> RagdollParts = new List<Collider>();
    private Animator anim;
    private Rigidbody rb;
    private bool isDead;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private List<Material> materialList = null;
    private Material dissolveShaderMat;
    private Material dissolveShaderMat2;
    [SerializeField] private float dissolveRate;
    private float timer;
    private bool readyToDissolve;
    [SerializeField] private float dissolveDelay;
    NavMeshAgent navMeshAgent;
    public float enemyHealth;
    public float bodyPartHealth = 100;
    bool test;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isDead);
        }
        else
        {
            test = (bool)stream.ReceiveNext();
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        skinnedMeshRenderer.GetMaterials(materialList);
        anim.enabled = true;
        SetRagdollParts();
    }

    private void Start()
    {
        dissolveShaderMat = materialList[0];
        dissolveShaderMat.SetFloat("_Dissolve_Ammount", 0);
        dissolveShaderMat2 = materialList[1];
        dissolveShaderMat2.SetFloat("_Dissolve_Ammount", 0);
    }

    private void Update()
    {
        if (isDead)
        {
            Invoke("ReadyToDissolve", dissolveDelay);
        }
        if (readyToDissolve)
        {
            timer += Time.deltaTime * dissolveRate;
            dissolveShaderMat.SetFloat("_Dissolve_Ammount", timer);
            dissolveShaderMat2.SetFloat("_Dissolve_Ammount", timer);
            if (timer >= 1) { timer = 1; PhotonNetwork.Destroy(this.gameObject); }
        }
    }

    void SetRagdollParts()
    {
        Collider[] colliders = this.gameObject.GetComponentsInChildren<Collider>();

        foreach (Collider c in colliders) { if (c.gameObject != this.gameObject && c.gameObject.layer != 2) { RagdollParts.Add(c); } }
    }

    public void DamageEnemy(float damage)
    {
        if (!isDead)
        {
            enemyHealth -= damage;

            if (enemyHealth <= 0)
            {
                enemyHealth = 0;
                photonView.RPC("Die", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void Die()
    {
        rb.useGravity = false;
        this.gameObject.GetComponent<CapsuleCollider>().enabled = false;
        anim.enabled = false;
        navMeshAgent.enabled = false;
        foreach (Collider c in RagdollParts)
        {
            c.isTrigger = false;
            c.attachedRigidbody.velocity = Vector3.zero;
            c.GetComponent<EnemyBodyPartHealthManager>().canDie = false;
        }

        rb.velocity = Vector3.zero;

        isDead = true;
    }

    private void ReadyToDissolve() { readyToDissolve = true; }
}
