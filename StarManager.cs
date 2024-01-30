using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StarManager : MonoBehaviour
{
    private float currentTime; // Tracks the current time since the level started or the timer was reset.

    int levelNumber; // The current level number.
    float previousTime; // Stores the previous best time for the level.
    public static float timeDisplayer { get; private set; } // Static property to display time externally.

    public bool timerRunning = true; // Flag to control the timer's running state.

    private void Start()
    {
        // Initialize timer to run at the start of the scene.
        timerRunning = true;
    }

    void FixedUpdate()
    {
        // Update the timer if it's running.
        if (timerRunning)
        {
            currentTime += Time.deltaTime; // Increment the current time by the elapsed time since last frame.
        }
    }

    public void WhenLvlPasses()
    {
        // Stop the timer when a level is completed.
        timerRunning = false;

        // Retrieve the current level number.
        levelNumber = SceneManager.GetActiveScene().buildIndex;

        // Construct a key string to store/retrieve the best time for the level.
        string levelTimeKey = "Level" + levelNumber + "ClearedTime";

        // Retrieve the previous best time for the level, defaulting to 200f if not set.
        previousTime = PlayerPrefs.GetFloat(levelTimeKey, 200f);

        // Update the best time if the current time is better (lower).
        if (currentTime < previousTime)
        {
            PlayerPrefs.SetFloat(levelTimeKey, currentTime);
        }

        // Update the static time displayer for external access.
        timeDisplayer = currentTime;
    }
}
