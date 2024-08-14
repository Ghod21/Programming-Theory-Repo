using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExpManager : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public UnityEngine.UI.Slider fillSlider;
    GameObject player;
    [SerializeField] GameObject enemy;
    Player playerScript;
    [SerializeField] GameObject fillArea;
    public int level = 1;

    AudioSource audioSource;
    AudioClip levelUpSound;

    int[] experienceThresholds = new int[] { 100, 180, 270, 390, 550 };

    private int currentThresholdIndex; // Index to track current threshold

    // Talents variables
    [SerializeField] GameObject[] shieldTalentsUI;
    [SerializeField] GameObject[] dashTalentsUI;
    [SerializeField] GameObject talentsUI;
    public bool reflectionTalentIsChosenExpManager;
    public bool shieldDamageTalentChosenExpManager;

    bool showTestTalentsLevel2 = true;
    bool showTestTalentsLevel3 = true;

    void Start()
    {
        player = GameObject.Find("Player");
        if (player != null)
        {
            playerScript = player.GetComponent<Player>();
        }
        else
        {
            Debug.LogError("Player object not found in the scene!");
        }
        UpdateLevelText();
        fillSlider.value = 0;
        audioSource = player.GetComponent<AudioSource>();
        levelUpSound = Resources.Load<AudioClip>("Audio/LevelUpSound");
    }

    void Update()
    {
        if (level < 6)
        {
            UpdateFillAmount();
            CheckLevelUp();
        }
        else
        {
            fillSlider.value = 1;
        }
        if(fillSlider.value == 0)
        {
            fillArea.SetActive(false);
        }
        else
        {
            fillArea.SetActive(true);
        }

        if (level == 2 && showTestTalentsLevel2)
        {
            ShowDashTalentsUI();
            showTestTalentsLevel2 = false;
        }
        if (level == 3 && showTestTalentsLevel3)
        {
            ShowShieldTalentsUI();
            showTestTalentsLevel3 = false;
        }
    }

    // --------------------------------------------------------------------------- TALENTS SECTION START --------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------- Dash part start

    void ShowDashTalentsUI()
    {
        talentsUI.SetActive(true);
        for (int i = 0; i < dashTalentsUI.Length; i++)
        {
            dashTalentsUI[i].SetActive(true);
        }
        playerScript.timeIsFrozen = true;
        Time.timeScale = 0f;
    }
    void HideDashTalentsUI()
    {
        Time.timeScale = 1f;
        talentsUI.SetActive(false);
        for (int i = 0; i < dashTalentsUI.Length; i++)
        {
            dashTalentsUI[i].SetActive(false);
        }
        playerScript.timeIsFrozen = false;
    }
    public void DoubleDashTalent()
    {
        Time.timeScale = 1f;
        HideDashTalentsUI();
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        playerScript.dashCooldownSeconds = 5f;
        playerScript.dashCountText.text = playerScript.remainingDashes.ToString();
        playerScript.doubleDashTalentChosen = true;
    }
    public void DashBackwardsTalent()
    {
        Time.timeScale = 1f;
        playerScript.StartCoroutine(playerScript.TrackBackwardsDashState());
        HideDashTalentsUI();
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        playerScript.backwardsDashTalentChosen = true;
        playerScript.dashSpeed = 15;
        playerScript.dashDuration = 0.3f;
        playerScript.dashFillImage.fillAmount = 1;
        playerScript.dashIsOnCooldown = false;
        playerScript.isCooldownCoroutineRunning = false;
    }
    public void LongDashTalent()
    {
        Time.timeScale = 1f;
        HideDashTalentsUI();
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        playerScript.dashSpeed = 15;
        playerScript.dashDuration = 0.3f;
        playerScript.dashFillImage.fillAmount = 1;
        playerScript.dashIsOnCooldown = false;
        playerScript.isCooldownCoroutineRunning = false;
    }

    // ------------------------------------------------------------------------------- Dash part end
    // --------------------------------------------------------------------------- Shield part start

    private void ShowShieldTalentsUI()
    {
        talentsUI.SetActive(true);
        for (int i = 0; i < shieldTalentsUI.Length; i++)
        {
            shieldTalentsUI[i].SetActive(true);
        }
        playerScript.timeIsFrozen = true;
        Time.timeScale = 0f;
    }
    private void HideShieldTalentsUI()
    {
        Time.timeScale = 1f;
        talentsUI.SetActive(false);
        for (int i = 0; i < shieldTalentsUI.Length; i++)
        {
            shieldTalentsUI[i].SetActive(false);
        }
        playerScript.timeIsFrozen = false;
    }

    public void ShieldAttackTalent()
    {
        Time.timeScale = 1f;
        HideShieldTalentsUI();
        playerScript.shieldHealth -= 5;
        playerScript.shieldAttackTalentChosen = true;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);

        playerScript.shieldHealth = 5;
        playerScript.shieldIsOnCooldown = false;
    }
    public void ShieldDamageTalent()
    {
        Time.timeScale = 1f;
        HideShieldTalentsUI();
        shieldDamageTalentChosenExpManager = true;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);

        playerScript.shieldHealth = 10;
        playerScript.shieldIsOnCooldown = false;
    }
    public void ShieldReflectionTalent()
    {
        Time.timeScale = 1f;
        HideShieldTalentsUI();
        reflectionTalentIsChosenExpManager = true;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);

        playerScript.shieldHealth = 10;
        playerScript.shieldIsOnCooldown = false;
    }

    // --------------------------------------------------------------------------- Shield part end
    // --------------------------------------------------------------------------- TALENTS SECTION END ----------------------------------------------------------------------------------

    void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "Level " + level;
        }
    }

    public void UpdateFillAmount()
    {
        if (fillSlider != null)
        {
            float fillAmount = playerScript.playerExperience / experienceThresholds[currentThresholdIndex];
            fillSlider.value = Mathf.Clamp01(fillAmount);
        }
    }

    void CheckLevelUp()
    {
        if (playerScript.playerExperience >= experienceThresholds[currentThresholdIndex])
        {
            audioSource.PlayOneShot(levelUpSound, DataPersistence.soundsVolume * 0.8f * DataPersistence.soundAdjustment);
            level++;
            playerScript.playerExperience -= experienceThresholds[currentThresholdIndex];

            // Move to the next threshold or loop back to the first one
            currentThresholdIndex = (currentThresholdIndex + 1) % experienceThresholds.Length;

            // Update the experience threshold for the next level
            UpdateLevelText();
        }
    }
}
