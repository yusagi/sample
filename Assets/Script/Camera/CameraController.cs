using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

#region enum

    // カメラ状態
	public enum CameraPhase{
		NORMAL,
		BATTLE,
	}

	// プレイヤー進行方向
	enum DirType{
		FRONT,
		BACK,
	}

#endregion

#region メンバ変数

	public Vector3 offsetPos{get;set;}
	public float offsetVAngle{get; set;}
	public float offsetHAngle{get;set;}

	public float dragSpeed = 8.0f;
	public float DRAG_PERMISSION = 10.0f;
	
	private Quaternion baseRotate;

	public Phase<CameraPhase> phase = new Phase<CameraPhase>();

	private IEnumerator<float> hRotate;
	private IEnumerator<float> vRotate;
	private IEnumerator<Vector3> osPos;

	// 通常カメラ
	public Vector3 NORMAL_OFFSET_POS = new Vector3(0, 3.4f, -7);
	public float NORMAL_OFFSET_V_ANGLE = 16.5f;
	public Vector3 NORMAL_OFFSET_BACK_POS = new Vector3(0, 3.4f, -18);
	public float NORMAL_OFFSET_BACK_V_ANGLE = 16.5f;

	// バトルカメラ
	public Vector3 BATTLE_OFFSET_POS;
	public float BATTLE_OFFSET_V_ANGLE;

	public Vector3 BATTLE_START_POS;
	public float BATTLE_START_V_ANGLE;
	public Vector3 BATTLE_START_BACK_POS;
	public float BATTLE_START_BACK_V_ANGLE;

	public bool battleCameraControlleComp{get;set;}

	private DirType dirType;

	public bool DBG_IS_NORMAL_FRONTONLY = false;

#endregion


#region Unity関数

	void Awake(){
		baseRotate = GameData.GetPlayer().rotation;
		transform.rotation = RotateXYAxis(baseRotate, offsetHAngle, NORMAL_OFFSET_V_ANGLE);
		transform.position = OffsetPos(transform.rotation, GameData.GetPlayer().position, NORMAL_OFFSET_POS);

		phase.Change(CameraPhase.NORMAL);
		
		offsetHAngle = 0.0f;
		offsetVAngle = NORMAL_OFFSET_V_ANGLE;
		offsetPos = NORMAL_OFFSET_POS;
		dirType = DirType.FRONT;

		hRotate = FLerp(offsetHAngle, offsetHAngle);
		while(hRotate.MoveNext());
		vRotate = FLerp(NORMAL_OFFSET_V_ANGLE, NORMAL_OFFSET_V_ANGLE);
		while(vRotate.MoveNext());
		osPos = VLerp(NORMAL_OFFSET_POS, NORMAL_OFFSET_POS);
		while(osPos.MoveNext());
	}
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void LateUpdate(){
		baseRotate = GetBaseRotate(baseRotate, GameData.GetPlayer().up);

		phase.Start();
		switch(phase.current){
			// 通常
			case CameraPhase.NORMAL: {
				DirType current = CheckDirType();
	
	if (DBG_IS_NORMAL_FRONTONLY){
		current = DirType.FRONT;
	}
				float vAngle = 0.0f;
				Vector3 offset = Vector3.zero;
				// プレイヤー進行方向
				switch(current){
					case DirType.FRONT: offset = NORMAL_OFFSET_POS; vAngle = NORMAL_OFFSET_V_ANGLE; break;
					case DirType.BACK: offset = NORMAL_OFFSET_BACK_POS; vAngle = NORMAL_OFFSET_BACK_V_ANGLE; break;
					default: Debug.LogError("out of ragne DirType"); break;
				}
				if (phase.IsFirst()){
					GameData.GetPlayer().GetComponent<Animator>().speed = 1f;
					GameData.GetEnemy().GetComponent<Animator>().speed = 1f;
					
					vRotate = FLerp(offsetVAngle, vAngle);
					osPos = VLerp(offsetPos, offset);
				}
				else if (current != dirType){
					vRotate = FLerp(offsetVAngle, vAngle);
					osPos = VLerp(offsetPos, offset);
					dirType = current;
				}
				NormalCamera(offset, vAngle);
			}
			break;
			// バトルモード
			case CameraPhase.BATTLE:{
				if (phase.IsFirst()){
					float vAngle = 0.0f;
					Vector3 offset = Vector3.zero;
					BATTLE_OFFSET_V_ANGLE = offsetVAngle;
					BATTLE_OFFSET_POS = offsetPos;
					vRotate = FLerp(offsetVAngle, BATTLE_OFFSET_V_ANGLE);
					osPos = VLerp(osPos.Current, BATTLE_OFFSET_POS);
				}
				// バトルモード状態
				switch(BattleManager._instance.battle.current){
					// バトル終了
					case BattleManager.Battle.BATTLE_END:{
						if(BattleManager._instance.battle.IsFirst()){
							phase.Change(CameraPhase.NORMAL);
						}
					}
					break;
					default:{
						bool tmpVRotate = vRotate.MoveNext();
						bool tmposPos = osPos.MoveNext();

						if (!BattleManager._instance.DBG_IS_CAMERA_STOP || (tmpVRotate && tmposPos)){
							offsetVAngle = vRotate.MoveNext() ? vRotate.Current : BATTLE_OFFSET_V_ANGLE;
							offsetPos = osPos.MoveNext() ? osPos.Current : BATTLE_OFFSET_POS;
							SetPose();
						}
					}	
					break;
				}
			}
			break;
		}
		phase.Update();
	}

