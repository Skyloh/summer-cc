using UnityEngine;
using UnityEngine.InputSystem;

namespace RobotPlatformer.Variables
{
    [CreateAssetMenu(fileName = "InputVectorVar", menuName = "ScriptableObjects/Variables/InputVector3", order = 1)]
    public sealed class InputVectorVariable : Vector3Variable, IReader<Vector3>, ISetup
    {
        [SerializeField] bool usesPerspective;
        [SerializeField] bool useMainCamera;

        private Transform perspective;

        public void ReadFrom(InputAction.CallbackContext context)
        {
            Vector2 v2 = context.ReadValue<Vector2>();

            Vector3 v = v2;

            // remap the y value to the z.
            v.z = v.y;
            v.y = 0f;

            if (usesPerspective)
            {
                v = Utils.FlatAlignFromPerspective(v, perspective);
            }

            Set(v);
        }

        public void SetPerspective(Transform perspective)
        {
            this.perspective = perspective;
        }

        public void Init()
        {
            if (!perspective && useMainCamera)
            {
                perspective = Camera.main.transform;
            }
        }
    }
}
