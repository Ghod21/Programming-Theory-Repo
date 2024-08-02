using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    // The script is for data persistence and UI.
    [SerializeField] TextMeshProUGUI timerUI;
    [SerializeField] GameObject player;
    [SerializeField] GameObject[] uiToggle;
    private Player playerScript;
    private int timerSeconds;
    private int timerMinutes;
    private bool isTimerRunning = false;
    public int difficultyMeter;

    private void Start()
    {
        UILogic();

        timerSeconds = 0;
        timerMinutes = 0;

        difficultyMeter = 60;
        playerScript = player.GetComponent<Player>();
        if(SceneManager.GetActiveScene().name == "MainScene")
        {
            isTimerRunning = true;  // Start the timer when the game starts
            StartCoroutine(Timer());
        }
    }

    private void Update()
    {
        timerUI.text = "Time: " + timerMinutes.ToString("D2") + ":" + timerSeconds.ToString("D2");

        if (Input.GetKeyDown(KeyCode.Space) && playerScript.playerHealth <= 0)
        {
            Restart();
        }
    }
    private void UILogic()
    {
        if(SceneManager.GetActiveScene().name == "Menu")
        {
            UILogicMenu();
        }
        else
        {
            UILogicMainScene();
        }
    }
    private void UILogicMenu()
    {
        uiToggle[0].SetActive(false);
        uiToggle[1].SetActive(false);
        uiToggle[2].SetActive(false);
        uiToggle[3].SetActive(false);
    }
    private void UILogicMainScene()
    {
        uiToggle[0].SetActive(true);
        uiToggle[1].SetActive(true);
        uiToggle[2].SetActive(true);
        uiToggle[3].SetActive(true);
    }

    IEnumerator Timer()
    {
        while (isTimerRunning)  // Loop while the timer should run
        {
            yield return new WaitForSeconds(1);  // Wait for 1 second
            timerSeconds++;
            difficultyMeter++;
            if (timerSeconds == 60)
            {
                timerSeconds = 0;
                timerMinutes++;
            }
        }
    }
    public void Restart()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
