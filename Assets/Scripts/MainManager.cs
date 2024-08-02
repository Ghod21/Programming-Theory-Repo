#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;

public class MainManager : MonoBehaviour
{
    // The script is for data persistence and UI.
    public static MainManager Instance;
    [SerializeField] TextMeshProUGUI timerUI;
    [SerializeField] GameObject player;
    [SerializeField] GameObject[] uiToggle;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] UnityEngine.UI.Toggle startInfoToggle;
    private int totalUiElements = 16;
    [SerializeField] TMP_InputField nameInputField;

    private Player playerScript;
    private int timerSeconds;
    private int timerMinutes;
    private bool isTimerRunning = false;
    public int difficultyMeter;
    private bool infoIsOn;
    private bool startInfoIsOn;

    private bool correctNameToStart;

    public string PlayerName
    {
        get => DataPersistence.currentPlayerName;
        private set
        {
            // Validate the input: only English letters and length between 3 and 10
            if (IsValidName(value))
            {
                DataPersistence.currentPlayerName = value;
                // Update the InputField to show the valid name
                nameInputField.text = DataPersistence.currentPlayerName;
            }
            else
            {
                Debug.LogWarning("Invalid name. Name must be between 3 and 10 characters and contain only English letters.");
            }
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            startInfoToggle.isOn = DataPersistence.startInfoDontShowData;
        }
        UISceneLogic();
        if (SceneManager.GetActiveScene().name == "Menu" && !DataPersistence.startInfoDontShowData)
        {
            StartInfo();
        }
        infoIsOn = false;

        timerSeconds = 0;
        timerMinutes = 0;

        difficultyMeter = 60;
        playerScript = player.GetComponent<Player>();
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            nameText.text = DataPersistence.currentPlayerName;
            isTimerRunning = true;  // Start the timer when the game starts
            StartCoroutine(Timer());
        }
    }

    private void Update()
    {
        UIStartInfoDontShowLogic();
        timerUI.text = "Time: " + timerMinutes.ToString("D2") + ":" + timerSeconds.ToString("D2");

        if (Input.GetKeyDown(KeyCode.Space) && playerScript.playerHealth <= 0)
        {
            Restart();
        }
        if(startInfoIsOn && Input.anyKeyDown)
        {
            startInfoIsOn = false;
            UILogicMenu();

            for (int i = 14; i < 16; i++)
            {
                if (uiToggle[i] != null)
                {
                    uiToggle[i].SetActive(false);
                }
            }
        }
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("Menu");
            }
        }
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            NameInputFieldUI();
            // Check if the InputField value has changed
            if (nameInputField.text != DataPersistence.currentPlayerName)
            {
                // Update the PlayerName property when the input field value changes
                PlayerName = nameInputField.text;
            }
        }

    }
    //  ....................................................................MENU UI PART START..............................................................
    public void PlayButtonClick()
    {
        if (correctNameToStart)
        {
            DataPersistence.Instance.SaveData();
            SceneManager.LoadScene("MainScene");
        }
    }
    public void ExitButtonClick()
    {
        DataPersistence.Instance.Exit();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void InfoButtonClick()
    {
        if (!infoIsOn)
        {
            uiToggle[5].SetActive(false);
            for (int i = 7; i < totalUiElements + 1; i++)
            {
                if (uiToggle[i] != null)
                {
                    uiToggle[i].SetActive(false);
                }
            }
            for (int i = 14; i < 15; i++)
            {
                if (uiToggle[i] != null)
                {
                    uiToggle[i].SetActive(true);
                }
            }
            uiToggle[16].SetActive(true);

            infoIsOn = true;
        } else
        {
            UILogicMenu();
            infoIsOn = false;
            for (int i = 14; i < totalUiElements + 1; i++)
            {
                if (uiToggle[i] != null)
                {
                    uiToggle[i].SetActive(false);
                }
            }
        }
    }
    private void StartInfo()
    {
        startInfoIsOn = true;
        for (int i = 0; i < totalUiElements + 1; i++)
        {
            if (uiToggle[i] != null)
            {
                uiToggle[i].SetActive(false);
            }
        }
        uiToggle[4].SetActive(true);
        uiToggle[15].SetActive(true);

        // Actual info
        uiToggle[14].SetActive(true);
    }
    private void UISceneLogic()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
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
        for (int i = 0; i < totalUiElements + 1; i++)
        {
            if (uiToggle[i] != null)
            {
                uiToggle[i].SetActive(false);
            }
        }

        for (int i = 4; i < 14; i++)
        {
            if (uiToggle[i] != null)
            {
                uiToggle[i].SetActive(true);
            }
        }
    }
    private void UILogicMainScene()
    {
        for (int i = 0; i < 3; i++)
        {
            if (uiToggle[i] != null)
            {
                uiToggle[i].SetActive(true);
            }
        }
    }

    private void UIStartInfoDontShowLogic()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            DataPersistence.startInfoDontShowData = startInfoToggle.isOn ? true : false;
        }
    }

    private void NameInputFieldUI()
    {
        DataPersistence.currentPlayerName = nameInputField.text;
    }
    private bool IsValidName(string name)
    {
        // Check if the name is between 3 and 10 characters and contains only English letters
        return Regex.IsMatch(name, "^[a-zA-Z]{3,10}$");
    }
    //  ....................................................................MENU UI PART END................................................................


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
