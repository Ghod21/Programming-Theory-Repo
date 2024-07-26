using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    // The script is for data persistence and UI.
    [SerializeField] TextMeshProUGUI timerUI;
    private int timerSeconds;
    private int timerMinutes;
    private bool isTimerRunning = false;
    public int difficultyMeter;

    private void Start()
    {
        timerSeconds = 0;
        timerMinutes = 0;
        isTimerRunning = true;  // Start the timer when the game starts
        StartCoroutine(Timer());
        difficultyMeter = 60;
    }

    private void Update()
    {
        timerUI.text = "Time: " + timerMinutes.ToString("D2") + ":" + timerSeconds.ToString("D2");
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
}
