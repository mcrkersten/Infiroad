using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

[CreateAssetMenu, System.Serializable]
public class PlayerInputBindings : ScriptableObject
{
    public InputType inputType;
    public List<BindingObject> bindings = new List<BindingObject>();

    public void SetBinding(int key, InputBinding binding)
    {
        BindingObject bo = bindings[key];
        bo.SetKeyPath(binding);
    }

    public bool CheckBindings()
    {
        foreach (BindingObject bo in bindings)
        {
            if (bo.inputBinding == null)
                return false;
        }
        return true;
    }

    [System.Serializable]
    public class BindingObject
    {
        [SerializeField] public string bindingName;
        public InputBinding inputBinding;

        public void SetKeyPath(InputBinding ib)
        {
            this.inputBinding = ib;
            bindingName = InputControlPath.ToHumanReadableString(ib.effectivePath);
        }
    }
}