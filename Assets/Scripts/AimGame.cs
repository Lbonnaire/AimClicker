// namespaces used
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using Unity.VisualScripting;

/// <summary>
/// Main class for the shooting tasks, currently only contains 1 type of task
/// uses the singleton of the Main class to store values from the aim task.
/// </summary>

public class AimGame : MonoBehaviour
{
    //~~~~~~~~ Variable initialization ~~~~~~~~~

    //Unity game object initialization
    public GameObject Orb,
                        UI,
                        Panel,
                        Player,
                        
                        TaskCompleted;

    // movement controls
    private StarterAssetsInputs _input;

    // score calculation
    int tarHit = 0;
    int tarMiss = 0;
    public float accuracy = 100;
    public float endScore;

    // value management
    public Dictionary<string, float> taskStatsDict;
    private string[] taskStatsArray;

    //task elements
    ArrayList freeSpaces;
    private Vector2 freeSpot;
    float interval;
    Vector2 gridMid;
    float defZ;
    Vector2[,] orbGrid;

    // UI Text elements
    public TMP_Text AccText,
                    Hits,
                    Misses,
                    EndAccText,
                    EndHits,
                    EndMisses,
                    FinalScore,
                    HighScore,
                    timeText,
                    
                    CountdownGO;

    // UI elements
    public Button   QuitButton,
                    EndStatsClose;

    // timers init
    public float timeRemaining = 10;
    public bool timerIsRunning = false;

    public float countdownTimer = 3;
    public bool countdownIsRunning = false;


    // Start is called before the first frame update
    void Start()
    {
        // method call to attach UI elements to script
        InitializeGameObjects();
        timerIsRunning = false;


        // Task base value and variable initialisation
        interval = Orb.transform.localScale.x * 1.2f; // space between orbs
        freeSpaces = new ArrayList(); // list of free slots for orbs to spawn    
        orbGrid = new Vector2[5, 5];
        float ymid = 3f; // middle of the grid
        gridMid = new Vector2(0f, ymid); // middle of the grid in 2D
        defZ = 4f;

        // add all empty orb slots into the list
        for (var i = 0; i < 5; i++)
        {
            for (var j = 0; j < 5; j++)
            {
                orbGrid[i, j] = new Vector2(i, j);
                freeSpaces.Add(orbGrid[i, j]);
            }
        }

        // Add three new orbs to the task
        for (var i = 0; i < 3; i++)
        {
            InstantiateNewOrb();
        }
    }

