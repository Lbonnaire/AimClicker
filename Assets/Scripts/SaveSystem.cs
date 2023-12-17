
using UnityEngine;
using System.IO;

using System.Runtime.Serialization.Formatters.Binary;
using System;


public static class SaveSystem
{
    // variable declaration for location fo saved file
    public static string directory = "/saveData/";
    public static string filename = "myData.txt";
    
    // Save the gamedata to the path in JSON
    public static void SaveData(GameData data)
    {
        string path = Application.persistentDataPath + directory;
        //if the path doesn't exist create a new file
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path + filename, json);

    }


    //Load the data from the save file and return it as gamedata 
    public static GameData LoadData()
    {

        string fullPath = Application.persistentDataPath + directory + filename;

        GameData data = new GameData();
        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            data = JsonUtility.FromJson<GameData>(json);
        }
        else
        {
            Debug.LogError("Save file not found " + fullPath);
            SaveData(data);
        }
        return data;
    }

}