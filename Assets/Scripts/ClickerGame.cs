// namespaces used
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using System.Reflection;
using System.Linq.Expressions;
using Unity.VisualScripting;



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
                    UpgradePrefab,
                    IntroScreen,
                    UpgAndGather;


    // Buttons 
    public Button   IntroBut,
                    UpgradeBut,
                    GatherBut,
                    QuitButton,
                    TaskBut1,
                    TaskBut2;
    

    // reference to know which UI element was last pressed
    public string lastClicked;

    // Dictionaries used for data handling
    public Dictionary<string, float> resDict;
    public Dictionary<string, float> mainDict;

    GameObject[] GoUpgArray;

    // array of resources
    private string[] resArray = new string[8] { "food", "wood", "stone", "minerals", "bronze", "copper", "iron", "gold" };

     // NOT IMPLEMENTED YET - upgrades to be discovered
    public List<string> UpgradeList = new List<string>();
    int resDiscovered=1;
   
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

    // colors for selected/deselected tabs
    private UnityEngine.Color selectedCol=  UnityEngine.Color.white;
    private UnityEngine.Color deselectedCol=  UnityEngine.Color.gray;
    

    bool introScreenClicked=false; // detect if introScreen was read and continued

   
    


    /// <summary>
    /// Method <c> FixedUpdate </c> is called once per frame
    /// </summary>
    void FixedUpdate()
    {
        // check that the main instance is already initialized
        CheckMainInit();
    }


    /// <summary>
    /// Method <c> CheckMainInit </c> checks if the singleton main instance is already initialized
    /// </summary>
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

    /// <summary>
    /// Method <c> InitializeGameObjects </c> initializes unity gameobjects and links them to the script
    /// </summary>
    private void InitializeGameObjects()
    {
        //~~~~~~~~ Variable initialization ~~~~~~~~~

        //Unity game object initialization  
        UI = gameObject;
        Upgrades = UI.transform.Find("UpgsAndGather").gameObject.transform.Find("Upgrades").gameObject;
        GatherRes = UI.transform.Find("UpgsAndGather").gameObject.transform.Find("GatherRes").gameObject;
        Resources = UI.transform.Find("Resources").gameObject;
        IntroScreen = UI.transform.Find("IntroScreen").gameObject;
        ResBg = Resources.gameObject.transform.Find("ResBg").gameObject;
        UpgAndGather = UI.transform.Find("UpgsAndGather").gameObject;
        TaskChoice = UI.transform.Find("TaskChoice").gameObject;
        IntroBut = IntroScreen.transform.Find("NextButton").gameObject.GetComponent<Button>();
        GatherBut = UpgAndGather.transform.Find("GatherBut").gameObject.GetComponent<Button>();
        UpgradeBut = UpgAndGather.transform.Find("UpgradeBut").gameObject.GetComponent<Button>();
        TaskBut1 = TaskChoice.transform.Find("StartTask").gameObject.GetComponent<Button>();
        TaskBut2 = TaskChoice.transform.Find("Return").gameObject.GetComponent<Button>();
        QuitButton = UI.transform.Find("QuitButton").gameObject.GetComponent<Button>();

        // hide the panel allowing the player to start the task
        TaskChoice.SetActive(false);
        Upgrades.SetActive(false);
        if (Main.instance.introScreenShown){
            IntroScreen.SetActive(false);
        }


        // set base color of tab buttons
        GatherBut.GetComponent<Image>().color = selectedCol;
        UpgradeBut.GetComponent<Image>().color = deselectedCol;

        // add listeners to the button gameobjects
        TaskBut1.onClick.AddListener(TaskClicked);
        TaskBut2.onClick.AddListener(ReturnClicked);
        IntroBut.onClick.AddListener(IntroClicked);
        GatherBut.onClick.AddListener(GatherTabClicked);
        UpgradeBut.onClick.AddListener(UpgradeTabClicked);
        QuitButton.onClick.AddListener(Quit);

        UIGeneration();
        
        
    }


    /// <summary>
    /// Method <c> UIGeneration </c> handles the instantiation and positioning of dynamic buttons and text
    /// </summary>
    public void UIGeneration(){
        /// gameobject array
        GameObject[] GoGatherArray = new GameObject[8];
        GameObject[] GoResArray = new GameObject[8];
        GoUpgArray = new GameObject[8];

        //Configure anchorpoints for Gather objects and Upgrade objects
        GameObject bgPanelGO = UI.transform.Find("UpgsAndGather").gameObject;
        

        // determine values for automated creation of buttons depending on resources discovered
        // dimensions of back panel
        float midPanelHeight = bgPanelGO.GetComponent<RectTransform>().rect.height;
        float midPanelWidth = bgPanelGO.GetComponent<RectTransform>().rect.width;
        //starting points at the top left of the back panel
        float midPanelxStartPoint = -midPanelWidth / 2;
        float midPanelyStartPoint = midPanelHeight / 2;

        // determine X coordinate anchor points for the buttons and put them in an array
        float midPanelAnchorPointX1 = midPanelxStartPoint + midPanelWidth * (0.25f);
        float midPanelAnchorPointX2 = midPanelxStartPoint + midPanelWidth * (0.75f);
        float[] midPanelAnchorXArray = new float[2] { midPanelAnchorPointX1, midPanelAnchorPointX2 };

        // determine Y coordinate anchor points for the buttons and put them in an array
        float midPanelAnchorPointY1 = midPanelyStartPoint - midPanelHeight * (0.11f);
        float midPanelAnchorPointY2 = midPanelyStartPoint - midPanelHeight * (0.22f);
        float midPanelAnchorPointY3 = midPanelyStartPoint - midPanelHeight * (0.33f);
        float midPanelAnchorPointY4 = midPanelyStartPoint - midPanelHeight * (0.44f);
        float midPanelAnchorPointY5 = midPanelyStartPoint - midPanelHeight * (0.55f);
        float midPanelAnchorPointY6 = midPanelyStartPoint - midPanelHeight * (0.66f);
        float midPanelAnchorPointY7 = midPanelyStartPoint - midPanelHeight * (0.77f);
        float midPanelAnchorPointY8 = midPanelyStartPoint - midPanelHeight * (0.88f);
        float[] midPanelAnchorYArray = new float[8] { midPanelAnchorPointY1, midPanelAnchorPointY2, midPanelAnchorPointY3, midPanelAnchorPointY4, midPanelAnchorPointY5, midPanelAnchorPointY6, midPanelAnchorPointY7, midPanelAnchorPointY8 };

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

        resDiscovered = (int)Main.instance.resDiscovered;

        // instantiate buttons and text UI objects based on how many resources have been discovered
        for (int i = 0; i < resArray.Length; i++)
        {
            // create an instance of a button, set its position in the unity hierarchy and change its name
            Instantiate(GatherPrefab, GatherRes.transform);
            GoGatherArray[i] = GatherRes.transform.Find("GatherPrefab(Clone)").gameObject;
            GoGatherArray[i].transform.SetParent(GatherRes.transform);
            GoGatherArray[i].name = "Gather " + resArray[i].Substring(0, 1).ToUpper() + resArray[i].Substring(1, resArray[i].Length - 1);

            // add a listener to the buttonm, change the text of the button and give it a tag
            GoGatherArray[i].GetComponent<Button>().onClick.AddListener(TaskButtonClicked);
            GoGatherArray[i].GetComponentInChildren<TMP_Text>().text = "Gather " + resArray[i].Substring(0, 1).ToUpper() + resArray[i].Substring(1, resArray[i].Length - 1);
            GoGatherArray[i].transform.tag = resArray[i].Substring(0, 1).ToUpper() + resArray[i].Substring(1, resArray[i].Length - 1);

            // set the pivot of the button and set its poisition using determined anchor points
            GoGatherArray[i].transform.localPosition = new Vector2(midPanelAnchorXArray[i % 2], midPanelAnchorYArray[Mathf.CeilToInt(((float)i + 1) / 2) - 1]);

            if (i<=UpgradeList.Count-1){
                try{
                    Debug.Log(Upgrades.transform.Find(UpgradeList[i]).gameObject);
                }catch{
                    
                Instantiate(UpgradePrefab, Upgrades.transform);
                GoUpgArray[i] = Upgrades.transform.Find("UpgradePrefab(Clone)").gameObject;
                GoUpgArray[i].transform.SetParent(Upgrades.transform);
                GoUpgArray[i].name = UpgradeList[i];

                // add a listener to the buttonm, change the text of the button and give it a tag
                GoUpgArray[i].GetComponent<Button>().onClick.AddListener(UpgradeClicked);
                GoUpgArray[i].GetComponentInChildren<TMP_Text>().text =UpgradeList[i];
                //GoUpgArray[i].transform.tag = GoUpgArray[i].name.Replace(" ",""); // Set tag name and remove spaces


                // set the pivot of the button and set its poisition using determined anchor points
                GoUpgArray[i].transform.localPosition = new Vector2(midPanelAnchorXArray[i % 2], midPanelAnchorYArray[Mathf.CeilToInt(((float)i + 1) / 2) - 1]);
                }

            }

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
        
        
        resDiscovered = (int)Main.instance.resDiscovered;

        // hide the undiscovered buttons and resource texts
        for (int i = resArray.Length - 1; i > resDiscovered - 1; i--)
        {
            GoResArray[i].gameObject.SetActive(false);
        }
        for (int i = resArray.Length - 1; i > resDiscovered - 1; i--)
        {
            GoGatherArray[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Method <c> CheckResDict </c> checks which resources have been discovered based on how many resources have been farmed
    /// </summary>
    /// TODO: this should be changed to resources that have been upgraded by the player (consuming resources)
    private void CheckResDisc()
    {
        bool resDiscChanged = false;
        // resources are discovered sequentially
        if (Main.instance.resDiscovered == 1)
        {
            if (Main.instance.GetValue("resDict", "food") > 20)
            {
                UpgradeList.Add("Discover Wood");
                //Main.instance.resDiscovered = 2;
                resDiscChanged = true;
            }
        }
        else if (Main.instance.resDiscovered == 2)
        {
            if (Main.instance.GetValue("resDict", "wood") > 40)
            {
                UpgradeList.Add("Discover Stone");
                //Main.instance.resDiscovered = 3;
                resDiscChanged = true;
            }
        }
        else if (Main.instance.resDiscovered == 3)
        {
            if (Main.instance.GetValue("resDict", "stone") > 50)
            {
                UpgradeList.Add("Discover Minerals");
                //Main.instance.resDiscovered = 4;
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

    /// <summary>
    /// Method <c> TaskClicked </c> is executed when the start task button is clicked
    /// </summary>
    public void TaskClicked()
    {
        // send to Task
        try{
            mainDict.Add("resDiscovered", Main.instance.resDiscovered);
        }catch{
            mainDict["resDiscovered"]=Main.instance.resDiscovered;
        }
        
        Main.instance.StoreMainDict(resDict, "resDict");
        Main.instance.StoreMainDict(mainDict, "mainDict");
        Main.instance.SaveData();
        Main.instance.LoadScene("AimScene");
    }

    /// <summary>
    /// Method <c> IntroClicked </c> is executed when the introduction sceen button is clicked
    /// </summary>
    public void IntroClicked(){
        IntroScreen.SetActive(false);
        Main.instance.introScreenShown = true;
    }

    /// <summary>
    /// Method <c> GatherClicked </c> is executed when the gather button is clicked
    /// </summary>
    public void GatherTabClicked(){
        GatherBut.GetComponent<Image>().color = selectedCol;
        UpgradeBut.GetComponent<Image>().color = deselectedCol;
        GatherRes.SetActive(true);
        Upgrades.SetActive(false);
    }

    /// <summary>
    /// Method <c> UpgradeClicked </c> is executed when the upgrade button is clicked
    /// </summary>
    public void UpgradeTabClicked(){
        GatherBut.GetComponent<Image>().color = deselectedCol;
        UpgradeBut.GetComponent<Image>().color = selectedCol;
        GatherRes.SetActive(false);
        Upgrades.SetActive(true);
        
        if (UpgradeList.Count>=1){
            Upgrades.transform.Find("NoUpg").gameObject.SetActive(false);
            UIGeneration(); // refresh upgrades
        }else{
            Upgrades.transform.Find("NoUpg").gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Method <c> UpgradeClicked </c> is executed when an upgrade has been clicked
    /// </summary>
    public void UpgradeClicked(){
        Debug.Log("upgrade clicked");
        Main.instance.resDiscovered ++;
        UpgradeList.Remove(EventSystem.current.currentSelectedGameObject.name);
        EventSystem.current.currentSelectedGameObject.SetActive(false);

        try{
            mainDict.Add("resDiscovered", Main.instance.resDiscovered);
        }catch{
            mainDict["resDiscovered"]=Main.instance.resDiscovered;
        }
        Main.instance.StoreMainDict(resDict, "resDict");
        Main.instance.StoreMainDict(mainDict, "mainDict");
        Main.instance.SaveData();

        UIGeneration(); // refresh resources
      
    }

    /// <summary>
    /// Method <c> ReturnClicked </c> is executed when the upgrade tab is clicked
    /// </summary>
    public void ReturnClicked()
    {
        // return to main screen     
        TaskChoice.SetActive(false);
        Resources.SetActive(true);
        UpgAndGather.SetActive(true);

    }

    /// <summary>
    /// Method <c> TaskButtonClicked </c> is executed when the task button is clicked
    /// </summary>
    public void TaskButtonClicked()
    {
        Main.instance.currentResource = EventSystem.current.currentSelectedGameObject.tag.ToLower();
        Debug.Log(Main.instance.currentResource);
        // Resources.SetActive(false);
        // UpgAndGather.SetActive(false);
        TaskChoice.SetActive(true);
        TaskChoice.transform.Find("UpgName").GetComponent<TMP_Text>().text = lastClicked;
    }

    public void Quit(){
        try{
            mainDict.Add("resDiscovered", Main.instance.resDiscovered);
        }catch{
            mainDict["resDiscovered"]=Main.instance.resDiscovered;
        }
        Main.instance.StoreMainDict(resDict, "resDict");
        Main.instance.StoreMainDict(mainDict, "mainDict");
        Main.instance.SaveData();
        Application.Quit();
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