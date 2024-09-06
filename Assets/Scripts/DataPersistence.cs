using UnityEngine;
using System.IO;


public class DataPersistence : MonoBehaviour
{
    public static DataPersistence Instance;
    public static float musicVolume = 0.5f;
    public static float soundsVolume = 0.5f;
    public static string currentPlayerName = " ";
    public static float currentPlayerScore = 0;
    public static string playerOne = "Bob";
    public static string playerTwo = "Alan";
    public static string playerThree = "Zee";
    public static float playerOneScore = 15;
    public static float playerTwoScore = 10;
    public static float playerThreeScore = 5;
    public static bool startInfoDontShowData = false;
    public static float soundAdjustment = 0.3f;
    public static string lastPlayerName = " ";
    public static bool easyDifficulty = true;

    private void Awake()
    {
        LoadData();


        // start of new code
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        // end of new code

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Function to pull information for between sessions data pers.
    }
    public void Exit()
    {
        SaveData();
    }

    [System.Serializable]
    class GameData
    {
        public float musicVol;
        public float soundVol;
        public string player1;
        public string player2;
        public string player3;
        public float player1Score;
        public float player2Score;
        public float player3Score;
        public bool startInfoDontShowAgain;
        public float soundAdjust;
        public string lastPlayerName;
        public bool easyDifficulty;
    }
    public void SaveData()
    {
        GameData data = new GameData();
        data.musicVol = musicVolume;
        data.soundVol = soundsVolume;
        data.player1 = playerOne;
        data.player2 = playerTwo;
        data.player3 = playerThree;
        data.player1Score = playerOneScore;
        data.player2Score = playerTwoScore;
        data.player3Score = playerThreeScore;
        data.startInfoDontShowAgain = startInfoDontShowData;
        data.lastPlayerName = lastPlayerName;
        data.easyDifficulty = easyDifficulty;

        string json = JsonUtility.ToJson(data);

        File.WriteAllText(Application.persistentDataPath + "/OitFsavefile.json", json);
    }
    public void LoadData()
    {
        string path = Application.persistentDataPath + "/OitFsavefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            GameData data = JsonUtility.FromJson<GameData>(json);

            musicVolume = data.musicVol;
            soundsVolume = data.soundVol;
            playerOne = data.player1;
            playerTwo = data.player2;
            playerThree = data.player3;
            playerOneScore = data.player1Score;
            playerTwoScore = data.player2Score;
            playerThreeScore = data.player3Score;
            startInfoDontShowData = data.startInfoDontShowAgain;
            lastPlayerName = data.lastPlayerName;
            easyDifficulty = data.easyDifficulty;
        }
    }

}
