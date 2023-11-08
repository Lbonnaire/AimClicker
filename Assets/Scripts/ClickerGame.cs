using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using System.Drawing;
using Unity.VisualScripting;

public class ClickerGame : MonoBehaviour
{
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
    */

    public Resource food;
    public Resource wood;
    public Resource stone;
    public Resource minerals;
    public Ore bronze,
                copper,
                iron,
                gold;

    public GameObject UI,
                    Resources,
                    Upgrades,
                    GatherRes,
                    ResBg,
                    TaskChoice,
                    ResPrefab,
                    GatherPrefab,
                    UpgAndGather;


    public GameObject[] GoResList,
                        GoGatherList;
    // public Button FoodBut,
    //                 WoodBut,
    //                 StoneBut,
    //                 MineralsBut,
    //                 BronzeBut,
    //                 CopperBut,
    //                 IronBut,
    //                 GoldBut;
    public Button[] GoButList;
    public Button TaskBut1;
    public Button TaskBut2;
    public string lastClicked; //needs implementation

    public Dictionary<string, float> resDict;
    public Dictionary<string, float> mainDict;

    private string[] resArray = new string[8] { "food", "wood", "stone", "minerals", "bronze", "copper", "iron", "gold" };
    private UnityEngine.Color[] colourArray = new UnityEngine.Color[8]{UnityEngine.Color.white, new UnityEngine.Color(150,75,0),UnityEngine.Color.gray, new UnityEngine.Color(127, 255, 212), new UnityEngine.Color(244,164,96),new UnityEngine.Color(255, 165, 0),new UnityEngine.Color(169, 169, 169),new UnityEngine.Color(255, 215, 0) };
    private string[] upgArray = new string[2] { "DiscWood", "DiscStone" };

    // void onEnable()
    // {

    // }

    // Start is called before the first frame update
    void Start()
    {


    }


    // Update is called once per frame
    void FixedUpdate()
    {
        CheckMainInit();
    }

    void CheckMainInit()
    {

        if (Main.instance.mainInitialized)
        {
                    resDict = new Dictionary<string, float>();
        for (int i = 0; i < resArray.Length; i++)
        {
            resDict.Add(resArray[i], 0f);
        }
        mainDict = new Dictionary<string, float>();
        

        string path = Application.persistentDataPath + "/saveData/" + "myData.txt";

        if (!File.Exists(path))
        {
            Main.instance.resDiscovered =1;
            mainDict.Add("resDiscovered", Main.instance.resDiscovered);
            Main.instance.StoreMainDict(resDict, "resDict");
            Main.instance.StoreMainDict(mainDict, "mainDict");
            
            Main.instance.SaveData();
            Debug.Log("saved!");
        }
        else
        {
            Main.instance.LoadData(); 
            resDict = Main.instance.GetDataDict("resDict");
        }

            Main.instance.mainInitialized = false;
            CheckResValues();
            InitializeGameObjects();
        }
    }


