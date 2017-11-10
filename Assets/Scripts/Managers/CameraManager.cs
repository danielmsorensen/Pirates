using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour {

    public new Camera camera { get; private set; }

    public Transform target;
    public float transitionTime;
    public float trackSmooth;

    Transform lastTarget;
    Vector3 lastTargetPosition;
    Vector3 vel;

    float transitionProgress;

    float z;

    void Awake() {
        camera = GetComponent<Camera>();
        z = transform.position.z;
    }

    void Start() {
        lastTarget = target;
    }

    void LateUpdate() {
        if (target != null) {
            Vector3 position;
            if (target != lastTarget) {
                transitionProgress = 0;
                lastTargetPosition = lastTarget.position;
            }
            if(transitionProgress < 1) {
                position = Vector3.Lerp(lastTargetPosition, target.position, transitionProgress);
                transitionProgress += Time.deltaTime / transitionTime;
            }
            else {
                position = Vector3.SmoothDamp(transform.position, target.position, ref vel, trackSmooth);
            }
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
        lastTarget = target;
        
    }
}
