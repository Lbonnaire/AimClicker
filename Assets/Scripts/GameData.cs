
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public float food;
    public float wood;
    public float stone;
    public float minerals;
    public float bronze;
    public float copper;
    public float iron;
    public float gold;
    public float resDiscovered=1;
    public float highscore;


    public void SetValue(string valueName, float value)
    {
        switch (valueName)
        {
            // resources
            case "food":
                this.food = value;
                break;
            case "wood":
                this.wood = value;
                break;
            case "stone":
                this.stone = value;
                break;
            case "minerals":
                this.minerals = value;
                break;
            case "bronze":
                this.bronze = value;
                break;
            case "copper":
                this.copper = value;
                break;
            case "iron":
                this.iron = value;
                break;
            case "gold":
                this.gold = value;
                break;

            //task 
            case "highscore":
                this.highscore = value;
                break;
                
            //main
            case "resDiscovered":
                this.resDiscovered = value;
                break;

        }
    }
    public float GetValue(string resName)
    {
        float valueToReturn = 0;
        switch (resName)
        {
            // resources
            case "food":
                valueToReturn = this.food;
                break;
            case "wood":
                valueToReturn = this.wood;
                break;
            case "stone":
                valueToReturn = this.stone;
                break;
            case "minerals":
                valueToReturn = this.minerals;
                break;
            case "bronze":
                valueToReturn = this.bronze;
                break;
            case "copper":
                valueToReturn = this.copper;
                break;
            case "iron":
                valueToReturn = this.iron;
                break;
            case "gold":
                valueToReturn = this.gold;
                break;

            //task
            case "highscore":
                valueToReturn = this.highscore;
                break;

            case "resDiscovered":
                valueToReturn = this.resDiscovered;
                break;


        }
        return valueToReturn;
    }
}
