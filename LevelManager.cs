using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    int levelsUnlocked; // The number of levels currently unlocked.
    public Button[] buttons; // Array of buttons for level selection.
    public GameObject[][] levelStars; // 2D array for storing stars for each level.
    public GameObject[] stars; // Array of stars for display.
    public TextMeshProUGUI[] lvlPercentTime; // Text elements for displaying level completion time in percentage.

    // Struct to hold data for each level.
    struct LevelData
    {
        public float percent; // Percentage of level completion.
        public float time; // Time taken to complete the level.
        public int score; // Score based on completion time.
        public float[] requiredClearanceTimes; // Times required to achieve different scores.
    }

    // Dictionary to store required clearance times for each level.
    Dictionary<int, float[]> requiredClearanceTimes = new Dictionary<int, float[]>();

    // Array to hold data for all levels.
    LevelData[] levels;

    void Start()
    {
        // Initializing required clearance times for each level.
        // Example: Level 0 requires 38s for 1 star, 36.5s for 2 stars, 35s for 3 stars.
        requiredClearanceTimes.Add(0, new float[] { 38, 36.5f, 35 });
        // ... other levels initialization

        // Fetching the number of levels unlocked from PlayerPrefs.
        levelsUnlocked = PlayerPrefs.GetInt("levelsUnlocked", 1);

        // Disabling all level buttons initially.
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }

        // Enabling buttons for unlocked levels.
        for (int i = 0; i < levelsUnlocked; i++)
        {
            buttons[i].interactable = true;
        }

        // Loading level data from PlayerPrefs and calculating scores.
        levels = new LevelData[8]; // Assuming 8 levels in total.
        for (int i = 0; i < levels.Length; i++)
        {
            // Fetch level completion percent and time from PlayerPrefs.
            levels[i].percent = PlayerPrefs.GetFloat("%" + (i + 1) + "Cleared", 0f);
            levels[i].time = PlayerPrefs.GetFloat("Level" + (i + 1), 200);

            // Fetch level cleared time.
            float clearedTime = PlayerPrefs.GetFloat("Level" + (i + 1) + "ClearedTime", 200);

            // Update UI for level completion times and percentages.
            if (clearedTime < 199)
            {
                lvlPercentTime[i].text = clearedTime.ToString("F2") + " sec";
            }
            else if (levels[i].percent < 0.99)
            {
                levels[i].percent = levels[i].percent * 100;
                lvlPercentTime[i].text = levels[i].percent.ToString("0") + " %";
            }

            // Initialize the required clearance times for each level.
            levels[i].requiredClearanceTimes = requiredClearanceTimes[i];

            // Calculate the score based on the level's completion time.
            for (int j = 0; j < levels[i].requiredClearanceTimes.Length; j++)
            {
                if (levels[i].time < levels[i].requiredClearanceTimes[j])
                {
                    levels[i].score = j + 1;
                    break;
                }
            }

            // Activate the appropriate number of stars based on the score.
            for (int j = 0; j < levels[i].score; j++)
            {
                levelStars[i][j].SetActive(true);
            }
        }
    }
}
