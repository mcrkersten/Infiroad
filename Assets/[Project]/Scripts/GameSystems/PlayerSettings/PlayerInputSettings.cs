using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
[CreateAssetMenu, System.Serializable]
public class PlayerInputSettings : ScriptableObject
{
    public InputType inputType;
    public List<BindingObject> bindings = new List<BindingObject>();

    [System.Serializable]
    public class BindingObject
    {
        [SerializeField] private KeyBinding binding;
        public KeyBinding Binding { get { return binding; } }

        private string keyPath;
        public string KeyPath { get { return keyPath; } }

        public void SetKeyPath(string keyPath)
        {
            this.keyPath = keyPath;
        }
    }
}
