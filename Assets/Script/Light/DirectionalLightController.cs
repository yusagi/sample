using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalLightController : MonoBehaviour {
	public Transform camera;
	public float hAngle = 0.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void LateUpdate(){
		transform.rotation = Quaternion.AngleAxis(90.0f, camera.right) * camera.rotation;

		transform.rotation = Quaternion.AngleAxis(hAngle, transform.right) * transform.rotation;
	}
}
