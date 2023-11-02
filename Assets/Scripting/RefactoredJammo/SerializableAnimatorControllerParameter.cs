using UnityEngine;

namespace RobotPlatformer.Animation
{
    [System.Serializable]
    public struct SerializableAnimatorControllerParameter
    {
        public AnimatorControllerParameterType type;

        public string name;
        public int hash;

        // dynamic is just too weird, sorry.
        public int intValue;
        public float floatValue;
        public bool boolValue;

        public SerializableAnimatorControllerParameter(AnimatorControllerParameter source)
        {
            name = source.name;
            hash = source.nameHash;
            type = source.type;

            intValue = source.defaultInt;
            floatValue = source.defaultFloat;
            boolValue = source.defaultBool;
        }
    }
}