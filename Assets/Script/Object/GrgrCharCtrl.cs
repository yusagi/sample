// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// using UnityEngine.UI;

// public class GrgrCharCtrl : MonoBehaviour
// {

//     #region enum 

//     public enum State
//     {
//         STOP,
//         FLICK_MOVE,
//         DRAG_MOVE,
//         BATTLE,
//         ASCENSION,
//     }

//     enum Gear
//     {
//         First,
//         Second,
//         Third
//     }

//     #endregion

//     #region 定数

//     // ビタ止め
//     public float MOVE_FRICK_STOP_ANGLE = 0; // フリックビタ止め条件値
//     public float MOVE_TOUCH_STOP_TIME = 1;  // タッチビタ止め条件値

//     // ASCENSION時間
//     private float ASCENSION_TIME = 3.0f;

//     // ドラッグ条件値
//     public float DRAG_PERMISSION = 10.0f;

//     // 地上からの高さ調整
//     public float HEIGHT_FROM_GROUND = -0.6f;

//     // ギア変化インターバル
//     public float GEAR_CHANGE_INTERVAL = 1.0f;
//     #endregion

//     #region メンバ変数

//     private GameObject m_Planet;

//     // 移動
//     public float minFriction = 0.001f;  // 最小摩擦力
//     public float maxFriction = 0.01f;   // 最大摩擦力

//     public float dragSpeed = 10.0f;         // ドラッグ移動速度

//     public float gearSecondBase = 30.0f;    // ギア2突入条件値
//     public float gearThirdBase = 40.0f;     // ギア3突入条件値
//     public float gearFirstAccele = 20.0f;   // ギア1加速度	
//     public float gearSecondAccele = 5.0f;   // ギア2加速度
//     public float gearThirdAccele = 2.0f;    // ギア3加速度
//     private float m_GearChangeInterval;     // ギア変化インターバル

//     public float maxSpeed = 50.0f;			// 最高速度
//     // HP
//     public int hp = 100;
//     // AP
//     public float ap { get; set; }
//     // コンポーネント
//     public AnimationManager m_AnmMgr { get; set; }// アニメーター
//                                                   // 付属クラス
//     public Rigidbody_grgr rigidbody { get; set; }
//     public SkillManager skillManager { get; set; }
//     // 状態
//     public Phase<State> state = new Phase<State>();
//     private Gear gear;  // ギア
    
//     // ASCENSION用
//     private float ascensionTimer = 0;
//     // コルーチン
//     IEnumerator<Quaternion> qLerp;
//     public IEnumerator m_Battle { get; set; }

//     // 外部入力
//     public TouchType m_TouchType { get; set; }
//     public Vector3 m_CurrentVelocity { get; set; }
//     public Quaternion m_CurrentRotate { get; set; }
//     public bool m_TouchStopFlag { get; set; }

//     // デバグ用
//     public State dbg_State;
//     public AnmState dbg_AnmState;
//     public BattleManager.ResultPhase dbg_BattleState;

//     #endregion

//     #region Unity関数

//     void Awake()
//     {
//         //m_Planet = GameManager.m_Planet;

//         // スキルマネージャー初期化
//         skillManager = new SkillManager();

//         // grgrリジッドボディ初期化
//         rigidbody = new Rigidbody_grgr(transform);
//         rigidbody.isMove = false;

//         // アニメーションマネージャーコンポーネント取得
//         m_AnmMgr = GetComponent<AnimationManager>();

//         // ギア初期化
//         gear = Gear.First;

//         // キャラクター状態初期化
//         state.Change(State.STOP);

//         // 座標初期化
//         transform.position = Rigidbody_grgr.RotateToPosition(transform.up, m_Planet.transform.position, m_Planet.transform.localScale.y * 0.5f, HEIGHT_FROM_GROUND);

//         // ギア変化インターバル初期化
//         m_GearChangeInterval = 0.0f;

//         // 外部入力変数初期化
//         m_TouchType = TouchType.None;
//         m_CurrentVelocity = Vector3.zero;
//         m_TouchStopFlag = false;
//         m_CurrentRotate = Quaternion.identity;
//     }

//     // Use this for initialization
//     void Start()
//     {
//         m_AnmMgr.ChangeAnimationLoopInFixedTime("Idle");
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         dbg_State = state.current;
//         dbg_AnmState = m_AnmMgr.GetState();
//         dbg_BattleState = GameData.GetBattleManager().m_ResultPhase.current;

//         // 最大速度を変更
//         rigidbody.maxVelocitySpeed = maxSpeed;

//         // 前フレーム情報を設定
//         rigidbody.prevPosition = transform.position;
//         rigidbody.prevVelocity = rigidbody.velocity;

//         // 状態別更新
//         CharactorUpdata();
//     }

//     #endregion

//     #region 状態別更新
//     void CharactorUpdata()
//     {
//         state.Start();
//         switch (state.current)
//         {
//             // ストップ状態
//             case State.STOP:
//                 {
//                     if (state.IsFirst())
//                     {
//                         transform.position = Rigidbody_grgr.RotateToPosition(transform.up, m_Planet.transform.position, m_Planet.transform.localScale.y * 0.5f, HEIGHT_FROM_GROUND);
//                         m_AnmMgr.ChangeAnimationLoopInFixedTime("Idle");
//                     }

