using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine.UI;

public class ExpManager : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public UnityEngine.UI.Slider fillSlider;
    GameObject player;
    [SerializeField] GameObject enemy;
    Player playerScript;
    [SerializeField] GameObject fillArea;
    [SerializeField] UnityEngine.UI.Image[] chosenTalentsUIImages;
    [SerializeField] GameObject[] chosenTalentsUI;
    public int level = 1;

    AudioSource audioSource;
    AudioClip levelUpSound;

    //int[] experienceThresholds = new int[] { 100, 180, 270, 390, 550 };
    int[] experienceThresholds = new int[] { 25, 40, 60, 85, 110, 140, 175, 215, 260, 310, 365, 425, 495 };




    private int currentThresholdIndex; // Index to track current threshold

    // Talents variables
    [SerializeField] GameObject[] shieldTalentsUI;
    [SerializeField] GameObject[] dashTalentsUI;
    [SerializeField] GameObject[] healthTalentsUI;
    [SerializeField] GameObject[] attackTalentsUI;
    [SerializeField] GameObject[] skillsTalentsUI;
    [SerializeField] GameObject[] minorTalentsUI;
    [SerializeField] GameObject talentsUI;
    private List<Action> talentFunctions = new List<Action>();
    public bool reflectionTalentIsChosenExpManager;
    public bool shieldDamageTalentChosenExpManager;

    public bool HealthPotionsTalentIsChosenExpManager;
    bool talentIsChosen = false;

    //bool minorTalentOneIsChosen = false;
    //bool minorTalentTwoIsChosen = false;
    [SerializeField] UnityEngine.UI.Image[] minorTalentImages;
    [SerializeField] TextMeshProUGUI[] minorTalentText;
    private List<Action<int>> minorTalentFunctions;
    private List<Action<int>> availableTalentFunctions;
    private int currentTalentIndex = 0;
    private float currentAnimationSpeedMultiplier = 1.0f;

    
    [SerializeField] UnityEngine.UI.Button[] minorTalentsButtons;
    bool minorTalentIsChosen = false;
    void Start()
    {
        SetTalentFunctions();
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
        SetMinorTalentFunctions();
        availableTalentFunctions = new List<Action<int>>(minorTalentFunctions);
        //ShowSkillsTalentsUI();
        //ShowMinorTalentsUI(); // Minor talents test
    }

    void Update()
    {
        if (level < experienceThresholds.Length - 1)
        {
            UpdateFillAmount();
            CheckLevelUp();
            LevelUpTalents();
        }
        else
        {
            fillSlider.value = 1;
        }
        if (fillSlider.value == 0)
        {
            fillArea.SetActive(false);
        }
        else
        {
            fillArea.SetActive(true);
        }
    }
    void SetMinorTalentFunctions()
    {
        minorTalentFunctions = new List<Action<int>>
    {
        minorAttackRange,
        minorAttackSpeed,
        //minorAttackDamage,
        //minorMoveSpeed,
        //minorExpPickUpRange,
        //minorDashCooldownMinus,
        //minorSpellCooldownMinus
    };
    }

    void LevelUpTalents()
    {
        if (level == 3 || level == 5 || level == 9 || level == 12)
        {
            if (!talentIsChosen)
            {
                InvokeRandomFunction();
                talentIsChosen = true;
                minorTalentIsChosen = false;
            }
        }
        else if (level == 7)
        {
            if (!talentIsChosen)
            {
                ShowSkillsTalentsUI();
                talentIsChosen = true;
                minorTalentIsChosen = false;
            }
        }
        else if (!minorTalentIsChosen && level != 1)
        {
            ShowMinorTalentsUI();
            minorTalentIsChosen = true;
            talentIsChosen = false;
        }
    }
    public void SetTalentFunctions()
    {
        talentFunctions.Add(ShowAttackTalentsUI);
        talentFunctions.Add(ShowHealthTalentsUI);
        talentFunctions.Add(ShowDashTalentsUI);
        talentFunctions.Add(ShowShieldTalentsUI);
    }

    // Talents UI showcase part start

    public void AssignTalentImage(Sprite talentSprite)
    {
        for (int i = 0; i < chosenTalentsUIImages.Length; i++)
        {
            if (chosenTalentsUIImages[i].sprite == null)
            {
                chosenTalentsUIImages[i].gameObject.SetActive(true);
                chosenTalentsUIImages[i].sprite = talentSprite;
                break;
            }
        }
    }
    public void InvokeRandomFunction()
    {
        if (talentFunctions.Count == 0)
        {
            Console.WriteLine("Out of talents");
            return;
        }

        System.Random random = new System.Random();
        int index = random.Next(talentFunctions.Count);

        talentFunctions[index].Invoke();

        talentFunctions.RemoveAt(index);
    }

    // Talents UI showcase part end

    // --------------------------------------------------------------------------- TALENTS SECTION START --------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------- Minor talents part start

    void ShowMinorTalentsUI()
    {
        talentsUI.SetActive(true);
        for (int i = 0; i < minorTalentsUI.Length; i++)
        {
            minorTalentsUI[i].SetActive(true);
        }
        playerScript.timeIsFrozen = true;
        Time.timeScale = 0f;
        minorTalentsButtons[0].onClick.RemoveAllListeners();
        minorTalentsButtons[1].onClick.RemoveAllListeners();

        availableTalentFunctions = new List<Action<int>>(minorTalentFunctions);

        AssignRandomTalent();
        AssignRandomTalent();

        currentTalentIndex = 0;
    }
    void HideMinorTalentsUI()
    {
        Time.timeScale = 1f;
        talentsUI.SetActive(false);
        for (int i = 0; i < minorTalentsUI.Length; i++)
        {
            minorTalentsUI[i].SetActive(false);
        }
        playerScript.timeIsFrozen = false;
    }
    void AssignRandomTalent()
    {
        if (currentTalentIndex >= minorTalentImages.Length || availableTalentFunctions.Count == 0)
            return;

        int randomIndex = UnityEngine.Random.Range(0, availableTalentFunctions.Count);
        availableTalentFunctions[randomIndex].Invoke(currentTalentIndex);

        availableTalentFunctions.RemoveAt(randomIndex);

        currentTalentIndex++;
    }

    void minorAttackRange(int index)
    {
        minorTalentImages[index].sprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/attackRangeMinor");
        minorTalentText[index].text = "Increase attack range";
        minorTalentsButtons[index].onClick.AddListener(() => minorAttackRangeButton());
    }
    public void minorAttackRangeButton()
    {
        HideMinorTalentsUI();

        // Functionality
        playerScript.attackRangeAdd += 0.2f;
        playerScript.swordSizeMultiplier += 0.1f;
        playerScript.SwordSizeForAttackRange();
        playerScript.AttackRangeCalculation();
    }
    void minorAttackSpeed(int index)
    {
        minorTalentImages[index].sprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/attackSpeedMinor");
        minorTalentText[index].text = "Increase attack speed";
        minorTalentsButtons[index].onClick.AddListener(() => minorAttackSpeedButton());
    }
    public void minorAttackSpeedButton()
    {
        HideMinorTalentsUI();

        // Functionality
        currentAnimationSpeedMultiplier += 0.05f;

        playerScript.animator.SetFloat("AttackAnimationSpeed", currentAnimationSpeedMultiplier);
        playerScript.attackSpeedMinorTalentAdaptation -= 0.05f;
    }
    void minorAttackDamage(int index)
    {
        //minorTalentImages[index].sprite = /* Укажите sprite для другого таланта */;
        minorTalentText[index].text = "Текст для другого таланта";
        HideMinorTalentsUI();

        // Functionality
        playerScript.attackAddFromMinorTalents += 0.17f;
    }
    void minorMoveSpeed(int index)
    {
        //minorTalentImages[index].sprite = /* Укажите sprite для другого таланта */;
        minorTalentText[index].text = "Текст для другого таланта";
        HideMinorTalentsUI();

        // Functionality

    }
    void minorExpPickUpRange(int index)
    {
        //minorTalentImages[index].sprite = /* Укажите sprite для другого таланта */;
        minorTalentText[index].text = "Текст для другого таланта";
        HideMinorTalentsUI();

        // Functionality

    }
    void minorDashCooldownMinus(int index)
    {
        //minorTalentImages[index].sprite = /* Укажите sprite для другого таланта */;
        minorTalentText[index].text = "Текст для другого таланта";
        HideMinorTalentsUI();

        // Functionality

    }
    void minorSpellCooldownMinus(int index)
    {
        //minorTalentImages[index].sprite = /* Укажите sprite для другого таланта */;
        minorTalentText[index].text = "Текст для другого таланта";
        HideMinorTalentsUI();

        // Functionality

    }

    // ----------------------------------------------------------------------------- Minor talents part end

    void ShowSkillsTalentsUI()
    {
        talentsUI.SetActive(true);
        for (int i = 0; i < skillsTalentsUI.Length; i++)
        {
            skillsTalentsUI[i].SetActive(true);
        }
        playerScript.timeIsFrozen = true;
        Time.timeScale = 0f;
    }
    void HideSkillsTalentsUI()
    {
        Time.timeScale = 1f;
        talentsUI.SetActive(false);
        for (int i = 0; i < skillsTalentsUI.Length; i++)
        {
            skillsTalentsUI[i].SetActive(false);
        }
        playerScript.timeIsFrozen = false;
    }
    public void FireSkillTalent()
    {
        Time.timeScale = 1f;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        HideSkillsTalentsUI();
        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Skills/fireSkill");
        chosenTalentsUIImages[4].gameObject.SetActive(true);
        chosenTalentsUIImages[4].sprite = x;

        playerScript.fireBreathTalentIsChosen = true;

        playerScript.skillImageObject.SetActive(true);
        playerScript.skillImage.sprite = Resources.Load<Sprite>("TalentsUIMaterials/Skills/fireSkillImage");
        playerScript.skillFillImage.sprite = Resources.Load<Sprite>("TalentsUIMaterials/Skills/fireSkillImage");
    }
    public void LightningSkillTalent()
    {
        Time.timeScale = 1f;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        HideSkillsTalentsUI();
        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Skills/lightningSkill");
        chosenTalentsUIImages[4].gameObject.SetActive(true);
        chosenTalentsUIImages[4].sprite = x;

        playerScript.lightningTalentIsChosen = true;

        playerScript.skillImageObject.SetActive(true);
        playerScript.skillImage.sprite = Resources.Load<Sprite>("TalentsUIMaterials/Skills/lightningSkillImage");
        playerScript.skillFillImage.sprite = Resources.Load<Sprite>("TalentsUIMaterials/Skills/lightningSkillImage");
    }
    public void BladeVortexSkillTalent()
    {
        Time.timeScale = 1f;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        HideSkillsTalentsUI();
        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Skills/vortexSkill");
        chosenTalentsUIImages[4].gameObject.SetActive(true);
        chosenTalentsUIImages[4].sprite = x;

        playerScript.bladeVortexSkillTalentIsChosen = true;

        playerScript.skillImageObject.SetActive(true);
        playerScript.skillImage.sprite = Resources.Load<Sprite>("TalentsUIMaterials/Skills/vortexSkillImage");
        playerScript.skillFillImage.sprite = Resources.Load<Sprite>("TalentsUIMaterials/Skills/vortexSkillImage");
    }

    // ------------------------------------------------------------------------------- Skills part end
    // ----------------------------------------------------------------------------- Attack part start

    void ShowAttackTalentsUI()
    {
        talentsUI.SetActive(true);
        for (int i = 0; i < attackTalentsUI.Length; i++)
        {
            attackTalentsUI[i].SetActive(true);
        }
        playerScript.timeIsFrozen = true;
        Time.timeScale = 0f;
    }
    void HideAttackTalentsUI()
    {
        Time.timeScale = 1f;
        talentsUI.SetActive(false);
        for (int i = 0; i < attackTalentsUI.Length; i++)
        {
            attackTalentsUI[i].SetActive(false);
        }
        playerScript.timeIsFrozen = false;
    }
    public void DamageAttackTalent()
    {
        Time.timeScale = 1f;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        HideAttackTalentsUI();
        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Attack/damageAttack");
        AssignTalentImage(x);

        playerScript.damageAttackTalentIsChosen = true;
    }
    public void RangeAttackTalent()
    {
        Time.timeScale = 1f;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        HideAttackTalentsUI();
        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Attack/rangeAttack");
        AssignTalentImage(x);

        playerScript.attackRangeTalentAdd = 1f;
        playerScript.swordSizePushAttackRangeTalentIsOn = 0.5f;
        playerScript.attackRangeTalentIsChosen = true;
        playerScript.SwordSizeForAttackRange();
        playerScript.AttackRangeCalculation();
    }
    public void BleedAttackTalent()
    {
        Time.timeScale = 1f;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        HideAttackTalentsUI();
        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Attack/bleedAttack");
        AssignTalentImage(x);

        playerScript.bleedAttackTalentIsChosen = true;
    }


    // ------------------------------------------------------------------------------- Attack part end
    // ----------------------------------------------------------------------------- Health part start

    void ShowHealthTalentsUI()
    {
        talentsUI.SetActive(true);
        for (int i = 0; i < healthTalentsUI.Length; i++)
        {
            healthTalentsUI[i].SetActive(true);
        }
        playerScript.timeIsFrozen = true;
        Time.timeScale = 0f;
    }
    void HideHealthTalentsUI()
    {
        Time.timeScale = 1f;
        talentsUI.SetActive(false);
        for (int i = 0; i < healthTalentsUI.Length; i++)
        {
            healthTalentsUI[i].SetActive(false);
        }
        playerScript.timeIsFrozen = false;
    }
    public void RegenHealthTalent()
    {
        Time.timeScale = 1f;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        HideHealthTalentsUI();
        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Health/regenHealth");
        AssignTalentImage(x);
        playerScript.StartCoroutine(playerScript.HealthRegenTalent());
    }
    public void PotionsHealthTalent()
    {
        Time.timeScale = 1f;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        HideHealthTalentsUI();
        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Health/potionsHealth");
        AssignTalentImage(x);
        HealthPotionsTalentIsChosenExpManager = true;
    }
    public void VampireHealthTalent()
    {
        Time.timeScale = 1f;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        HideHealthTalentsUI();
        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Health/vampireHealth");
        AssignTalentImage(x);
        playerScript.vampireHealthTalentIsChosen = true;
    }

    // ------------------------------------------------------------------------------- Health part end
    // ------------------------------------------------------------------------------- Dash part start

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
        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Dash/doubleDash");
        AssignTalentImage(x);
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
        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Dash/backDash");
        AssignTalentImage(x);
    }
    public void SprintDashTalent()
    {
        Time.timeScale = 1f;
        HideDashTalentsUI();
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        playerScript.sprintDashTalentChosen = true;
        playerScript.dashFillImage.fillAmount = 1;
        playerScript.dashIsOnCooldown = false;
        playerScript.isCooldownCoroutineRunning = false;
        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Dash/sprintDash");
        AssignTalentImage(x);
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

        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Shield/attackShield");
        AssignTalentImage(x);
    }
    public void ShieldDamageTalent()
    {
        Time.timeScale = 1f;
        HideShieldTalentsUI();
        shieldDamageTalentChosenExpManager = true;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);

        playerScript.shieldHealth = 10;
        playerScript.shieldIsOnCooldown = false;

        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Shield/thornsShield");
        AssignTalentImage(x);
    }
    public void ShieldReflectionTalent()
    {
        Time.timeScale = 1f;
        HideShieldTalentsUI();
        reflectionTalentIsChosenExpManager = true;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);

        playerScript.shieldHealth = 10;
        playerScript.shieldIsOnCooldown = false;

        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Shield/reflectShield");
        AssignTalentImage(x);
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
            //InvokeRandomFunction();
            playerScript.playerExperience -= experienceThresholds[currentThresholdIndex];

            // Move to the next threshold or loop back to the first one
            currentThresholdIndex = (currentThresholdIndex + 1) % experienceThresholds.Length;

            // Update the experience threshold for the next level
            UpdateLevelText();
        }
    }
}
