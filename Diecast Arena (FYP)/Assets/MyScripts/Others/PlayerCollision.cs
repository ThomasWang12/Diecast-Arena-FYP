using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] List<Collider> colliders;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void GetRimColliders()
    {
        List<GameObject> rimColliders = new List<GameObject>();
        Methods.GetChildRecursive(gameObject, rimColliders, "Rim Collider");
        foreach (var rimCollider in rimColliders)
            colliders.Add(rimCollider.GetComponent<SphereCollider>());
    }

    public void Toggle(bool state)
    {
        rb.useGravity = state;
        rb.isKinematic = !state;
        foreach (var collider in colliders)
            collider.enabled = state;
    }
}
