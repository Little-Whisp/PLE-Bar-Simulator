    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;

    public class GameTimer : MonoBehaviour
    {
        public float timeRemaining = 180f; // 3 minutes
        public TextMeshProUGUI timerText;

        void Update()
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                int minutes = Mathf.FloorToInt(timeRemaining / 60);
                int seconds = Mathf.FloorToInt(timeRemaining % 60);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
            else
            {
                timerText.text = "00:00";
                // Trigger end of game
            }
        }
    }
