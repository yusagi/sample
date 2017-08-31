//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//using UnityEngine.UI;

//public class PlayerController : MonoBehaviour {

//#region enum 

//	public enum State{
//		STOP,
//		FLICK_MOVE,
//		DRAG_MOVE,
//		BATTLE,
//		ASCENSION,
//	}

//	enum Gear{
//		First,
//		Second,
//		Third
//    }

//#endregion

//#region 定数

//    // ビタ止め
//    public float MOVE_FRICK_STOP_ANGLE = 0; // フリックビタ止め条件値
//    public float MOVE_TOUCH_STOP_TIME = 1;  // タッチビタ止め条件値

//    // ASCENSION時間
//    private float ASCENSION_TIME = 3.0f;

//    // ドラッグ条件値
//    public float DRAG_PERMISSION = 10.0f;

//    // 地上からの高さ調整
//    public float HEIGHT_FROM_GROUND = -0.6f;
//#endregion

//#region メンバ変数

//    private GameObject m_Planet;

//    // 移動
//    public float minFriction = 0.001f; 	// 最小摩擦力
//	public float maxFriction = 0.01f; 	// 最大摩擦力

//	public float dragSpeed = 10.0f;			// ドラッグ移動速度

//	public float gearSecondBase = 30.0f;	// ギア2突入条件値
//	public float gearThirdBase = 40.0f;		// ギア3突入条件値
//	public float gearFirstAccele = 20.0f;	// ギア1加速度	
//	public float gearSecondAccele = 5.0f;	// ギア2加速度
//	public float gearThirdAccele = 2.0f;	// ギア3加速度

//	public float maxSpeed = 50.0f;			// 最高速度
//    // HP
//    public int hp = 100;
//    // AP
//    public float ap { get; set; }
//    // コンポーネント
//    public AnimationManager m_AnmMgr { get; set; }// アニメーター
//	// 付属クラス
//	public Rigidbody_grgr rigidbody { get; set; }
//	public SkillManager skillManager{get;set;}
//	// 状態
//	public Phase<State> state = new Phase<State>();
//	private Gear gear;  // ギア
//    // フラグ
//    public bool m_IsBattleAnmStart { get; set; }      // ジャンプバトル開始
//    public bool m_IsBattleAnmPlay { get; set; }           // ジャンプバトル中
//    // ASCENSION用
//    private float ascensionTimer = 0;
//	// コルーチン
//	IEnumerator<Quaternion> qLerp;
//	public IEnumerator battle{get;set;}

//    // 外部入力
//    public TouchType m_TouchType { get; set; }
//    public Vector3 m_CurrentVelocity { get; set; }
//    public Quaternion m_CurrentRotate { get; set; }
//    public bool m_TouchStopFlag { get; set; }

//#endregion

//#region Unity関数

//    void Awake()
//    {
//        m_Planet = GameManager.m_Planet;

//        // スキルマネージャー初期化
//        skillManager = new SkillManager();
//        foreach(var sData in SkillDataBase.DATAS)
//        {
//            skillManager.AddSkill(sData.Value);
//        }

//        // grgrリジッドボディ初期化
//        rigidbody = new Rigidbody_grgr(transform);
//        rigidbody.isMove = false;

//        // アニメーションマネージャーコンポーネント取得
//        m_AnmMgr = GetComponent<AnimationManager>();
        
//        // ギア初期化
//        gear = Gear.First;

//        // キャラクター状態初期化
//        state.Change(State.STOP);

//        // バトルアニメーションフラグ初期化
//        m_IsBattleAnmStart = false;
//        m_IsBattleAnmPlay = false;

//        // 座標初期化
//        transform.position = Rigidbody_grgr.RotateToPosition(transform.up, m_Planet.transform.position, m_Planet.transform.localScale.y * 0.5f, HEIGHT_FROM_GROUND);

//        // 外部入力変数初期化
//        m_TouchType = TouchType.None;
//        m_CurrentVelocity = Vector3.zero;
//        m_TouchStopFlag = false;
//        m_CurrentRotate = Quaternion.identity;
//    }

//    // Use this for initialization
//    void Start ()
//    {
//    }
    
//    // Update is called once per frame
//    void Update () {
//        // 最大速度を変更
//        rigidbody.maxVelocitySpeed = maxSpeed;

//        // 前フレーム情報を設定
//		rigidbody.prevPosition = transform.position;
//		rigidbody.prevVelocity = rigidbody.velocity;

