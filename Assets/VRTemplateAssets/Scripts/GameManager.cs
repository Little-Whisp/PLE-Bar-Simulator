using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        // Make this a global singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnAvatarServed(string avatarTag, string prompt)
    {
        Debug.Log($"[GameManager] Player served a '{avatarTag}' based on prompt: '{prompt}'");

    }
}
