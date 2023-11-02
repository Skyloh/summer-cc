using UnityEngine;

namespace RobotPlatformer.Variables
{
    [CreateAssetMenu(fileName = "FloatVar", menuName = "ScriptableObjects/Variables/Float", order = 1)]
    public sealed class FloatVariable : AVariable<float> { }
}