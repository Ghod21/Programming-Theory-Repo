#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;

public class MainManager : MonoBehaviour
{
    // The script is for data persistence and UI.
    public static MainManager Instance;
    [SerializeField] TextMeshProUGUI timerUI;
    [SerializeField] TextMeshProUGUI[] wallOfFameLeaders;
    [SerializeField] GameObject player;
    [SerializeField] GameObject[] uiToggle;
    [SerializeField] GameObject[] gameOverUI;
    [SerializeField] GameObject[] winUI;
    [SerializeField] TextMeshProUGUI[] gameOverUIText;
    [SerializeField] TextMeshProUGUI[] winUIText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] UnityEngine.UI.Toggle startInfoToggle;
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] UnityEngine.UI.Image nameFieldImage;
    [SerializeField] GameObject wrongNameText;
    [SerializeField] GameObject audioManager;
    [SerializeField] AudioClip[] menuSounds;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] Player playerScript;
    [SerializeField] TextMeshProUGUI difficultyText;
    public bool win = false;
    public bool canEsc = true;

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
    float soundAdjustment = DataPersistence.soundAdjustment;


    [SerializeField] UnityEngine.UI.Slider bossHealthSlider;
    [SerializeField] GameObject expUIToMoveOnBossSpawn;
    [SerializeField] GameObject bossHPUIBar;
    [SerializeField] GameObject fillSliderAreaToOffAtBossZeroHP;
    [SerializeField] BossEnemy bossEnemy;
    Vector2 rectExpTransformDefault = new Vector2(-60, 31);
    public bool bossFightIsActive;
    [SerializeField] GameObject pauseUIText;
    public bool paused;
    ExpManager expManager;

    //Win
    [SerializeField] GameObject[] confettiSpawnPoints;
    ParticleSystem confetti1;
    ParticleSystem confetti2;

    [SerializeField] UnityEngine.UI.Button easyDifficultyButton;
    [SerializeField] UnityEngine.UI.Button hardDifficultyButton;
    [SerializeField] private TextMeshProUGUI logoText;

    public string PlayerName // ENCAPSULATION
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
        Screen.SetResolution(1024, 580, false);
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            nameInputField.text = DataPersistence.lastPlayerName;
        }
        expManager = FindObjectOfType<ExpManager>();
        canEsc = true;
        confetti1 = Resources.Load<ParticleSystem>("Prefabs/Confetti1");
        confetti2 = Resources.Load<ParticleSystem>("Prefabs/Confetti2");
        leaderBoardUpdated = false;
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            //startInfoToggle.isOn = DataPersistence.startInfoDontShowData;
            WallOfFameUpdate();
        }
        UISceneLogic();
        if (SceneManager.GetActiveScene().name == "Menu" && !DataPersistence.startInfoDontShowData)
        {
            StartInfo();
            DataPersistence.startInfoDontShowData = true;
        }
        infoIsOn = false;

        timerSeconds = 0;
        timerMinutes = 0;

        difficultyMeter = 30; // Switch for difficulty. 30 is normal. __________________________
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            nameText.text = DataPersistence.currentPlayerName;
            if (DataPersistence.easyDifficulty)
            {
                //difficultyText.color = HexToColor("#7EF62B");
                difficultyText.text = "Easy";
            }
            else
            {
                //difficultyText.color = HexToColor("#FCBE2E");
                difficultyText.text = "Hard";
            }

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
        if (DataPersistence.easyDifficulty && SceneManager.GetActiveScene().name == "Menu")
        {
            easyDifficultyButton.interactable = false;
            hardDifficultyButton.interactable = true;
            logoText.color = HexToColor("#7EF62B");

            DataPersistence.easyDifficulty = true;
        } else if (SceneManager.GetActiveScene().name == "Menu")
        {
            hardDifficultyButton.interactable = false;
            easyDifficultyButton.interactable = true;
            logoText.color = HexToColor("#FCBE2E");

            DataPersistence.easyDifficulty = false;
        }
    }

    private void Update()
    {
        if (!playerScript.gameOver)
        {
            //UIStartInfoDontShowLogic();
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
            if (Input.GetKeyDown(KeyCode.Escape) && SceneManager.GetActiveScene().name == "MainScene")
            {
                if (!paused && Time.timeScale != 0f)
                {
                    Time.timeScale = 0f;
                    pauseUIText.SetActive(true);
                    paused = true;
                } else if (paused)
                {
                    Time.timeScale = 1f;
                    pauseUIText.SetActive(false);
                    paused = false;
                    if (playerScript.isShielding && !Input.GetMouseButton(1))
                    {
                        playerScript.ShieldStop();
                    }
                }
            }
        }
        else
        {
            GameOverUI();
        }
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            if (Input.GetKeyDown(KeyCode.M) && canEsc)
            {
                SceneManager.LoadScene("Menu");
            }
            nameText.text = DataPersistence.currentPlayerName + ": " + DataPersistence.currentPlayerScore.ToString();
            if (bossFightIsActive)
            {
                BossFightUIUpdate();
            }
        }
    }

    //  ....................................................................MENU UI PART START..............................................................
    public void PlayButtonClick()
    {
        if (correctNameToStart)
        {
            audioSource.PlayOneShot(menuSounds[0], DataPersistence.soundsVolume * 10f * soundAdjustment);
            DataPersistence.lastPlayerName = PlayerName;
            DataPersistence.Instance.SaveData();
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            audioSource.PlayOneShot(menuSounds[1], DataPersistence.soundsVolume * 4f * soundAdjustment);
            StartCoroutine(NameFieldColorChange());
        }
    }
    public void ExitButtonClick()
    {
        audioSource.PlayOneShot(menuSounds[1], DataPersistence.soundsVolume * 4f * soundAdjustment);
        DataPersistence.Instance.Exit();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void InfoButtonClick()
    {
        audioSource.PlayOneShot(menuSounds[0], DataPersistence.soundsVolume * 10f * soundAdjustment);
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

            easyDifficultyButton.gameObject.SetActive(false);
            hardDifficultyButton.gameObject.SetActive(false);

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
            easyDifficultyButton.gameObject.SetActive(true);
            hardDifficultyButton.gameObject.SetActive(true);
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
        easyDifficultyButton.gameObject.SetActive(false);
        hardDifficultyButton.gameObject.SetActive(false);

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
        easyDifficultyButton.gameObject.SetActive(true);
        hardDifficultyButton.gameObject.SetActive(true);
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

    //private void UIStartInfoDontShowLogic()
    //{
    //    if (SceneManager.GetActiveScene().name == "Menu")
    //    {
    //        DataPersistence.startInfoDontShowData = startInfoToggle.isOn ? true : false;
    //    }
    //}

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
        for (int i = 0; i < expManager.minorTalentSlots.Length; i++)
        {
            if (expManager.minorTalentSlots[i] != null)
            {
                expManager.minorTalentSlots[i].gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < expManager.minorTalentCountTexts.Length; i++)
        {
            if (expManager.minorTalentCountTexts[i] != null)
            {
                expManager.minorTalentCountTexts[i].gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < expManager.chosenTalentsUIImages.Length; i++)
        {
            if (expManager.chosenTalentsUIImages[i] != null)
            {
                expManager.chosenTalentsUIImages[i].gameObject.SetActive(false);
            }
        }
        playerScript.skillFillImage.gameObject.SetActive(false);
        playerScript.skillImage.gameObject.SetActive(false);

        bossHPUIBar.SetActive(false);
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

    public void BossFightUIEnable()
    {
        RectTransform rectTransform = expUIToMoveOnBossSpawn.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(-100, -100);
        bossFightIsActive = true;
        //expUIToMoveOnBossSpawn.SetActive(false);
        bossHPUIBar.SetActive(true);
        bossHealthSlider.minValue = 0;
        if (DataPersistence.easyDifficulty)
        {
            bossHealthSlider.maxValue = 100;
        }
        else
        {
            bossHealthSlider.maxValue = 150;
        }

    }
    void BossFightUIUpdate()
    {
        if (bossEnemy.enemyHealth > 0)
        {
            bossHealthSlider.value = bossEnemy.enemyHealth;
        }
        else
        {
            fillSliderAreaToOffAtBossZeroHP.SetActive(false);
        }
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
            if (win)
            {
                playerNames[0] = "(Winner) " + currentPlayerName;
            } else
            {
                playerNames[0] = currentPlayerName;
            }


            leaderBoardUpdated = true;
        }
        else if (currentPlayerScore > playerScores[1] && !leaderBoardUpdated)
        {
            // Current player takes the second place
            // Shift down the previous second place
            playerScores[2] = playerScores[1];
            playerNames[2] = playerNames[1];

            playerScores[1] = currentPlayerScore;
            if (win)
            {
                playerNames[1] = "(Winner) " + currentPlayerName;
            }
            else
            {
                playerNames[1] = currentPlayerName;
            }

            leaderBoardUpdated = true;
        }
        else if (currentPlayerScore > playerScores[2] && !leaderBoardUpdated)
        {
            // Current player takes the third place
            playerScores[2] = currentPlayerScore;
            if (win)
            {
                playerNames[2] = "(Winner) " + currentPlayerName;
            }
            else
            {
                playerNames[2] = currentPlayerName;
            }

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

    public IEnumerator Win()
    {
        win = true;
        canEsc = false;
        playerScript.playerHealth = 30;
        WinUI();
        bossHPUIBar.SetActive(false);
        StartCoroutine(ClearAfterWin());
        yield return new WaitForSeconds(3f);
        winUI[winUI.Length - 1].SetActive(true);
        canEsc = true;

        while (true)
        {
            int numberOfConfetti = Random.Range(1, 4);

            for (int i = 0; i < numberOfConfetti; i++)
            {
                GameObject randomSpawnPoint = confettiSpawnPoints[Random.Range(0, confettiSpawnPoints.Length)];

                ParticleSystem selectedConfetti = Random.value < 0.5f ? confetti1 : confetti2;

                ParticleSystem spawnedConfetti = Instantiate(selectedConfetti, randomSpawnPoint.transform.position, Quaternion.identity);

                spawnedConfetti.Play();

                playerScript.audioSource.PlayOneShot(playerScript.audioClips[20], DataPersistence.soundsVolume * 3f * DataPersistence.soundAdjustment);
                Destroy(spawnedConfetti.gameObject, 5f);
                yield return new WaitForSeconds(0.2f);
            }

            yield return new WaitForSeconds(1.5f);
        }
    }
    IEnumerator ClearAfterWin()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            enemy.enemyHealth = 0;
            yield return new WaitForSeconds(0.2f);
        }
        Experience[] expObjects = FindObjectsOfType<Experience>();

        foreach (Experience exp in expObjects)
        {
            Destroy(exp.gameObject);
        }
    }
    private void WinUI()
    {
        for (int i = 0; i < uiToggle.Length; i++)
        {
            if (uiToggle[i] != null)
            {
                uiToggle[i].SetActive(false);
            }
        }
        for (int i = 0; i < expManager.minorTalentSlots.Length; i++)
        {
            if (expManager.minorTalentSlots[i] != null)
            {
                expManager.minorTalentSlots[i].gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < expManager.minorTalentCountTexts.Length; i++)
        {
            if (expManager.minorTalentCountTexts[i] != null)
            {
                expManager.minorTalentCountTexts[i].gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < expManager.chosenTalentsUIImages.Length; i++)
        {
            if (expManager.chosenTalentsUIImages[i] != null)
            {
                expManager.chosenTalentsUIImages[i].gameObject.SetActive(false);
            }
        }
        playerScript.skillFillImage.gameObject.SetActive(false);
        playerScript.skillImage.gameObject.SetActive(false);

        UpdateLeaderboard();
        WallOfFameUpdate();
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            // Show GameOver UI
            for (int i = 0; i < winUI.Length - 1; i++)
            {
                if (winUI[i] != null)
                {
                    winUI[i].SetActive(true);
                }
            }
            float[] playerScores = { DataPersistence.playerOneScore, DataPersistence.playerTwoScore, DataPersistence.playerThreeScore };
            if (!deathSoundsPlayed)
            {
                if (DataPersistence.currentPlayerScore <= playerScores[0] && DataPersistence.currentPlayerScore <= playerScores[1] && DataPersistence.currentPlayerScore <= playerScores[2])
                {
                    winUIText[0].text = "Good Run!";// No new high score
                    playerScript.audioSource.PlayOneShot(playerScript.audioClips[8], DataPersistence.soundsVolume * soundAdjustment);
                }
                else
                {
                    winUIText[0].text = "Congratulations - You are on the Wall of Fame!";// New high score
                    playerScript.audioSource.PlayOneShot(playerScript.audioClips[8], DataPersistence.soundsVolume * soundAdjustment);
                }
                deathSoundsPlayed = true;
                AudioManager audioManagerScript = audioManager.GetComponent<AudioManager>();
                audioManagerScript.playMusic = false;
            }


            winUIText[1].text = DataPersistence.currentPlayerName;// PlayerName
            winUIText[2].text = DataPersistence.currentPlayerScore.ToString(); // Player Score
        }
    }

    public void SetEasyDifficulty()
    {
        easyDifficultyButton.interactable = false;
        hardDifficultyButton.interactable = true;
        logoText.color = HexToColor("#7EF62B");
        audioSource.PlayOneShot(menuSounds[0], DataPersistence.soundsVolume * 10f * soundAdjustment);

        DataPersistence.easyDifficulty = true;
    }
    public void SetHardDifficulty()
    {
        hardDifficultyButton.interactable = false;
        easyDifficultyButton.interactable = true;
        logoText.color = HexToColor("#FCBE2E");
        audioSource.PlayOneShot(menuSounds[0], DataPersistence.soundsVolume * 10f * soundAdjustment);

        DataPersistence.easyDifficulty = false;
    }
    public Color HexToColor(string hex)
    {
        hex = hex.Replace("#", "");

        if (hex.Length != 6 && hex.Length != 8)
        {
            Debug.LogError("Invalid HEX color code.");
            return Color.white;
        }

        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte a = 255;

        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }

        return new Color32(r, g, b, a);
    }
}
