using UnityEngine;
using System.IO;


public class DataPersistence : MonoBehaviour
{
    public static DataPersistence Instance;
    public static float musicVolume = 0.5f;
    public static float soundsVolume = 0.5f;
    public static string currentPlayerName = " ";
    public static string playerOne = " ";
    public static string playerTwo = " ";
    public static string playerThree = " ";
    public static bool startInfoDontShowData;

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
        public bool startInfoDontShowAgain;
    }
    public void SaveData()
    {
        GameData data = new GameData();
        data.musicVol = musicVolume;
        data.soundVol = soundsVolume;
        data.player1 = playerOne;
        data.player2 = playerTwo;
        data.player3 = playerThree;
        data.startInfoDontShowAgain = startInfoDontShowData;

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
            startInfoDontShowData = data.startInfoDontShowAgain;
        }
    }

}
