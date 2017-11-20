using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceMouse : MonoBehaviour {

    public float smoothing;
    public float maxSpeed;

    float targetRotation;
    float vel;

    void Update() {
        targetRotation = Vector2.SignedAngle(Vector2.up, CameraManager.active.camera.ScreenToWorldPoint(Input.mousePosition) - transform.position);
    }

    void LateUpdate() {
        Vector3 rot = transform.eulerAngles;
        rot.z = Mathf.SmoothDampAngle(rot.z, targetRotation, ref vel, smoothing, (maxSpeed == 0) ? float.MaxValue : maxSpeed, Time.deltaTime);
        transform.eulerAngles = rot;
    }
}
