using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PhysicsObject : Photon.MonoBehaviour {

	public new Rigidbody2D rigidbody { get; private set; }
    public new Rigidbody2D rigidbody2D { get { return rigidbody; } }
    public new Collider2D collider { get; private set; }
    public new Collider2D collider2D { get { return collider; } }

    protected virtual void Awake() {
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
    }
}
