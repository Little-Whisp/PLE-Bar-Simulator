using System.IO;
using UnityEngine;

public class Exporter : MonoBehaviour
{
    public Score scoreManager;

    public void ExportDataBench()
    {
        string path = Path.Combine(Application.persistentDataPath, "PromptResults.csv");

        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("Prompt,Avatar");

            foreach (var entry in scoreManager.dataBench)
            {
                writer.WriteLine($"{entry.prompt},{entry.avatarTag}");
            }
        }

        Debug.Log("[Exporter] Saved file to: " + path);
    }
}
