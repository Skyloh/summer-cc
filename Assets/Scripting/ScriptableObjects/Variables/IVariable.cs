namespace RobotPlatformer.Variables
{
    public interface IVariable<T>
    {
        T Get();
        void Set(T value);
        void Reset();
        bool HasNew();
    }
}