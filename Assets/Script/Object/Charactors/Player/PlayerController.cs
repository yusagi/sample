using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

#region enum 

	public enum State{
		STOP,
		FLICK_MOVE,
		DRAG_MOVE,
		BATTLE,
		SKILL_BATTLE_END,
	}

	enum Gear{
		First,
		Second,
		Third
	}
	
#endregion

#region メンバ変数
	// 摩擦
	public float minFriction = 0.001f; 	// 最小摩擦力
	public float maxFriction = 0.01f; 	// 最大摩擦力
	// 移動
	public float DRAG_PERMISSION = 10.0f;	// ドラッグ条件値
	public float dragSpeed = 10.0f;			// ドラッグ移動速度
	public float gearSecondBase = 30.0f;	// ギア2突入条件値
	public float gearThirdBase = 40.0f;		// ギア3突入条件値
	public float gearFirstAccele = 20.0f;	// ギア1加速度	
	public float gearSecondAccele = 2.0f;	// ギア2加速度
	public float gearThirdAccele = 0.8f;	// ギア3加速度
	public float maxSpeed = 50.0f;			// 最高速度
	public Vector3 currentFlickVelocity{get;set;}	// 最新フレームのフリック移動量
	// ビタ止め
	public float MOVE_FRICK_STOP_ANGLE = 0;	// フリックビタ止め条件値
	public float MOVE_TOUCH_STOP_TIME = 1;	// タッチビタ止め条件値
	// コンポーネント
	private Animator animator;			// アニメーター
	// 付属クラス
	private Rigidbody_grgr rigidbody;
	// 状態
	public Phase<State> state = new Phase<State>();
	private Gear gear;	// ギア
	// UI
	public Text curS;	// 速度
	// 地上からの高さ調整
	public float HEIGHT_FROM_GROUND = -0.6f;
	
#endregion

#region Unity関数
	void Awake()
	{
		rigidbody = new Rigidbody_grgr(transform);
		rigidbody.isMove = false;

		transform.position = Rigidbody_grgr.RotateToPosition(transform.up, GameData.GetPlanet().position, GameData.GetPlanet().localScale.y * 0.5f, HEIGHT_FROM_GROUND);

		animator = GetComponent<Animator>();

		gear = Gear.First;
		state.Change(State.STOP);
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		rigidbody.prevPosition = transform.position;
		
		state.Start();
		switch(state.current){
			case State.STOP: {
				currentFlickVelocity = Vector3.zero;
				
				// ドラッグ移動条件
				if (TouchController.GetTouchType() == TouchController.TouchType.Drag){
					state.Change(State.DRAG_MOVE);
				}
				// フリック移動条件
				else if (TouchController.GetTouchType() == TouchController.TouchType.Frick){
					FLICK_MOVE();
					state.Change(State.FLICK_MOVE);
				}
			}
			break;
			case State.DRAG_MOVE:{
				DRAG_MOVE();
			}
			break;
			case State.FLICK_MOVE: {
				FLICK_MOVE();
			}
			break;
			case State.BATTLE:{
				if (state.IsFirst())
					animator.speed = 0.0f;
			}
			break;
			case State.SKILL_BATTLE_END:{
				if (state.IsFirst()){
					rigidbody.friction = minFriction;
					animator.speed = 1.0f;
				}

				Vector3 toEnemy = GameData.GetEnemy().position - GameData.GetPlayer().position;
				Vector3 front = Vector3.ProjectOnPlane(toEnemy, transform.up).normalized;
				transform.rotation = Quaternion.LookRotation(front, transform.up);

				rigidbody.velocity = transform.forward * rigidbody.GetSpeed();
				rigidbody.AddForce(transform.forward);
				float length = rigidbody.GetSpeed() * Time.deltaTime * animator.speed;
				float angle = length / (2.0f*Mathf.PI*GameData.GetPlanet().transform.localScale.y*0.5f) * 360.0f;
				transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;
				transform.position = Rigidbody_grgr.RotateToPosition(transform.up, GameData.GetPlanet().position, GameData.GetPlanet().localScale.y * 0.5f, HEIGHT_FROM_GROUND);
			}
			break;
		}
		state.Update();

		rigidbody.Update();
		
		curS.text = (int)rigidbody.GetSpeed() + "km";
	}

	

#endregion

