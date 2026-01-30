using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MainManager : MonoBehaviour
{
    public Brick BrickPrefab;
    public int LineCount = 6;
    public Rigidbody Ball;

    public string Name;
    public Text ScoreText;
    public GameObject GameOverText;
    public Text HighScoreText;
    public Button QuitButton;

    private bool m_Started = false;
    private int m_Points;
    private int m_HighScore = 0;
    private string HighScoreName = "Name";

    private bool m_Loaded = false;
    
    private bool m_GameOver = false;
    public static MainManager mainManager;
    
    // Start is called before the first frame update
    void Awake()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            HighScoreName = data.HighScoreName;
            m_HighScore = data.m_HighScore;
        }

        if(GameObject.Find("MainManager") != null)
        {
            mainManager = this;
            DontDestroyOnLoad(mainManager);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if(GameObject.Find("Quit") != null)
        {
            QuitButton = GameObject.Find("Quit").GetComponent<Button>();
            DontDestroyOnLoad(QuitButton);
        }
    }

    private void Update()
    {
        if(SceneManager.GetActiveScene().name == "main")
        {
            if (!m_Loaded)
            {
                LoadGame();
                QuitButton.gameObject.SetActive(false);
                m_Loaded = true;
            }
            if (!m_Started)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    m_Started = true;
                    float randomDirection = Random.Range(-1.0f, 1.0f);
                    Vector3 forceDir = new Vector3(randomDirection, 1, 0);
                    forceDir.Normalize();

                    Ball.transform.SetParent(null);
                    Ball.AddForce(forceDir * 2.0f, ForceMode.VelocityChange);
                }
            }
            else if (m_GameOver)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    m_Loaded = false;
                    m_GameOver = false;
                    m_Started = false;
                }
            }
        }
        if (FindObjectsByType<Brick>(FindObjectsSortMode.None).Length == 0 && m_Started)
        {
            LoadBlocks();
        }
        
    }

    void AddPoint(int point)
    {
        m_Points += point;
        ScoreText.text = $"Score : {m_Points}";
    }

    public void GameOver()
    {
        SetHighScore();
        QuitButton.gameObject.SetActive(true);
        m_GameOver = true;
        GameOverText.SetActive(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("main");
    }

    private void LoadGame()
    {
        m_Points = 0;
        Ball = GameObject.Find("Ball").GetComponent<Rigidbody>();
        ScoreText = GameObject.Find("ScoreText").GetComponent<Text>();
        HighScoreText = GameObject.Find("HighScoreText").GetComponent<Text>();
        GameOverText = GameObject.Find("GameoverText");
        
        HighScoreText.text = $" High Score: {HighScoreName} : {m_HighScore}";

        GameOverText.SetActive(false);
        LoadBlocks();
    }

    private void LoadBlocks()
    {
        const float step = 0.6f;
        int perLine = Mathf.FloorToInt(4.0f / step);
        
        int[] pointCountArray = new [] {1,1,2,2,5,5};
        for (int i = 0; i < LineCount; ++i)
        {
            for (int x = 0; x < perLine; ++x)
            {
                Vector3 position = new Vector3(-1.5f + step * x, 2.5f + i * 0.3f, 0);
                var brick = Instantiate(BrickPrefab, position, Quaternion.identity);
                brick.PointValue = pointCountArray[i];
                brick.onDestroyed.AddListener(AddPoint);
            }
        }
    }

    public void EditName(string name)
    {
        Name = name;
    }

    private void SetHighScore()
    {
        
        if (m_HighScore < m_Points)
        {
            m_HighScore = m_Points;
            HighScoreName = Name;
        }

        SaveData data = new SaveData();
        data.HighScoreName = HighScoreName;
        data.m_HighScore = m_HighScore;

        string json = JsonUtility.ToJson(data);

        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);

        HighScoreText.text = $" High Score: {HighScoreName} : {m_HighScore}";
    }

    public void Quit()
    {
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    [System.Serializable]
    class SaveData
    {
        public string HighScoreName;
        public int m_HighScore;
    }
}
