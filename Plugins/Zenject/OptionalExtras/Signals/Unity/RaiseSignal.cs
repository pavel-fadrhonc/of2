#if ODIN_INSPECTOR
using Sirenix.Serialization;
using Sirenix.OdinInspector;
#endif
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Plugins.Zenject.OptionalExtras.Signals.Unity
{
    public interface ISignal {}

    public class RaiseSignal : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string _signalType;
        private object[] _parameters;
        
        [HideInInspector][SerializeField]
        private byte[] _parametersSerializedBA;

        [SerializeField]
        private List<Object> referencedObjects = new List<Object>(); 

        private Type _signalTypeClass;
        private FieldInfo[] _paramFieldInfos;

        [Inject] 
        private SignalBus _signalBus;

        private bool _init;

        private void OnEnable()
        {
            if (!_init)
                Init();
        }

    #if ODIN_INSPECTOR
        [Button]
    #endif
        public void Raise()
        {
            var signal = Activator.CreateInstance(_signalTypeClass);

            for (var index = 0; index < _paramFieldInfos.Length; index++)
            {
                var signalField = _paramFieldInfos[index];
                if (_parameters.Length > index)
                {
                    signalField.SetValue(signal, _parameters[index]);
                }
            }

            _signalBus.Fire(signal);
        }

        public void OnBeforeSerialize()
        {
            if (!_init)
                Init();

            if (!_init)
                return;

    #if ODIN_INSPECTOR
            referencedObjects.Clear();
            _parametersSerializedBA =
     SerializationUtility.SerializeValueWeak(_parameters as object, DataFormat.JSON, out referencedObjects);
    #endif
        }

        public void OnAfterDeserialize()
        {
    #if ODIN_INSPECTOR        
            _parameters =
                SerializationUtility.DeserializeValueWeak(_parametersSerializedBA, DataFormat.JSON, referencedObjects) as object[];
    #endif
        }

        public void Init()
        {
            if (_signalType == null)
                return;
            
            _signalTypeClass = Type.GetType(_signalType);
            _paramFieldInfos = _signalTypeClass.GetFields();

            if (_parameters == null)
                _parameters = new object[_paramFieldInfos.Length];
            
            _init = true;
        }
    }
}

