// namespaces used
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;



/// <summary>
/// Idle game handler class, configuration of UI elements
/// </summary>
public class ClickerGame : MonoBehaviour
{
    //~~~~~~~~ Variable initialization ~~~~~~~~~

    //Unity game objects
    public GameObject UI,
                    Resources,
                    Upgrades,
                    GatherRes,
                    ResBg,
                    TaskChoice,
                    ResPrefab,
                    GatherPrefab,
                    UpgAndGather;

    // Buttons 
    public Button TaskBut1;
    public Button TaskBut2;

    // reference to know which UI element was last pressed
    public string lastClicked;

    // Dictionaries used for data handling
    public Dictionary<string, float> resDict;
    public Dictionary<string, float> mainDict;

    // array of resources
    private string[] resArray = new string[8] { "food", "wood", "stone", "minerals", "bronze", "copper", "iron", "gold" };

    // colors to be assigned to each resource
    // food     - white
    // wood     - dark brown
    // stone    - gray
    // minerals - aquamarine
    // bronze   - sandy brown
    // copper   - orange
    // iron     - dark gray
    // gold     - gold
    private UnityEngine.Color[] colourArray = new UnityEngine.Color[8] { UnityEngine.Color.white, new UnityEngine.Color(150, 75, 0), UnityEngine.Color.gray, new UnityEngine.Color(127, 255, 212), new UnityEngine.Color(244, 164, 96), new UnityEngine.Color(255, 165, 0), new UnityEngine.Color(169, 169, 169), new UnityEngine.Color(255, 215, 0) };


    // NOT IMPLEMENTED YET - upgrades to be discovered
    private string[] upgArray = new string[2] { "DiscWood", "DiscStone" };




    // Update is called once per frame
    void FixedUpdate()
    {
        // check that the main instance is already initialized
        CheckMainInit();
    }

    // method to check if the singleton main instance is already initialized
    void CheckMainInit()
    {
        // if the main instance has been initialized, initialize base values and call from the data handler
        if (Main.instance.mainInitialized)
        {
            //initial values of dicitonaries
            resDict = new Dictionary<string, float>();
            for (int i = 0; i < resArray.Length; i++)
            {
                resDict.Add(resArray[i], 0f);
            }
            mainDict = new Dictionary<string, float>();

            // check if pre-existing data file exists
            string path = Application.persistentDataPath + "/saveData/" + "myData.txt";

            // if no file exists, create a file with the initial values (0)
            if (!File.Exists(path))
            {
                Main.instance.resDiscovered = 1;
                mainDict.Add("resDiscovered", Main.instance.resDiscovered);
                Main.instance.StoreMainDict(resDict, "resDict");
                Main.instance.StoreMainDict(mainDict, "mainDict");
                Main.instance.SaveData();
                Debug.Log("saved!");
            }
            else // otherwise use the values that are stored in the file
            {
                Main.instance.LoadData();
                resDict = Main.instance.GetDataDict("resDict");
            }
            
           
            Main.instance.mainInitialized = false; // reset the boolean to make sure these values don't get initialized every fixed update

            // continue initialization of varaible
            CheckResDisc(); // see which resources are already discovered
            InitializeGameObjects(); // as method name implies 
        }
    }