//                     // ドラッグ移動条件
//                     if (m_TouchType == TouchType.Drag)
//                     {
//                         Vector3 velocity = DragVelocity();
//                         GrgrMove(velocity * Time.deltaTime, 0.0f);
//                         state.Change(State.DRAG_MOVE);
//                     }
//                     // フリック移動条件
//                     else if (m_TouchType == TouchType.Frick)
//                     {
//                         Vector3 velocity = FlickVelocity();
//                         FlickMove(velocity);
//                         state.Change(State.FLICK_MOVE);
//                     }
//                 }
//                 break;
//             // ドラッグ移動状態
//             case State.DRAG_MOVE:
//                 {
//                     Vector3 velocity = DragVelocity();

//                     // 現在のドラッグ移動量
//                     float mVal = velocity.magnitude;

//                     // ストップ移動条件
//                     if (mVal < UtilityMath.epsilon)
//                     {
//                         rigidbody.velocity = Vector3.zero;
//                         rigidbody.friction = maxFriction;
//                         gear = Gear.First;
//                         state.Change(State.STOP);
//                         return;
//                     }

//                     GrgrMove(velocity * Time.deltaTime, 0.0f);
//                 }
//                 break;
//             // フリック移動状態
//             case State.FLICK_MOVE:
//                 {
//                     if (state.IsFirst())
//                     {
//                         m_AnmMgr.ChangeAnimationLoopInFixedTime("Run");
//                     }

//                     Vector3 velocity = FlickVelocity();
//                     // 現在フレームの移動量
//                     float s = (velocity.magnitude + rigidbody.GetSpeed()) * Time.deltaTime;
//                     // ストップ移動条件
//                     if (s < UtilityMath.epsilon || IsMoveStop())
//                     {
//                         rigidbody.velocity = Vector3.zero;
//                         rigidbody.friction = maxFriction;
//                         gear = Gear.First;
//                         state.Change(State.STOP);
//                         m_TouchStopFlag = false;
//                         return;
//                     }
//                     FlickMove(velocity);
//                 }
//                 break;
//             // 昇天
//             case State.ASCENSION:
//                 {
//                     Ascension();
//                 }
//                 break;
//             // バトル状態
//             case State.BATTLE:
//                 {
//                     BattleUpdate();
//                 }
//                 break;
//         }
//         state.Update();
//     }
//     #endregion

//     #region 回転
//     // UPのベクトルを元に向きを回転
//     public void GrgrRotateUP(Vector3 up){
//         Vector3 front = Vector3.ProjectOnPlane(transform.forward, up);
//         transform.rotation = Quaternion.LookRotation(front, up);
//     }
//     #endregion

//     #region ドラッグ移動
//     // ドラッグ移動量
//     Vector3 DragVelocity()
//     {

//         if (m_CurrentVelocity.magnitude < DRAG_PERMISSION)
//         {
//             return Vector3.zero;
//         }

//         Vector3 velocity;
//         velocity = m_CurrentRotate * m_CurrentVelocity.normalized;
//         velocity = Vector3.ProjectOnPlane(velocity, transform.up).normalized;

//         return velocity * dragSpeed;
//     }
//     #endregion

//     #region 昇天
//     void AscensionIni(Vector3 impact)
//     {
//         rigidbody.isMove = true;
//         rigidbody.velocity = impact;
//         ascensionTimer = ASCENSION_TIME;
//         m_AnmMgr.ChangeAnimationInFixedTime("Damage_Down", "Idle");
//     }

//     void Ascension()
//     {
//         ascensionTimer -= Time.deltaTime;
//         if (ascensionTimer < 0)
//         {
//             rigidbody.isMove = false;
//             rigidbody.velocity = Vector3.zero;
//             transform.rotation = Random.rotation;
//             hp = 100;
//             state.Change(State.STOP);
//         }
//         rigidbody.Update();
//     }
//     #endregion

//     #region 衝突処理
//     void OnCollisionStay(Collision other)
//     {
//         // 柱との衝突処理
//         if (other.gameObject.tag == "Piller")
//         {
//             if (other.collider.GetComponent<PillerDeadTime>().isDead)
//                 return;


//             other.collider.GetComponent<PillerDeadTime>().isDead = true;
//             Vector3 impact = (other.collider.transform.up + rigidbody.velocity);
//             other.collider.GetComponent<PillerController>().rigidbody.AddForce(impact * 0.1f);
//             return;
//         }
//     }
//     #endregion

//     #region ビタ止め
//     // ビタ止めフラグ関数
//     bool IsMoveStop()
//     {
//         return (rigidbody.GetSpeed() > UtilityMath.epsilon) && (m_TouchStopFlag || IsMoveFrickStop());
//     }

//     // フリックビタ止めフラグ関数
//     bool IsMoveFrickStop()
//     {
//         Vector3 frickVel = m_CurrentVelocity;
//         Vector3 backVel = new Vector3(0, 0, -1);
//         float dot = Vector3.Dot(backVel.normalized, frickVel.normalized);
//         float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
//         return angle < MOVE_FRICK_STOP_ANGLE;
//     }
//     #endregion

// }