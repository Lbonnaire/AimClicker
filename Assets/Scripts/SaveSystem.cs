
using UnityEngine;
using System.IO;

using System.Runtime.Serialization.Formatters.Binary;


public static class SaveSystem
{
    public static string directory = "/saveData/";
    public static string filename = "myData.txt";

    public static void SaveData(GameData data)
    {
        Debug.Log("dataIsBeingSaved");
        //BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + directory;
        Debug.Log(Application.persistentDataPath);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path + filename, json);

    }

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