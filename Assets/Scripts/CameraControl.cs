using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{

    public float scrollFactor, vertFactor, horiFactor;
    public Transform trans;
    public Transform cameraTrans;

    // Use this for initialization
    void Start() {
        cameraTrans.LookAt(trans.position);
    }

    // Update is called once per frame
    void Update() {
        float scroll = Input.GetAxis("Mouse ScrollWheel") * scrollFactor * Time.deltaTime;
        cameraTrans.position += cameraTrans.forward * scroll;

        float h = Input.GetAxis("Horizontal") * horiFactor * Time.deltaTime;
        trans.Rotate(Vector3.up, h, Space.World);

        float v = Input.GetAxis("Vertical") * vertFactor * Time.deltaTime;
        cameraTrans.Rotate(cameraTrans.right, v, Space.World);
    }
}
