using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu, System.Serializable]
public class PlayerInputBindings : ScriptableObject
{
    public InputBindings inputBindings;

    public void SetBinding(int key, InputBinding binding)
    {
        BindingObject bo = inputBindings.bindings[key];
        bo.SetKeyPath(binding);
        bo.key = key;
    }

    public bool CheckBindings()
    {
        foreach (BindingObject bo in inputBindings.bindings)
        {
            if (bo.inputBinding == null)
                return false;
        }
        return true;
    }
}

[System.Serializable]
public class BindingObject
{
    [SerializeField] public string bindingName;
    public InputBinding inputBinding;
    public int key;

    public void SetKeyPath(InputBinding ib)
    {
        this.inputBinding = ib;
        bindingName = InputControlPath.ToHumanReadableString(ib.effectivePath);
    }
}

[System.Serializable]
public class InputBindings
{
    public InputType inputType;
    public List<BindingObject> bindings = new List<BindingObject>();
}