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
    [SerializeField] GameObject talentsUI;
    public bool reflectionTalentIsChosenExpManager;
    public bool shieldDamageTalentChosenExpManager;

    bool showTestTalents = true;

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

        if (level == 2 && showTestTalents)
        {
            ShowShieldTalentsUI();
            showTestTalents = false;
        }
    }

    // --------------------------------------------------------------------------- TALENTS SECTION START --------------------------------------------------------------------------------
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
        Debug.Log("HideShieldTalentsUI started");
        Time.timeScale = 1f;
        talentsUI.SetActive(false);
        for (int i = 0; i < shieldTalentsUI.Length; i++)
        {
            shieldTalentsUI[i].SetActive(false);
        }
        playerScript.timeIsFrozen = false;
        Debug.Log("HideShieldTalentsUI completed");
    }

    public void ShieldAttackTalent()
    {
        Time.timeScale = 1f;
        HideShieldTalentsUI();
        playerScript.shieldHealth -= 5;
        playerScript.shieldAttackTalentChosen = true;
        // UI sound

        playerScript.shieldHealth = 5;
        playerScript.shieldIsOnCooldown = false;
    }
    public void ShieldDamageTalent()
    {
        Time.timeScale = 1f;
        HideShieldTalentsUI();
        shieldDamageTalentChosenExpManager = true;
        // UI sound

        playerScript.shieldHealth = 10;
        playerScript.shieldIsOnCooldown = false;
    }
    public void ShieldReflectionTalent()
    {
        Time.timeScale = 1f;
        HideShieldTalentsUI();
        reflectionTalentIsChosenExpManager = true;
        // UI sound

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
