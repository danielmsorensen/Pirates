using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerMotor : MonoBehaviour {

    public Player player { get; private set; }

    public float maxMoveSpeed;
    public float maxTurnSpeed;

    float thrust;
    float torque;

    float speed;

    float accel;
    float turnForce;

    void Awake() {
        player = GetComponent<Player>();
    }

    void Start() {
        accel = maxMoveSpeed * player.rigidbody.drag;
        turnForce = maxTurnSpeed * player.rigidbody.angularDrag / 16.20501f;
    }

    void Update() {
        thrust = Input.GetAxisRaw("Vertical") * accel;
        torque = -Input.GetAxisRaw("Horizontal") * Mathf.Lerp(0, turnForce, speed / maxMoveSpeed);
    }

    void FixedUpdate() {
        speed = player.rigidbody.velocity.magnitude;

        player.rigidbody.AddRelativeForce(Vector2.up * thrust * player.rigidbody.mass);
        player.rigidbody.AddTorque(torque * player.rigidbody.mass * Time.deltaTime);
    }
}
