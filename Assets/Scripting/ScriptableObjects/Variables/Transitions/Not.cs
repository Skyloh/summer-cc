using RobotPlatformer.Variables;
using TNRD;
using UnityEngine;

namespace RobotPlatformer.Transitions
{
    [CreateAssetMenu(fileName = "Not", menuName = "ScriptableObjects/Transitions/Not", order = 1)]
    public sealed class Not : ATransition
    {
        [SerializeField] private SerializableInterface<IVariable<bool>> value1;

        public override bool Get()
        {
            return !value1.Value.Get();
        }

        public override bool HasNew()
        {
            return value1.Value.HasNew();
        }

        public override void Reset()
        {
            value1.Value.Reset();
        }
    }

}