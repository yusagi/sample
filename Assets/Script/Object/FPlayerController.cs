using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class FPlayerController : PlayerControllerBase {

#region enum

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
	public float DRAG_PERMISSION = 1.0f;	// ドラッグ条件値
	public float dragSpeed = 5.0f;			// ドラッグ移動速度

	public float chaseSpeed = 3.0f;			// 追跡モード時加速度
	
	public float gearSecondBase = 30.0f;	// ギア2突入条件値
	public float gearThirdBase = 40.0f;		// ギア3突入条件値

	public float gearFirstAccele = 20.0f;	// ギア1加速度	
	public float gearSecondAccele = 5.0f;	// ギア2加速度
	public float gearThirdAccele = 1.0f;	// ギア3加速度

	public float maxSpeed = 100.0f;			// 最高速度

	public Vector3 currentFlickVelocity{get;set;}	// 最新フレームのフリック移動量

	// ビタ止め
	public float MOVE_FRICK_STOP_ANGLE = 45.0f;	// フリックビタ止め条件値
	public float MOVE_TOUCH_STOP_TIME = 1.0f;	// タッチビタ止め条件値
	
	// コンポーネント
	private Rigidbody_grgr rigidbody;	// grgrリジッドボディ
	private Animator animator;			// アニメーター

	// 状態
	private Gear gear;					// ギア
	
	// UI
	public Text pFri;	// 摩擦力
	public Text accele;	// 加速度
	public Text curS;	// 速度	
	public Text decay;	// 減衰

public bool isHitStop = true;
#endregion

#region Unity関数
	void Awake()
	{
		rigidbody = GetComponent<Rigidbody_grgr>();
		animator = GetComponent<Animator>();

		gear = Gear.First;
		state = State.Stop;
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		rigidbody.prevPosition = transform.position;

		if ((state == State.Stop || state == State.Chase) && TouchController.GetTouchType() == TouchController.TouchType.Frick){
			state = State.Move;
		}
		currentFlickVelocity = Vector3.zero;
		
		switch(state){
			case State.Stop: {
				DragMove();
			}
			break;
			case State.Move: {
				FlickMove();
			}
			break;
			case State.Chase:{
				ChaseMove();
			}
			break;
			case State.HitStop:{
			}
			break;
		}
		
			curS.text = (int)rigidbody.GetSpeed() + "km";
			decay.text = "減衰 " + (rigidbody.GetPrevSpeed() - rigidbody.GetSpeed());
			pFri.text = "摩擦 " + rigidbody.friction;
	}
#endregion

#region 移動

	// フリック移動処理
	void FlickMove(){
		currentFlickVelocity = FlickVelocity();
		Vector3 moveVelocity = currentFlickVelocity;

		// 現在フレームの移動量
		float s = (moveVelocity.magnitude + rigidbody.GetSpeed()) * Time.deltaTime;
		// 動いてなかったら または ビタ止め時
		if (s < UtilityMath.epsilon || IsMoveStop()){
			rigidbody.velocity = Vector3.zero;
			rigidbody.friction = maxFriction;
			gear = Gear.First;
			state = State.Stop;
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
				if (moveVelocity.magnitude > UtilityMath.epsilon){
					front = moveVelocity.normalized;
				}
				// リジッドボディのベクトルをプレイヤーの立つ平面に投射したベクトル
				else{
					front = Vector3.ProjectOnPlane(rigidbody.velocity, transform.up).normalized;
				}
				rigidbody.velocity = front * rigidbody.GetSpeed();
				rigidbody.AddForce(moveVelocity);
			}

			// ギアの設定
			{
				// ギアの段階による速度制限
				switch(gear){
					case Gear.First: rigidbody.velocity = rigidbody.velocity.normalized * Mathf.Min(rigidbody.GetSpeed(), gearSecondBase + 1.0f); break;
					case Gear.Second: rigidbody.velocity = rigidbody.velocity.normalized * Mathf.Min(rigidbody.GetSpeed(), gearThirdBase + 1.0f); break;
				}

				// ギア切り替え
				{
					gear = Gear.First;
					if (rigidbody.GetSpeed() >= gearSecondBase)
						gear = Gear.Second;
					if (rigidbody.GetSpeed() >= gearThirdBase)
						gear = Gear.Third;
				}
			}
			
			// 摩擦軽減処理
			{
				// 最高速度との差の比
				float t = Mathf.Clamp(rigidbody.GetSpeed()/ maxSpeed, 0.0f, 1.0f);
				
				// 摩擦の計算
				double friction = UtilityMath.OutExp(t, 1.0f, minFriction, maxFriction);
				
				rigidbody.friction = (float)friction * animator.speed;
			}

			// 移動量を角度として考え移動する
			{
				float length = rigidbody.GetSpeed() * Time.deltaTime * animator.speed;
				if (length == 0){
					return;
				}
				float angle = length / (2.0f*Mathf.PI*GameData.GetPlanet().transform.localScale.y*0.5f) * 360.0f;
				//transform.position = planet.position;
				transform.rotation = Quaternion.LookRotation(rigidbody.velocity.normalized, transform.up);
				transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;
				//transform.position += transform.up * planet.localScale.y;
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
	void DragMove(){
		Vector3 moveVelocity = DragVelocity();

		if (moveVelocity.magnitude < UtilityMath.epsilon){
			rigidbody.velocity = Vector3.zero;
			rigidbody.friction = maxFriction;
			gear = Gear.First;
			animator.SetBool("Run", false);
			return;
		}

		Transform planet = GameData.GetPlanet();
		
		float length = moveVelocity.magnitude * Time.deltaTime;
		float angle = length / (2.0f*Mathf.PI*planet.transform.localScale.y*0.5f) * 360.0f;
		transform.position = planet.position;
		Quaternion rotate = Quaternion.LookRotation(moveVelocity.normalized, transform.up);
		transform.rotation = Quaternion.AngleAxis(angle, transform.right) * rotate;
		transform.position += transform.up * planet.localScale.y;
		//animator.SetFloat("Speed", moveVelocity.magnitude);
	}

	// 追跡移動処理
	void ChaseMove(){
		// ビタ止めフラグ
		if (IsMoveStop()){
			rigidbody.velocity = Vector3.zero;
			rigidbody.friction = maxFriction;
			gear = Gear.First;
			state = State.Stop;
			animator.SetBool("Run", false);
		}
		else{
			// ベクトル作成
			Vector3 front = Vector3.ProjectOnPlane(rigidbody.velocity.normalized, transform.up).normalized;

			// フリックを移動量に加算
			{
				// 最大速度を変更
				rigidbody.maxVelocitySpeed = maxSpeed;
				rigidbody.velocity = front * rigidbody.GetSpeed();
				rigidbody.AddForce(rigidbody.velocity.normalized * chaseSpeed);
			}

			// ギア切り替え
			{
				gear = Gear.First;
				if (rigidbody.GetSpeed() >= gearSecondBase)
					gear = Gear.Second;
				if (rigidbody.GetSpeed() >= gearThirdBase)
					gear = Gear.Third;
			}

			// 摩擦軽減処理
			{
				// 最高速度との差の比
				float t = Mathf.Clamp(rigidbody.GetSpeed()/ maxSpeed, 0.0f, 1.0f);
				
				// 摩擦の計算
				double friction = UtilityMath.OutExp(t, 1.0f, minFriction, maxFriction);
				
				rigidbody.friction = (float)friction * animator.speed;
			}

			// 移動量を角度として考え移動する
			{
				float angle = UtilityMath.LenToAngle(rigidbody.velocity.magnitude*Time.deltaTime, GameData.GetPlanet().localScale.y*0.5f);
				transform.rotation = Quaternion.LookRotation(rigidbody.velocity.normalized, transform.up);
				transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;
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


			accele.text = "加速 " + speed * tmp;
		}

		state = State.Move;

		return velocity;
	}

	// ドラッグ移動量
	Vector3 DragVelocity(){
		if (!TouchController.IsPlanetTouch())
			return Vector3.zero;

		Vector3 velocity = Vector3.zero;
		
		// ドラッグ移動
		if (rigidbody.GetSpeed() < UtilityMath.epsilon && !TouchController.IsFlickSuccess())
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

			other.gameObject.GetComponent<PlanetWalk>().enabled = false;
			other.collider.GetComponent<PillerDeadTime>().isDead = true;
			Vector3 impact = (other.collider.transform.up + rigidbody.velocity);
			other.collider.GetComponent<Rigidbody_grgr>().AddForce(impact*0.1f);
			return;
		}

		// 敵との衝突処理
		if (other.gameObject.tag == "Enemy")
		{
			if (other.gameObject.GetComponent<PlanetWalk>().enabled == false)
				return;

			Vector3 impact = (other.transform.up + rigidbody.velocity.normalized) * rigidbody.GetSpeed() * 0.03f;
			other.collider.GetComponent<Rigidbody_grgr>().velocity = Vector3.zero;
			other.collider.GetComponent<Rigidbody_grgr>().AddForce(impact);

			Vector3 front = Vector3.ProjectOnPlane(-impact.normalized, other.transform.up).normalized;
			other.transform.rotation = Quaternion.LookRotation(front, other.transform.up);

			if (!isHitStop){
				other.collider.GetComponent<PlanetWalk>().enabled = false;
				other.collider.GetComponent<Rigidbody_grgr>().isMove = true;
				other.collider.GetComponent<EnemyControllerBase>().state = EnemyControllerBase.State.Ascension;
				other.collider.GetComponent<Animator>().SetBool("Run", false);
				other.collider.GetComponent<Animator>().SetTrigger("DamageDown");
			}
			else
				HitStop.isStart = true;
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
