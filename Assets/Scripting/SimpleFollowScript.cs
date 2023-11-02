using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFollowScript : MonoBehaviour
{
    [SerializeField] Transform tracked;
    [SerializeField] Vector3 offset;

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 dest = tracked.position + offset;
        transform.position = Vector3.Lerp(dest, transform.position, 0.1f);
    }
}