//        // 状態別更新
//        CharactorUpdata();
//	}



//    #endregion

//#region 状態別更新
//    void CharactorUpdata()
//    {
//        state.Start();
//        switch (state.current)
//        {
//            // ストップ状態
//            case State.STOP:
//                {
//                    if (state.IsFirst())
//                    {
//                        transform.position = Rigidbody_grgr.RotateToPosition(transform.up, m_Planet.transform.position, m_Planet.transform.localScale.y * 0.5f, HEIGHT_FROM_GROUND);
//                    }

//                    // ドラッグ移動条件
//                    if (m_TouchType == TouchType.Drag)
//                    {
//                        Vector3 velocity = DragVelocity();
//                        GrgrMove(velocity * Time.deltaTime, 0.0f);
//                        state.Change(State.DRAG_MOVE);
//                    }
//                    // フリック移動条件
//                    else if (m_TouchType == TouchType.Frick)
//                    {
//                        Vector3 velocity = FlickVelocity();
//                        FlickMove(velocity);
//                        state.Change(State.FLICK_MOVE);
//                    }
//                }
//                break;
//            // ドラッグ移動状態
//            case State.DRAG_MOVE:
//                {
//                    Vector3 velocity = DragVelocity();

//                    // 現在のドラッグ移動量
//                    float mVal = velocity.magnitude;

//                    // ストップ移動条件
//                    if (mVal < UtilityMath.epsilon)
//                    {
//                        rigidbody.velocity = Vector3.zero;
//                        rigidbody.friction = maxFriction;
//                        gear = Gear.First;
//                        m_AnmMgr.ChangeAnimationLoop("Idle", 0.1f, 0);
//                        state.Change(State.STOP);
//                        return;
//                    }

//                    GrgrMove(velocity * Time.deltaTime, 0.0f);
//                }
//                break;
//            // フリック移動状態
//            case State.FLICK_MOVE:
//                {
//                    if (state.IsFirst())
//                    {
//                        m_AnmMgr.ChangeAnimationLoop("Run", 0.1f, 0);
//                        break;
//                    }

//                    Vector3 velocity = FlickVelocity();
//                    // 現在フレームの移動量
//                    float s = (velocity.magnitude + rigidbody.GetSpeed()) * Time.deltaTime;
//                    // ストップ移動条件
//                    if (s < UtilityMath.epsilon || IsMoveStop())
//                    {
//                        rigidbody.velocity = Vector3.zero;
//                        rigidbody.friction = maxFriction;
//                        gear = Gear.First;
//                        state.Change(State.STOP);
//                        m_AnmMgr.ChangeAnimationLoop("Idle", 0.1f, 0);
//                        m_TouchStopFlag = false;
//                        return;
//                    }
//                    FlickMove(velocity);
//                }
//                break;
//            // 昇天
//            case State.ASCENSION:
//                {
//                    Ascension();
//                }
//                break;
//            // バトル状態
//            case State.BATTLE:
//                {
//                    BattleUpdate();
//                }
//                break;
//        }
//        state.Update();
//    }
//    #endregion

//#region 移動

//    // 球体ぐるぐる移動
//    void GrgrMove(Vector3 velocity, float jamp){
//		// 移動量を円弧とし、角度を求めて移動する
//		if (velocity.magnitude > Vector3.kEpsilon){
//			float arc = velocity.magnitude;
//			float angle = arc / (2.0f*Mathf.PI*m_Planet.transform.transform.localScale.y*0.5f) * 360.0f;
//			transform.rotation = Quaternion.LookRotation(velocity.normalized, transform.up);
//			transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;
//		}
//		transform.position = Rigidbody_grgr.RotateToPosition(transform.up, m_Planet.transform.position, m_Planet.transform.localScale.y * 0.5f, HEIGHT_FROM_GROUND + jamp);
//	}

//    #region フリック移動
//    // フリック移動処理
//    void FlickMove(Vector3 velocity){
//		// フリックをgrgrリジッドボディの移動量に加算
//		{
//            FrickAddForce(velocity);
//		}

//		// ギアの設定
//		{
//			// ギアの段階による速度制限
//			GearSpeedLimit();

//			// ギア切り替え
//			GearChange();
//		}
		
