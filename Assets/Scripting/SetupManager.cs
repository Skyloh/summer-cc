using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace RobotPlatformer
{
    public sealed class SetupManager : MonoBehaviour
    {
        [SerializeField] UnityEvent launch;

        private void Awake()
        {
            launch.Invoke();
        }
    }

}