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
                    TaskChoice,
                    Food,
                    Wood,
                    Stone,
                    Minerals,
                    Bronze,
                    Copper,
                    Iron,
                    Gold;

    public Button GatherFood;

    public Button Task1;
    public Button Task2;

    public string lastClicked; //needs implementation

    private int resourcesShown = 1;

    public Dictionary<string, float> resDict;
    private string[] resArray = new string[8] { "food", "wood", "stone", "minerals", "bronze", "copper", "iron", "gold" };

    void onEnable()
    {

    }

    // Start is called before the first frame update
    void Start()
    {

        resDict = new Dictionary<string, float>();
        for (int i = 0; i < resArray.Length; i++)
        {
            resDict.Add(resArray[i], 0f);
        }

        string path = Application.persistentDataPath + "/saveData/" + "myData.txt";
        InitializeGameObjects();
        if (!File.Exists(path))
        {
            Main.instance.StoreMainDict(resDict, "resDict");
            Main.instance.SaveData();
            Debug.Log("saved!");
        }
        else
        {
            resDict = Main.instance.GetDataDict("resDict");
        }

    }


    // Update is called once per frame
    void Update()
    {

    }


    private void InitializeGameObjects()
    {
        UI = gameObject;
        Resources = UI.transform.Find("Resources").gameObject;
        Upgrades = UI.transform.Find("Upgrades").gameObject;
        TaskChoice = UI.transform.Find("TaskChoice").gameObject;

        Food = Resources.transform.Find("Food").gameObject;
        Wood = Resources.transform.Find("Wood").gameObject;
        Stone = Resources.transform.Find("Stone").gameObject;
        Minerals = Resources.transform.Find("Minerals").gameObject;
        Bronze = Resources.transform.Find("Bronze").gameObject;
        Copper = Resources.transform.Find("Copper").gameObject;
        Iron = Resources.transform.Find("Iron").gameObject;
        Gold = Resources.transform.Find("Gold").gameObject;

        GatherFood = Upgrades.transform.Find("GatherFood").gameObject.GetComponent<Button>();
        Task1 = TaskChoice.transform.Find("Food").gameObject.GetComponent<Button>();
        Task2 = TaskChoice.transform.Find("Task2").gameObject.GetComponent<Button>();

        GatherFood.onClick.AddListener(UpgButtonClicked);
        Task1.onClick.AddListener(TaskClicked);

        food = new Resource("food", 0, Food);
        wood = new Resource("wood", 0, Wood);
        stone = new Resource("stone", 0, Stone);
        minerals = new Resource("minerals", 0, Minerals);
        bronze = new Ore("bronze", 0, Bronze, "bronze");
        copper = new Ore("copper", 0, Copper, "copper");
        iron = new Ore("iron", 0, Iron, "iron");
        gold = new Ore("gold", 0, Gold, "gold");
        resourcesShown = 1;
        lastClicked = "Gather Food";
        TaskChoice.SetActive(false);
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

        Main.instance.currentResource = EventSystem.current.currentSelectedGameObject.transform.name.ToLower();
        Debug.Log(Main.instance.currentResource);
        Main.instance.LoadScene("AimScene");
    }

    public void UpgButtonClicked()
    {
        Main.instance.DoNothing();
        Resources.SetActive(false);
        Upgrades.SetActive(false);
        TaskChoice.SetActive(true);
        TaskChoice.transform.Find("UpgName").GetComponent<TMP_Text>().text = lastClicked;

        Debug.Log("button clicked");
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