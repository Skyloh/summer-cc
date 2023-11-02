using UnityEngine.InputSystem;

namespace RobotPlatformer.Variables
{
    public interface IReader<T>
    {
        void ReadFrom(InputAction.CallbackContext context);
    }
}
