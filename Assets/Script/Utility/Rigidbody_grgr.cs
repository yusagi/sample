using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rigidbody_grgr {

	// 回転からプラネット上の座標を取得
	public static Vector3 RotateToPosition(Vector3 chrUp, Vector3 planetPosition, float planetRad, float up = 0.0f)
	{
		Vector3 rayStart = planetPosition + (chrUp* (planetRad + 0.1f));
		RaycastHit hitInfo;
		Physics.Raycast(rayStart, -chrUp, out hitInfo, Mathf.Infinity, (int)Layer.PLANET);

		Vector3 position = hitInfo.point + (chrUp * up);
		
		return position;
	}

#region メンバ変数

	public float maxVelocitySpeed {get;set;}
	public Vector3 velocity{ get; set;}
	public Vector3 prevVelocity{get; set;}
	public Vector3 prevPosition{get;set;}
	public bool isMove{get;set;}
	public float friction{get;set;}

	private Transform my;

#endregion

	public Rigidbody_grgr(Transform obj){
		my = obj;
		maxVelocitySpeed = Mathf.Infinity;
		velocity = Vector3.zero;
		prevVelocity = velocity;
		prevPosition = my.position;
		isMove = true;
		friction = 0.001f;
	}
	
	// Update is called once per frame
	public void Update () {
		prevVelocity = velocity;
		
		velocity *= (1 - friction);

		if (isMove)
			my.position += velocity;
	}

	public void AddForce(Vector3 force){
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
