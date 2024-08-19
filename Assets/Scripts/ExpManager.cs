using System;
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
    [SerializeField] UnityEngine.UI.Image[] chosenTalentsUIImages;
    [SerializeField] GameObject[] chosenTalentsUI;
    public int level = 1;

    AudioSource audioSource;
    AudioClip levelUpSound;

    int[] experienceThresholds = new int[] { 100, 180, 270, 390, 550 };

    private int currentThresholdIndex; // Index to track current threshold

    // Talents variables
    [SerializeField] GameObject[] shieldTalentsUI;
    [SerializeField] GameObject[] dashTalentsUI;
    [SerializeField] GameObject[] healthTalentsUI;
    [SerializeField] GameObject[] attackTalentsUI;
    [SerializeField] GameObject[] skillsTalentsUI;
    [SerializeField] GameObject talentsUI;
    private List<Action> talentFunctions = new List<Action>();
    public bool reflectionTalentIsChosenExpManager;
    public bool shieldDamageTalentChosenExpManager;

    public bool HealthPotionsTalentIsChosenExpManager;

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
        ShowSkillsTalentsUI();
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
    // ----------------------------------------------------------------------------- Skills part start

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

        playerScript.fireBreathTalentIsChosen = true;
    }
    public void LightningSkillTalent()
    {
        Time.timeScale = 1f;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        HideSkillsTalentsUI();
        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Skills/lightningSkill");

        playerScript.lightningTalentIsChosen = true;
    }
    public void BladeVortexSkillTalent()
    {
        Time.timeScale = 1f;
        audioSource.PlayOneShot(playerScript.audioClips[10], DataPersistence.soundsVolume * 4f * DataPersistence.soundAdjustment);
        HideSkillsTalentsUI();
        Sprite x = Resources.Load<Sprite>("TalentsUIMaterials/Skills/vortexSkill");

        playerScript.bladeVortexSkillTalentIsChosen = true;
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
        for (int i = 0; i <healthTalentsUI.Length; i++)
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
            InvokeRandomFunction();
            playerScript.playerExperience -= experienceThresholds[currentThresholdIndex];

            // Move to the next threshold or loop back to the first one
            currentThresholdIndex = (currentThresholdIndex + 1) % experienceThresholds.Length;

            // Update the experience threshold for the next level
            UpdateLevelText();
        }
    }
}
