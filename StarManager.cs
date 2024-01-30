using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class StarManager : MonoBehaviour {

    private float currentTime;

    int levelNumber;
    float previousTime;
    public static float timeDisplayer { get; private set; }

    public bool timerRunning = true;

    private void Start()
    {
        timerRunning = true;
    }

    void FixedUpdate()
    {
        if (timerRunning)
        {
            currentTime += Time.deltaTime;
        }
    }

    public void WhenLvlPasses()
    {
        timerRunning = false;
        levelNumber = SceneManager.GetActiveScene().buildIndex;
        string levelTimeKey = "Level" + levelNumber + "ClearedTime";
        previousTime = PlayerPrefs.GetFloat(levelTimeKey, 200f);
        if (currentTime < previousTime)
        {
            PlayerPrefs.SetFloat(levelTimeKey, currentTime);
        }
        timeDisplayer = currentTime;
     }
}
