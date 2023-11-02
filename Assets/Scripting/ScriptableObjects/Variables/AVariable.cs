using UnityEngine;

namespace RobotPlatformer.Variables
{
    public abstract class AVariable<T> : ScriptableObject, IVariable<T>
    {
        [SerializeField] private T value;
        [SerializeField] bool alwaysUpdate = false;
        private bool m_hasNew;

        public virtual T Get()
        {
            m_hasNew = false;
            return value;
        }

        public virtual void Set(T value)
        {
            m_hasNew = !this.value.Equals(value);
            this.value = value;
        }

        public virtual void Reset()
        {
            m_hasNew = true;
            this.value = default;
        }

        public virtual bool HasNew()
        {
            bool b = m_hasNew;
            m_hasNew = false;
            return alwaysUpdate || b;
        }
    }

}