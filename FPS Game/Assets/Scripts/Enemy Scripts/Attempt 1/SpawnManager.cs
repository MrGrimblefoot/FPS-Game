using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviour
{
    public GameObject playerPrefab1;
    public GameObject playerPrefab2;
    public GameObject playerPrefab3;
    public GameObject playerPrefab4;
    public string playerPrefab1ToSpawn;
    public string playerPrefab2ToSpawn;
    public string playerPrefab3ToSpawn;
    public string playerPrefab4ToSpawn;

    [SerializeField] private Weapon[] guns;

    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private GameObject loadoutPicker;

    [SerializeField] private GameObject cam;
    [SerializeField] private Transform camPos;
    [SerializeField] private GameObject newCam;

    private void Start()
    {
        loadoutPicker = GameObject.Find("HUD/Loadout Selection");
        HandleSpawn();
    }

    public void HandleSpawn()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        newCam = Instantiate(cam, camPos.transform.position, camPos.transform.rotation);
        loadoutPicker.SetActive(true);
    }

    public void SpawnCharacter1()
    {
        Debug.Log("Spawn Character 1");
        Transform tempSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        playerPrefab1.GetComponent<WeaponSystem>().loadout[0] = guns[0];
        playerPrefab1.GetComponent<WeaponSystem>().loadout[1] = guns[1];
        PhotonNetwork.Instantiate(playerPrefab1ToSpawn, tempSpawnPoint.position, tempSpawnPoint.rotation);
        loadoutPicker.SetActive(false);
        Destroy(newCam);
    }

    public void SpawnCharacter2()
    {
        Transform tempSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Debug.Log("Spawn Character 2");
        playerPrefab2.GetComponent<WeaponSystem>().loadout[0] = guns[2];
        playerPrefab2.GetComponent<WeaponSystem>().loadout[1] = guns[3];
        PhotonNetwork.Instantiate(playerPrefab2ToSpawn, tempSpawnPoint.position, tempSpawnPoint.rotation);
        loadoutPicker.SetActive(false);
        Destroy(newCam);
    }

    public void SpawnCharacter3()
    {
        Transform tempSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Debug.Log("Spawn Character 3");
        playerPrefab3.GetComponent<WeaponSystem>().loadout[0] = guns[4];
        playerPrefab3.GetComponent<WeaponSystem>().loadout[1] = guns[5];
        PhotonNetwork.Instantiate(playerPrefab3ToSpawn, tempSpawnPoint.position, tempSpawnPoint.rotation);
        loadoutPicker.SetActive(false);
        Destroy(newCam);
    }

    public void SpawnCharacter4()
    {
        Transform tempSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Debug.Log("Spawn Character 4");
        playerPrefab4.GetComponent<WeaponSystem>().loadout[0] = guns[0];
        playerPrefab4.GetComponent<WeaponSystem>().loadout[1] = guns[3];
        PhotonNetwork.Instantiate(playerPrefab4ToSpawn, tempSpawnPoint.position, tempSpawnPoint.rotation);
        loadoutPicker.SetActive(false);
        Destroy(newCam);
    }
}
