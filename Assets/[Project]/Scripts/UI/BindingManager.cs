using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class BindingManager : MonoBehaviour
{
    public List<PlayerInputBindings> playerInputBindings = new List<PlayerInputBindings>();

    public List<InputType> CheckBindingFiles()
    {
        List<InputType> bindingErrors = new List<InputType>();
        List<PlayerInputBindings> bindings = new List<PlayerInputBindings>();
        foreach(InputType inputType in Enum.GetValues(typeof(InputType)))
        {
            PlayerInputBindings b = BindingFileManager.LoadBindingData(inputType);
            if (b != null)
                bindings.Add(b);
            else
                bindingErrors.Add(inputType);
        }

        foreach (PlayerInputBindings binding in bindings)
        {
            PlayerInputBindings selectedBinding = playerInputBindings.Where(i => i.inputType == binding.inputType).First();
            var index = bindings.IndexOf(selectedBinding);

            if (index != -1)
                playerInputBindings[index] = binding;
        }
        return bindingErrors;
    }
}
