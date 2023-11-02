using RobotPlatformer.Variables;
using System.Collections.Generic;
using UnityEngine;

namespace RobotPlatformer.Animation
{
    [RequireComponent(typeof(Animator))]
    public sealed class AnimatorCoupler : MonoBehaviour
    {
        private Animator _animator;

        // variable tables
        [SerializeField] private List<SerializableKeyValuePair<string, AVariable<float>>> _floatVarTable;
        [SerializeField] private List<SerializableKeyValuePair<string, AVariable<int>>> _intVarTable;
        [SerializeField] private List<SerializableKeyValuePair<string, AVariable<bool>>> _boolVarTable;

        private Dictionary<int, AVariable<float>> _floatValueSource;
        private Dictionary<int, AVariable<int>> _intValueSource;
        private Dictionary<int, AVariable<bool>> _boolValueSource;

        [SerializeField] private SerializableAnimatorControllerParameter[] _paramArray;

        // sets up the fields.
        private void Awake()
        {
            _animator = GetComponent<Animator>();

            SetupDictionaries();
        }

        // iterates through every parameter and updates them.
        //
        // TODO: optimize this loop.
        private void Update()
        {
            foreach (var param in _paramArray)
            {
                // if only i was better at reflection, I could make this switch go away :pensive:
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Float:

                        if (TryGetNew(_floatValueSource[param.hash], out float f))
                        {
                            _animator.SetFloat(param.hash, f);
                        }

                        break;

                    case AnimatorControllerParameterType.Int:

                        if (TryGetNew(_intValueSource[param.hash], out int i))
                        {
                            _animator.SetInteger(param.hash, i);
                        }

                        break;

                    case AnimatorControllerParameterType.Bool:

                        if (TryGetNew(_boolValueSource[param.hash], out bool b))
                        {
                            _animator.SetBool(param.hash, b);
                        }

                        break;
                }
            }
        }


        // This does not get compiled into the build.
        //
        // Fills the param array with values sourced from the attached animator controller.
        private void OnValidate()
        {
            var anim = GetComponent<Animator>();
            _paramArray = new SerializableAnimatorControllerParameter[anim.parameterCount];

            var paramSource = anim.parameters;
            for (int i = 0; i < anim.parameterCount; i+=1)
            {
                _paramArray[i] = new(paramSource[i]);
            }
        }

        private void SetupDictionaries()
        {
            _floatValueSource = new();
            _intValueSource = new();
            _boolValueSource = new();

            foreach (var item in _paramArray)
            {
                switch (item.type)
                {
                    case AnimatorControllerParameterType.Float:
                        _floatValueSource.Add(item.hash, TryGetFromTable(item.name, _floatVarTable));
                        break;

                    case AnimatorControllerParameterType.Int:
                        _intValueSource.Add(item.hash, TryGetFromTable(item.name, _intVarTable));
                        break;

                    case AnimatorControllerParameterType.Bool:
                        _boolValueSource.Add(item.hash, TryGetFromTable(item.name, _boolVarTable));
                        break;

                    default:
                        break;
                }
            }
        }

        private V TryGetFromTable<K, V>(K key, List<SerializableKeyValuePair<K, V>> table)
        {
            foreach (var item in table)
            {
                if (key.GetHashCode() == item.key.GetHashCode())
                {
                    return item.value;
                }
            }

            throw new System.Exception("Key not found.");
        }

        private bool TryGetNew<T>(IVariable<T> source, out T dest)
        {
            if (source.HasNew())
            {
                dest = source.Get();
                return true;
            }

            dest = default;
            return false;
        }
    }

}