using RobotPlatformer.Variables;
using UnityEngine;

namespace RobotPlatformer.Transitions
{
    [System.Serializable]
    public class TransitionEntry
    {
        [SerializeField] private bool invert = false;
        [SerializeField] private BoolVariable expression;

        public bool Evaluate()
        {
            bool result = expression.Get();

            return invert ? !result : result;
        }
    }
}