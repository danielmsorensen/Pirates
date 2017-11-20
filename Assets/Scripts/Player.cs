using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class Player : PhysicsObject {

    public PlayerMotor motor { get; private set; }

    protected override void Awake() {
        base.Awake();
        motor = GetComponent<PlayerMotor>();
    }

    void Start() {
        if(photonView.isMine) {
            motor.enabled = true;
        }
        else {
            motor.enabled = false;
        }
    }
}
