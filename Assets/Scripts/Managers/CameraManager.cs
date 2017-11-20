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

    public static CameraManager active { get; private set; }

    void Awake() {
        camera = GetComponent<Camera>();
        active = this;
    }

    void LateUpdate() {
        if (target != null) {
            Vector3 position;
            if (target != lastTarget) {
                if (lastTarget != null) {
                    transitionProgress = 0;
                    lastTargetPosition = lastTarget.position;
                }
                else {
                    lastTargetPosition = transform.position;
                }
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
