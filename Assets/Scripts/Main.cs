using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public static Main instance;
    public Dictionary<string,float> resDict;
    public Dictionary<string,float> taskStatsDict;
    private string[] resArray; 
    private string[] taskStatsArray; 
    GameData data;

    //// LOG
    // sens  
    // cm/360
    // fov
    // score accuracy etc.


    // save system not working, maybe make it a json? maybe don't use dictionaries


    
    private void Awake()    
    {
        //List<Dictionary<string, object>> importantQuizData = CSVReader.Read("ImportantQuiz");

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);

        }
        DontDestroyOnLoad(gameObject);

    }

    // Start is called before the first frame update
    void Start()
    {
        resArray =new string[8]{"food","wood","stone","minerals","bronze", "copper","iron","gold"};
        this.resDict = new Dictionary<string, float>();
        for( int i =0; i<resArray.Length;i++){
            this.resDict.Add(resArray[i], 0f); 
        }
        taskStatsArray = new string[1]{"highscore"};
        this.taskStatsDict = new Dictionary<string, float>
        {
            
            { "highscore", 0f }
        };
        /*for( int i =0; i<taskStatsArray.Length;i++){
       this.taskStatsDict.Add(taskStatsArray[i], 0f); 
   }*/

        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

// save dict of dict!!!
     public void SaveData(Dictionary<string,float> dict, string dictName)
    {
        StoreMainDict(dict,dictName);
        SaveSystem.SaveData(instance);
    }

    public void LoadData()
    {
        data = SaveSystem.LoadData();
        this.resDict = data.resDict;
        Debug.Log("resDict");
        DebugLogDict(resDict);

        this.taskStatsDict = data.taskStatsDict;
        Debug.Log("taskStats");
        DebugLogDict(taskStatsDict);
    }
    void DebugLogDict(Dictionary<string,float> dictToPrint){
        foreach(KeyValuePair<string,float> kvp in dictToPrint){
            Debug.Log(kvp.Key.ToString()+": "+ kvp.Value.ToString());
        }
    }
    public void DoNothing(){
        Debug.Log("I did nothing");
    }
    public Dictionary<string, float> GetData(string dictName){
 
        LoadData();
        return GetDataDict(dictName);
        
    }


    public Dictionary<string, float> GetDataDict(string dictName){
        Dictionary<string, float> dictToReturn= new Dictionary<string, float>();
        switch (dictName)
        {
            case "resDict":
                dictToReturn = data.resDict;
                break;
            case "taskStatsDict":
                dictToReturn = data.taskStatsDict;
                break;
        }
        return dictToReturn;
    }

        public void StoreMainDict(Dictionary<string, float>newDict, string dictName){
        switch (dictName)
        {
            case "resDict":
                this.resDict = newDict;
                break;
            case "taskStatsDict":
                this.taskStatsDict= newDict;
                DebugLogDict(taskStatsDict);
                break;
        }

    }


    public void LoadScene(string sceneName){
        SceneManager.LoadScene(sceneName);
    }
}
