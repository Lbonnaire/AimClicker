using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public static Main instance;
    public Dictionary<string, float> resDict;
    public Dictionary<string, float> taskStatsDict;
    private string[] resArray = new string[8] { "food", "wood", "stone", "minerals", "bronze", "copper", "iron", "gold" };

    private string[] taskStatsArray = new string[1] { "highscore" };

    public string currentResource;
    GameData data;

    //// LOG
    // sens  
    // cm/360
    // fov
    // score accuracy etc.

    private void Awake()
    {

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
        currentResource = "food";
        InitializeDicts();

        LoadData();

        // checks if it's the first time the program is launched and saves initial values if so
        try
        {
            Debug.Log(this.data.food);
        }
        catch
        {
            StoreMainDict(this.resDict, "resDict");
            StoreMainDict(this.taskStatsDict, "taskStatsDict");
            SaveData();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    // Save data in local file
    public void SaveData()
    {
        SaveSystem.SaveData(this.data);
    }

    // Load data from saved file
    public void LoadData()
    {
        this.data = SaveSystem.LoadData();
        this.resDict = new Dictionary<string, float>();
        for (int i = 0; i < resArray.Length; i++)
        {
            this.resDict.Add(resArray[i], this.data.GetValue(resArray[i]));
        }

        for (int i = 0; i < taskStatsArray.Length; i++)
        {
            this.resDict.Add(taskStatsArray[i], this.data.GetValue(taskStatsArray[i]));
        }
    }

    // print out a dictionary value by value (Debugging)
    public void PrintDict(Dictionary<string, float> dictToPrint)
    {
        try
        {
            foreach (KeyValuePair<string, float> kvp in dictToPrint)
            {

                Debug.Log(kvp.Key.ToString() + ": " + kvp.Value.ToString());
            }
        }
        catch
        {
            Debug.Log("noDict");
        }

    }
    public void DoNothing()
    {
        Debug.Log("I did nothing");
    }

    // 
    /*public Dictionary<string, float> GetData(string dictName)
    {

        LoadData();
        return GetDataDict(dictName);

    }*/

    // get dictionary from data values
    public Dictionary<string, float> GetDataDict(string dictName)
    {
        Dictionary<string, float> dictToReturn = new Dictionary<string, float>();
        LoadData();
        switch (dictName)
        {
            case "resDict":
                for (int i = 0; i < resArray.Length; i++)
                {
                    dictToReturn.Add(resArray[i], this.data.GetValue(resArray[i]));
                }
                break;
            case "taskStatsDict":
                for (int i = 0; i < taskStatsArray.Length; i++)
                {
                    dictToReturn.Add(taskStatsArray[i], this.data.GetValue(taskStatsArray[i]));
                }
                break;
        }
        return dictToReturn;
    }

    // store dictionary values into data
    public void StoreMainDict(Dictionary<string, float> newDict, string dictName)
    {
        switch (dictName)
        {
            case "resDict":
                //this.resDict = newDict;
                try
                {
                    Debug.Log(resDict["wood"]);
                }
                catch
                {
                    InitializeDicts();
                }


                for (int i = 0; i < resArray.Length; i++)
                {
                    this.data.SetValue(resArray[i], newDict[resArray[i]]);
                }
                break;

            case "taskStatsDict":
                for (int i = 0; i < taskStatsArray.Length; i++)
                {
                    this.data.SetValue(taskStatsArray[i], newDict[taskStatsArray[i]]);
                }
                break;
        }

    }

    // initialize dictionaries
    private void InitializeDicts()
    {
        this.data = new GameData();

        this.resDict = new Dictionary<string, float>();
        //Debug.Log("check array init: "+ this.resArray[0]);

        for (int i = 0; i < this.resArray.Length; i++)
        {
            this.resDict.Add(this.resArray[i], 0f);
        }
        // Debug.Log("check dict init: "+ this.resDict[this.resArray[0]]);


        this.taskStatsDict = new Dictionary<string, float>();
        for (int i = 0; i < this.taskStatsArray.Length; i++)
        {
            this.taskStatsDict.Add(this.taskStatsArray[i], 0f);
        }
        //PrintDict(this.taskStatsDict);
    }

    // Get value from dictionaries
    private float GetValue(string dictName, string valName)
    {
        float valueToReturn = 0;
        if (dictName == "resDict")
        {
            valueToReturn = this.resDict[valName];
        }
        else if (dictName == "taskStatsDict")
        {
            valueToReturn = this.taskStatsDict[valName];
        }

        return valueToReturn;
    }

    // update resources in data and dictionaries
    public void UpdateResources(float resGained)
    {
        this.data.SetValue(currentResource, this.data.GetValue(currentResource) + resGained);
        this.resDict = GetDataDict("resDict");
        Debug.Log(this.resDict["food"]);
    }

    // Load next scene
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
