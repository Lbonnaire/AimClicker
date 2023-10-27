
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public static class SaveSystem
{
    public static void SaveData(Main instance)
    {

        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player.data";
        //Debug.Log(Application.persistentDataPath);
        FileStream stream = new FileStream(path, FileMode.Create);

        GameData data = new GameData(instance);

        formatter.Serialize(stream, data);
        stream.Close();


    }

    public static GameData LoadData()
    {

        string path = Application.persistentDataPath + "/player.data";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            GameData data = formatter.Deserialize(stream) as GameData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found " + path);
            return null;
        }
    }

}