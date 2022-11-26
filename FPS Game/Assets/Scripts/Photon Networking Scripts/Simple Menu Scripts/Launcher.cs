using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Launcher : MonoBehaviourPunCallbacks
{
    public GameObject loadingMenu;
    public GameObject mainMenu;

    //(1) when we start up the game...
    public void Awake()
    {
        loadingMenu.SetActive(true);
        mainMenu.SetActive(false);
        //we sync the scenes.
        PhotonNetwork.AutomaticallySyncScene = true;
        Connect();
    }

    //(3) After we connect to Photon, it will call Join(), which...
    public override void OnConnectedToMaster()
    {
        //JoinRandom();

        loadingMenu.SetActive(false);
        mainMenu.SetActive(true);

        base.OnConnectedToMaster();
    }

    //(4a or 5) if there is a room, or if you create a room, this calls StartGame(), which...
    public override void OnJoinedRoom()
    {
        StartGame();

        base.OnJoinedRoom();
    }

    //(4b) If it fails, then this calls CreateRoom(), which...
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();

        base.OnJoinRandomFailed(returnCode, message);
    }

    //(2) then we connect to Photon.
    public void Connect()
    {
        PhotonNetwork.GameVersion = "0.0.0";
        PhotonNetwork.ConnectUsingSettings();
    }

    //(3) ...tries to join you to a random room.
    public void JoinRandom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    //(4b) ...creates a room.
    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom("");
    }

    //(4a or 5) ...checks to see if you are the only one in the room. If so, it will start the level.
    public void StartGame()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            PhotonNetwork.LoadLevel(1);
        }
    }
}
