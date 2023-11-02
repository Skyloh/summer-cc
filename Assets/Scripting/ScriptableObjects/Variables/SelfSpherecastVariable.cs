using UnityEngine;

namespace RobotPlatformer.Variables
{
    [CreateAssetMenu(fileName = "SelfSpherecastHitVar", menuName = "ScriptableObjects/Variables/SelfSpherecastHit", order = 1)]
    public sealed class SelfSpherecastVariable : RaycastHitVariable
    {
        [Header("Sweep length does not take sphere radius into account!"), Space(15)]
        [SerializeField] private Vector3 _rayTransformOffset;
        [SerializeField] private Vector3 _rayDir;
        [SerializeField] private float _sphereRadius;
        [SerializeField] private float _rayLength;

        public void DoCast(GameObject source)
        {
            Physics.SphereCast(source.transform.position + _rayTransformOffset, _sphereRadius, _rayDir, out RaycastHit info, _rayLength);

            Set(info);
        }
    }

}