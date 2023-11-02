using RobotPlatformer.Variables;
using UnityEngine;

namespace RobotPlatformer.Player.Behavior
{
    public sealed class RotateToVelocityBehavior : ABehavior
    {
        [Header("Contextual Variables")]
        [SerializeField] FloatVariable _frameDelta;
        [SerializeField] BoolVariable _applyYRotToVisual;

        [Header("Exposed Constants")]
        [SerializeField, Tooltip("The behaviors in which this state should be ignored.")] ABehavior[] applyVisualRotationInBehaviors;
        [SerializeField, Tooltip("The Transform that receives all of the rotation in addition to the Main Transform")] Transform visualBody; // TODO
        [SerializeField, Tooltip("If the Y rotation should be applied to the Main Transform.")] bool ApplyYRotationToMain;
        [SerializeField] float RotationLerp;

        private int m_currentState;
        private int m_visualRotationFlags;
        private Vector3 m_lastNonZeroVelo;

        protected override void Awake()
        {
            base.Awake();

            m_visualRotationFlags = Utils.ComposeBehaviors(applyVisualRotationInBehaviors);
        }

        protected override bool IsActive(int state)
        {
            m_currentState = state;
            return EvaluateTransitions();
        }

        public override void Affect(Rigidbody body)
        {
            Vector3 velo = body.velocity;

            if (!ApplyYRotationToMain)
            {
                velo.y = 0f;
            }

            if (velo == Vector3.zero) // guard statement for no-direction rotation.
            {
                return;
            }

            m_lastNonZeroVelo = velo.normalized;

            /*Quaternion rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(velo, Vector3.up),
                    RotationLerp * _frameDelta.Get()); // not the "right way" to lerp, but it's rotation. who cares.*/

            body.MoveRotation(Quaternion.LookRotation(m_lastNonZeroVelo, Vector3.up));


            if (!visualBody) // guard statement for unassigned visual body.
            {
                return;
            }


            bool inVisualRotationState = (m_visualRotationFlags & m_currentState) != 0;

            if (inVisualRotationState)
            {
                /*visualBody.rotation = Quaternion.LookRotation(
                    _applyYRotToVisual.Get() ? body.velocity : velo, 
                    Vector3.up);*/

                visualBody.rotation = Quaternion.Slerp(
                        visualBody.rotation,
                        Quaternion.LookRotation(_applyYRotToVisual.Get() ? body.velocity : m_lastNonZeroVelo, Vector3.up),
                        RotationLerp * _frameDelta.Get());
            }
            else if (!inVisualRotationState && !visualBody.rotation.Equals(default))
            {
                visualBody.rotation = default; // resetting the rotation 
            }
        }
    }
}