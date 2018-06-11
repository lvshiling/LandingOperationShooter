using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    private static GameModeManager _instance;

    public static GameModeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameModeManager>();
            }
            return _instance;
        }
    }

    [System.Serializable]
    public struct GameMode
    {
        public string name;
        public float damagBot;
        public float botHealth;
        public int rewardBot;
    }

    public int indexMode;
    public GameMode[] gameMode;

    public void SelectGameMode(int index)
    {
        indexMode = index;
    }

    public GameMode GetGameMode()
    {
        return gameMode[indexMode];
    }
}