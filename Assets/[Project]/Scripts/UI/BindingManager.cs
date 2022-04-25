using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class BindingManager : MonoBehaviour
{
    public PlayerInputBindings selectedBinding;
    public List<PlayerInputBindings> playerInputBindings = new List<PlayerInputBindings>();
    [SerializeField] private List<PlayerInputBindings> defaultPlayerInputBindings = new List<PlayerInputBindings>();

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

        List<InputType> fixedErrors = new List<InputType>();
        foreach (InputType inputType in bindingErrors)
        {
            PlayerInputBindings selectedBinding = defaultPlayerInputBindings.Where(i => i.inputType == inputType).FirstOrDefault();
            if(selectedBinding != null)
            {
                bindings.Add(selectedBinding);
                fixedErrors.Add(inputType);
            }
        }

        foreach (InputType inputType in fixedErrors)
        {
            bindingErrors.Remove(inputType);
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
