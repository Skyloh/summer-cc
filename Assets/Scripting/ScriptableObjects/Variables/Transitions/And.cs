using RobotPlatformer.Variables;
using TNRD;
using UnityEngine;

namespace RobotPlatformer.Transitions
{
    [CreateAssetMenu(fileName = "And", menuName = "ScriptableObjects/Transitions/And", order = 1)]
    public sealed class And : ATransition
    {
        [SerializeField] private SerializableInterface<IVariable<bool>> value1;
        [SerializeField] private SerializableInterface<IVariable<bool>> value2;

        public override bool Get()
        {
            return value1.Value.Get() && value2.Value.Get();
        }

        public override bool HasNew()
        {
            return value1.Value.HasNew() || value2.Value.HasNew();
        }

        public override void Reset()
        {
            value1.Value.Reset();
            value2.Value.Reset();
        }
    }

}