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

    int[] experienceThresholds = new int[] {
    50, 58, 67, 78, 90, 105, 122, 141, 163, 187, 213, 242,
    274, 308, 345, 384, 425, 468, 514, 561, 611, 662, 715,
    770
};




    private int currentThresholdIndex; // Index to track current threshold

    // Talents variables
    [SerializeField] GameObject[] shieldTalentsUI;
    [SerializeField] GameObject[] dashTalentsUI;
    [SerializeField] GameObject[] healthTalentsUI;
    [SerializeField] GameObject[] attackTalentsUI;
    [SerializeField] GameObject[] skillsTalentsUI;
    [SerializeField] GameObject[] minorTalentsUI;
    [SerializeField] GameObject[] talentsUI;
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
    public float currentAnimationSpeedMultiplier = 1.0f;
    public float expRangePickUp = 7;


    [SerializeField] UnityEngine.UI.Button[] minorTalentsButtons;
    bool minorTalentIsChosen = false;

    [SerializeField] private UnityEngine.UI.Image[] minorTalentSlots; // Слоты для отображения иконок талантов
    [SerializeField] private TextMeshProUGUI[] minorTalentCountTexts; // Тексты для отображения счетчика талантов
    private Dictionary<string, int> selectedTalents = new Dictionary<string, int>(); // Хранит таланты и количество их выборов
    [SerializeField] private Talent[] talents; // Массив или список всех талантов
    private Dictionary<string, int> talentSlotIndices = new Dictionary<string, int>(); // Словарь для хранения индексов слотов для каждого таланта

    bool healthRegenMinorOnce = false;

    int skillCooldownLimit = 7;
    int dashCooldownLimit = 7;
    int healthRegenCooldownLimit = 3;
    bool healthRegenMajorTalentIsChosen = false;




    void Start()
    {
        MinorTalentsSet();
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
        if (level < experienceThresholds.Length + 1)
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
        minorAttackSpeed,
        minorAttackDamage,
        minorAttackRange,
        minorMoveSpeed,
        //minorExpPickUpRange,
        minorDashCooldownMinus,
        minorSpellCooldownMinus,
        minorShieldRegenTime,
        minorHealthRegen
    };
    }

    void LevelUpTalents()
    {
        if (level == 3 || level == 7 || level == 12 || level == 15)
        {
            if (!talentIsChosen)
            {
                InvokeRandomFunction();
                talentIsChosen = true;
            }
        }
        else if (level == 10)
        {
            if (!talentIsChosen)
            {
                ShowSkillsTalentsUI();
                talentIsChosen = true;
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
    void NoShieldWithoutRMBAfterPause()
    {
        if (!Input.GetMouseButtonUp(1))
        {
            playerScript.ShieldStop();
        }
    }

    void ShowMinorTalentsUI()
    {
        talentsUI[0].SetActive(true);
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
        talentsUI[0].SetActive(false);
        for (int i = 0; i < minorTalentsUI.Length; i++)
        {
            minorTalentsUI[i].SetActive(false);
        }
        playerScript.timeIsFrozen = false;
        NoShieldWithoutRMBAfterPause();
    }


    void minorAttackRange(int index)
    {
        minorTalentImages[index].sprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/attackRangeMinor");
        minorTalentText[index].text = "Slight increase in attack range";
        minorTalentsButtons[index].onClick.AddListener(() => minorAttackRangeButton());
    }
    public void minorAttackRangeButton()
    {
        HideMinorTalentsUI();
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);

        // Functionality
        playerScript.attackRangeAdd += 0.15f;
        playerScript.swordSizeMultiplier += 0.15f;
        playerScript.SwordSizeForAttackRange();
        playerScript.AttackRangeCalculation();
        SelectTalent(0);
    }
    void minorAttackSpeed(int index)
    {
        minorTalentImages[index].sprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/attackSpeedMinor");
        minorTalentText[index].text = "Slight increase in attack speed";
        minorTalentsButtons[index].onClick.AddListener(() => minorAttackSpeedButton());
    }
    public void minorAttackSpeedButton()
    {
        HideMinorTalentsUI();
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);

        // Functionality
        currentAnimationSpeedMultiplier += 0.1f;

        playerScript.animator.SetFloat("AttackAnimationSpeed", currentAnimationSpeedMultiplier);
        playerScript.attackSpeedMinorTalentAdaptation = currentAnimationSpeedMultiplier;
        SelectTalent(1);
    }
    void minorAttackDamage(int index)
    {
        minorTalentImages[index].sprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/attackDamageMinor");
        minorTalentText[index].text = "Slight increase in attack damage";
        minorTalentsButtons[index].onClick.AddListener(() => minorAttackDamageButton());
    }
    public void minorAttackDamageButton()
    {
        HideMinorTalentsUI();
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);

        // Functionality
        playerScript.attackAddFromMinorTalents += 0.25f;
        SelectTalent(2);
    }

    void minorMoveSpeed(int index)
    {
        minorTalentImages[index].sprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/moveSpeedMinor");
        minorTalentText[index].text = "Slight increase in move speed";
        minorTalentsButtons[index].onClick.AddListener(() => minorMoveSpeedButton());
    }
    public void minorMoveSpeedButton()
    {
        HideMinorTalentsUI();
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);

        // Functionality
        playerScript.speedAddFromMinorTalent += 0.05f;
        SelectTalent(3);
    }
    void minorExpPickUpRange(int index)
    {
        minorTalentImages[index].sprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/expRangeMinor");
        minorTalentText[index].text = "Increase exp pickup range";
        minorTalentsButtons[index].onClick.AddListener(() => minorExpPickUpRangeButton());
    }
    public void minorExpPickUpRangeButton()
    {
        HideMinorTalentsUI();
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);

        // Functionality
        expRangePickUp += 4;
        ExpRadiusSet();
        SelectTalent(4);
    }
    void ExpRadiusSet()
    {
        Experience[] experienceObjects = FindObjectsOfType<Experience>();

        foreach (Experience experience in experienceObjects)
        {
            SphereCollider sphereCollider = experience.GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                sphereCollider.radius = expRangePickUp;
            }
        }
    }

    void minorDashCooldownMinus(int index)
    {
        minorTalentImages[index].sprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/cooldownTimerMinor");
        minorTalentText[index].text = "Decrease dash cooldown";
        minorTalentsButtons[index].onClick.AddListener(() => minorDashCooldownMinusButton());
    }
    public void minorDashCooldownMinusButton()
    {
        HideMinorTalentsUI();
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);

        // Functionality
        playerScript.dashCooldownMinorAdd += 0.3328571428571429f;
        SelectTalent(5);
        dashCooldownLimit--;
        if (dashCooldownLimit <= 0)
        {
            minorTalentFunctions.Remove(minorDashCooldownMinus);
        }
    }
    void minorSpellCooldownMinus(int index)
    {
        minorTalentImages[index].sprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/cooldownTimerSpellMinor");
        minorTalentText[index].text = "Decrease spell cooldown";
        minorTalentsButtons[index].onClick.AddListener(() => minorSpellCooldownMinusButton());
    }
    public void minorSpellCooldownMinusButton()
    {
        HideMinorTalentsUI();
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);

        // Functionality
        playerScript.spellCooldown -= 1;
        SelectTalent(6);
        skillCooldownLimit--;
        if (skillCooldownLimit <= 0)
        {
            minorTalentFunctions.Remove(minorSpellCooldownMinus);
        }
    }
    void minorShieldRegenTime(int index)
    {
        minorTalentImages[index].sprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/shieldRegenTimerMinor");
        minorTalentText[index].text = "Decrease shield regen time";
        minorTalentsButtons[index].onClick.AddListener(() => minorShieldRegenTimeButton());
    }
    public void minorShieldRegenTimeButton()
    {
        HideMinorTalentsUI();
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);

        // Functionality
        playerScript.shieldIncrementAdd += 1f;
        SelectTalent(7);
    }
    void minorHealthRegen(int index)
    {
        minorTalentImages[index].sprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/healthRegenMinor");
        minorTalentText[index].text = "Slow health regeneration over time";
        minorTalentsButtons[index].onClick.AddListener(() => minorHealthRegenButton());
    }
    public void minorHealthRegenButton()
    {
        HideMinorTalentsUI();
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);

        // Functionality
        
        SelectTalent(8);
        playerScript.healthRegenMinorAdd++;
        if (healthRegenMinorOnce)
        {
            if(!healthRegenMajorTalentIsChosen)
            {
                playerScript.healthRegenCooldownMinus++;
            } else
            {
                playerScript.healthRegenCooldownMinus += 2;
            }
            
            healthRegenCooldownLimit--;
            if (healthRegenCooldownLimit <=0)
            {
                minorTalentFunctions.Remove(minorHealthRegen);
            }
        }
        if (!healthRegenMinorOnce)
        {
            playerScript.StartCoroutine(playerScript.HealthRegenTalent());
            healthRegenMinorOnce = true;
        }
    }

    // ----------------------------------------------------------------------------- Minor talents part end

    void ShowSkillsTalentsUI()
    {
        talentsUI[2].SetActive(true);
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
        talentsUI[2].SetActive(false);
        for (int i = 0; i < skillsTalentsUI.Length; i++)
        {
            skillsTalentsUI[i].SetActive(false);
        }
        playerScript.timeIsFrozen = false;
        NoShieldWithoutRMBAfterPause();
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
        talentsUI[1].SetActive(true);
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
        talentsUI[1].SetActive(false);
        for (int i = 0; i < attackTalentsUI.Length; i++)
        {
            attackTalentsUI[i].SetActive(false);
        }
        playerScript.timeIsFrozen = false;
        NoShieldWithoutRMBAfterPause();
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

        playerScript.attackRangeTalentAdd = 0.45f;
        playerScript.swordSizePushAttackRangeTalentIsOn = 0.45f;
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
        talentsUI[1].SetActive(true);
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
        talentsUI[1].SetActive(false);
        for (int i = 0; i < healthTalentsUI.Length; i++)
        {
            healthTalentsUI[i].SetActive(false);
        }
        playerScript.timeIsFrozen = false;
        NoShieldWithoutRMBAfterPause();
    }
    public void RegenHealthTalent()
    {
        Time.timeScale = 1f;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        HideHealthTalentsUI();
        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Health/regenHealth");
        AssignTalentImage(x);
        playerScript.healthRegenCooldown -= 5;
        playerScript.healthRegenCooldownMinus *= 2;
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
        talentsUI[1].SetActive(true);
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
        talentsUI[1].SetActive(false);
        for (int i = 0; i < dashTalentsUI.Length; i++)
        {
            dashTalentsUI[i].SetActive(false);
        }
        playerScript.timeIsFrozen = false;
        NoShieldWithoutRMBAfterPause();
    }
    public void DoubleDashTalent()
    {
        Time.timeScale = 1f;
        HideDashTalentsUI();
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        playerScript.dashCooldownSeconds = 0.2f;
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
        talentsUI[1].SetActive(true);
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
        talentsUI[1].SetActive(false);
        for (int i = 0; i < shieldTalentsUI.Length; i++)
        {
            shieldTalentsUI[i].SetActive(false);
        }
        playerScript.timeIsFrozen = false;
        NoShieldWithoutRMBAfterPause();
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
            minorTalentIsChosen = false;
        }
    }

    // Minor Talents UI Functionality ---------------------------------------------------------------------
    void MinorTalentsSet()
    {
        talents = new Talent[]
        {
        new Talent { talentName = "minorAttackRange", talentSprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/attackRangeMinor")},
        new Talent { talentName = "minorAttackSpeed", talentSprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/attackSpeedMinor")},
        new Talent { talentName = "minorAttackDamage", talentSprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/attackDamageMinor")},
        new Talent { talentName = "minorMoveSpeed", talentSprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/moveSpeedMinor")},
        new Talent { talentName = "minorExpPickUpRange", talentSprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/expRangeMinor")},
        new Talent { talentName = "minorDashCooldownMinus", talentSprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/cooldownTimerMinor")},
        new Talent { talentName = "minorSpellCooldownMinus", talentSprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/cooldownTimerSpellMinor")},
        new Talent { talentName = "minorShieldRegenTime", talentSprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/shieldRegenTimerMinor")},
        new Talent { talentName = "minorHealthRegen", talentSprite = Resources.Load<Sprite>("TalentsUIMaterials/Minor/healthRegenMinor")}
        };
    }


    public void SelectTalent(int index)
    {
        if (index >= 0 && index < talents.Length)
        {
            Talent selectedTalent = talents[index];
            AddTalent(selectedTalent.talentName, selectedTalent.talentSprite);
        }
    }

    public void AddTalent(string talentName, Sprite talentSprite)
    {
        if (selectedTalents.ContainsKey(talentName))
        {
            // Если талант уже был выбран, увеличиваем его счетчик
            selectedTalents[talentName]++;
            UpdateTalentUI(talentName);
        }
        else
        {
            // Если талант еще не был выбран, добавляем его в список и UI
            selectedTalents[talentName] = 1;
            AddTalentToUI(talentName, talentSprite);
        }
    }

    // Функция для добавления таланта в UI
    private void AddTalentToUI(string talentName, Sprite talentSprite)
    {
        for (int i = 0; i < minorTalentSlots.Length; i++)
        {
            if (minorTalentSlots[i].sprite == null) // Находим первый свободный слот
            {
                minorTalentSlots[i].sprite = talentSprite;
                minorTalentCountTexts[i].text = "";
                minorTalentSlots[i].gameObject.SetActive(true);

                // Сохраняем индекс слота для данного таланта
                talentSlotIndices[talentName] = i;

                break;
            }
        }
    }

    // Функция для обновления UI при увеличении счетчика таланта
    private void UpdateTalentUI(string talentName)
    {
        if (talentSlotIndices.TryGetValue(talentName, out int slotIndex))
        {
            if (slotIndex >= 0 && slotIndex < minorTalentCountTexts.Length)
            {
                // Проверяем, что спрайт в слоте соответствует ожидаемому
                if (minorTalentSlots[slotIndex].sprite == talents[GetTalentIndex(talentName)].talentSprite)
                {
                    minorTalentCountTexts[slotIndex].text = "x" + selectedTalents[talentName].ToString(); // Обновляем счетчик
                }
            }
        }
    }

    // Функция для получения индекса таланта по его имени
    private int GetTalentIndex(string talentName)
    {
        for (int i = 0; i < talents.Length; i++)
        {
            if (talents[i].talentName == talentName)
                return i;
        }
        return -1; // Если не найдено
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
}
