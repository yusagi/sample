using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController1 : MonoBehaviour {

	public Transform player;
	
	public Vector3 offSetPos;
	public float offsetVerticalAngle;
	
	private bool isDrag;
	private float offsetHorizontalAngle;

	void Awake()
	{
		//springVelocity = Vector3.zero;
		isDrag = false;
		transform.position = player.position;
		transform.position += -player.forward;
		transform.rotation = player.rotation;
		
		Camera1();
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	void LateUpdate(){
		Camera1();
	}

	// プレイヤーを追従するカメラ 縦のアングルや位置をインスペクターで調整する
	void Camera1(){
		// カメラ：プレイヤーに追従
		Vector3 front = (player.position - transform.position).normalized;
		front = Vector3.ProjectOnPlane(front, player.up);
		Quaternion rotate = Quaternion.LookRotation(front, player.up);
		transform.rotation = rotate;

		// カメラ：横旋回
		Quaternion hTurning = Quaternion.AngleAxis(DragAngle(), transform.up);

		// カメラ：縦旋回
		offsetVerticalAngle = Mathf.Clamp(offsetVerticalAngle, -30.0f, 60.0f);
		Quaternion vTurning = Quaternion.AngleAxis(offsetVerticalAngle, transform.right);
		
		transform.rotation 
			= hTurning 
			* vTurning 
			* transform.rotation;
			
		transform.position =
			player.position + 
			transform.right * offSetPos.x +
			transform.up * offSetPos.y +
			transform.forward * offSetPos.z;
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
	
	public void DragStart(){
		isDrag = true;
	}
	public void DragStop(){
		isDrag = false;
	}
}
