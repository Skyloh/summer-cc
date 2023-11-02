using RobotPlatformer.Player.Behavior;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace RobotPlatformer.Player
{
    public class BehaviorCompositionController : MonoBehaviour
    {
        [SerializeField, Tooltip("If non-empty, overrides the initial composed state so that these states are the only ones active.")]
        private ABehavior[] activeOnStart;
        [SerializeField] private ABehavior[] orderOfExecution;

        [SerializeField] private int _composedState;
        [SerializeField] UnityEvent<GameObject> _fixedUpdateInvocation;

        private IBehavior[] m_states;
        private Rigidbody m_rigidbody;

        void Awake()
        {
            if (DoesArrayExistAndIsNonEmpty(orderOfExecution))
            {
                m_states = orderOfExecution;
            }

            // if we have states to prioritize, start off in them.
            if (DoesArrayExistAndIsNonEmpty(activeOnStart))
            {
                _composedState = Utils.ComposeBehaviors(activeOnStart);
            }

            m_rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            _fixedUpdateInvocation.Invoke(gameObject);

            foreach (IBehavior state in m_states)
            {
                state.PreUpdate();

                if (state.Active(ref _composedState))
                {
                    state.Affect(m_rigidbody);
                }
            }
        }

        private bool DoesArrayExistAndIsNonEmpty<T>(T[] array)
        {
            return array != null && array.Length > 0;
        }
    }
}