using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleCamaera : MonoBehaviour {

    public float wheelPow = 0.5f;
    public float rotYowPow = 1.0f;
    public float rotPitchPow = 1.0f;
    public float speed = 1.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float scroll = 0;
        if (Input.GetKey(KeyCode.Z))
            scroll = wheelPow;
        if (Input.GetKey(KeyCode.X))
            scroll = -wheelPow;
        transform.position += transform.forward * scroll;

        float rotY = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
            rotY = -rotYowPow;
        if (Input.GetKey(KeyCode.RightArrow))
            rotY = rotYowPow;
        transform.rotation = Quaternion.AngleAxis(rotY, Vector3.up) * transform.rotation;

        float rotX = 0;
        if (Input.GetKey(KeyCode.UpArrow))
            rotX = -rotPitchPow;
        if (Input.GetKey(KeyCode.DownArrow))
            rotX = rotPitchPow;
        transform.rotation = Quaternion.AngleAxis(rotX, transform.right) * transform.rotation;

        Vector3 velocity = Vector3.zero;
        if (Input.GetKey(KeyCode.A))
            velocity = -transform.right;
        if (Input.GetKey(KeyCode.D))
            velocity = transform.right;
        if (Input.GetKey(KeyCode.W))
            velocity = transform.up;
        if (Input.GetKey(KeyCode.S))
            velocity = -transform.up;
        velocity *= speed;

        transform.position += velocity;
    }
}