#endregion

#region 通常カメラ

	// ドラッグの回転量取得
	float DragAngle(){
		float drag = 0.0f;

		if (TouchController.IsPlanetTouch())
			return drag;

		drag = TouchController.GetDragVelocity().x;
		if (drag >= 0){
			drag = Mathf.Sqrt(Mathf.Pow(drag, 2));
		}
		else{
			drag = -Mathf.Sqrt(Mathf.Pow(drag, 2));
		}

		if (Mathf.Pow(drag, 2) < Mathf.Pow(DRAG_PERMISSION, 2))
			return 0.0f;
		return drag * Time.deltaTime * dragSpeed;
	}

	// プレイヤーの進行方向確認
	DirType CheckDirType(){
		Vector3 vel = GameData.GetPlayer().GetComponent<PlayerController>().rigidbody.velocity;
		if (vel.magnitude < UtilityMath.epsilon){
			return DirType.FRONT;
		}

		float dot = Vector3.Dot(Vector3.ProjectOnPlane(vel, transform.up).normalized, transform.forward);

		if (dot > 0){
			return DirType.FRONT;
		}
		else
			return DirType.BACK;
	}

	// プレイヤーの動きに並行して付いていくカメラ
	void NormalCamera(Vector3 offset, float vAngle){
		// 横回転角度
		offsetHAngle += DragAngle();
		// 角度の正規化
		{
			int i = (int)offsetHAngle;
			float f = offsetHAngle - i;
			offsetHAngle = (i % 360) + f;
			if (offsetHAngle < 0)
				offsetHAngle += 360;
		}

		// 縦回転
		offsetVAngle = (vRotate.MoveNext()) ? vRotate.Current : vAngle;

		// offset
		offsetPos = (osPos.MoveNext()) ? osPos.Current : offset;

		// カメラポーズ設定
		SetPose();
	}
#endregion

#region コルーチン
	// 横回転補間
	IEnumerator<float> HorizontalRotate(float start, float end){
		float s = start; //offsetHAngle;
		float e = end;// Mathf.Acos(Vector3.Dot(transform.forward, player.forward)) * Mathf.Rad2Deg;
		float t = 0.0f;
		Vector3 cross = Vector3.Cross(transform.forward, GameData.GetPlayer().forward).normalized + transform.up;
		if (cross.magnitude <= UtilityMath.epsilon){
			e = -e + 360;
		}
		//float 
		while(t < 1.0f){
			t += Time.deltaTime;
			t = Mathf.Min(t, 1.0f);
			float angle = Mathf.LerpAngle(s, e, t);
			yield return angle;
		}
	}

	// float補間
	IEnumerator<float> FLerp(float start, float end, float time = 1.0f){
		float s = start;
		float e = end;
		float t = 0.0f;
		while(t < 1.0f){
			t = Time.deltaTime / time + t;
			float result = Mathf.LerpAngle(s, e, t);
			yield return (t >= 1.0f) ? end : result;
		}
	}

	// Quaternion補間
	IEnumerator<Quaternion> QLerp(Quaternion start, Quaternion end, float time = 1.0f){
		Quaternion s = start;
		Quaternion e = end;
		float t = 0.0f;
		while(t < 1.0f){
			t = Time.deltaTime / time + t;
			Quaternion result = Quaternion.Slerp(s, e, t);
			yield return (t >= 1.0f) ? end : result;
		}
	}

	// Vector3補間
	IEnumerator<Vector3> VLerp(Vector3 start, Vector3 end, float time = 1.0f){
		Vector3 s = start;
		Vector3 e = end;
		float t = 0.0f;
		while(t < 1.0f){
			t = Time.deltaTime / time + t;
			Vector3 result = Vector3.Slerp(s, e, t);
			yield return (t >= 1.0f) ? end : result;
		}
	}

