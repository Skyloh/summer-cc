using RobotPlatformer.Variables;
using UnityEngine;

namespace RobotPlatformer.Player.Behavior
{
    public sealed class MoveBehavior : ABehavior
    {
        [Header("Contextual Variables")]
        [SerializeField] private BoolVariable _isMovePressed;

        [SerializeField, Tooltip("Classified as mutating in case runtime changes are required.")]
        private FloatVariable _frameDelta;

        [SerializeField, Tooltip("The magnitude of a slope's similarity to the perspective input.")] private FloatVariable _slopeSimToPerspInput;
        [SerializeField] private Vector3Variable _perspectiveAlignedInput;

        [Header("Animator Mutating Variables")]
        // In the future, pull these out of Movement. It shouldn't be a predicate to have these values exposed for the gameobject to be able to move.
        [SerializeField] private FloatVariable _inputMagnitudeDestination;

        [Header("Exposed Constants")]
        [SerializeField] private float MaxSpeed = 4f;
        [SerializeField] private float Acceleration = 7500f;
        [SerializeField,
            Range(-1f, 1f),
            Tooltip("The upper bound for if an input is considered \"too similar\" to current velocity. If true, " +
            "it is ignored unless the current velocity isn't capped yet.")]
        private float InputVeloSimilarityBound = 0.1f;

        [SerializeField] private float InputSimilarityExponent = 1.5f;

        [SerializeField, Range(0f, 1f), Tooltip("The upper bound for the magnitude normalized lateral velocity of the Rigidbody.")]
        private float NormLatVeloMagBound = 1f;

        private float m_prevSpeedValue;

        public override void PreUpdate()
        {
            if (_inputMagnitudeDestination)
            {
                float v = Mathf.Lerp(m_prevSpeedValue, _isMovePressed.Get() ? 1f : 0f, 0.1f);
                m_prevSpeedValue = v;

                _inputMagnitudeDestination.Set(v);
            }
        }

        /*protected override bool IsActive(int _)
        {
            return _isMovePressed.Get();
        }*/

        public override void Affect(Rigidbody body)
        {
            Vector3 velocity = body.velocity;
            Vector3 perspAlignedInput = _perspectiveAlignedInput.Get();

            float inputSimilarityToVelocity = Vector3.Dot(perspAlignedInput, velocity.normalized);
            float normalizedLateralVelocityMagnitude = Utils.GetSquaredLateralVelocity(velocity) / Mathf.Pow(MaxSpeed, 2f);

            if (inputSimilarityToVelocity < InputVeloSimilarityBound || normalizedLateralVelocityMagnitude < NormLatVeloMagBound)
            {
                float inputSimilarityScalar = Mathf.Pow(Mathf.Abs(inputSimilarityToVelocity - 2), InputSimilarityExponent);

                body.AddForce(
                    Acceleration * _frameDelta.Get() * inputSimilarityScalar * _slopeSimToPerspInput.Get()
                    * perspAlignedInput);
            }
        }
    }
}