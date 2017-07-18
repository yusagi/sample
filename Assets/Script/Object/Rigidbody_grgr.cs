using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rigidbody_grgr : MonoBehaviour {

	public float maxVelocitySpeed {get;set;}
	public Vector3 velocity{ get; set;}
	public Vector3 prevVelocity{get; set;}
	public Vector3 prevPosition{get;set;}
	public bool isMove = true;
	public float friction = 0.0f;


	void Awake(){
		maxVelocitySpeed = Mathf.Infinity;
		velocity = Vector3.zero;
		prevVelocity = velocity;
		prevPosition = transform.position;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}

	void LateUpdate(){
		prevVelocity = velocity;
		
		velocity *= (1 - friction);

		if (isMove)
			transform.position += velocity;
	}

	public void AddForce(Vector3 force){
		// Vector3 vel = (force.magnitude > UtilityMath.epsilon) ? force : velocity;
		// velocity = vel.normalized * Mathf.Min(velocity.magnitude + force.magnitude, maxVelocitySpeed);
		Vector3 vel = velocity + force;
		float speed = Mathf.Min(vel.magnitude, maxVelocitySpeed);
		velocity = vel.normalized * speed;
	}

	public float GetSpeed(){
		return velocity.magnitude;
	}

	public float GetPrevSpeed(){
		return prevVelocity.magnitude;
	}
}
