using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HomebaseNavigationManager : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform start;
    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject findGamePage;
    [SerializeField] private GameObject createGamePage;
    private float currentSpeed;
    [SerializeField] private float normalSpeed;
    [SerializeField] private float fineSpeed;
    [SerializeField] private float delay;
    [SerializeField] private Transform[] locations;
    [SerializeField] private GameObject roomButtons;
    [SerializeField] private ComplexLauncher complexLauncher;
    public int backLocation;

    void Start()
    {
        target = start;
        backButton.SetActive(false);
        roomButtons.SetActive(false);
        complexLauncher = FindObjectOfType<ComplexLauncher>();
        backLocation = 0;
        currentSpeed = normalSpeed;
    }

    void Update()
    {
        cam.transform.position = Vector3.Lerp(cam.transform.position, target.position, currentSpeed * Time.deltaTime);
        cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, target.rotation, currentSpeed * Time.deltaTime);
    }

    public void GoToSingleplayer()
    {
        HandleMove(1, true);
    }

    public void GoToMultiplayer()
    {
        HandleMove(2, true);
        roomButtons.SetActive(true);
    }

    public void GoToLoadout()
    {
        HandleMove(3, true);
    }

    public void GoToWOP()
    {
        HandleMove(4, true);
    }

    public void GoToFindRooms()
    {
        HandleMove(5, true);
        roomButtons.SetActive(false);
        StartCoroutine(EnablePage(findGamePage, true, delay));
        currentSpeed = fineSpeed;
    }

    public void GoToCreateRooms()
    {
        HandleMove(6, true);
        roomButtons.SetActive(false);
        StartCoroutine(EnablePage(createGamePage, true, delay));
        currentSpeed = fineSpeed;
    }

    public void GoBack()
    {
        HandleMove(backLocation, false);
        findGamePage.SetActive(false);
        createGamePage.SetActive(false);
        currentSpeed = normalSpeed;
    }

    private void HandleMove(int location, bool backButtonState)
    {
        //Debug.Log("Starting the HandleMove function!");

        for (int i = 0; i < locations.Length; i++) { locations[i].gameObject.SetActive(!backButtonState); }
        backButton.SetActive(backButtonState);

        target = locations[location];

        if (target != locations[5] && target != locations[6]) { backLocation = 0; } else { backLocation = 2; }

        if(target == locations[0]) { roomButtons.SetActive(false); }
        else if (backLocation == 2) { roomButtons.SetActive(false); backButton.SetActive(true); }
        else if (target == locations[2]) { roomButtons.SetActive(true); backButton.SetActive(true); }

        //Debug.Log("Done with the HandleMove function!");
    }

    private IEnumerator EnablePage(GameObject page, bool shouldEnable, float delay)
    {
        yield return new WaitForSeconds(delay);
        page.SetActive(shouldEnable);
    }
}
