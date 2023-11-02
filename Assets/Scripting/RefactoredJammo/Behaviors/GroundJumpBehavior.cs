using RobotPlatformer.Variables;
using TNRD;
using UnityEngine;
using UnityEngine.Events;

namespace RobotPlatformer.Player.Behavior
{
    public sealed class GroundJumpBehavior : ABehavior
    {
        [Header("Contextual Variables")]
        [SerializeField] private SerializableInterface<IVariable<bool>> _onIllegalSlope;

        [Header("Contextual Mutating Variables")]
        [SerializeField] private SerializableInterface<IVariable<bool>> _isGrounded;
        [SerializeField] private SerializableInterface<IVariable<bool>> _isJumpPressed;
        [SerializeField] private SerializableInterface<IVariable<int>> _jumpCount; // externally tracks jump count for transition purposes

        [Header("Exposed Constants")]
        [SerializeField] private int JumpCount = 1; // 0 = no jumping, 1 = grounded jump, 2 = double jump, etc.
        [SerializeField] private float JumpHeight = 2.25f; // a tad inaccurate, give or take .5f.
        [SerializeField, Tooltip("The amount the player is displaced by immediately after pressing jump. Increase this if stuck to ground.")] 
        private float InstantJumpYOffset = 0.1f;
        [SerializeField] private UnityEvent<int> OnForceApplied; // int is for the jump count

        private int m_jumpCount; // internally tracks jump count
        private float m_jumpForce;

        protected override void Awake() // awake or start, doesn't matter.
        {
            base.Awake();

            m_jumpCount = JumpCount;
            _jumpCount.Value.Set(JumpCount);

            m_jumpForce = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * JumpHeight);
        }

        public override void PreUpdate()
        {
            if (_isGrounded.Value.Get() && !_onIllegalSlope.Value.Get())
            {
                m_jumpCount = JumpCount;
                _jumpCount.Value.Set(m_jumpCount);
            }
        }

        public override void Affect(Rigidbody body)
        {
            m_jumpCount -= 1;
            _jumpCount.Value.Set(m_jumpCount);

            _isJumpPressed.Value.Set(false);
            _isGrounded.Value.Set(false);

            OnForceApplied.Invoke(m_jumpCount);

            // velocity change is just better than other types because it is the most consistent.
            // additionally, it allows me to concretely specify what the vertical velocity will always
            // be, unlike Impulse where I had to guess a little.
            body.AddForce((m_jumpForce - body.velocity.y) * Vector3.up, ForceMode.VelocityChange);

            // a cheeky little workaround to the "instant gravity springforce" issue
            body.MovePosition(transform.position + Vector3.up * InstantJumpYOffset);
        }
    }
}