//		// 摩擦軽減処理
//		{
//			// 最高速度との差の比
//			float t = Mathf.Clamp(rigidbody.GetSpeed() / maxSpeed, 0.0f, 1.0f);
			
//			// 摩擦の計算
//			double friction = UtilityMath.OutExp(t, 1.0f, minFriction, maxFriction);
			
//			rigidbody.friction = (float)friction;
//		}
//		GrgrMove(rigidbody.velocity * Time.deltaTime, 0.0f);

//		rigidbody.Update();
//	}
	
//	// フリック移動量
//	Vector3 FlickVelocity(){
//		if (m_TouchType != TouchType.Frick){
//			return Vector3.zero;
//		}

//        if (m_CurrentVelocity.magnitude < Vector3.kEpsilon)
//        {
//            return Vector3.zero;
//        }

//		float speed = 0.0f;
//		// ギアの段階に応じた加速度を設定
//		{
//			switch(gear){
//				case Gear.First: speed = gearFirstAccele; break;
//				case Gear.Second: speed = gearSecondAccele; break;
//				case Gear.Third: speed = gearThirdAccele; break;
//			}
//		}
        
//		// 移動ベクトル作成
//		{
//			Vector3 velocity = m_CurrentRotate * m_CurrentVelocity;
			
//			float tmp = Mathf.Clamp(velocity.magnitude, 0.0f, 1080);
//			tmp = Mathf.Clamp(tmp / 1080, 0.1f, 1.0f);

//            return Vector3.ProjectOnPlane(velocity, transform.up).normalized * tmp * speed;
//		}
//    }

//    // 球体ぐるぐるrigidbody.velocity加算
//    void FrickAddForce(Vector3 velocity)
//    {
//        // リジッドボディのベクトルを変更
//        Vector3 front;
//        // フリック方向
//        if (velocity.magnitude > UtilityMath.epsilon)
//        {
//            front = velocity.normalized;
//        }
//        // リジッドボディのベクトルをプレイヤーの立つ平面に投射したベクトル
//        else
//        {
//            front = Vector3.ProjectOnPlane(rigidbody.velocity, transform.up).normalized;
//        }
//        rigidbody.velocity = front * rigidbody.GetSpeed();
//        rigidbody.AddForce(velocity);
//    }
//    #endregion

//    #region ドラッグ移動
//    // ドラッグ移動量
//    Vector3 DragVelocity(){

//        if (m_CurrentVelocity.magnitude < DRAG_PERMISSION)
//        {
//            return Vector3.zero;
//        }

//        Vector3 velocity;
//        velocity = m_CurrentRotate * m_CurrentVelocity.normalized;
//        velocity = Vector3.ProjectOnPlane(velocity, transform.up).normalized;

//        return velocity * dragSpeed;
//	}
//    #endregion

//    #endregion

//#region 昇天
//    void AscensionIni(Vector3 impact){
//		rigidbody.isMove = true;
//		rigidbody.velocity = impact;
//        ascensionTimer = ASCENSION_TIME;
//        m_AnmMgr.ChangeAnimation("DamageDown", 0.1f, 0, 1);
//	}

//	void Ascension(){
//		ascensionTimer -= Time.deltaTime;
//		if (ascensionTimer < 0){
//			rigidbody.isMove = false;
//			rigidbody.velocity = Vector3.zero;
//			transform.rotation = Random.rotation;
//			m_AnmMgr.ChangeAnimationLoop("Idle", 0.1f, 0);
//			hp = 100;
//			state.Change(State.STOP);
//		}
//		rigidbody.Update();
//	}
//#endregion

//#region 衝突処理
//	void OnCollisionStay(Collision other)
//	{
//		// 柱との衝突処理
//		if (other.gameObject.tag == "Piller")
//		{
//			if (other.collider.GetComponent<PillerDeadTime>().isDead)
//				return;
			
//			GameData.killPillers++;

//			other.collider.GetComponent<PillerDeadTime>().isDead = true;
//			Vector3 impact = (other.collider.transform.up + rigidbody.velocity);
//			other.collider.GetComponent<PillerController>().rigidbody.AddForce(impact*0.1f);
//			return;
//		}
//	}
//#endregion

//#region ビタ止め
//	// ビタ止めフラグ関数
//	bool IsMoveStop(){
//		return (rigidbody.GetSpeed() > UtilityMath.epsilon) && (m_TouchStopFlag || IsMoveFrickStop());
//	}

