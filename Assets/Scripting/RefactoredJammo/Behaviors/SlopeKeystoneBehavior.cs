using RobotPlatformer.Variables;
using UnityEngine;

namespace RobotPlatformer.Player.Behavior
{
    // TODO: there appears to be a bounce that occurs when landing on a slope. Place jammo about a slope and let him drop a bit to see it.
    // The state is entered and exited then entered again.
    public sealed class SlopeKeystoneBehavior : ABehavior // keystone just means any other behavior with "Slope" in the name requires this one.
    {
        [Header("Contextual Variables")]
        [SerializeField] private RaycastHitVariable _spherecastHitInfo;
        [SerializeField] private Vector3Variable _perspAlignedInput; // wish I didn't have this, because I really feel like I don't need it?

        [Header("Contextual Mutating Variables")]
        [SerializeField] private BoolVariable _onIllegalSlope;
        [SerializeField] private FloatVariable _slopeSimToPerspInput;

        [Header("Exposed Constants")]
        [SerializeField, Range(-180f, 180f)] private float SlideAngle = 45f;
        [SerializeField] private float EdgeShoveForce = 35f;
        [SerializeField, Range(-1f, 1f)] private float SlopeComparisonAccuracy = 0.9f;
        [SerializeField, Range(0f, 1f)] private float CheckRayOriginMixer = 0.8f;
        [SerializeField] private float CheckRayOriginYOffset = 1f;
        [SerializeField] private float SlopeEdgeRaycastLength = 1.5f;
        [SerializeField] private AnimationCurve SlopeScalarCurve;

        private float m_normalAngleFromUp;

        public override void PreUpdate()
        {
            // this needs to be here because there is a case where we transition from a slope to grounded too fast and miss the angle period where
            // we reassign onIllegalSlope (1>), but also where the slope is legal (SLIDE_ANGLE<). In our case, that makes the window 1 - 45 degrees.
            // As such, running into the slope can sometimes leave you sliding on the floor as you miss the reassignment-to-false window.
            _onIllegalSlope.Set(false);
            _slopeSimToPerspInput.Set(1f);
        }

        protected override bool IsActive(int _)
        {
            // we're on an illegal slope if we didn't miss but also if the slope is too slanted for us to stand on.
            m_normalAngleFromUp = Vector3.Angle(Vector3.up, _spherecastHitInfo.Get().normal);

            // if we aren't on a flat surface, we need to make sure we're not hanging on a corner.
            // to do this, we'll use a simple raycast offset by the spherecast's collision point to check if we're 
            // really on a slant. If we don't hit something, that means the place the spherecast hit is the edge of
            // a vertical face that doesn't slant toward us
            return _spherecastHitInfo.DidCastHit() && EvaluateTransitions() && Mathf.Abs(m_normalAngleFromUp) > 1f; // if the normal is not nigh-perfectly vertical
        }

        public override void Affect(Rigidbody body)
        {
            RaycastHit hitInfo = _spherecastHitInfo.Get();

            Vector3 position = transform.position;
            Vector3 point = hitInfo.point;

            // ray origin is halfway between point and position on XZ.
            Vector3 origin = new Vector3(
                Mathf.Lerp(position.x, point.x, CheckRayOriginMixer),
                position.y + CheckRayOriginYOffset,
                Mathf.Lerp(position.z, point.z, CheckRayOriginMixer));

            bool isAngleIllegal = m_normalAngleFromUp > SlideAngle;

            // NOTE:
            // used to have a !didInitialSphereCastMiss. The weird thing is that this entire block shouldnt run if there is no ground.
            // while Vector3's default is a 0 magnitude vector, i didnt want to take a chance of this somehow activating, so I added the miss check
            // to the If Statement.
            bool onIllegalSlope =
                isAngleIllegal
                && Physics.Raycast(origin, Vector3.down, out RaycastHit check, SlopeEdgeRaycastLength) // are we on a slope, or just hanging on an edge?
                && Vector3.Dot(check.normal, hitInfo.normal) > SlopeComparisonAccuracy; // are the two normals we hit close enough in slope-ness?
            
            Vector3 flattenedHitInfoNormal = Utils.Flatten(hitInfo.normal, true);

            // if the second conditional isnt there, we slide down legal slopes due to this force.
            if (!onIllegalSlope && isAngleIllegal)
            {
                // note that this force is also applied for a very brief period during the below mentioned "reassignment window"
                // it doesn't really matter because it's applying a subtle force in a direction you are already moving in, but 
                // it is important to document anyway.
                body.AddForce(flattenedHitInfoNormal * EdgeShoveForce, ForceMode.Force);
            }

            if (onIllegalSlope)
            {
                // calculates the similarity between the normal's lateral direction vs the player's desired move direction this frame.
                // _slopeSimToPerspInput.Set((Vector3.Dot(_perspAlignedInput.Get(), flattenedHitInfoNormal) + 1));
                float dot = Vector3.Dot(_perspAlignedInput.Get(), flattenedHitInfoNormal);
                _slopeSimToPerspInput.Set(SlopeScalarCurve.Evaluate((dot + 1) / 2)); 
                // dot = [0,1]
                // 0 is most different from slope direction, whereas 1 is most similar
                // similar is the case in which we want MORE force.
            }

            _onIllegalSlope.Set(onIllegalSlope);
        }
    }
}