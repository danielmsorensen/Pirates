using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerMotor : MonoBehaviour {

    public Player player { get; private set; }

    public float moveForce;
    public float turnForce;

    float thrust;
    float torque;

    void Awake() {
        player = GetComponent<Player>();
    }

    void Update() {
        thrust = Input.GetAxisRaw("Vertical") * moveForce;
        torque = -Input.GetAxisRaw("Horizontal") * turnForce;
    }

    void FixedUpdate() {
        player.rigidbody.AddForce(transform.up * thrust * player.rigidbody.mass);
        player.rigidbody.AddTorque(torque * player.rigidbody.mass);
    }
}