//	// フリックビタ止めフラグ関数
//	bool IsMoveFrickStop(){
//        Vector3 frickVel = m_CurrentVelocity;
//		Vector3 backVel = new Vector3(0, 0, -1);
//		float dot = Vector3.Dot(backVel.normalized, frickVel.normalized);
//		float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
//		return angle < MOVE_FRICK_STOP_ANGLE;
//	}
//#endregion

//#region ギア

//	// ギアの段階による速度制限
//	void GearSpeedLimit(){
//		switch(gear){
//			case Gear.First: rigidbody.velocity = rigidbody.velocity.normalized * Mathf.Min(rigidbody.GetSpeed(), gearSecondBase + 1.0f); break;
//			case Gear.Second: rigidbody.velocity = rigidbody.velocity.normalized * Mathf.Min(rigidbody.GetSpeed(), gearThirdBase + 1.0f); break;
//		}
//	}
			
//	// ギアの切り替え
//	void GearChange(){
//		float speed = rigidbody.GetSpeed();
//		gear = Gear.First;
//		if (speed >= gearSecondBase)
//			gear = Gear.Second;
//		if (speed >= gearThirdBase)
//			gear = Gear.Third;
//	}
	
//#endregion

//#region バトルモード
//	// バトルモード更新
//	void BattleUpdate(){
//		if (state.IsFirst()){
//			m_CurrentVelocity = Vector3.zero;
//		}

//		switch(BattleManager._instance.battle.current){
//			// エンカウント
//			case BattleManager.Battle.SKILL_BATTLE_START:{
//				if (BattleManager._instance.battle.IsFirst()){
//					return;
//				}
				
//				FlickMove(Vector3.zero);
//			}
//			break;
//			// スキルバトルプレイ
//			case BattleManager.Battle.SKILL_BATTLE_PLAY:{
//				if (BattleManager._instance.battle.IsFirst()){
//        			BattleManager._instance.battlePlayflags.Add(0, false);

//                        StartCoroutine(battle);

//                        return;
//				}
//			}
//			break;
//			// スキルバトル終了
//			case BattleManager.Battle.SKILL_BATTLE_END:{
//				FlickMove(Vector3.zero);
//			}
//			break;
//			// バトル終了
//			case BattleManager.Battle.BATTLE_END:{
//				if (BattleManager._instance.battle.IsFirst()){
//					// ライフが0になったら
//					if (hp <= 0){
//						Vector3 impact = transform.up;
//						AscensionIni(impact);
//						state.Change(State.ASCENSION);
//					}
//					else{
//						//animator.m_Animator.Play("Idle");
//						state.Change(State.FLICK_MOVE);
//					}
//				}
//			}
//			break;
//		}
//	}

//	// バトル結果更新 
//	void BattleResultUpdate(){
//		BattleManager.ResultPhase phase = BattleManager._instance.resultPahse.current;
//		switch(phase){
//			case BattleManager.ResultPhase.FIRST:{
//				if (BattleManager._instance.resultPahse.IsFirst()){
//					BattleResultSetAnm(BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>(), phase);
//				}
//			}
//			break;
//			case BattleManager.ResultPhase.SECOND:{
//				if (BattleManager._instance.resultPahse.IsFirst()){
//					BattleResultSetAnm(BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>(), phase);
//				}
//			}
//			break;
//			case BattleManager.ResultPhase.THIRD:{
//				if (BattleManager._instance.resultPahse.IsFirst()){
//					BattleResultSetAnm(BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>(), phase);
//				}
//			}
//			break;
//			case BattleManager.ResultPhase.FOURTH:{
//				if (BattleManager._instance.resultPahse.IsFirst()){
//					BattleResultSetAnm(BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>(), phase);
//				}
//			}
//			break;
//		}
//	}

