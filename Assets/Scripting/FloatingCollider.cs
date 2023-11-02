using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingCollider : MonoBehaviour
{
    private Rigidbody _rbody;
    [SerializeField] private Collider _colly;

    [SerializeField] private Transform _boundTransform;
    [SerializeField] private Vector3 _boundTransformOffset;

    // v this explanation is out of date v
    // the larger this is, the greater-angle of slopes we try to "stick" to.
    // when this is changed, Damping/Spring coeffs need to be modified so that
    // Jammo sticks to the slopes as he moves up/down them.
    // also need to increase the raycast length of the Grounded raycast in PlayerController
    // 
    // overshoot is needed so that we stick to slopes instead of just falling off.
    // problem is overshoot just leads to us sticking to the ground
    // again, this wont be an issue when converted to a state machine, as we
    // can just tie the floating collider behavior activate to the grounded
    // state, so that as soon as we just we no longer have to worry about it.
    [SerializeField] private float overShoot = 0.1f; 

    [SerializeField] private float rideHeight;
    [SerializeField] private float dampingCoefficient;
    [SerializeField] private float springCoefficient;

    [SerializeField] private float sphereCastRadius;
    [SerializeField] private LayerMask layerMask;

    // testing
    [SerializeField] private Mesh testMesh;
    [SerializeField] private float testHeight;

    private RaycastHit _info;


    private bool test;

    private void Awake()
    {
        if (!_colly)
        {
            _colly = GetComponent<Collider>();
        }

        _rbody = GetComponent<Rigidbody>();
    }

    public void UpdateForce(bool canAddSpringForce)
    {
        if (canAddSpringForce && Physics.SphereCast(GetRay(), sphereCastRadius, out _info, rideHeight + overShoot - sphereCastRadius, layerMask))
        {
            var v = GetSpringForce();
            _rbody.AddForce(v, ForceMode.Force);
            //Debug.Log("Dist: " + _info.distance + " | Coeff: " + (springCoefficient * (rideHeight - _info.distance - sphereCastRadius)) + " | Damp: " + (dampingCoefficient * _rbody.velocity.y));
            //Debug.Log("Force: " + v);
        }

        // was recently un-removed. hope it works still lol.
        UpdateBoundTransform();
    }

    private Vector3 GetSpringForce()
    {
        // the math on this is a bit funky, but it works?
        var dot = Vector3.Dot(_info.normal, _rbody.velocity.normalized);

        return (springCoefficient * (rideHeight - _info.distance - sphereCastRadius) - (dampingCoefficient * dot * _rbody.velocity.magnitude)) * Vector3.up;
    }

    private void UpdateBoundTransform()
    {
        if (Physics.Raycast(GetRay(), out RaycastHit data, rideHeight + sphereCastRadius, layerMask))
        {
            _boundTransform.position = _colly.bounds.center + _boundTransformOffset * data.distance;
        }
    }

    private Ray GetRay()
    {
        return new Ray(_colly.bounds.center, Vector3.down);
    }


    // testing method, delete
    /*private void FixedUpdate()
    {
        // this seems to have worked better???? wtf is happening???
        // UpdateForce(true); please dont use this tho, this is abhorrid

        if (_rbody.velocity.y > 1)
        {
            test = true;
        }

        if (test && _rbody.velocity.y < 0f && transform.position.y > rideHeight)
        {
            Debug.Log("Height Reached: " + transform.position.y);
            test = false;
        }
    }*/

    private void OnDrawGizmosSelected()
    {
        var center = _colly.bounds.center;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(center, center + Vector3.down * (_info.collider != null ? _info.distance : rideHeight + overShoot - sphereCastRadius));
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(center + (Vector3.down * (_info.collider != null ? _info.distance : rideHeight + overShoot - sphereCastRadius)), sphereCastRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(center, center + Vector3.down * rideHeight);

        if (_info.collider != null)
        {
            Gizmos.color = Color.yellow;
            float force = GetSpringForce().y;
            
            Gizmos.DrawWireMesh(testMesh, 0, center + force * Vector3.up, Quaternion.identity, new Vector3(.5f, force, .5f));
        }

        testHeight = center.y; // .5 is the due to the thickness of the ground plane.
    }
}
