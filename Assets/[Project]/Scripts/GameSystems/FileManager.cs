using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class FileManager
{
    private static string RaceDataPath => Path.Combine(Application.persistentDataPath, "RaceData.run");

    public static void SaveRaceData(RaceData data)
    {
        string path = RaceDataPath;
        string tempPath = path + ".tmp";
        try
        {
            // Write to a temp file first so a failed save never corrupts the existing best run
            using (FileStream stream = new FileStream(tempPath, FileMode.Create))
            {
                new BinaryFormatter().Serialize(stream, data);
            }
            if (File.Exists(path))
                File.Delete(path);
            File.Move(tempPath, path);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not save race data to {path}: {e.Message}");
        }
    }

    public static RaceData LoadRaceData()
    {
        string path = RaceDataPath;
        if (!File.Exists(path))
            return null;

        try
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return new BinaryFormatter().Deserialize(stream) as RaceData;
            }
        }
        catch (System.Exception e)
        {
            // Corrupt or outdated file: treat as "no best run yet" instead of breaking the game mode
            Debug.LogWarning($"Could not load race data from {path}: {e.Message}");
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
        if (distances == null || distances.Count == 0)
            return 0;
        if(index < distances.Count)
            return distances[index];
        else
        {
            return distances[distances.Count - 1];
        }
    }
}