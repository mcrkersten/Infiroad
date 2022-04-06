using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class FileManager
{
    public static void SaveRaceData(RaceData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/RaceData.run";
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static RaceData LoadRaceData()
    {
        string path = Application.persistentDataPath + "/RaceData.run";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            RaceData data = formatter.Deserialize(stream) as RaceData;
            return data;
        }
        else
        {
            return null;
        }
    }
}

[System.Serializable]
public class RaceData
{
    public int distance;
    public int time;
    private List<int> distances;

    public RaceData(int distance, int time)
    {
        distances = new List<int>();
        this.distance = distance;
        this.time = time;
    }

    public void AddDistance(int d)
    {
        distances.Add(d);
    }

    public int ReadDistance(int index)
    {
        if(index < distances.Count)
            return distances[index];
        else
        {
            return distances[distances.Count - 1];
        }
    }
}