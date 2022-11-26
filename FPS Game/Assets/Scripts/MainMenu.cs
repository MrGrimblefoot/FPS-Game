using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public Launcher launcher;

    public void JoinRandomMatch() { launcher.JoinRandom(); }

    public void CreateMatch() { launcher.CreateRoom(); }

    public void QuitGame() { Application.Quit(); }
}
