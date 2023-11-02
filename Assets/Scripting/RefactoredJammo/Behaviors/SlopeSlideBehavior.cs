using RobotPlatformer.Variables;
using UnityEngine;

namespace RobotPlatformer.Player.Behavior
{
    // TODO
    // look into the potential of having states have identical BitFlags so that if one state activates, both are "active".
    // In the context of slope keystone and slope slide, it's a possibility.
    public sealed class SlopeSlideBehavior : ABehavior
    {
        [Header("Contextual Variables")]
        [SerializeField] private BoolVariable _onIllegalSlope;
        [SerializeField] private RaycastHitVariable _raycastHit;

        [Header("Contextual Mutating Variables")]
        [SerializeField] private BoolVariable _isSliding;

        [Header("Exposed Constants")]
        [SerializeField] private Transform DirectionalSourceBody;
        [SerializeField] private float SlopeShoveForce = 35f;
        [SerializeField, Tooltip("If the Y velo is less than this, toggles visual sliding.")] 
        private float MinSlidingYVelo = -1f;
        [SerializeField, Range(-1f, 1f), Tooltip("If the dot of the flat slope normal with the transform is greater than this, toggles visual sliding.")] 
        private float SlidingDirectionalDot = -0.1f;
        [SerializeField, Range(0f, 90f), Tooltip("You shouldn't need to touch this, but this is the bound for the angle with Vector3.down that flips the downslope vector.")] 
        private float DownslopeFlipAngle = 89f;

        /*protected override bool IsActive(int state)
        {
            return _onIllegalSlope.Get();
        }*/

        public override void OnStateExit()
        {
            _isSliding.Set(false);
        }

        public override void Affect(Rigidbody body)
        {
            Vector3 hitDataNormal = _raycastHit.Get().normal;
            Vector3 bodyForward = DirectionalSourceBody.forward;

            Vector3 downSlopeDirection = -(bodyForward - Vector3.Dot(bodyForward, hitDataNormal) * hitDataNormal).normalized;

            // if velo is moving downwards, invert the force direction. Since the velo is moving down, the trans.forward is facing
            // downwards as well, meaning our "downslope" vector is actually up-slope and therefore needs to be flipped.
            if (Vector3.Angle(downSlopeDirection, Vector3.down) > DownslopeFlipAngle)
            {
                downSlopeDirection *= -1f;
            }

            if (_isSliding)
            {
                _isSliding.Set(
                    body.velocity.y < MinSlidingYVelo
                    || 
                    Vector3.Dot(Utils.Flatten(hitDataNormal, true), bodyForward) > SlidingDirectionalDot);
            }

            // slope force to push player away from slope
            body.AddForce(downSlopeDirection * SlopeShoveForce, ForceMode.Force);
        }
    }
}