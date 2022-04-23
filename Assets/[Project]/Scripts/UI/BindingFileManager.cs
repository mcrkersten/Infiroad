using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class BindingFileManager
{
    public static void SaveBindingData(PlayerInputBindings data)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/BindingData_" + data.inputType + ".bindings";
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static PlayerInputBindings LoadBindingData(InputType inputType)
    {
        string path = Application.persistentDataPath + "/BindingData_" + inputType + ".bindings";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            PlayerInputBindings data = formatter.Deserialize(stream) as PlayerInputBindings;
            return data;
        }
        else
        {
            return null;
        }
    }
}