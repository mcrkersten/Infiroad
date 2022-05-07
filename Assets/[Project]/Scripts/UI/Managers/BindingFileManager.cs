using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class BindingFileManager
{
    public static void SaveBindingData(InputType inputType)
    {
        PlayerInputBindings data = null;
        switch (inputType)
        {
            case InputType.Keyboard:
                data = BindingManager.Instance.playerInputBindings[0];
                break;
            case InputType.Gamepad:
                data = BindingManager.Instance.playerInputBindings[1];
                break;
            case InputType.Wheel:
                data = BindingManager.Instance.playerInputBindings[2];
                break;
        }
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/BindingData_" + data.inputBindings.inputType + ".bindings";
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, data.inputBindings);
        stream.Close();
    }

    public static PlayerInputBindings LoadBindingData(InputType inputType)
    {
        string path = Application.persistentDataPath + "/BindingData_" + inputType + ".bindings";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            InputBindings data = formatter.Deserialize(stream) as InputBindings;
            PlayerInputBindings newB = ScriptableObject.CreateInstance<PlayerInputBindings>();
            newB.inputBindings = data;
            return newB;
        }
        else
        {
            return null;
        }
    }
}