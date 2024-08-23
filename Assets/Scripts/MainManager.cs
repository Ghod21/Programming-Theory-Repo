#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class MainManager : MonoBehaviour
{
    // The script is for data persistence and UI.
    public static MainManager Instance;
    [SerializeField] TextMeshProUGUI timerUI;
    [SerializeField] TextMeshProUGUI[] wallOfFameLeaders;
    [SerializeField] GameObject player;
    [SerializeField] GameObject[] uiToggle;
    [SerializeField] GameObject[] gameOverUI;
    [SerializeField] TextMeshProUGUI[] gameOverUIText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Toggle startInfoToggle;
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] Image nameFieldImage;
    [SerializeField] GameObject wrongNameText;
    [SerializeField] GameObject audioManager;
    [SerializeField] AudioClip[] menuSounds;
    [SerializeField] public AudioSource audioSource;
    private Player playerScript;


    private int totalUiElements = 16;
    private int timerSeconds;
    private int timerMinutes;
    private bool isTimerRunning = false;
    public int difficultyMeter;
    private bool infoIsOn;
    private bool startInfoIsOn;
    bool deathSoundsPlayed;
    private bool correctNameToStart;
    bool leaderBoardUpdated;
    float soundAdjustment = 0.6f;

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
        leaderBoardUpdated = false;
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            startInfoToggle.isOn = DataPersistence.startInfoDontShowData;
            WallOfFameUpdate();
        }
        UISceneLogic();
        if (SceneManager.GetActiveScene().name == "Menu" && !DataPersistence.startInfoDontShowData)
        {
            StartInfo();
        }
        infoIsOn = false;

        timerSeconds = 0;
        timerMinutes = 0;

        difficultyMeter = 30; // Switch for difficulty. 30 is normal. __________________________
        playerScript = player.GetComponent<Player>();
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            nameText.text = DataPersistence.currentPlayerName;
            isTimerRunning = true;  // Start the timer when the game starts
            StartCoroutine(Timer());
            for (int i = 0; i < gameOverUI.Length; i++)
            {
                if (gameOverUI[i] != null)
                {
                    gameOverUI[i].SetActive(false);
                }
            }
            DataPersistence.currentPlayerScore = 0;
        }
    }

    private void Update()
    {
        if (!playerScript.gameOver)
        {
            UIStartInfoDontShowLogic();
            timerUI.text = "Time: " + timerMinutes.ToString("D2") + ":" + timerSeconds.ToString("D2");
            if (startInfoIsOn && Input.anyKeyDown)
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
            if (SceneManager.GetActiveScene().name == "Menu")
            {
                NameInputFieldUI();
                // Check if the InputField value has changed
                if (nameInputField.text != DataPersistence.currentPlayerName)
                {
                    // Update the PlayerName property when the input field value changes
                    PlayerName = nameInputField.text;
                }

                // Check if the name is valid
                correctNameToStart = IsValidName(nameInputField.text);
            }
        }
        else
        {
            GameOverUI();
        }
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("Menu");
            }
            nameText.text = DataPersistence.currentPlayerName + ": " + DataPersistence.currentPlayerScore.ToString();
        }
    }
    //  ....................................................................MENU UI PART START..............................................................
    public void PlayButtonClick()
    {
        if (correctNameToStart)
        {
            audioSource.PlayOneShot(menuSounds[0], DataPersistence.soundsVolume * 4f * soundAdjustment);
            DataPersistence.Instance.SaveData();
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            audioSource.PlayOneShot(menuSounds[1], DataPersistence.soundsVolume * 1.2f * soundAdjustment);
            StartCoroutine(NameFieldColorChange());
        }
    }
    public void ExitButtonClick()
    {
        audioSource.PlayOneShot(menuSounds[1], DataPersistence.soundsVolume * 1.2f * soundAdjustment);
        DataPersistence.Instance.Exit();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void InfoButtonClick()
    {
        audioSource.PlayOneShot(menuSounds[0], DataPersistence.soundsVolume * 4f * soundAdjustment);
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
        }
        else
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
        for (int i = 0; i < uiToggle.Length; i++)
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
    private void GameOverUI()
    {
        for (int i = 0; i < uiToggle.Length; i++)
        {
            if (uiToggle[i] != null)
            {
                uiToggle[i].SetActive(false);
            }
        }
        UpdateLeaderboard();
        WallOfFameUpdate();
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            // Show GameOver UI
            for (int i = 0; i < gameOverUI.Length; i++)
            {
                if (gameOverUI[i] != null)
                {
                    gameOverUI[i].SetActive(true);
                }
            }
            float[] playerScores = { DataPersistence.playerOneScore, DataPersistence.playerTwoScore, DataPersistence.playerThreeScore };
            if (!deathSoundsPlayed)
            {
                if (DataPersistence.currentPlayerScore <= playerScores[0] && DataPersistence.currentPlayerScore <= playerScores[1] && DataPersistence.currentPlayerScore <= playerScores[2])
                {
                    gameOverUIText[0].text = "Good Try!";// No new high score
                    playerScript.audioSource.PlayOneShot(playerScript.audioClips[7], DataPersistence.soundsVolume * soundAdjustment);
                }
                else
                {
                    gameOverUIText[0].text = "Congratulations - You are on the Wall of Fame!";// New high score
                    playerScript.audioSource.PlayOneShot(playerScript.audioClips[8], DataPersistence.soundsVolume * soundAdjustment);
                }
                deathSoundsPlayed = true;
                AudioManager audioManagerScript = audioManager.GetComponent<AudioManager>();
                audioManagerScript.playMusic = false;
            }


            gameOverUIText[1].text = DataPersistence.currentPlayerName;// PlayerName
            gameOverUIText[2].text = DataPersistence.currentPlayerScore.ToString(); // Player Score
        }
    }
    private void WallOfFameUpdate()
    {
        wallOfFameLeaders[0].text = DataPersistence.playerOne + ": " + DataPersistence.playerOneScore.ToString();
        wallOfFameLeaders[1].text = DataPersistence.playerTwo + ": " + DataPersistence.playerTwoScore.ToString();
        wallOfFameLeaders[2].text = DataPersistence.playerThree + ": " + DataPersistence.playerThreeScore.ToString();
    }
    IEnumerator NameFieldColorChange()
    {
        wrongNameText.SetActive(true);
        ChangeColor("#FFCCC9");
        yield return new WaitForSeconds(1f);
        ChangeColor("#CDFFC9");
        wrongNameText.SetActive(false);
    }
    public void ChangeColor(string hexColor)
    {
        if (ColorUtility.TryParseHtmlString(hexColor, out Color newColor))
        {
            nameFieldImage.color = newColor;
        }
        else
        {
            Debug.LogError("Invalid Hex Color");
        }
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
    private void UpdateLeaderboard()
    {
        // Create arrays for player names and scores
        string[] playerNames = { DataPersistence.playerOne, DataPersistence.playerTwo, DataPersistence.playerThree };
        float[] playerScores = { DataPersistence.playerOneScore, DataPersistence.playerTwoScore, DataPersistence.playerThreeScore };

        string currentPlayerName = DataPersistence.currentPlayerName;
        float currentPlayerScore = DataPersistence.currentPlayerScore;

        // Check if the current player's score is higher than any of the top three scores
        if (currentPlayerScore > playerScores[0] && !leaderBoardUpdated)
        {
            // Current player takes the first place
            // Shift down the previous first and second places
            playerScores[2] = playerScores[1];
            playerNames[2] = playerNames[1];

            playerScores[1] = playerScores[0];
            playerNames[1] = playerNames[0];

            playerScores[0] = currentPlayerScore;
            playerNames[0] = currentPlayerName;

            leaderBoardUpdated = true;
        }
        else if (currentPlayerScore > playerScores[1] && !leaderBoardUpdated)
        {
            // Current player takes the second place
            // Shift down the previous second place
            playerScores[2] = playerScores[1];
            playerNames[2] = playerNames[1];

            playerScores[1] = currentPlayerScore;
            playerNames[1] = currentPlayerName;

            leaderBoardUpdated = true;
        }
        else if (currentPlayerScore > playerScores[2] && !leaderBoardUpdated)
        {
            // Current player takes the third place
            playerScores[2] = currentPlayerScore;
            playerNames[2] = currentPlayerName;

            leaderBoardUpdated = true;
        }

        // Update DataPersistence with the new leaderboard
        DataPersistence.playerOne = playerNames[0];
        DataPersistence.playerOneScore = playerScores[0];
        DataPersistence.playerTwo = playerNames[1];
        DataPersistence.playerTwoScore = playerScores[1];
        DataPersistence.playerThree = playerNames[2];
        DataPersistence.playerThreeScore = playerScores[2];

        // Save updated data
        DataPersistence.Instance.SaveData();
    }


}