    // Update method is called every frame
    // used to handle timers for the pre task countdown and the task duration 
    void Update()
    {
        // if the task hasn't started start the countdown
        if (countdownIsRunning)
        {
            Countdown();
            Cursor.visible = false;
        }
        // if the countdown has ended and the task has started check if the timer is finished
        else if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                // when the task is finished show the end screen
                timeRemaining = 0;
                DisplayTime(timeRemaining);
                timerIsRunning = false;
                TaskCompleted.SetActive(true);
                Cursor.lockState = false ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = true;
                UpdateStats();
            }
        }
    }


    // Initializee the gameobjects from the Unity hierarchy and link them to the script
    private void InitializeGameObjects()
    {
        _input = GameObject.Find("Player").GetComponent<StarterAssetsInputs>();
        UI = GameObject.Find("UI");
        Panel = UI.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject;
        TaskCompleted = UI.transform.Find("Canvas").gameObject.transform.Find("TaskCompleted").gameObject;
        Player = GameObject.Find("PlayerCameraRoot");
        EndStatsClose = TaskCompleted.transform.Find("EndStats").GetComponentInChildren<Button>();
        EndStatsClose.onClick.AddListener(EndButtonClicked);
        QuitButton = UI.transform.Find("Canvas").gameObject.transform.Find("QuitButton").gameObject.GetComponent<Button>();
        QuitButton.onClick.AddListener(Quit);

        CountdownGO = UI.transform.Find("Canvas").gameObject.transform.Find("Countdown").gameObject.GetComponent<TMP_Text>();
        AccText = GameObject.Find("Accuracy").GetComponent<TMP_Text>();
        Hits = GameObject.Find("Hits").GetComponent<TMP_Text>();
        Misses = GameObject.Find("Misses").GetComponent<TMP_Text>();
        timeText = GameObject.Find("Time").GetComponent<TMP_Text>();
        EndAccText = GameObject.Find("EndAccuracy").GetComponent<TMP_Text>();
        EndHits = GameObject.Find("EndHits").GetComponent<TMP_Text>();
        EndMisses = GameObject.Find("EndMisses").GetComponent<TMP_Text>();
        FinalScore = GameObject.Find("FinalScore").GetComponent<TMP_Text>();
        HighScore = GameObject.Find("Highscore").GetComponent<TMP_Text>();
        timeText = GameObject.Find("Time").GetComponent<TMP_Text>();


        AccText.text = "Accuracy: 100%";
        CountdownGO.gameObject.SetActive(false);
        TaskCompleted.SetActive(false);

        // gather stats from the value handler in Main. In this case only retrieves the previous highscore
        taskStatsArray = new string[1] { "highscore" };
        taskStatsDict = new Dictionary<string, float>();
        for (int i = 0; i < taskStatsArray.Length; i++)
        {
            taskStatsDict.Add(taskStatsArray[i], 0f);
        }
        Dictionary<string, float> dictFromData = Main.instance.GetDataDict("taskStatsDict");
        if (Main.instance.GetDataDict("taskStatsDict") == null)
        {
            Main.instance.StoreMainDict(taskStatsDict, "taskStatsDict");
            Main.instance.SaveData();

        }

    }

    // Change visible UI elements and start the task countdown
    public void StartTask()
    {
        Panel.SetActive(false);
        CountdownGO.gameObject.SetActive(true);
        countdownIsRunning = true;
        Countdown();
    }

    // Handle the countdown prior to the task
    public void Countdown()
    {
        if (countdownIsRunning)
        {
            // countdown timer is running
            if (countdownTimer > 0)
            {
                countdownTimer -= Time.deltaTime;
                DisplayCountdown(countdownTimer);
            }
            // end countdown and start task timer
            else
            {
                countdownTimer = 0;
                countdownIsRunning = false;
                CountdownGO.gameObject.SetActive(false);
                timerIsRunning = true;
                Cursor.lockState = true ? CursorLockMode.Locked : CursorLockMode.None;
            }
        }
    }

    // Look for a free spot to place an orb
    private Vector2 FindFreePos()
    {
        int randomSpace = Random.Range(0, freeSpaces.Count); // choose a random slot in the available slot
        freeSpot = (Vector2)freeSpaces[randomSpace];    // assign the slot to the variable
        freeSpaces.RemoveAt(randomSpace);   // remove the taken spot from the list of free slots.

        // print out all spaces in console
        string debuglist = "Spaces = ";
        foreach (Vector2 space in freeSpaces)
        {
            debuglist += "(" + space.x + "," + space.y + ") ; ";
        }
        //Debug.Log(debuglist);

        return freeSpot;

    }

    // Create a new instance of an orb
    private void InstantiateNewOrb()
    {
        // generate position of the new orb to be instantiated
        Vector2 freeSpot = FindFreePos();
        int xpos = (int)freeSpot.x - 2; // -2 to shift from {0...4} to {-2...2}
        int ypos = (int)freeSpot.y - 2;

        // instantiate the orb
        Instantiate(Orb, new Vector3(gridMid.x + xpos * interval, gridMid.y - ypos * interval, defZ), Quaternion.identity);
    }


    // if the player misses the target add a miss
    public void OnMiss(RaycastHit hit)
    {
        if (timerIsRunning)
        {
            tarMiss += 1;
            UpdateStats();
        }
    }

    // if the player hits the target add a hit and replace the missing orb
    public void OnHit(RaycastHit hit)
    {
        if (timerIsRunning)
        {
            GameObject HitOrb = hit.transform.gameObject;
            Vector2 OrbPos = new Vector2(((HitOrb.transform.position.x - gridMid.x) / interval) + 2, ((HitOrb.transform.position.y - gridMid.y) / interval) + 2);
            freeSpaces.Add(OrbPos);
            Destroy(HitOrb);
            tarHit += 1;
            UpdateStats();
            InstantiateNewOrb();
        }
    }

    // assign new values to the stats and UI elements that display the stats
    public void UpdateStats()
    {
        if (timerIsRunning)
        {
            accuracy = (tarHit * 100) / (tarHit + tarMiss);
            AccText.text = "Accuracy: " + accuracy.ToString() + "%";
            Hits.text = "Hits: " + tarHit.ToString();
            Misses.text = "Misses: " + tarMiss.ToString();
        }
        else
        {
            EndAccText.text = AccText.text;
            EndHits.text = Hits.text;
            EndMisses.text = Misses.text;
            endScore = (tarHit - tarMiss);
            FinalScore.text = "Final score: " + endScore.ToString();

            // update the highscore
            taskStatsDict = Main.instance.GetDataDict("taskStatsDict");
            if (endScore > taskStatsDict["highscore"])
            {
                taskStatsDict["highscore"] = endScore;
            }
            HighScore.text = "Highscore: " + taskStatsDict["highscore"].ToString();
        }
    }

    // final update of the values in value handler and load the clicker scene
    public void EndButtonClicked()
    {
        Main.instance.UpdateResources(endScore);
        Main.instance.StoreMainDict(taskStatsDict, "taskStatsDict");
        Main.instance.SaveData();
        Main.instance.mainInitialized = true;
        Main.instance.LoadScene("Clicker");
    }

    // method to display the time during the task
    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // method to diplay the countdown time prior to the task starting
    void DisplayCountdown(float timeToDisplay)
    {
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        CountdownGO.text = string.Format("{0}", seconds);
    }

    public void Quit(){
        Main.instance.SaveData();
        Application.Quit();
    }

}
