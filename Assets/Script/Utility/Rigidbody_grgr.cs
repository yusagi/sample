using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rigidbody_grgr {

	// 座標からプラネット垂直な回転を求める
	public static Quaternion PositionToRotate(Vector3 center, Vector3 position, Vector3 front){
		Vector3 up = (position - center);
		Vector3 forward = Vector3.ProjectOnPlane(front, up);

		return Quaternion.LookRotation(forward, up);
	}

	// 回転からプラネット上の座標を取得
	public static Vector3 RotateToPosition(Vector3 chrUp, Vector3 planetPosition, float planetRad, float up = 0.0f)
	{
		Vector3 rayStart = planetPosition + (chrUp* (planetRad + 0.1f));
		RaycastHit hitInfo;
		Physics.Raycast(rayStart, -chrUp, out hitInfo, Mathf.Infinity, (int)LayerMask.PLANET);

		Vector3 position = hitInfo.point + (chrUp * up);
		
		return position;
	}

	// 2点間の弧の長さを求める
	public static float Arc(Vector3 center, float planetRad, Vector3 a, Vector3 b){
		Vector3 toA = (a - center).normalized;
		Vector3 toB = (b - center).normalized;

		float angle = Mathf.Acos(Vector3.Dot(toA, toB)) * Mathf.Rad2Deg;

		float arc = (2.0f * Mathf.PI * planetRad) * (angle / 360.0f);
		
		return arc;
	}

	// 弧の長さから角度を求める
	public static float Angle(float arc, float planetRad){
		float angle = (arc * 360.0f) / (2.0f * Mathf.PI * planetRad);

		return angle;
	}

	// 球体の2点間を補間
	public static Vector3 GrgrLerp(Vector3 center, float planetRad, Vector3 a, Vector3 b, float t){
		Vector3 p = Vector3.Lerp(a, b, t);

		Vector3 up = (p - center).normalized;

		return RotateToPosition(up, center, planetRad);
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
