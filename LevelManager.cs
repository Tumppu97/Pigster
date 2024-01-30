using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Numerics;
using UnityEditor;

public class LevelManager : MonoBehaviour
{
    int levelsUnlocked;
    public Button[] buttons;
    public GameObject[][] levelStars;
    public GameObject[] stars;
    public TextMeshProUGUI[] lvlPercentTime;

    struct LevelData
    {
        public float percent;
        public float time;
        public int score;
        public float[] requiredClearanceTimes;
    }

    Dictionary<int, float[]> requiredClearanceTimes = new Dictionary<int, float[]>();

    LevelData[] levels;

    void Start()
    {
        requiredClearanceTimes.Add(0, new float[] { 38, 36.5f, 35 });
        requiredClearanceTimes.Add(1, new float[] { 40, 37.5f, 36 });
        requiredClearanceTimes.Add(2, new float[] { 41.5f, 39.5f, 37.2f });
        requiredClearanceTimes.Add(3, new float[] { 42, 39, 38 });
        requiredClearanceTimes.Add(4, new float[] { 41, 39, 37.5f });
        requiredClearanceTimes.Add(5, new float[] { 43.5f, 42.5f, 41.5f });
        requiredClearanceTimes.Add(6, new float[] { 40, 39.5f, 38.5f });
        requiredClearanceTimes.Add(7, new float[] { 48, 47, 45.5f });


        levelsUnlocked = PlayerPrefs.GetInt("levelsUnlocked", 1);

        
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }

        for (int i = 0; i < levelsUnlocked; i++)
        {
            buttons[i].interactable = true;
        }

        // Load level data from PlayerPrefs
        levels = new LevelData[8];
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].percent = PlayerPrefs.GetFloat("%" + (i + 1) + "Cleared", 0f);
            levels[i].time = PlayerPrefs.GetFloat("Level" + (i + 1), 200);
            
            float clearedTime = PlayerPrefs.GetFloat("Level" + (i + 1) + "ClearedTime", 200);

            if (clearedTime < 199)
            {
                lvlPercentTime[i].text = clearedTime.ToString("F2") + " sec";
            }
            else if (levels[i].percent < 0.99)
            {
                levels[i].percent = levels[i].percent * 100;
                lvlPercentTime[i].text = levels[i].percent.ToString("0") + " %";
            }

            // Initialize the required clearance times for each level
            levels[i].requiredClearanceTimes = requiredClearanceTimes[i];


            Debug.Log(levels[i].requiredClearanceTimes);

            for (int j = 0; j < levels[i].requiredClearanceTimes.Length; j++)
            {
                if (levels[i].time < levels[i].requiredClearanceTimes[j])
                {
                    levels[i].score = j + 1;
                    break;
                }
            }

            for (int j = 0; j < levels[i].score; j++)
            {
                levelStars[i][j].SetActive(true);
            }
        }
    }
}
