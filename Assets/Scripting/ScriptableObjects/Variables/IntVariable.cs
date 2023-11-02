using UnityEngine;

namespace RobotPlatformer.Variables
{
    [CreateAssetMenu(fileName = "IntVar", menuName = "ScriptableObjects/Variables/Int", order = 1)]
    public sealed class IntVariable : AVariable<int> { }
}