    // Initialize unity gameobjects and link them to the script
    private void InitializeGameObjects()
    {
        //~~~~~~~~ Variable initialization ~~~~~~~~~

        //Unity game object initialization  
        UI = gameObject;
        Upgrades = UI.transform.Find("UpgsAndGather").gameObject.transform.Find("Upgrades").gameObject;
        GatherRes = UI.transform.Find("UpgsAndGather").gameObject.transform.Find("GatherRes").gameObject;
        Resources = UI.transform.Find("Resources").gameObject;
        ResBg = Resources.gameObject.transform.Find("ResBg").gameObject;
        UpgAndGather = UI.transform.Find("UpgsAndGather").gameObject;
        TaskChoice = UI.transform.Find("TaskChoice").gameObject;
        TaskBut1 = TaskChoice.transform.Find("StartTask").gameObject.GetComponent<Button>();
        TaskBut2 = TaskChoice.transform.Find("Return").gameObject.GetComponent<Button>();

        // add listeners to the two button gameobjects
        TaskBut1.onClick.AddListener(TaskClicked);
        TaskBut2.onClick.AddListener(ReturnClicked);
        /// gameobject array
        GameObject[] GoGatherArray = new GameObject[8];
        GameObject[] GoResArray = new GameObject[8];

        //Configure anchorpoints for Gather objects and Upgrade objects
        GameObject bgPanelGO = UI.transform.Find("UpgsAndGather").gameObject;
        

        // determine values for automated creation of buttons depending on resources discovered
        // dimensions of back panel
        float Gatherheight = bgPanelGO.GetComponent<RectTransform>().rect.height;
        float Gatherwidth = bgPanelGO.GetComponent<RectTransform>().rect.width;
        //starting points at the top left of the back panel
        float GatherxStartPoint = -Gatherwidth / 2;
        float GatheryStartPoint = Gatherheight / 2;

        // determine X coordinate anchor points for the buttons and put them in an array
        float gatherAnchorPointX1 = GatherxStartPoint + Gatherwidth * (0.25f);
        float gatherAnchorPointX2 = GatherxStartPoint + Gatherwidth * (0.75f);
        float[] gatherAnchorXArray = new float[2] { gatherAnchorPointX1, gatherAnchorPointX2 };

        // determine Y coordinate anchor points for the buttons and put them in an array
        float gatherAnchorPointY1 = GatheryStartPoint - Gatherheight * (0.11f);
        float gatherAnchorPointY2 = GatheryStartPoint - Gatherheight * (0.22f);
        float gatherAnchorPointY3 = GatheryStartPoint - Gatherheight * (0.33f);
        float gatherAnchorPointY4 = GatheryStartPoint - Gatherheight * (0.44f);
        float gatherAnchorPointY5 = GatheryStartPoint - Gatherheight * (0.55f);
        float gatherAnchorPointY6 = GatheryStartPoint - Gatherheight * (0.66f);
        float gatherAnchorPointY7 = GatheryStartPoint - Gatherheight * (0.77f);
        float gatherAnchorPointY8 = GatheryStartPoint - Gatherheight * (0.88f);
        float[] gatherAnchorYArray = new float[8] { gatherAnchorPointY1, gatherAnchorPointY2, gatherAnchorPointY3, gatherAnchorPointY4, gatherAnchorPointY5, gatherAnchorPointY6, gatherAnchorPointY7, gatherAnchorPointY8 };

        // determine values for automated creation text elements depending on resources discovered
        // dimensions of back panel for resources
        float resheight = Resources.GetComponent<RectTransform>().rect.height;
        float reswidth = Resources.GetComponent<RectTransform>().rect.width;

        //starting points at the top left of the back panel 
        float resxStartPoint = -reswidth / 2;
        float resyStartPoint = resheight / 2;

        // determine X coordinate anchor point for the text and put them in an array
        float resAnchorPointX = resxStartPoint + reswidth * (0.05f);

        // determine Y coordinate anchor points for the buttons and put them in an array
        float resAnchorPointY1 = resyStartPoint - resheight * (0.1f);
        float resAnchorPointY2 = resyStartPoint - resheight * (0.2f);
        float resAnchorPointY3 = resyStartPoint - resheight * (0.3f);
        float resAnchorPointY4 = resyStartPoint - resheight * (0.4f);
        float resAnchorPointY5 = resyStartPoint - resheight * (0.5f);
        float resAnchorPointY6 = resyStartPoint - resheight * (0.6f);
        float resAnchorPointY7 = resyStartPoint - resheight * (0.7f);
        float resAnchorPointY8 = resyStartPoint - resheight * (0.8f);
        float resAnchorPointY9 = resyStartPoint - resheight * (0.9f);
        float[] resAnchorYArray = new float[9] { resAnchorPointY1, resAnchorPointY2, resAnchorPointY3, resAnchorPointY4, resAnchorPointY5, resAnchorPointY6, resAnchorPointY7, resAnchorPointY8, resAnchorPointY9 };

        // instantiate buttons and text UI objects based on how many resources have been discovered
        for (int i = 0; i < resArray.Length; i++)
        {
            // create an instance of a button, set its position in the unity hierarchy and change its name
            Instantiate(GatherPrefab, GatherRes.transform);
            GoGatherArray[i] = GatherRes.transform.Find("GatherPrefab(Clone)").gameObject;
            GoGatherArray[i].transform.SetParent(GatherRes.transform);
            GoGatherArray[i].name = "Gather " + resArray[i].Substring(0, 1).ToUpper() + resArray[i].Substring(1, resArray[i].Length - 1);

            // add a listener to the buttonm, change the text of the button and give it a tag
            GoGatherArray[i].GetComponent<Button>().onClick.AddListener(UpgButtonClicked);
            GoGatherArray[i].GetComponentInChildren<TMP_Text>().text = "Gather " + resArray[i].Substring(0, 1).ToUpper() + resArray[i].Substring(1, resArray[i].Length - 1);
            GoGatherArray[i].transform.tag = resArray[i].Substring(0, 1).ToUpper() + resArray[i].Substring(1, resArray[i].Length - 1);

            // set the pivot of the button and set its poisition using determined anchor points
            GoGatherArray[i].transform.localPosition = new Vector2(gatherAnchorXArray[i % 2], gatherAnchorYArray[Mathf.CeilToInt(((float)i + 1) / 2) - 1]);

            // create an instance of the resource text, set its position in the unity hierarchy and change its name
            Instantiate(ResPrefab, ResBg.transform);
            GoResArray[i] = ResBg.transform.Find("ResPrefab(Clone)").gameObject;
            GoResArray[i].transform.SetParent(ResBg.transform);
            GoResArray[i].name = resArray[i].Substring(0, 1).ToUpper() + resArray[i].Substring(1, resArray[i].Length - 1);

            // change the value of the resource text and give it a tag
            GoResArray[i].GetComponent<TMP_Text>().text = GoResArray[i].name + ": " + Main.instance.GetValue("resDict", resArray[i]).ToString();
            GoResArray[i].GetComponent<TMP_Text>().color = colourArray[i];
            GoResArray[i].transform.tag = resArray[i].Substring(0, 1).ToUpper() + resArray[i].Substring(1, resArray[i].Length - 1);
            
            // set the pivot of the resource text and set its poisition using determined anchor points
            GoResArray[i].transform.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            GoResArray[i].transform.localPosition = new Vector2(resAnchorPointX, resAnchorYArray[i]);
        }
        
    
        lastClicked = ""; // give variable initial value;
        
        
        float resDiscovered = Main.instance.resDiscovered;

        // hide the undiscovered buttons and resource texts
        for (int i = resArray.Length - 1; i > resDiscovered - 1; i--)
        {
            GoResArray[i].gameObject.SetActive(false);
        }
        for (int i = resArray.Length - 1; i > resDiscovered - 1; i--)
        {
            GoGatherArray[i].gameObject.SetActive(false);
        }

        // hide the panel allowing the player to start the task
        TaskChoice.SetActive(false);
    }

