using UnityEngine;

namespace RobotPlatformer.Variables
{
    [CreateAssetMenu(fileName = "RaycastHitVar", menuName = "ScriptableObjects/Variables/RaycastHit", order = 1)]
    public class RaycastHitVariable : AVariable<RaycastHit> 
    {
        private bool _didHit;

        public override void Set(RaycastHit value)
        {
            base.Set(value);

            _didHit = value.collider != null;
        }

        public bool DidCastHit()
        {
            return _didHit;
        }
    }
}
