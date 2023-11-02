using RobotPlatformer.Variables;

namespace RobotPlatformer.Transitions
{
    public abstract class ATransition : BoolVariable
    {
        public override abstract bool Get();

        public override abstract bool HasNew();

        public override abstract void Reset();

        public override void Set(bool value)
        {
            throw new System.NotImplementedException();
        }
    }

}