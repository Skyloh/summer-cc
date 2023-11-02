using RobotPlatformer.Variables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyCoupler : MonoBehaviour
{
    private Rigidbody m_rigidbody;

    // could be a UnityEvent, but I don't care for the overhead right about now.
    [SerializeField] private FloatVariable velocityDestination;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }
}