//	// バトルリザルトセットアニメーション
//	void BattleResultSetAnm(SkillChoiceBoardController controller, BattleManager.ResultPhase resultPhase){
//		int damage = 0;
//		switch(controller.GetAnimationType(resultPhase, SkillChoiceBoardController.DataType.PLAYER)){
//			case AnimationType.NORMAL_ATTACK:{
//				SkillData data = controller.GetSkillData(resultPhase, SkillChoiceBoardController.DataType.PLAYER);
//				m_AnmMgr.ChangeAnimation(data._anmName, 0.1f, 0, 0.8f);
//				damage = data._attack;
//			}
//			break;
//			case AnimationType.COUNTER_ATTACK:{
//				SkillData data = controller.GetSkillData(resultPhase, SkillChoiceBoardController.DataType.ENEMY);
//				m_AnmMgr.ChangeAnimation("Land", 0.1f, 0, 0.3f);
//				m_AnmMgr.ChainAnimation(new AnmData("Rising_P", 0.1f, 0, 1));
//				damage = data._attack;
//				hp += damage;
//			}
//			break;
//			case AnimationType.NONE:{
//				m_AnmMgr.ChangeAnimationLoop("Idle", 0.1f, 0);
//				m_AnmMgr.SetAnmState(AnmState.END);
//			}
//			break;
//		}

//		GameManager.m_Enemy.transform.GetComponent<EnemyController>().hp -= damage;
//	}

//    // シャンプバトル
//    public IEnumerator JampBattle()
//    {
//        // 時間
//        float time = 1.0f;

//        // 移動
//        float angle = Mathf.Acos(Vector3.Dot(transform.up, GameManager.m_Enemy.transform.up)) * Mathf.Rad2Deg;
//        float arc = (2 * Mathf.PI * m_Planet.transform.localScale.y * 0.5f) * (angle / 360.0f);
//        Vector3 targetVel = (GameManager.m_Enemy.transform.position + GameManager.m_Enemy.transform.right) - transform.position;
//        Quaternion baseRotate = transform.rotation;

//        // ジャンプ
//        float height = BattleManager._instance.DBG_ACTION_JAMP;
//        float distance = Mathf.Sqrt(height);

//		// 止めるtを計算
//		float stop_t = 0.0f;
//		if (arc > 3.0f){
//			float x = (arc * 0.5f) - 1.5f;
//			stop_t = x / arc;
//		}
		
//        float t = 0.0f;

//        while(t < 1f)
//        {
//            t += (Time.deltaTime / time);
//            t = Mathf.Min(t, 1.0f);

//            // 移動
//            transform.rotation = baseRotate;
//            float val = Mathf.Lerp(0.0f, arc, t);
//            Vector3 front = Vector3.ProjectOnPlane(targetVel, transform.up).normalized;

//            // ジャンプ
//            float x = Mathf.Lerp(0.0f, 2*distance, t);
//            float jamp = -(Mathf.Pow(x - distance, 2)) + height;

//            GrgrMove(front * val, jamp);

//            // アクション
//            if (t > stop_t && !m_IsBattleAnmPlay)
//            {
//                if (!m_IsBattleAnmStart)
//                {
//                    m_IsBattleAnmStart = true;

//                }
//                while (!m_IsBattleAnmPlay)
//                {
//                    BattleResultUpdate();
//                    yield return null;
//                }
//            }

//            yield return null;
//        }

//        m_IsBattleAnmStart = false;
//        m_IsBattleAnmPlay = false;

//        rigidbody.velocity = transform.forward * rigidbody.GetSpeed();

//        BattleManager._instance.battlePlayflags[0] = true;
//    }

//	// 正面衝突バトル
//	public IEnumerator FrontBattle(){
//		Vector3 toEnemy = GameManager.m_Enemy.transform.position - transform.position;
//		Vector3 front = Vector3.ProjectOnPlane(toEnemy, transform.up).normalized;
//		transform.rotation = Quaternion.LookRotation(front, transform.up);
//		rigidbody.velocity = front * rigidbody.GetSpeed();

//		while(!m_IsBattleAnmStart){
//			float distance = Vector3.Distance(rigidbody.prevPosition, GameManager.m_Enemy.transform.GetComponent<EnemyController>().rigidbody.prevPosition);
//			if (distance <= BattleManager._instance.DBG_PLAY_DISTANCE){
//				m_IsBattleAnmStart = true;
//			}
//			else{
//				rigidbody.velocity = Vector3.ProjectOnPlane(rigidbody.velocity, transform.up).normalized * rigidbody.GetSpeed();
//				GrgrMove(rigidbody.velocity * Time.deltaTime, 0.0f);
//				yield return null;
//			}
//		}

//		while(!m_IsBattleAnmPlay){
//			BattleResultUpdate();
//			yield return null;
//		}

//		m_IsBattleAnmStart = false;
//		m_IsBattleAnmPlay = false;
//		BattleManager._instance.battlePlayflags[0] = true;
//	}

	
//#endregion
//}
