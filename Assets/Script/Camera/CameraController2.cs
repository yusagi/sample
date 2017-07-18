using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController2 : MonoBehaviour {

	public Transform player;
	public Transform target;
	public Transform planet;
	
	// カメラ2用変数
	public Vector3 maxOffSetPos;
	public Vector3 minOffSetPos;
	public float stiffness, friction, mass = 0.1f;
	public bool forceSpeedChange = true;
	private Vector3 springVelocity;
	
	private bool isDrag;
	private float offsetHorizontalAngle;
	

	void Awake()
	{
		//springVelocity = Vector3.zero;
		isDrag = false;
		transform.position = player.position;
		transform.position += -player.forward;
		transform.rotation = player.rotation;
		
		Camera2();
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	void LateUpdate(){
		Camera2();
	}
	
	// 敵との距離に応じてカメラ位置やプレイヤー速度が変わるカメラ
	void Camera2()
	{
		// 球体の円周とプレイヤーと敵の距離の比
		float t;

		// 敵までの距離と球体の円周の比を求める
		{
			// プレイヤーとエネミーの角度から距離を出して、その距離に応じてカメラ位置の値を変化させたい
			Vector3 tVelocity = target.position - planet.position;
			Vector3 pVelocity = player.position - planet.position;
			float angle = Mathf.Acos(Vector3.Dot(tVelocity.normalized, pVelocity.normalized)) * Mathf.Rad2Deg;
			// 円周の半分の長さ
			float cirHalfLen = (2.0f*Mathf.PI*planet.transform.localScale.y*0.5f) * 0.5f;
			float length = (2.0f * Mathf.PI * planet.transform.localScale.y*0.5f) * (angle / 360.0f);
			t = length / cirHalfLen;
		}

		// 移動したプレイヤーの方向を向いていく
		{
			// カメラ：プレイヤーに追従
			Vector3 front = (player.position - transform.position).normalized;
			front = Vector3.ProjectOnPlane(front, player.up);
			Quaternion rotate = Quaternion.LookRotation(front, player.up);
			transform.rotation = rotate;

			// 比の応じた設置位置を求める
			Vector3 offSet = Vector3.Lerp(minOffSetPos, maxOffSetPos, t);

			// 回転後のセットされる位置を計算
			Vector3 position =
				player.position + 
				transform.right * offSet.x +
				transform.up * offSet.y +
				transform.forward * offSet.z;

			// バネ移動でその位置へ向かう
			SpringVelocity(transform.position, position, stiffness, friction, mass);
			transform.position += springVelocity;
		}
		
		// プレイヤー方向を向く
		{
			// 再度プレイヤー方向を計算
			Vector3 front = (player.position - transform.position).normalized;
			float angle = Mathf.Acos(Vector3.Dot(transform.forward, front)) * Mathf.Rad2Deg;
			// カメラ：縦旋回
			Quaternion vTurning = Quaternion.AngleAxis(angle, transform.right);
			transform.rotation = vTurning * transform.rotation;
		}
	}

	float DragAngle(){
		float drag = 0.0f;
		if (isDrag){
			drag = TouchController.GetDragVelocity().x;
			if (drag >= 0){
				drag = Mathf.Sqrt(Mathf.Pow(drag, 2));
			}
			else{
				drag = -Mathf.Sqrt(Mathf.Pow(drag, 2));
			}
		}
		return drag;
	}

	// バネっぽい移動をするベクトルを返す
	void SpringVelocity(Vector3 curPosition, Vector3 restPosition, float stiffness, float friction, float mass)
	{
		Vector3 stretch = curPosition - restPosition;
		Vector3 force = -stiffness * stretch;
		Vector3 acceleration = force / mass;
		springVelocity = friction * (springVelocity + acceleration);
	}
	
	public void DragStart(){
		isDrag = true;
	}
	public void DragStop(){
		isDrag = false;
	}
}
