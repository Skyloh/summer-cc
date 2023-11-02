using RobotPlatformer.Variables;
using System;
using TNRD;
using UnityEngine;

namespace RobotPlatformer.Transitions
{
    public abstract class AComparison<T> : ATransition, ISetup where T : IComparable<T>
    {
        /// <summary>
        /// There are more, but i dont need them right now.
        /// </summary>
        private enum Comparator
        {
            GreaterThan,
            LessThan,
            Equal
        }

        [Header("Comparison Fields")]

        [SerializeField] private SerializableInterface<IVariable<T>> value1;
        [SerializeField] private Comparator mode;
        [SerializeField] private SerializableInterface<IVariable<T>> value2;

        private delegate bool ComparisonOperation(T a, T b);
        private ComparisonOperation operation;

        public override bool Get()
        {
            return operation.Invoke(value1.Value.Get(), value2.Value.Get());
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

        // cursed, yes.
        public void Init()
        {
            switch (mode)
            {
                case Comparator.GreaterThan:
                    operation = (T a, T b) => { return a.CompareTo(b) > 0; };
                    break;
                case Comparator.LessThan:
                    operation = (T a, T b) => { return a.CompareTo(b) < 0; };
                    break;
                case Comparator.Equal:
                    operation = (T a, T b) => { return a.CompareTo(b) == 0; };
                    break;
                default:
                    break;
            }
        }
    }

}