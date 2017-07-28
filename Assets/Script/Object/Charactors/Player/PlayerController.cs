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
		ASCENSION,
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
	public float resultSpeed = 10.0f;
	public Vector3 currentVelocity{get;set;}	// 最新フレームのフリック移動量
	// ビタ止め
	public float MOVE_FRICK_STOP_ANGLE = 0;	// フリックビタ止め条件値
	public float MOVE_TOUCH_STOP_TIME = 1;	// タッチビタ止め条件値
	// コンポーネント
	private Animator animator;			// アニメーター
	// 付属クラス
	public Rigidbody_grgr rigidbody;
	public SkillManager skillManager{get;set;}
	// 状態
	public Phase<State> state = new Phase<State>();
	private Gear gear;	// ギア
	// UI
	public Text curS;	// 速度
	// 地上からの高さ調整
	public float HEIGHT_FROM_GROUND = -0.6f;
	// 敵との衝突フラグ
	public bool isEnemyCollision{get;set;}
	// ASCENSION用
	private float ascensionTimer = 0;
	private float ASCENSION_TIME = 3.0f;
	// HP
	public float hp = 100;
	// AP
	public float ap{get;set;}
	
#endregion

#region Unity関数
	void Awake()
	{
		skillManager = new SkillManager();
		skillManager.AddSkill(SkillDataBase.DATAS[SkillType.PUNCH]);
		skillManager.AddSkill(SkillDataBase.DATAS[SkillType.KICK]);
		skillManager.AddSkill(SkillDataBase.DATAS[SkillType.HIGH_KICK]);
		skillManager.AddSkill(SkillDataBase.DATAS[SkillType.DEFENSE]);
		skillManager.AddSkill(SkillDataBase.DATAS[SkillType.COUNTER]);
		
		rigidbody = new Rigidbody_grgr(transform);
		rigidbody.isMove = false;

		transform.position = Rigidbody_grgr.RotateToPosition(transform.up, GameData.GetPlanet().position, GameData.GetPlanet().localScale.y * 0.5f, HEIGHT_FROM_GROUND);

		animator = GetComponent<Animator>();

		gear = Gear.First;
		state.Change(State.STOP);

		ascensionTimer = ASCENSION_TIME;
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		// 最大速度を変更
		rigidbody.maxVelocitySpeed = maxSpeed;

		rigidbody.prevPosition = transform.position;
		rigidbody.prevVelocity = rigidbody.velocity;
		
		state.Start();
		switch(state.current){
			// ストップ状態
			case State.STOP: {
				if (state.IsFirst()){
					currentVelocity = Vector3.zero;
					transform.position = Rigidbody_grgr.RotateToPosition(transform.up, GameData.GetPlanet().position, GameData.GetPlanet().localScale.y*0.5f, HEIGHT_FROM_GROUND);
				}
				
				// ドラッグ移動条件
				if (TouchController.GetTouchType() == TouchController.TouchType.Drag){
					currentVelocity = DragVelocity();
					GrgrMove(currentVelocity);
					state.Change(State.DRAG_MOVE);
				}
				// フリック移動条件
				else if (TouchController.GetTouchType() == TouchController.TouchType.Frick){
					currentVelocity = FlickVelocity();
					FlickMove();
					state.Change(State.FLICK_MOVE);
				}
			}
			break;
			// ドラッグ移動状態
			case State.DRAG_MOVE:{
				currentVelocity = DragVelocity();

				// 現在のドラッグ移動量
				float mVal = currentVelocity.magnitude;

				// ストップ移動条件
				if (mVal < UtilityMath.epsilon){
					rigidbody.velocity = Vector3.zero;
					rigidbody.friction = maxFriction;
					gear = Gear.First;
					animator.SetBool("Run", false);
					state.Change(State.STOP);
					return;
				}
				
				GrgrMove(currentVelocity);
			}
			break;
			// フリック移動状態
			case State.FLICK_MOVE: {
				currentVelocity = FlickVelocity();
				// 現在フレームの移動量
				float s = (currentVelocity.magnitude + rigidbody.GetSpeed()) * Time.deltaTime;
				// ストップ移動条件
				if (s < UtilityMath.epsilon || IsMoveStop()){
					rigidbody.velocity = Vector3.zero;
					rigidbody.friction = maxFriction;
					gear = Gear.First;
					state.Change(State.STOP);
					animator.SetBool("Run", false);
					return;
				}
				FlickMove();
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
			break;
			// 昇天
			case State.ASCENSION:{
				Ascension();
			}
			break;
			// バトル状態
			case State.BATTLE:{
				if (state.IsFirst()){
					currentVelocity = Vector3.zero;
					animator.speed = BattleManager._instance.SLOW_START;
				}
				switch(BattleManager._instance.battle.current){
					// スキルバトル結果
					case BattleManager.Battle.SKILL_BATTLE_RESULT:{
						if (BattleManager._instance.battle.IsFirst()){
							animator.speed = 1.0f;
						}

						// 勝敗
						switch(skillManager.result){
							// 勝ち
							case SkillAttributeResult.WIN:{
								Vector3 toEnemy = GameData.GetEnemy().position - GameData.GetPlayer().position;
								Vector3 front = Vector3.ProjectOnPlane(toEnemy.normalized, transform.up).normalized;
								GrgrAddForce(front * resultSpeed);
								GearChange();
								GrgrMove(rigidbody.velocity);
							}
							break;
							// それ以外
							default:{
								FlickMove();
							}
							break;
						}
					}
					break;
					// バトル終了
					case BattleManager.Battle.BATTLE_END:{
						if (BattleManager._instance.battle.IsFirst()){
							animator.speed = 1.0f;
							state.Change(State.FLICK_MOVE);
						}
					}
					break;
					default:{
						float time = (BattleManager._instance.SLOW_TOTAL_TIME - BattleManager._instance.slowTimer);
						switch(BattleManager._instance.slowType){
							case EaseType.LINEAR2:{
								float t;
								float start;
								float end;
								float totalTime;
								if (time < BattleManager._instance.SLOW_TIME){
									t = time;
									totalTime = BattleManager._instance.SLOW_TIME;
									start = BattleManager._instance.SLOW_START;
									end = BattleManager._instance.SLOW_END;
								}
								else{
									t = time - BattleManager._instance.SLOW_TIME;
									totalTime = BattleManager._instance.SLOW_TIME2;
									start = BattleManager._instance.SLOW_START2;
									end = BattleManager._instance.SLOW_END2;
								}

								animator.speed = (float)UtilityMath.Linear(t, totalTime, end, start);
							}
							break;
							default:{
								float start = BattleManager._instance.SLOW_START;
								float end = BattleManager._instance.SLOW_END;
								animator.speed = (float)UtilityMath.GetEasing(BattleManager._instance.slowType, time, BattleManager._instance.SLOW_TOTAL_TIME, end, start);
							}
							break;
						}
						FlickMove();
					}
					break;
				}
			}
			break;
		}
		state.Update();

		curS.text = (int)(rigidbody.GetSpeed() * 3.6f) + "km";
	}

	

#endregion

#region 移動

	// 球体ぐるぐる移動
	void GrgrMove(Vector3 velocity){
		// 移動量を円弧とし、角度を求めて移動する
		if (velocity.magnitude > UtilityMath.epsilon){
			float speed = velocity.magnitude;
			float arc = speed * Time.deltaTime * animator.speed;
			float angle = arc / (2.0f*Mathf.PI*GameData.GetPlanet().transform.localScale.y*0.5f) * 360.0f;
			transform.rotation = Quaternion.LookRotation(velocity.normalized, transform.up);
			transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;
		}
		transform.position = Rigidbody_grgr.RotateToPosition(transform.up, GameData.GetPlanet().position, GameData.GetPlanet().localScale.y * 0.5f, HEIGHT_FROM_GROUND);
	}

	// 球体ぐるぐるrigidbody.velocity加算
	void GrgrAddForce(Vector3 veloctiy){
		// リジッドボディのベクトルを変更
		Vector3 front;
		// フリック方向
		if (veloctiy.magnitude > UtilityMath.epsilon){
			front = veloctiy.normalized;
		}
		// リジッドボディのベクトルをプレイヤーの立つ平面に投射したベクトル
		else{
			front = Vector3.ProjectOnPlane(rigidbody.velocity, transform.up).normalized;
		}
		rigidbody.velocity = front * rigidbody.GetSpeed();
		rigidbody.AddForce(veloctiy);
	}

	// フリック移動処理
	void FlickMove(){
		// フリックを移動量に加算
		{
			GrgrAddForce(currentVelocity);
		}

		// ギアの設定
		{
			// ギアの段階による速度制限
			GearSpeedLimit();

			// ギア切り替え
			GearChange();
		}
		
		// 摩擦軽減処理
		{
			// 最高速度との差の比
			float t = Mathf.Clamp(rigidbody.GetSpeed() / maxSpeed, 0.0f, 1.0f);
			
			// 摩擦の計算
			double friction = UtilityMath.OutExp(t, 1.0f, minFriction, maxFriction);
			
			rigidbody.friction = (float)friction * animator.speed;
		}
		GrgrMove(rigidbody.velocity);

		rigidbody.Update();
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
			velocity = GameData.GetCamera().rotation * velocity;
			
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

#region 昇天
	void Ascension(){
		ascensionTimer -= Time.deltaTime;
		if (ascensionTimer < 0){
			rigidbody.isMove = false;
			rigidbody.velocity = Vector3.zero;
			transform.rotation = Random.rotation;
			ascensionTimer = ASCENSION_TIME;
			animator.SetBool("DamageDown", false);
			state.Change(State.STOP);
		}
		rigidbody.Update();
	}
#endregion

#region 衝突処理
	void OnCollisionStay(Collision other)
	{
		// 柱との衝突処理
		if (other.gameObject.tag == "Piller")
		{
			if (other.collider.GetComponent<PillerDeadTime>().isDead)
				return;
			
			GameData.killPillers++;

			other.collider.GetComponent<PillerDeadTime>().isDead = true;
			Vector3 impact = (other.collider.transform.up + rigidbody.velocity);
			other.collider.GetComponent<PillerController>().rigidbody.AddForce(impact*0.1f);
			return;
		}

		// 敵との衝突処理
		if (other.collider.tag == "Enemy")
		{
			// 敵吹っ飛び中
			if (other.collider.GetComponent<EnemyController>().state.current == EnemyController.State.ASCENSION){
				return;
			}

			// バトル中に衝突
			if (state.current == State.BATTLE){
				// リザルト以外で当たったらバトル強制終了
				if (BattleManager._instance.battle.current != BattleManager.Battle.SKILL_BATTLE_RESULT){
					if (BattleManager._instance.battle.current != BattleManager.Battle.BATTLE_END)
						BattleManager.BattleForceEnd();
					return;
				}

				if (skillManager.result == SkillAttributeResult.WIN){
					Vector3 impact = (other.transform.up + rigidbody.velocity.normalized) * rigidbody.GetSpeed() * 0.03f;
					other.collider.GetComponent<EnemyController>().rigidbody.velocity = Vector3.zero;
					other.collider.GetComponent<EnemyController>().rigidbody.AddForce(impact);
					Vector3 front = Vector3.ProjectOnPlane(-impact.normalized, other.transform.up).normalized;
					other.transform.rotation = Quaternion.LookRotation(front, other.transform.up);
					other.collider.GetComponent<EnemyController>().rigidbody.isMove = true;
					other.collider.GetComponent<Animator>().SetBool("Run", false);
					other.collider.GetComponent<Animator>().SetBool("DamageDown", true);
					GameData.GetEnemy().GetComponent<EnemyController>().state.Change(EnemyController.State.ASCENSION);
				}
				else if (skillManager.result == SkillAttributeResult.LOSE){
					Vector3 impact = (transform.up - transform.forward) * GameData.GetEnemy().GetComponent<EnemyController>().speed * 0.03f;
					rigidbody.velocity = Vector3.zero;
					rigidbody.AddForce(impact);
					rigidbody.isMove = true;
					animator.SetBool("Run", false);
					animator.SetBool("DamageDown", true);
					state.Change(State.ASCENSION);
				}
				else{
					state.Change(State.FLICK_MOVE);
					GameData.GetEnemy().GetComponent<EnemyController>().state.Change(EnemyController.State.MOVE);
				}
				GameData.GetBattleManager().GetComponent<BattleManager>().skillBattleResultEnd = true;
			}
			// バトル外で衝突
			else{
				// other.collider.GetComponent<Animator>().SetBool("Run", false);
				// other.collider.GetComponent<Animator>().SetBool("DamageDown", true);
				// GameData.GetEnemy().GetComponent<EnemyController>().state.Change(EnemyController.State.ASCENSION);
			}
		}
	}
#endregion

#region ビタ止め
	// ビタ止めフラグ関数
	bool IsMoveStop(){
		return (rigidbody.GetSpeed() > UtilityMath.epsilon) && (IsMoveTouchStop() || IsMoveFrickStop());
	}

	// フリックビタ止めフラグ関数
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

	// タッチ止めフラグ関数
	bool IsMoveTouchStop(){
		if (!TouchController.IsPlanetTouch()){
			return false;
		}
		return (TouchController.GetTouchTimer() > MOVE_TOUCH_STOP_TIME);
	}
#endregion

#region ギア

	// ギアの段階による速度制限
	void GearSpeedLimit(){
		switch(gear){
			case Gear.First: rigidbody.velocity = rigidbody.velocity.normalized * Mathf.Min(rigidbody.GetSpeed(), gearSecondBase + 1.0f); break;
			case Gear.Second: rigidbody.velocity = rigidbody.velocity.normalized * Mathf.Min(rigidbody.GetSpeed(), gearThirdBase + 1.0f); break;
		}
	}
			
	// ギアの切り替え
	void GearChange(){
		float speed = rigidbody.GetSpeed();
		gear = Gear.First;
		if (speed >= gearSecondBase)
			gear = Gear.Second;
		if (speed >= gearThirdBase)
			gear = Gear.Third;
	}
	
#endregion
}
