using RobotPlatformer.Variables;
using UnityEngine;
using TNRD; // not sure about the overhead this adds, but it's nice.
// rn im getting along fine with using scriptable objects deriving from the Variable stuff, tbh. might not need this yet.

namespace RobotPlatformer.Player.Behavior
{
    public sealed class FloatingColliderBehavior : ABehavior
    {
        [Header("Contextual Variables")]
        // [SerializeField] private SerializableInterface<IVariable<bool>> _isJumpPressed;
        // [SerializeField] private SerializableInterface<IVariable<bool>> _onIllegalSlope;
        [SerializeField] private SerializableInterface<IVariable<bool>> _isGrounded;
        [SerializeField] private SerializableInterface<IVariable<RaycastHit>> _raycastHit;
        
        [Header("Exposed Constants")]
        [SerializeField] private Transform BoundTransform;
        [SerializeField] private Vector3 BoundTransformOffset;

        // overshoot is needed so that we stick to slopes instead of just falling off.
        // problem is overshoot just leads to us sticking to the ground
        // again, this wont be an issue when converted to a state machine, as we
        // can just tie the floating collider behavior activate to the grounded
        // state, so that as soon as we just we no longer have to worry about it.
        [SerializeField, Tooltip("Increase this if you notice Jammo behaving strangely with Slopes.")] private float OverShoot = 0.1f;

        [SerializeField] private Vector3 SpringForceAxisUp = Vector3.up;
        [SerializeField] private float RideHeight = 1f;
        [SerializeField] private float DampingCoefficient = 150f;
        [SerializeField] private float SpringCoefficient = 2000f;

        [SerializeField] private float SpherecastRadius = 0.5f; // by default is 0.5f (default capsule collider radius)
        [SerializeField] private LayerMask LayerMask; // Everything
        [SerializeField] private float YLerpTime = 0.2f;

        private Collider m_collider;
        private RaycastHit m_info;
        private float m_yOffset = 0f;

        #if UNITY_EDITOR
        [Header("Exposed For Debugging")]
        public Vector3 rbodyvelo;
        [SerializeField] private Mesh testMesh;
        #endif

        protected override void Awake()
        {
            base.Awake();
            m_collider = GetComponent<Collider>();
        }
/*
        protected override bool IsActive(int _)
        {
            return !_isJumpPressed.Value.Get() && !_onIllegalSlope.Value.Get() && _isGrounded.Value.Get();
        }*/

        public override void PreUpdate()
        {
            // not so sure about this always being performed, but it wont handle transitions properly if placed in Affect.
            // Perhaps I can give this to a different script to handle.
            if (_isGrounded.Value.Get())
            {
                var v = BoundTransform.position;
                v.y = m_yOffset = Mathf.Lerp(m_yOffset, _raycastHit.Value.Get().point.y, YLerpTime);
                BoundTransform.position = v;
            }
        }

        public override void Affect(Rigidbody body)
        {
        #if UNITY_EDITOR
            rbodyvelo = body.velocity;
#endif

            // icky raycasts :(
            // TODO: I could work this into the grounded raycast to reduce calls, but that's for later.
            //
            // Raycast seems to work better here than Spherecast. Here's the spherecast implementation, anyways:
            //   if (Physics.SphereCast(GetRay(), SpherecastRadius, out m_info, RideHeight + OverShoot - SpherecastRadius, LayerMask))
            // It struggles with transitioning between state activation and deactivation. I'll leave the spherecast radius field in, but it's kinda irrelevant.
            //if (Physics.Raycast(GetRay(), out m_info, RideHeight + OverShoot - SpherecastRadius, LayerMask))
            if (Physics.SphereCast(GetRay(), SpherecastRadius, out m_info, RideHeight + OverShoot - SpherecastRadius, LayerMask))
            {
                body.AddForce(GetSpringForce(body.velocity) * SpringForceAxisUp, ForceMode.Force);
            }
        }

        private float GetSpringForce(Vector3 velocity)
        {
            // the math on this is a bit funky, but it works?
            return 
                SpringCoefficient * (RideHeight - m_info.distance - SpherecastRadius) // kx
                - (DampingCoefficient * Vector3.Dot(m_info.normal, velocity.normalized) * velocity.magnitude); // -db
        }

        private Ray GetRay()
        {
            return new Ray(m_collider.bounds.center, Vector3.down);
        }

        private void OnDrawGizmosSelected()
        {
            var center = GetComponent<Collider>().bounds.center;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(center, center + Vector3.down * (m_info.collider != null ? m_info.distance : RideHeight + OverShoot - SpherecastRadius));
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(center + (Vector3.down * (m_info.collider != null ? m_info.distance : RideHeight + OverShoot - SpherecastRadius)), SpherecastRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(center, center + Vector3.down * RideHeight);

            if (m_info.collider != null)
            {
                Gizmos.color = Color.yellow;
                float force = GetSpringForce(rbodyvelo);

                Gizmos.DrawWireMesh(testMesh, 0, center + force * Vector3.up, Quaternion.identity, new Vector3(.5f, force, .5f));
            }
        }
    }
}