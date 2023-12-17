// namespaces used
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main class singleton instance used to handle data storage and saving between scenes
/// </summary>
public class Main : MonoBehaviour
{
    //~~~~~~~~ Variable initialization ~~~~~~~~~
    // instance
    public static Main instance;

    // dicationaries
    public Dictionary<string, float> resDict;
    public Dictionary<string, float> taskStatsDict;
    public Dictionary<string, float> mainDict;

    // arrays
    private string[] resArray = new string[8] { "food", "wood", "stone", "minerals", "bronze", "copper", "iron", "gold" };
    private string[] taskStatsArray = new string[1] { "highscore" };
    private string[] mainArray = new string[1]{"resDiscovered"};

    // initialize the amount of resources discovered
    public float resDiscovered=1;


    public string currentResource;

    GameData data;
    public bool mainInitialized= false;
    public bool introScreenShown=false;

    //// LOG
    // sens  
    // cm/360
    // fov
    // logic behind progression of resource management.

    // Make sure that this is the only instance of main that is currently active
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
        this.currentResource = "food"; // set an initial value
        InitializeDicts(); // give base values to the dictionaries
        LoadData(); // load data from the saved file

        // checks if it's the first time the program is launched and saves initial values if so debug loaded data to see if previous data was saved
        try
        {
            Debug.Log(this.data.food);
        }
        catch // if no data can be pulled, save the starting values to create an initial file 
        {
            StoreMainDict(this.resDict, "resDict");
            StoreMainDict(this.taskStatsDict, "taskStatsDict");
            StoreMainDict(this.mainDict,"mainDict");
            SaveData();
        }
        Debug.Log(resDiscovered +" resources discovered");
        SetResDisc(); // sets the amount of resources that have been discovered
        mainInitialized = true;
    }

    // Save data in local file
    public void SaveData()
    {
        SaveSystem.SaveData(this.data);
    }

    // Load data from saved file
    public void LoadData()
    {
        this.data = SaveSystem.LoadData(); // load data from the save system

        this.resDict = new Dictionary<string, float>();
         // add the resources data to instance dictionary
        for (int i = 0; i < resArray.Length; i++)
        {
            this.resDict.Add(resArray[i], this.data.GetValue(resArray[i]));
        }

        // add the task data to instance dictionary
        for (int i = 0; i < taskStatsArray.Length; i++)
        {   try { // try filling in the dictionary
            this.taskStatsDict.Add(taskStatsArray[i], this.data.GetValue(taskStatsArray[i])); 
            }catch{ // edit the values of the dictionary if it has already been initialized
              this.taskStatsDict[taskStatsArray[i]]= this.data.GetValue(taskStatsArray[i]);  
            }

        }
        resDiscovered = this.data.GetValue("resDiscovered");
    }

    // for each resource with a non-zero value, increase the amount of resources discovered
    private void SetResDisc(){
        for(int i=0; i<resArray.Length;i++){
            if (this.resDict[resArray[i]]>0){
                resDiscovered++; // increase this also when a new value is discovered
            }
        }
    }

    // print out a dictionary value by value (Debugging)
    public void PrintDict(Dictionary<string, float> dictToPrint)
    {
        try // try to print out the dictionary and its values
        {
            foreach (KeyValuePair<string, float> kvp in dictToPrint)
            {

                Debug.Log(kvp.Key.ToString() + ": " + kvp.Value.ToString());
            }
        }
        catch // if it doesn't work print out an error
        {
            Debug.Log("Dictionary could not be printed");
        }

    }

    // get dictionary from the stored instance's GameData values
    public Dictionary<string, float> GetDataDict(string dictName)
    {
        Dictionary<string, float> dictToReturn = new Dictionary<string, float>();
        
        switch (dictName) // choose which dictionary was requested
        {
            case "resDict": // if the resource dictionary was requested 
                for (int i = 0; i < resArray.Length; i++)
                {
                    dictToReturn.Add(resArray[i], this.data.GetValue(resArray[i]));
                }
                break;
            case "taskStatsDict": // if the task dictionary was requested 
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
        switch (dictName) // choose which dictionary was requested
        {
            case "mainDict": // if the main dictionary was requested 
                // set the main gamedata values to the ones provided in the method
                for (int i = 0; i < mainArray.Length; i++)
                {
                    this.data.SetValue(mainArray[i], newDict[mainArray[i]]);
                }
                break;
            case "resDict": // if the resource dictionary was requested 
                // set the resource gamedata values to the ones provided in the method
                for (int i = 0; i < resArray.Length; i++)
                {
                    this.data.SetValue(resArray[i], newDict[resArray[i]]);
                }
                break;

            case "taskStatsDict": // if the task dictionary was requested 
                // set the task gamedata values to the ones provided in the method
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
        // create an object of gamedata
        this.data = new GameData();

        // give initial values to the resource and task dictionaries
        this.resDict = new Dictionary<string, float>();
        for (int i = 0; i < this.resArray.Length; i++)
        {
            this.resDict.Add(this.resArray[i], 0f);
        }
        this.taskStatsDict = new Dictionary<string, float>();
        for (int i = 0; i < this.taskStatsArray.Length; i++)
        {
            try { // try filling in the dictionary
                this.taskStatsDict.Add(this.taskStatsArray[i], 0); 
            }catch{ // edit the values of the dictionary if it has already been initialized
              this.taskStatsDict[this.taskStatsArray[i]]= 0;  
            }
        }

        // set initial resources discovered to 1
        Main.instance.resDiscovered=1;
    }

    // Get value from dictionaries stored locally on instance
    public float GetValue(string dictName, string valName)
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
       
    }

    // Load next scene
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
