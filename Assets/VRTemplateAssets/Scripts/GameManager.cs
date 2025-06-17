using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Systems")]
    public PromptManager promptManager;
    public GameTimer gameTimer;
    public Transform playerHead;
    public MovementLock playerMovementLock;

    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject tutorialCanvas;
    public GameObject popupPanel;
    public GameObject tutorialImage1;
    public GameObject tutorialImage2;
    public GameObject pauseMenu;



    private bool isPaused = false;
    private int tutorialStep = 0;

    private PlayerData currentData;
    private string dataPath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            dataPath = Application.persistentDataPath + "/";
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (promptManager != null)
            promptManager.ResetCategoryCycle();

        Time.timeScale = 1f;

        if (mainMenu != null)
            mainMenu.SetActive(true);

        if (popupPanel != null)
            popupPanel.SetActive(false);

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (playerMovementLock != null)
            playerMovementLock.SetMovementEnabled(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }


    public void StartGame()
    {
        CreateNewPlaythrough();

        if (mainMenu != null)
            mainMenu.SetActive(false);

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(true);

        popupPanel.SetActive(true);
        tutorialStep = 0;
        ShowTutorialStep();
        Time.timeScale = 0f;
    }

    public void NextTutorialStep()
    {
        Debug.Log("NextTutorialStep called, current step: " + tutorialStep);

        tutorialStep++;
        if (tutorialStep > 1)
            tutorialStep = 0;

        ShowTutorialStep();
    }

    private void ShowTutorialStep()
    {
        tutorialImage1.SetActive(tutorialStep == 0);
        tutorialImage2.SetActive(tutorialStep == 1);
    }

    public void CloseTutorial()
    {
        popupPanel.SetActive(false);
        Time.timeScale = 1f;

        if (playerMovementLock != null)
            playerMovementLock.SetMovementEnabled(true);

        if (gameTimer != null)
            gameTimer.StartTimer();
    }

    public void CloseMenu(GameObject menu)
    {
        menu.SetActive(false);
        Time.timeScale = 1f;

        if (playerMovementLock != null)
            playerMovementLock.SetMovementEnabled(true);

        if (gameTimer != null && !gameTimer.IsRunning())
            gameTimer.StartTimer();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pauseMenu != null)
        {
            if (isPaused)
            {
                // Freeze current forward
                Vector3 forward = new Vector3(playerHead.forward.x, 0f, playerHead.forward.z).normalized;

                float distance = 2.0f; // You can play with this value
                Vector3 menuPosition = playerHead.position + forward * distance;
                pauseMenu.transform.position = menuPosition;

                // Make it face the player
                Vector3 lookDirection = menuPosition - playerHead.position;
                lookDirection.y = 0;
                pauseMenu.transform.rotation = Quaternion.LookRotation(lookDirection);
            }

            pauseMenu.SetActive(isPaused);
        }

        if (playerMovementLock != null)
            playerMovementLock.SetMovementEnabled(!isPaused);

        if (gameTimer != null)
            gameTimer.PauseTimer(isPaused);
    }


    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (playerMovementLock != null)
            playerMovementLock.SetMovementEnabled(true);

        if (gameTimer != null)
            gameTimer.PauseTimer(false);
    }

    public void RestartGame()
    {
        DeleteCurrentData();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMenu()
    {
        DeleteCurrentData();
        SceneManager.LoadScene("MainMenu");
    }

    private void CreateNewPlaythrough()
    {
        currentData = new PlayerData
        {
            playerID = Guid.NewGuid().ToString(),
            startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            dataLog = new List<string>()
        };
    }

    public void LogEvent(string logEntry)
    {
        if (currentData != null)
            currentData.dataLog.Add($"{DateTime.Now:HH:mm:ss} - {logEntry}");
    }

    public void CompletePlaythrough()
    {
        if (currentData != null)
        {
            string json = JsonUtility.ToJson(currentData, true);
            string fileName = $"playthrough_{currentData.playerID}.json";
            File.WriteAllText(dataPath + fileName, json);
            Debug.Log("[GameManager] Playthrough saved: " + fileName);
        }
    }

    private void DeleteCurrentData()
    {
        currentData = null;
        Debug.Log("[GameManager] Current data deleted (session incomplete).");
    }

    public void OnAvatarServed(string avatarTag, string prompt)
    {
        string log = $"Served: {avatarTag} | Prompt: {prompt}";
        Debug.Log("[GameManager] " + log);
        LogEvent(log);
    }
}

[System.Serializable]
public class PlayerData
{
    public string playerID;
    public string startTime;
    public List<string> dataLog;
}
