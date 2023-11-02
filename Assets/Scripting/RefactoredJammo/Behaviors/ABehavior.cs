using RobotPlatformer.Transitions;
using UnityEngine;

namespace RobotPlatformer.Player.Behavior
{
    public abstract class ABehavior : MonoBehaviour, IBehavior
    {
        [SerializeField, Tooltip("These must all evaluate to true in order to enter the state. No transitions = always active.")] 
        private TransitionEntry[] transitions;

        [SerializeField, Tooltip("The bitflag represented by this number is used to determine if the state is active.")] private int BitPosition;

        // the bitflag representation of the tier, done by left-shifting 1 by the tier value.
        protected int BehaviorBitRepresentation { get; private set; }

        protected virtual void Awake()
        {
            BehaviorBitRepresentation = 1 << BitPosition;
        }

        public abstract void Affect(Rigidbody body);

        public virtual void OnStateEnter() { } // pass
        public virtual void OnStateExit() { } // pass
        public virtual void PreUpdate() { } // pass

        public bool Active(ref int state)
        {
            bool toStayOrEnter = IsActive(state);
            bool isAlreadyActive = (BehaviorBitRepresentation & state) != 0;

            if (!isAlreadyActive && toStayOrEnter)
            {
                OnStateEnter();
                state |= BehaviorBitRepresentation; // adds the bit
            }
            else if (isAlreadyActive && !toStayOrEnter)
            {
                OnStateExit();
                state ^= BehaviorBitRepresentation; // removes the bit
            }

            return toStayOrEnter; // isActive(state) is not included, bc then we'd never leave the state.
        }

        protected virtual bool IsActive(int state)
        {
            return EvaluateTransitions();
        }

        protected bool EvaluateTransitions()
        {
            foreach (TransitionEntry t in transitions)
            {
                if (!t.Evaluate())
                {
                    return false;
                }
            }

            return true;
        }

        public int GetTier()
        {
            return BitPosition;
        }
    }
}