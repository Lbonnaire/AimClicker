
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public Dictionary<string,float> resDict;
    public Dictionary<string,float> taskStatsDict;

    //private string[] resArray =new string[8]{"food","wood","stone","minerals","bronze", "copper","iron","gold"};
    


    public GameData(Main instance)
    {
        resDict = instance.resDict;
        taskStatsDict = instance.taskStatsDict;
    }
}
