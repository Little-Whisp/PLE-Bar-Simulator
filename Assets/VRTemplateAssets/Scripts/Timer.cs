using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public float timeRemaining = 20f; 
    public TextMeshProUGUI timerText;

    public GameObject resultsPanel;
    public ResultsUI resultsUI; 

    private bool hasEnded = false;

    void Start()
    {
        resultsPanel.SetActive(false); 
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

            resultsPanel.SetActive(true);     
            resultsUI.DisplayDataBench();      
        }
    }
}