#region 移動

	// フリック移動処理
	void FLICK_MOVE(){
		currentFlickVelocity = FlickVelocity();
		
		// 最新のフリック移動量
		float fmVal = currentFlickVelocity.magnitude;

		// 現在のリジッドボディの移動量
		float rmVal = rigidbody.GetSpeed();
		
		// 現在フレームの移動量
		float s = (fmVal + rmVal) * Time.deltaTime;
		// 動いてなかったら または ビタ止め時
		if (s < UtilityMath.epsilon || IsMoveStop()){
			rigidbody.velocity = Vector3.zero;
			rigidbody.friction = maxFriction;
			gear = Gear.First;
			state.Change(State.STOP);
			animator.SetBool("Run", false);
		}
		// フリック移動
		else
		{
			// フリックを移動量に加算
			{
				// 最大速度を変更
				rigidbody.maxVelocitySpeed = maxSpeed;
				// リジッドボディのベクトルを変更
				Vector3 front;
				// フリック方向
				if (fmVal > UtilityMath.epsilon){
					front = currentFlickVelocity.normalized;
				}
				// リジッドボディのベクトルをプレイヤーの立つ平面に投射したベクトル
				else{
					front = Vector3.ProjectOnPlane(rigidbody.velocity, transform.up).normalized;
				}
				rigidbody.velocity = front * rmVal;
				rigidbody.AddForce(currentFlickVelocity);

				// 力を加えた新たなリギッドボディの移動量
				rmVal = rigidbody.GetSpeed();
			}

			// ギアの設定
			{
				// ギアの段階による速度制限
				switch(gear){
					case Gear.First: rigidbody.velocity = rigidbody.velocity.normalized * Mathf.Min(rmVal, gearSecondBase + 1.0f); break;
					case Gear.Second: rigidbody.velocity = rigidbody.velocity.normalized * Mathf.Min(rmVal, gearThirdBase + 1.0f); break;
				}

				// ギア切り替え
				{
					gear = Gear.First;
					if (rmVal >= gearSecondBase)
						gear = Gear.Second;
					if (rmVal >= gearThirdBase)
						gear = Gear.Third;
				}
			}
			
			// 摩擦軽減処理
			{
				// 最高速度との差の比
				float t = Mathf.Clamp(rmVal / maxSpeed, 0.0f, 1.0f);
				
				// 摩擦の計算
				double friction = UtilityMath.OutExp(t, 1.0f, minFriction, maxFriction);
				
				rigidbody.friction = (float)friction * animator.speed;
			}

			// 移動量を角度として考え移動する
			{
				float length = rmVal * Time.deltaTime * animator.speed;
				if (length == 0){
					return;
				}
				float angle = length / (2.0f*Mathf.PI*GameData.GetPlanet().transform.localScale.y*0.5f) * 360.0f;
				transform.rotation = Quaternion.LookRotation(rigidbody.velocity.normalized, transform.up);
				transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;
				transform.position = Rigidbody_grgr.RotateToPosition(transform.up, GameData.GetPlanet().position, GameData.GetPlanet().localScale.y * 0.5f, HEIGHT_FROM_GROUND);
			}

			// ビタ止め移行モーション変化
			{
				if (TouchController.IsPlanetTouch())
				{
					if (TouchController.GetTouchTimer() > MOVE_TOUCH_STOP_TIME * 0.5f)
						animator.SetBool("Run", false);
					else
						animator.SetBool("Run", true);
				}
			}
		}
	}

	// ドラッグ移動処理
	void DRAG_MOVE(){
		Vector3 moveVelocity = DragVelocity();

		// 現在のドラッグ移動量
		float mVal = moveVelocity.magnitude;

		if (mVal < UtilityMath.epsilon){
			rigidbody.velocity = Vector3.zero;
			rigidbody.friction = maxFriction;
			gear = Gear.First;
			animator.SetBool("Run", false);
			state.Change(State.STOP);
			return;
		}

		Transform planet = GameData.GetPlanet();
		
		float length = mVal * Time.deltaTime;
		float angle = length / (2.0f*Mathf.PI*planet.transform.localScale.y*0.5f) * 360.0f;
		transform.rotation = Quaternion.LookRotation(moveVelocity.normalized, transform.up);
		transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;
		transform.position = Rigidbody_grgr.RotateToPosition(transform.up, GameData.GetPlanet().position, GameData.GetPlanet().localScale.y * 0.5f, HEIGHT_FROM_GROUND);
	}
	
	// フリック移動量
	Vector3 FlickVelocity(){
		if (!TouchController.IsPlanetTouch()){
			return Vector3.zero;
		}

		float speed = 0.0f;
		// 移動量変化
		{
			switch(gear){
				case Gear.First: speed = gearFirstAccele; break;
				case Gear.Second: speed = gearSecondAccele; break;
				case Gear.Third: speed = gearThirdAccele; break;
			}
		}

		Vector3 velocity = Vector3.zero;
		// 移動ベクトル作成
		{
			
			velocity = -TouchController.GetFlickVelocity();
			velocity = GameData.GetCamera().GetComponent<CameraController3>().GetHorizontalRotate() * velocity;
			
			float tmp = Mathf.Clamp(velocity.magnitude, 0.0f, 1080);
			tmp = Mathf.Clamp(tmp / 1080, 0.1f, 1.0f);

			velocity = Vector3.ProjectOnPlane(velocity, transform.up).normalized * tmp * speed;
		}

		return velocity;
	}

	// ドラッグ移動量
	Vector3 DragVelocity(){
		if (!TouchController.IsPlanetTouch())
			return Vector3.zero;

		Vector3 velocity = Vector3.zero;
		
		// ドラッグ移動
		if (!TouchController.IsFlickSuccess())
		{
			velocity = -TouchController.GetDragVelocity();
			if (velocity.magnitude < DRAG_PERMISSION)
				return Vector3.zero;

			velocity = GameData.GetCamera().rotation * velocity.normalized;
			velocity = Vector3.ProjectOnPlane(velocity, transform.up).normalized;
		}

		return velocity * dragSpeed;
	}