    // check which resources have been discovered based on how many resources have been farmed
    // TODO: this should be changed to resources that have been upgraded by the player (consuming resources)
    private void CheckResDisc()
    {
        bool resDiscChanged = false;
        // resources are discovered sequentially
        if (Main.instance.resDiscovered == 1)
        {
            if (Main.instance.GetValue("resDict", "food") > 100)
            {
                Main.instance.resDiscovered = 2;
                resDiscChanged = true;
            }
        }
        else if (Main.instance.resDiscovered == 2)
        {
            if (Main.instance.GetValue("resDict", "wood") > 30)
            {
                Main.instance.resDiscovered = 3;
                resDiscChanged = true;
            }
        }
        else if (Main.instance.resDiscovered == 3)
        {
            if (Main.instance.GetValue("resDict", "stone") > 50)
            {
                Main.instance.resDiscovered = 4;
                resDiscChanged = true;
            }
        }

        // If a new resource has been discovered  
        if (resDiscChanged)
        {   
            // reinitialize the objects
            CheckMainInit();
        }
    }

    public void TaskClicked()
    {
        // send to Task
        Main.instance.StoreMainDict(resDict, "resDict");
        Main.instance.SaveData();
        Main.instance.LoadScene("AimScene");
    }
    public void ReturnClicked()
    {
        // return to main screen     
        TaskChoice.SetActive(false);
        Resources.SetActive(true);
        UpgAndGather.SetActive(true);

    }

    // WIP: show the upgrades panel
    public void UpgButtonClicked()
    {
        Main.instance.currentResource = EventSystem.current.currentSelectedGameObject.tag.ToLower();
        Debug.Log(Main.instance.currentResource);
        Resources.SetActive(false);
        UpgAndGather.SetActive(false);
        TaskChoice.SetActive(true);
        TaskChoice.transform.Find("UpgName").GetComponent<TMP_Text>().text = lastClicked;
    }
}




//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
// unused code that might be relevant
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

/* resources
    - food
    - wood
    - stone
    - ore
        - bronze
        - copper
        - iron
        - gold
    - minerals
    
    upgrades
    - buildings
    - people
    - faith
    
    public Resource food;
    public Resource wood;
    public Resource stone;
    public Resource minerals;
    public Ore bronze,
                copper,
                iron,
                gold;

public class Resource
{
    protected string name { get; set; }
    protected int amount { get; set; }
    public GameObject UIGameObject;

    public Resource()
    {
        name = "";
        amount = 0;
    }

    public Resource(string name, int amount, GameObject UIGameObject)
    {
        this.name = name;
        this.amount = amount;
        this.UIGameObject = UIGameObject;
    }

    public int GetResourceAmount()
    {
        return this.amount;
    }

    public void SetResourceAmount(int amount)
    {
        this.amount = amount;
    }

}

public class Ore : Resource
{
    protected string type { get; set; }

    public Ore() : base() { }

    public Ore(string name, int amount, GameObject UIGameObject, string type)
    {
        this.name = name;
        this.amount = amount;
        this.UIGameObject = UIGameObject;
        this.type = type;

    }



}*/