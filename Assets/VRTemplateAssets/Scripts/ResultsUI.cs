using UnityEngine;
using TMPro;

public class ResultsUI : MonoBehaviour
{
    public TextMeshProUGUI logText;
    public Score scoreManager;


    public void DisplayDataBench()
    {
        if (scoreManager.dataBench.Count == 0)
        {
            logText.text = "No data recorded yet.";
            return;
        }

        logText.text = "";

        foreach (var entry in scoreManager.dataBench)
        {
            logText.text +=
                $"<b>Prompt:</b> {entry.prompt}\n" +
                $"<b>Avatar:</b> {entry.avatarTag}\n\n";
        }
    }
}
