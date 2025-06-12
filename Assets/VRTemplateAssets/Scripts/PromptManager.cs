using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PromptManager", menuName = "ScriptableObjects/PromptManager", order = 1)]
public class PromptManager : ScriptableObject
{
    [TextArea] public List<string> agencyPrompts;
    [TextArea] public List<string> experiencePrompts;
    [TextArea] public List<string> prosocialPrompts;
    [TextArea] public List<string> antisocialPrompts;

    private Dictionary<string, List<string>> categories;
    private Dictionary<string, List<int>> usedIndicesPerCategory;

    private string[] cycleOrder = new string[] { "Agency", "Experience", "Prosocial", "Antisocial" };
    private int currentCategoryIndex = 0;

    private void OnEnable()
    {
        categories = new Dictionary<string, List<string>>()
        {
            { "Agency", agencyPrompts },
            { "Experience", experiencePrompts },
            { "Prosocial", prosocialPrompts },
            { "Antisocial", antisocialPrompts }
        };

        usedIndicesPerCategory = new Dictionary<string, List<int>>()
        {
            { "Agency", new List<int>() },
            { "Experience", new List<int>() },
            { "Prosocial", new List<int>() },
            { "Antisocial", new List<int>() }
        };
    }

    public string GetNextPrompt()
    {
        string category = cycleOrder[currentCategoryIndex];
        currentCategoryIndex = (currentCategoryIndex + 1) % cycleOrder.Length;

        List<string> prompts = categories[category];
        List<int> usedIndices = usedIndicesPerCategory[category];

        if (prompts.Count == 0)
            return $"No prompts in {category}.";

        if (usedIndices.Count >= prompts.Count)
            usedIndices.Clear();

        int index;
        do
        {
            index = Random.Range(0, prompts.Count);
        } while (usedIndices.Contains(index));

        usedIndices.Add(index);
        return prompts[index];
    }

    public void ResetCategoryCycle()
    {
        currentCategoryIndex = 0;
        foreach (var list in usedIndicesPerCategory.Values)
        {
            list.Clear();
        }
    }
}
