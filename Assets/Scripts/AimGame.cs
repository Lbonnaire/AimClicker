using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;

public class AimGame : MonoBehaviour
{

    public GameObject   Orb,
                        UI,
                        Panel,
                        Player,
                        TaskCompleted;
                    
     private StarterAssetsInputs _input;

     float interval;
    Vector2 gridMid; 
    float defZ;
     
    Vector2[,] orbGrid;


    int tarHit = 0;
    int tarMiss = 0;

    public TMP_Text AccText,
                    Hits,
                    Misses,
                    EndAccText,
                    EndHits,
                    EndMisses,
                    FinalScore,
                    HighScore,
                    timeText,
                    Countdown;

    public float accuracy =100;
    public static Main mainInstance;    
    ArrayList freeSpaces;
    public Button EndStatsClose;
 
    public Dictionary<string,float> taskStatsDict;
    private string[] taskStatsArray; 
    public float timeRemaining = 10;
    public bool timerIsRunning = false;

    public float countdownTimer =3;
    public bool countdownIsRunning =false;
   

    private Vector2 freeSpot ;

    // Start is called before the first frame update
    void Start()
    {
        InitializeGameObjects();

        // grid 5x5, 
        // size of orbs
        // space between orbs
        timerIsRunning = false;
        interval = Orb.transform.localScale.x*1.2f;
        //Debug.Log(Orb.transform.localScale.x);
        float ymid= 3f;
        
        gridMid = new Vector2(0f,ymid);
        defZ =4f;
        freeSpaces = new ArrayList();
        orbGrid = new Vector2[5,5];

        for (var i=0; i<5;i++){
            for (var j=0;j<5;j++){
                orbGrid[i,j]= new Vector2(i,j);
                freeSpaces.Add(orbGrid[i,j]);

            }
        }

        for (var i =0; i<3; i++){
            InstantiateNewOrb();
        }
    }
    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                //Debug.Log("Time has run out!");
                timeRemaining = 0;
                DisplayTime(timeRemaining);

                timerIsRunning = false;
                TaskCompleted.SetActive(true);
                Cursor.lockState = false ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = true; 
                UpdateStats();
            }
        }else if (countdownIsRunning){
            StartCountdown();
            
            Cursor.visible = false;
        }
    }

    private void InitializeGameObjects(){
        
        mainInstance = new Main();
        _input = GameObject.Find("Player").GetComponent<StarterAssetsInputs>();
        UI = GameObject.Find("UI");
        Panel = UI.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject;
        TaskCompleted = UI.transform.Find("Canvas").gameObject.transform.Find("TaskCompleted").gameObject;
        Player = GameObject.Find("PlayerCameraRoot"); 
        EndStatsClose = TaskCompleted.transform.Find("EndStats").GetComponentInChildren<Button>();
        EndStatsClose.onClick.AddListener(EndButtonClicked);

        Countdown = UI.transform.Find("Canvas").gameObject.transform.Find("Countdown").gameObject.GetComponent<TMP_Text>();
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
        Countdown.gameObject.SetActive(false);
        TaskCompleted.SetActive(false);
        

        taskStatsArray = new string[1]{"highscore"};
        taskStatsDict = new Dictionary<string, float>();
        for( int i =0; i<taskStatsArray.Length;i++){
            taskStatsDict.Add(taskStatsArray[i], 0f); 
        }
        Dictionary<string,float> dictFromData = mainInstance.GetData("taskStatsDict");
        if (mainInstance.GetData("taskStatsDict")==null){
            mainInstance.SaveData(taskStatsDict,"taskStatsDict");

        }

    }

    public void StartTask(){
        Panel.SetActive(false);
        Countdown.gameObject.SetActive(true);
        countdownIsRunning=true;
        StartCountdown();
    }

    public void StartCountdown(){
        if(countdownIsRunning){
            
         if (countdownTimer > 0)
            {
                countdownTimer -= Time.deltaTime;
                DisplayCountdown(countdownTimer);
            }
            else
            {

                countdownTimer = 0;
                countdownIsRunning = false;
                Countdown.gameObject.SetActive(false);
                timerIsRunning=true;
                Cursor.lockState = true ? CursorLockMode.Locked : CursorLockMode.None;
            }
        }
    }

    private Vector2 FindFreePos(){
        int randomSpace = Random.Range(0,freeSpaces.Count);
        freeSpot = (Vector2) freeSpaces[randomSpace];
        freeSpaces.RemoveAt(randomSpace);
        //Debug.Log(freeSpot);
        string debuglist ="Spaces = ";
        foreach(Vector2 space in freeSpaces){
            debuglist+= "("+space.x+","+space.y+") ; ";
        }
        //Debug.Log(debuglist);

        return freeSpot;
        
    }

    private void InstantiateNewOrb(){
            
            Vector2 freeSpot = FindFreePos();
            int xpos = (int)freeSpot.x-2; // -2 to shift from {0...4} to {-2...2}
            int ypos = (int)freeSpot.y-2;
            //orbGrid[(int)freeSpot.x, (int)freeSpot.y]= new Vector2((int)freeSpot.x, (int)freeSpot.y);
            
            Instantiate(Orb, new Vector3(gridMid.x+xpos*interval,gridMid.y-ypos*interval,defZ),Quaternion.identity);
            //Debug.Log(xpos);
            //Debug.Log(ypos);
            //Debug.Log("X:"+(gridMid.x+xpos*interval)+", Y:"+ (gridMid.y+ypos*interval));
    }

   

    public void OnMiss(RaycastHit hit){
        if(timerIsRunning){
            tarMiss+=1;
            UpdateStats();
        }
    }
 

    public void OnHit(RaycastHit hit)
    {
        if(timerIsRunning){

        
        GameObject HitOrb = hit.transform.gameObject;
        Vector2 OrbPos = new Vector2(((HitOrb.transform.position.x-gridMid.x)/interval)+2,((HitOrb.transform.position.y-gridMid.y)/interval)+2);
        //Debug.Log("X:"+OrbPos.x.ToString()+", Y:"+OrbPos.y.ToString());
        freeSpaces.Add(OrbPos);
        Destroy(HitOrb);
        tarHit+=1;
        UpdateStats();
        InstantiateNewOrb();
        }
    }

    public void UpdateStats(){
        if(timerIsRunning){
            accuracy = (tarHit*100)/(tarHit+tarMiss);
            AccText.text = "Accuracy: "+accuracy.ToString() +"%";
            Hits.text = "Hits: "+tarHit.ToString();
            Misses.text = "Misses: "+tarMiss.ToString();
        }else{
            Debug.Log("ShowingEndStats");
            EndAccText.text = AccText.text;
            EndHits.text=Hits.text;
            EndMisses.text=Misses.text;
            float endScore =(tarHit-tarMiss);
            FinalScore.text = "Final score: "+endScore.ToString();
            taskStatsDict = mainInstance.GetData("taskStatsDict");
            if (endScore>taskStatsDict["highscore"]){
                taskStatsDict["highscore"]=endScore;
                // Add new highscore indicator
            }
            HighScore.text = "Highscore: "+taskStatsDict["highscore"].ToString();
        }
    }





    public void EndButtonClicked(){
        mainInstance.SaveData(taskStatsDict,"taskStatsDict");

        
        mainInstance.LoadScene("Clicker");
    }
    
    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);  
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }       
    void DisplayCountdown(float timeToDisplay){
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        Countdown.text = string.Format("{0}", seconds);
    }
    
}