#endregion

#region 衝突処理
	void OnCollisionEnter(Collision other)
	{
		// 柱との衝突処理
		if (other.gameObject.tag == "Piller")
		{
			if (other.collider.GetComponent<PillerDeadTime>().isDead)
				return;
			
			GameData.killPillers++;

			other.gameObject.GetComponent<Charactor>().planetWalk.isActive = false;
			other.collider.GetComponent<PillerDeadTime>().isDead = true;
			Vector3 impact = (other.collider.transform.up + rigidbody.velocity);
			other.collider.GetComponent<Charactor>().rigidbody.AddForce(impact*0.1f);
			return;
		}

		// 敵との衝突処理
		if (other.gameObject.tag == "Enemy")
		{
			if (other.gameObject.GetComponent<EnemyController>().state.current == EnemyController.State.ASCENSION)
				return;

			Vector3 impact = (other.transform.up + rigidbody.velocity.normalized) * rigidbody.GetSpeed() * 0.03f;
			other.collider.GetComponent<EnemyController>().rigidbody.velocity = Vector3.zero;
			other.collider.GetComponent<EnemyController>().rigidbody.AddForce(impact);

			Vector3 front = Vector3.ProjectOnPlane(-impact.normalized, other.transform.up).normalized;
			other.transform.rotation = Quaternion.LookRotation(front, other.transform.up);

			other.collider.GetComponent<EnemyController>().rigidbody.isMove = true;
			other.collider.GetComponent<Animator>().SetBool("Run", false);
			other.collider.GetComponent<Animator>().SetTrigger("DamageDown");

			GameData.GetBattleManager().GetComponent<BattleManager>().battle.Change(BattleManager.Battle.BATTLE_END);
		}
	}
#endregion

#region ビタ止め
	bool IsMoveStop(){
		return (rigidbody.GetSpeed() > UtilityMath.epsilon) && (IsMoveTouchStop() || IsMoveFrickStop());
	}

	bool IsMoveFrickStop(){
		if (!TouchController.IsPlanetTouch()){
			return false;
		}
		
		Vector3 frickVel = -TouchController.GetFlickVelocity();
		Vector3 backVel = new Vector3(0, 0, -1);
		float dot = Vector3.Dot(backVel.normalized, frickVel.normalized);
		float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
		return angle < MOVE_FRICK_STOP_ANGLE;
	}

	bool IsMoveTouchStop(){
		if (!TouchController.IsPlanetTouch()){
			return false;
		}
		return (TouchController.GetTouchTimer() > MOVE_TOUCH_STOP_TIME);
	}
#endregion
}
