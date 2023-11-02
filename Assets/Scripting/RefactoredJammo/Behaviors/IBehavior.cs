using UnityEngine;

namespace RobotPlatformer.Player.Behavior
{
    public interface IBehavior
    {
        int GetTier(); // used for sorting
        bool Active(ref int state); // returns if state is active and is to be activated this frame, but also modifies the state to reflect it.
        void OnStateEnter();
        void OnStateExit();
        void PreUpdate(); // invoked regardless of state activation
        void Affect(Rigidbody body);
    }
}