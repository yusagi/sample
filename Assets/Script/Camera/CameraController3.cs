using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController3 : MonoBehaviour {

#region enum

    // カメラ状態
	public enum CameraPhase{
		NORMAL,
		PULL,
		BATTLE,
	}

	// バトルモードフェーズ
	public enum Battle{
		START,
		SKILL_BATTLE_START,
		SKILL_BATTLE_END,
	}

#endregion

#region メンバ変数

	public Vector3 offsetPos{get;set;}
	public float offsetVerticalAngle{get; set;}
	public float offsetHorizontalAngle{get;set;}
	public float dragSpeed = 1.0f;
	public float DRAG_PERMISSION = 10.0f;
	public float slowSpeed = 0.3f;
	
	private Quaternion baseRotate;
	private Quaternion horizontalRotate = Quaternion.identity;

	public Phase<CameraPhase> phase = new Phase<CameraPhase>();
	public Phase<Battle> phase_battle = new Phase<Battle>();

	private IEnumerator<float> hRotate;
	private IEnumerator<float> vRotate;
	private IEnumerator<Vector3> osPos;

	public Vector3 NORMAL_OFFSET_POS;
	public float NORMAL_OFFSET_V_ANGLE;

	public float PULL_OFFSET_POS_Z = -28;
	public float PULL_OFFSET_V_ANGLE = 31;

	public float BATTLE_OFFSET_POS_Z = 50;
	public float BATTLE_OFFSET_V_ANGLE;
	public float BATTLE_AREA_ANGLE = 30.0f;
	public float BATTLE_ENCOUNT_TIMER = 1.0f;
	
	public Transform battle_Image;
	public Transform skillboard_Image;

#endregion


#region Unity関数

	void Awake(){
		baseRotate = GameData.GetPlayer().rotation;
		transform.rotation = RotateXYAxis(baseRotate, offsetHorizontalAngle, NORMAL_OFFSET_V_ANGLE);
		transform.position = OffsetPos(transform.rotation, GameData.GetPlayer().position, NORMAL_OFFSET_POS);

		prevUp = GameData.GetPlayer().up;

		phase.Change(CameraPhase.NORMAL);

		hRotate = FLerp(offsetHorizontalAngle, offsetHorizontalAngle);
		while(hRotate.MoveNext());
		vRotate = FLerp(NORMAL_OFFSET_V_ANGLE, NORMAL_OFFSET_V_ANGLE);
		while(vRotate.MoveNext());
		osPos = VLerp(NORMAL_OFFSET_POS, NORMAL_OFFSET_POS);
		while(osPos.MoveNext());
	}

	// Use this for initialization
	void Start () {
		// Vector3 result;
		// result = Vector3.Cross(Vector3.up, Vector3.forward);
		// Debug.Log(result);
	}
	
	// Update is called once per frame
	void Update () {
	}

private Vector3 prevUp;
	void LateUpdate(){
		Quaternion i;
		// Vector3 playerProjePlaneUp =  Vector3.ProjectOnPlane(Vector3.ProjectOnPlane(GameData.GetPlayer().up, Vector3.right), Vector3.forward).normalized;
		// // 表
		// if (Vector3.Dot(playerProjePlaneUp, Vector3.up) == 1){
		// 	i = Quaternion.LookRotation(Vector3.forward, Vector3.up);
		// 	baseRotate = 
		// }
		// // 裏
		// else{
		// 	i = Quaternion.LookRotation(-Vector3.forward, -Vector3.up);
		// 	baseRotate = GetBaseRotate(i, GameData.GetPlayer().up);
		// }
		//baseRotate = GetBaseRotate(baseRotate, GameData.GetPlayer().up);
		Vector3 right = Vector3.Cross(prevUp, GameData.GetPlayer().up).normalized;
		float angle = Mathf.Acos(Vector3.Dot(prevUp, GameData.GetPlayer().up)) * Mathf.Rad2Deg;
		baseRotate = Quaternion.AngleAxis(angle, right) * baseRotate;
		prevUp = GameData.GetPlayer().up;
		// i = baseRotate;
		// Debug.DrawRay(GameData.GetPlanet().position, (i * Vector3.right) * 1000, Color.red);
		// Debug.DrawRay(GameData.GetPlanet().position, (i * Vector3.up) * 1000, Color.green);
		// Debug.DrawRay(GameData.GetPlanet().position, (i * Vector3.forward) * 1000, Color.blue);

		phase.Start();
		switch(phase.current){
			// 通常
			case CameraPhase.NORMAL: {
				if (phase.IsFirst()){
					GameData.GetPlayer().GetComponent<Animator>().speed = 1f;
					GameData.GetEnemy().GetComponent<Animator>().speed = 1f;
					
					Vector3 up = baseRotate * Vector3.up; //(GameData.GetPlayer().rotation * Vector3.up).normalized;
					Vector3 up2 = Vector3.ProjectOnPlane(transform.up, (baseRotate * Vector3.right)).normalized;
					float vAngle = Mathf.Acos(Vector3.Dot(up, up2)) * Mathf.Rad2Deg;
					
					vRotate = FLerp(vAngle, NORMAL_OFFSET_V_ANGLE);
					osPos = VLerp(osPos.Current, NORMAL_OFFSET_POS);
				}
				NormalCamera();
			}
			break;
			// 見下ろしカメラ
			case CameraPhase.PULL:{
				if (phase.IsFirst()){
					float anmSpeed = 1.0f;
					//if (isSlow)
					 anmSpeed = 0.3f;
					GameData.GetPlayer().GetComponent<Animator>().speed = anmSpeed;
					GameData.GetEnemy().GetComponent<Animator>().speed =  anmSpeed;
					vRotate = FLerp(vRotate.Current, PULL_OFFSET_V_ANGLE);
					osPos = VLerp(osPos.Current, new Vector3(NORMAL_OFFSET_POS.x, NORMAL_OFFSET_POS.y, PULL_OFFSET_POS_Z));
				}
				PullCamera();
				// 通常カメラ条件
				if (!IsChangePullCamera()){
					phase.Change(CameraPhase.NORMAL);
				}
			}
			break;
			// バトルモード
			case CameraPhase.BATTLE:{
				if (phase.IsFirst()){
					phase_battle.Change(Battle.START);
				}
				phase_battle.Start();
				{
					switch(phase_battle.current){
						// エンカウント演出
						case Battle.START:{
							if(phase_battle.IsFirst()){
								vRotate = FLerp(vRotate.Current, PULL_OFFSET_V_ANGLE);
								osPos = VLerp(osPos.Current, new Vector3(NORMAL_OFFSET_POS.x, NORMAL_OFFSET_POS.y, PULL_OFFSET_POS_Z));
							}

							if (phase_battle.phaseTime >= BattleManager.BATTLE_START_TIME){
								BattleCamera();
							}
						}
						break;
						case Battle.SKILL_BATTLE_START:{
						}
						break;
						case Battle.SKILL_BATTLE_END:{
							// 縦回転
							offsetVerticalAngle = vRotate.Current;
							
							// 回転
							transform.rotation = RotateXYAxis(baseRotate, offsetHorizontalAngle, offsetVerticalAngle);
							
							// offset
							offsetPos = osPos.Current;

							// 座標
							transform.position = OffsetPos(transform.rotation, GameData.GetPlayer().position, offsetPos);
						}
						break;
					}
				}
				phase_battle.Update();
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

	// プレイヤーの動きに並行して付いていくカメラ
	void NormalCamera(){
		// 横回転角度
		offsetHorizontalAngle += DragAngle();
		// 角度の正規化
		{
			int i = (int)offsetHorizontalAngle;
			float f = offsetHorizontalAngle - i;
			offsetHorizontalAngle = (i % 360) + f;
			if (offsetHorizontalAngle < 0)
				offsetHorizontalAngle += 360;
		}

		// 縦回転
		offsetVerticalAngle = (vRotate.MoveNext()) ? vRotate.Current : NORMAL_OFFSET_V_ANGLE;

		// 回転
		transform.rotation = RotateXYAxis(baseRotate, offsetHorizontalAngle, offsetVerticalAngle);

		// offset
		offsetPos = (osPos.MoveNext()) ? osPos.Current : NORMAL_OFFSET_POS;
		
		// 座標設定
		transform.position = OffsetPos(transform.rotation, GameData.GetPlayer().position, offsetPos);
	}
#endregion

#region 見下ろしカメラ
	void PullCamera(){
		// 縦回転
		offsetHorizontalAngle = (vRotate.MoveNext()) ? vRotate.Current : PULL_OFFSET_V_ANGLE;
		
		// 回転
		transform.rotation = RotateXYAxis(baseRotate, offsetHorizontalAngle, offsetHorizontalAngle);
		
		// offset
		offsetPos = (osPos.MoveNext()) ? osPos.Current : new Vector3(NORMAL_OFFSET_POS.x, NORMAL_OFFSET_POS.y, PULL_OFFSET_POS_Z);

		// 座標
		transform.position = OffsetPos(transform.rotation, GameData.GetPlayer().position, offsetPos);
	}
#endregion

#region バトルカメラ

	void BattleCamera(){
		bool tmp_vRot = vRotate.MoveNext();
		bool tmp_osPos = osPos.MoveNext();

		// 縦回転
		offsetVerticalAngle = (tmp_vRot) ? vRotate.Current : PULL_OFFSET_V_ANGLE;
		
		// 回転
		transform.rotation = RotateXYAxis(baseRotate, offsetHorizontalAngle, offsetVerticalAngle);
		
		// offset
		offsetPos = (tmp_osPos) ? osPos.Current : new Vector3(NORMAL_OFFSET_POS.x, NORMAL_OFFSET_POS.y, PULL_OFFSET_POS_Z);

		// 座標
		transform.position = OffsetPos(transform.rotation, GameData.GetPlayer().position, offsetPos);

		if (!tmp_vRot && !tmp_osPos){
			phase_battle.Change(Battle.SKILL_BATTLE_START);
		}
	}
#endregion

#region コルーチン
	// 横回転補間
	IEnumerator<float> HorizontalRotate(float start, float end){
		float s = start; //offsetHorizontalAngle;
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
			float result = Mathf.Lerp(s, e, t);
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
		return RotateXYAxis(baseRotate, offsetHorizontalAngle, 0.0f);
	}
	
	// プレイヤーと敵の距離が一定値より近いか
	public bool IsChangePullCamera(){
		// if (!isPull)
		// 	return false;

		bool result = true;

		Transform target = GameData.GetEnemy();

		if (target.GetComponent<EnemyController>().state.current == EnemyController.State.ASCENSION)
			result = false;

		Quaternion tmpRotate = transform.rotation;
		Vector3 tmpPos = transform.position;

		transform.rotation = RotateXYAxis(baseRotate, offsetHorizontalAngle, PULL_OFFSET_V_ANGLE);
		transform.position = OffsetPos(transform.rotation, GameData.GetPlayer().position, new Vector3(offsetPos.x, offsetPos.y, PULL_OFFSET_POS_Z));

		Vector3 t = 
				target.position + target.up * 
				(target.GetComponent<CapsuleCollider>().height * 0.5f * target.transform.localScale.y);
		Vector3 sPos = Camera.main.WorldToScreenPoint(t);
		if (sPos.x < 0 || sPos.x > Screen.width)
			result = false;

		float h = 100;
		if (sPos.y < 0 + h || sPos.y > Screen.height - h)
			result = false;

		Ray ray = Camera.main.ScreenPointToRay(sPos);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, (int)(~(Layer.PLAYER | Layer.PILLER)))){
			if (hit.collider.tag == "Planet"){
				result = false;
			}
		}

		transform.rotation = tmpRotate;
		transform.position = tmpPos;

		return result;
	}

	bool IsChangeBattleCamera(){
		float angle = Mathf.Acos(Vector3.Dot(GameData.GetPlayer().up, GameData.GetEnemy().up)) * Mathf.Rad2Deg;
		if (angle <= BATTLE_AREA_ANGLE){
			return true;
		}
		return false;
	}

	// rotateの体勢で上方向がupと並行になる回転を取得
	public Quaternion GetBaseRotate(Quaternion rotate, Vector3 up){
			// Vector3 toPlayer = GameData.GetPlayer().up;// (player.position - planet.position).normalized;
			// Vector3 toCamera = rotate * Vector3.up;// (transform.position - planet.position).normalized;
			// float dot = Vector3.Dot(toPlayer, toCamera);
			// dot = Mathf.Clamp(dot, -1, 1);
			// float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
			// Vector3 axis = Vector3.Cross(toCamera, toPlayer).normalized;
			// return Quaternion.AngleAxis(angle, axis) * rotate;

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

#endregion
}
