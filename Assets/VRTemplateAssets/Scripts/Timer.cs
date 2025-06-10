using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public float timeRemaining = 20f;
    public TextMeshProUGUI timerText;

    private Score scoreManager;


    //public GameObject resultsPanel;
    public ResultsUI resultsUI;

    private bool hasEnded = false;

    void Start()
    {
        //resultsPanel.SetActive(false);
        scoreManager = GetComponent<Score>();
        Debug.Log("found score manager");
        Debug.Log(scoreManager);
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
        else if (!hasEnded)
        {
            hasEnded = true;
            timerText.text = "00:00";

            Debug.Log("show the score panel");
            resultsUI.DisplayDataBench();

            // Convert your dataBench into a List<PromptResult>
            List<PromptResult> currentSession = new List<PromptResult>();
            foreach (var entry in scoreManager.dataBench)
            {
                currentSession.Add(new PromptResult
                {
                    prompt = entry.prompt,
                    avatarTag = entry.avatarTag
                });
            }

            PromptSaveSystem.SaveResults(currentSession);
        }
    }

}