#endregion

#region 共通関数

	// プレイヤーと並行でかつヨウ回転だけしたカメラの回転
	public Quaternion GetHorizontalRotate(){
		return RotateXYAxis(baseRotate, offsetHAngle, 0.0f);
	}

	// 現在のカメラの枠内か？
	public bool IsInCameraFramework(){
		bool result = true;

		Vector3 tmpPos = transform.position;
		Quaternion tmpRot = transform.rotation;

		Vector3 position = Vector3.zero;
		float vAngle = 0.0f;
		switch(dirType){
			case DirType.FRONT:{
				position = BATTLE_START_POS;
				vAngle = BATTLE_START_V_ANGLE;
			}
			break;
			case DirType.BACK:{
				position = BATTLE_START_BACK_POS;
				vAngle = BATTLE_START_BACK_V_ANGLE; 
			}
			break;
		}
		// position = offsetPos;
		// vAngle = offsetVAngle;

		// 回転
		transform.rotation = RotateXYAxis(baseRotate, offsetHAngle, vAngle);
		// 座標設定
		transform.position = OffsetPos(transform.rotation, GameData.GetPlayer().position, position);

		// ターゲット座標
		Vector3 t = GameData.GetEnemy().position + GameData.GetEnemy().up * (GameData.GetEnemy().GetComponent<CapsuleCollider>().height);
		Vector3 sPos = Camera.main.WorldToScreenPoint(t);
		if (sPos.x < 0 || sPos.x > Screen.width)
			result = false;

		float h = 100;
		if (sPos.y - h < 0 || sPos.y > Screen.height - h)
			result = false;

		Ray ray = Camera.main.ScreenPointToRay(sPos);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, (int)(~(Layer.PLAYER | Layer.PILLER)))){
			if (hit.collider.tag == "Planet"){
				result = false;
			}
		}
		else{
			result = false;
		}

		transform.position = tmpPos;
		transform.rotation = tmpRot;

		return result;
	}

	// rotateの体勢で上方向がupと並行になる回転を取得
	public Quaternion GetBaseRotate(Quaternion rotate, Vector3 up){
			Vector3 front = (rotate * Vector3.forward).normalized;
			front = Vector3.ProjectOnPlane(front, up).normalized;
			return Quaternion.LookRotation(front, up);
	}

	// rotateをrightとupで回転した値を返す
	Quaternion RotateXYAxis(Quaternion rotate, float hAngle, float vAngle){
		// 横回転
		Quaternion hTurning = Quaternion.AngleAxis(hAngle, (rotate * Vector3.up));

		// 縦回転
		Quaternion vTurning = Quaternion.AngleAxis(vAngle, (rotate * Vector3.right));
		
		return hTurning * vTurning * rotate;
	}

	// rotateの体勢で基準点からoffset分調整した値を返す
	Vector3 OffsetPos(Quaternion rotate, Vector3 origin, Vector3 offset){
		Vector3 position = 
			origin +
			(rotate * Vector3.right).normalized * offset.x +
			(rotate * Vector3.up).normalized * offset.y +
			(rotate * Vector3.forward).normalized * offset.z;

		return position;
	}

	// カメラ回転、座標設定
	void SetPose(){
		// 回転
		transform.rotation = RotateXYAxis(baseRotate, offsetHAngle, offsetVAngle);
		
		// 座標設定
		transform.position = OffsetPos(transform.rotation, GameData.GetPlayer().position, offsetPos);
	}

#endregion
}
