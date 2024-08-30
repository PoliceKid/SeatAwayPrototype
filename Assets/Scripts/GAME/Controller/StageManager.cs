using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageManager 
{
    public int CurrentStage { get; private set; }
    public int CurrentLevel { get; private set; }

    private Dictionary<int, List<GameObject>> stages;

    public StageManager()
    {
        CurrentStage = 1;
        CurrentLevel = 1;
        stages = new Dictionary<int, List<GameObject>>();
        LoadAllStages();
    }

    // Method to load all stages and levels into memory
    private void LoadAllStages()
    {
        for (int stage = 1; ; stage++)
        {
            var levels = new List<GameObject>();
            for (int level = 1; ; level++)
            {
                string levelPath = $"Prefabs/Stages/Stage{stage}/Level{level}";
                GameObject levelPrefab = Resources.Load<GameObject>(levelPath);

                if (levelPrefab != null)
                {
                    levels.Add(levelPrefab);
                }
                else
                {
                    break; // No more levels in this stage
                }
            }

            if (levels.Count > 0)
            {
                stages[stage] = levels;
            }
            else
            {
                break; // No more stages
            }
        }
    }

    // Method to get the current level GameObject
    public GameObject GetCurrentLevelPrefab()
    {
        if (stages.ContainsKey(CurrentStage) && stages[CurrentStage].Count >= CurrentLevel)
        {
            return stages[CurrentStage][CurrentLevel - 1];
        }
        else
        {
            Debug.LogWarning("Invalid stage or level.");
            return null;
        }
    }

    // Method to load the next level
    public GameObject LoadNextLevel()
    {
        CurrentLevel++;
        if (!stages.ContainsKey(CurrentStage) || stages[CurrentStage].Count < CurrentLevel)
        {
            CurrentStage++;
            CurrentLevel = 1;
        }

        if (stages.ContainsKey(CurrentStage) && stages[CurrentStage].Count >= CurrentLevel)
        {
            return GetCurrentLevelPrefab();
        }
        else
        {
            Debug.Log("No more levels to load.");
            return null;
        }
    }

    // Method to restart the current level
    public GameObject RestartLevel()
    {
        return GetCurrentLevelPrefab();
    }

    // Method to reset the game and start from the first stage and level
    public void ResetGame()
    {
        CurrentStage = 1;
        CurrentLevel = 1;
    }

    // Method to load a specific stage and level
    public GameObject LoadLevel(int stage, int level)
    {
        if (stages.ContainsKey(stage) && stages[stage].Count >= level)
        {
            CurrentStage = stage;
            CurrentLevel = level;
            return GetCurrentLevelPrefab();
        }
        else
        {
            Debug.LogWarning("Invalid stage or level.");
            return null;
        }
    }
}
