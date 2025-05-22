using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PromptManager", menuName = "ScriptableObjects/PromptManager", order = 1)]
public class PromptManager : ScriptableObject
{
    [TextArea]
    public List<string> prompts;

    private List<int> usedIndices = new List<int>();

    public string GetRandomPrompt()
    {
        if (prompts == null || prompts.Count == 0)
            return "No prompts available.";

        // All prompts used — reset
        if (usedIndices.Count >= prompts.Count)
        {
            usedIndices.Clear();
        }

        // Pick unused index
        int index;
        do
        {
            index = Random.Range(0, prompts.Count);
        }
        while (usedIndices.Contains(index));

        usedIndices.Add(index);
        return prompts[index];
    }
}