    private void InitializeGameObjects()
    {
        UI = gameObject;

        Upgrades = UI.transform.Find("UpgsAndGather").gameObject.transform.Find("Upgrades").gameObject;
        GatherRes = UI.transform.Find("UpgsAndGather").gameObject.transform.Find("GatherRes").gameObject;
        Resources = UI.transform.Find("Resources").gameObject;
        ResBg = Resources.gameObject.transform.Find("ResBg").gameObject;
        UpgAndGather = UI.transform.Find("UpgsAndGather").gameObject;
        //Configure anchorpoints for Gather objects and Upgrade objects
        GameObject bgPanelGO = UI.transform.Find("UpgsAndGather").gameObject;

        float Gatherheight = bgPanelGO.GetComponent<RectTransform>().rect.height;
        float Gatherwidth = bgPanelGO.GetComponent<RectTransform>().rect.width;
        float GatherxStartPoint = -Gatherwidth / 2;
        float GatheryStartPoint = Gatherheight / 2;

        float gatherAnchorPointX1 = GatherxStartPoint + Gatherwidth * (0.25f);
        float gatherAnchorPointX2 = GatherxStartPoint + Gatherwidth * (0.75f);
        float[] gatherAnchorXArray = new float[2] { gatherAnchorPointX1, gatherAnchorPointX2 };

        float gatherAnchorPointY1 = GatheryStartPoint - Gatherheight * (0.11f);
        float gatherAnchorPointY2 = GatheryStartPoint - Gatherheight * (0.22f);
        float gatherAnchorPointY3 = GatheryStartPoint - Gatherheight * (0.33f);
        float gatherAnchorPointY4 = GatheryStartPoint - Gatherheight * (0.44f);
        float gatherAnchorPointY5 = GatheryStartPoint - Gatherheight * (0.55f);
        float gatherAnchorPointY6 = GatheryStartPoint - Gatherheight * (0.66f);
        float gatherAnchorPointY7 = GatheryStartPoint - Gatherheight * (0.77f);
        float gatherAnchorPointY8 = GatheryStartPoint - Gatherheight * (0.88f);
        float[] gatherAnchorYArray = new float[8] { gatherAnchorPointY1, gatherAnchorPointY2, gatherAnchorPointY3, gatherAnchorPointY4, gatherAnchorPointY5, gatherAnchorPointY6, gatherAnchorPointY7, gatherAnchorPointY8 };


        float resheight = Resources.GetComponent<RectTransform>().rect.height;
        float reswidth = Resources.GetComponent<RectTransform>().rect.width;
        float resxStartPoint = -reswidth / 2;
        float resyStartPoint = resheight / 2;
        float resAnchorPointX = resxStartPoint + reswidth * (0.05f);
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


        TaskChoice = UI.transform.Find("TaskChoice").gameObject;

        GameObject[] GoGatherList = new GameObject[8];
        GameObject[] GoResList = new GameObject[8];
        //GoButList = new Button[8]{FoodBut,WoodBut,StoneBut,MineralsBut,BronzeBut,CopperBut,IronBut,GoldBut};

        // make array after instantiation dumbass
        for (int i = 0; i < resArray.Length; i++)
        {
            Instantiate(ResPrefab, ResBg.transform);
            Instantiate(GatherPrefab, GatherRes.transform);
            GoResList[i] = ResBg.transform.Find("ResPrefab(Clone)").gameObject;
            GoGatherList[i] = GatherRes.transform.Find("GatherPrefab(Clone)").gameObject;
            GoResList[i].transform.SetParent(ResBg.transform);
            GoGatherList[i].transform.SetParent(GatherRes.transform);

            GoGatherList[i].name = "Gather " + resArray[i].Substring(0, 1).ToUpper() + resArray[i].Substring(1, resArray[i].Length - 1);
            GoResList[i].name = resArray[i].Substring(0, 1).ToUpper() + resArray[i].Substring(1, resArray[i].Length - 1);

            var texture = UnityEngine.Resources.Load("UISprite") as Texture2D;
            GoGatherList[i].GetComponent<Button>().onClick.AddListener(UpgButtonClicked);
            GoGatherList[i].GetComponentInChildren<TMP_Text>().text = "Gather " + resArray[i].Substring(0, 1).ToUpper() + resArray[i].Substring(1, resArray[i].Length - 1);
            GoResList[i].GetComponent<TMP_Text>().text = GoResList[i].name + ": " + Main.instance.GetValue("resDict", resArray[i]).ToString();
            GoResList[i].GetComponent<TMP_Text>().color = colourArray[i];
            GoGatherList[i].transform.tag = resArray[i].Substring(0, 1).ToUpper() + resArray[i].Substring(1, resArray[i].Length - 1);
            GoResList[i].transform.tag = resArray[i].Substring(0, 1).ToUpper() + resArray[i].Substring(1, resArray[i].Length - 1);

            GoResList[i].transform.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

            GoResList[i].transform.localPosition = new Vector2(resAnchorPointX, resAnchorYArray[i]);
            GoGatherList[i].transform.localPosition = new Vector2(gatherAnchorXArray[i % 2], gatherAnchorYArray[Mathf.CeilToInt(((float)i + 1) / 2) - 1]);


        }

        TaskBut1 = TaskChoice.transform.Find("StartTask").gameObject.GetComponent<Button>();
        TaskBut2 = TaskChoice.transform.Find("Return").gameObject.GetComponent<Button>();

        TaskBut1.onClick.AddListener(TaskClicked);
        TaskBut2.onClick.AddListener(ReturnClicked);

        lastClicked = "";
        float resDiscovered = Main.instance.resDiscovered;
        for (int i = resArray.Length - 1; i > resDiscovered - 1; i--)
        {
            GoResList[i].gameObject.SetActive(false);
        }
        for (int i = resArray.Length - 1; i > resDiscovered - 1; i--)
        {
            GoGatherList[i].gameObject.SetActive(false);
        }

        TaskChoice.SetActive(false);
    }

    private void CheckResValues()
    {
        bool resDiscChanged = false;
        // conditions should be bought upgrades instead
        if (Main.instance.resDiscovered == 1)
        {
            if (Main.instance.GetValue("resDict", "food") > 100)
            {
                Main.instance.resDiscovered=2;
                resDiscChanged = true;
            }
        }
        else if (Main.instance.resDiscovered == 2)
        {
            if (Main.instance.GetValue("resDict", "wood") > 30)
            {
                Main.instance.resDiscovered=3;
                resDiscChanged = true;
            }
        }
        else if (Main.instance.resDiscovered == 3)
        {
            if (Main.instance.GetValue("resDict", "stone") > 50)
            {
                Main.instance.resDiscovered=4;
                resDiscChanged = true;
            }
        }
        if (resDiscChanged)
        {
            CheckMainInit();
        }


    }


    public void ButtonClicked()
    {

        lastClicked = EventSystem.current.currentSelectedGameObject.transform.parent.name;

        if (lastClicked == "Upgrades")
        {
            UpgButtonClicked();
        }
        else if (lastClicked == "TaskChoice")
        {
            TaskClicked();
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
        // send to Task     
        TaskChoice.SetActive(false);
        Resources.SetActive(true);
        UpgAndGather.SetActive(true);
        
    }

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



}