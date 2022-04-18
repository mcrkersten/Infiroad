using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

[CreateAssetMenu, System.Serializable]
public class PlayerInputSettings : ScriptableObject
{
    public InputType inputType;
    public List<BindingObject> bindings = new List<BindingObject>();

    public void SetBinding(KeyBinding key, InputBinding binding)
    {
        BindingObject bo = bindings.FirstOrDefault(x => x.Binding == key);
        bo.SetKeyPath(binding);
    }

    public bool CheckBindings()
    {
        foreach (BindingObject bo in bindings)
        {
            if (bo.KeyPath == null)
                return false;
        }
        return true;
    }

    [System.Serializable]
    public class BindingObject
    {
        [SerializeField] private KeyBinding binding;
        public KeyBinding Binding { get { return binding; } }

        private InputBinding inputBinding;
        [SerializeField] public string bindingName;
        public InputBinding KeyPath { get { return inputBinding; } }

        public void SetKeyPath(InputBinding ib)
        {
            this.inputBinding = ib;
            bindingName = InputControlPath.ToHumanReadableString(ib.effectivePath);
        }
    }
}
