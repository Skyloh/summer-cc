using RobotPlatformer.Variables;
using TNRD;
using UnityEngine;

namespace RobotPlatformer.Player.Behavior
{
    // doesn't really seem to do much, but what it does is important.
    public sealed class GroundKeystoneBehavior : ABehavior // keystone just means any other behavior with "Ground" in the name requires this one.
    {
        [Header("Contextual Variables")]
        [SerializeField] private SerializableInterface<IVariable<bool>> _onIllegalSlope;
        [SerializeField] private SerializableInterface<IVariable<RaycastHit>> _raycastHit;

        [Header("Contextual Mutating Variables")]
        [SerializeField] private SerializableInterface<IVariable<bool>> _isGrounded;
        [SerializeField] private SerializableInterface<IVariable<float>> _yVeloDestination;

        [Header("Exposed Constants")]
        [SerializeField] private float GroundedDistance = 0.6f;

        public override void PreUpdate()
        {
            RaycastHit data = _raycastHit.Value.Get();
            bool didInitialSphereCastMiss = data.collider == null;

            // sets us to grounded if we didn't miss a collision and the hit was within "grounded" range.
            // We are also grounded if we're on an illegal slope, but that has a caveat in that we can't jump.
            _isGrounded.Value.Set((!didInitialSphereCastMiss && data.distance < GroundedDistance) || _onIllegalSlope.Value.Get()); // .6
        }

        public override void Affect(Rigidbody body)
        {
            _yVeloDestination.Value.Set(body.velocity.y); // this doesn't need to be in here. It's a passive listening thing.
        }
    }
}