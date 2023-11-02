using UnityEngine;
using UnityEngine.InputSystem;

namespace RobotPlatformer.Variables
{
    [CreateAssetMenu(fileName = "InputBoolVar", menuName = "ScriptableObjects/Variables/InputBoolean", order = 1)]
    public sealed class InputBoolVariable : BoolVariable, IReader<bool>, ISetup
    {
        [SerializeField, Tooltip("1 = Started, 2 = Performed, 4 = Canceled. 3 = Start+Performed, etc.")] int listeningFlags;
        [SerializeField, Tooltip("1 = Started, 2 = Performed, 4 = Canceled. 3 = Start+Performed, etc.")] int invertedFlags;
        ContextEnum flag;

        bool invertStarted;
        bool invertPerformed;
        bool invertCanceled;

        [SerializeField, Tooltip("If one context evaluates to true, short circuit the rest.")] bool shortCircuit;
        [SerializeField] bool ignoreFollowingContexts = false;

        bool m_ignore = false;

        public void Init()
        {
            flag = (ContextEnum)listeningFlags;
            ContextEnum tempFlag = (ContextEnum)invertedFlags;

            invertStarted = (tempFlag & ContextEnum.Started) == ContextEnum.Started;
            invertPerformed = (tempFlag & ContextEnum.Performed) == ContextEnum.Performed;
            invertCanceled = (tempFlag & ContextEnum.Canceled) == ContextEnum.Canceled;
        }

        // go through each flag and see if we need to listen to it.
        // for every one we do need to listen to, check to see if the
        // state is active. If it is, we don't need to check anymore,
        // as we short-circuit on True.
        public void ReadFrom(InputAction.CallbackContext context)
        {
            if (m_ignore)
            {
                if (context.canceled)
                {
                    m_ignore = false;
                }

                return;
            }

            bool result = false;

            if ((!shortCircuit || !result) && context.started && (flag & ContextEnum.Started) == ContextEnum.Started)
            {
                result = !invertStarted;
                m_ignore = ignoreFollowingContexts;
            }

            if ((!shortCircuit || !result) && context.performed && (flag & ContextEnum.Performed) == ContextEnum.Performed)
            {
                result = !invertPerformed;
                m_ignore = ignoreFollowingContexts;
            }

            if ((!shortCircuit || !result) && context.canceled && (flag & ContextEnum.Canceled) == ContextEnum.Canceled)
            {
                result = !invertCanceled;
                m_ignore = ignoreFollowingContexts;
            }

            Set(result);
        }
    }
}
