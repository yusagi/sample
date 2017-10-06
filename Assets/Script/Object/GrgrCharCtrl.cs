using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class GrgrCharCtrl : MonoBehaviour
{

    #region enum 

    public enum State
    {
        STOP,
        FLICK_MOVE,
        DRAG_MOVE,
        BATTLE,
        ASCENSION,
    }

    enum Gear
    {
        First,
        Second,
        Third
    }

    #endregion

    #region 定数

    // ビタ止め
    public float MOVE_FRICK_STOP_ANGLE = 0; // フリックビタ止め条件値
    public float MOVE_TOUCH_STOP_TIME = 1;  // タッチビタ止め条件値

    // ASCENSION時間
    private float ASCENSION_TIME = 3.0f;

    // ドラッグ条件値
    public float DRAG_PERMISSION = 10.0f;

    // 地上からの高さ調整
    public float HEIGHT_FROM_GROUND = -0.6f;

    // ギア変化インターバル
    public float GEAR_CHANGE_INTERVAL = 1.0f;
    #endregion

    #region メンバ変数

    private GameObject m_Planet;

    // 移動
    public float minFriction = 0.001f;  // 最小摩擦力
    public float maxFriction = 0.01f;   // 最大摩擦力

    public float dragSpeed = 10.0f;         // ドラッグ移動速度

    public float gearSecondBase = 30.0f;    // ギア2突入条件値
    public float gearThirdBase = 40.0f;     // ギア3突入条件値
    public float gearFirstAccele = 20.0f;   // ギア1加速度	
    public float gearSecondAccele = 5.0f;   // ギア2加速度
    public float gearThirdAccele = 2.0f;    // ギア3加速度
    private float m_GearChangeInterval;     // ギア変化インターバル

    public float maxSpeed = 50.0f;			// 最高速度
    // HP
    public int hp = 100;
    // AP
    public float ap { get; set; }
    // コンポーネント
    public AnimationManager m_AnmMgr { get; set; }// アニメーター
                                                  // 付属クラス
    public Rigidbody_grgr rigidbody { get; set; }
    public SkillManager skillManager { get; set; }
    // 状態
    public Phase<State> state = new Phase<State>();
    private Gear gear;  // ギア
    
    // ASCENSION用
    private float ascensionTimer = 0;
    // コルーチン
    IEnumerator<Quaternion> qLerp;
    public IEnumerator m_Battle { get; set; }

    // バトルシステム
    System.Action m_BattleSystem;

    // 外部入力
    public TouchType m_TouchType { get; set; }
    public Vector3 m_CurrentVelocity { get; set; }
    public Quaternion m_CurrentRotate { get; set; }
    public bool m_TouchStopFlag { get; set; }

    // デバグ用
    public State dbg_State;
    public AnmState dbg_AnmState;
    public BattleManager.ResultPhase dbg_BattleState;

    #endregion

    #region Unity関数

    void Awake()
    {
        m_Planet = GameManager.m_Planet;

        // スキルマネージャー初期化
        skillManager = new SkillManager();

        // grgrリジッドボディ初期化
        rigidbody = new Rigidbody_grgr(transform);
        rigidbody.isMove = false;

        // アニメーションマネージャーコンポーネント取得
        m_AnmMgr = GetComponent<AnimationManager>();

        // ギア初期化
        gear = Gear.First;

        // キャラクター状態初期化
        state.Change(State.STOP);

        // 座標初期化
        transform.position = Rigidbody_grgr.RotateToPosition(transform.up, m_Planet.transform.position, m_Planet.transform.localScale.y * 0.5f, HEIGHT_FROM_GROUND);

        // ギア変化インターバル初期化
        m_GearChangeInterval = 0.0f;

        // バトルシステム初期化
        m_BattleSystem = null;

        // 外部入力変数初期化
        m_TouchType = TouchType.None;
        m_CurrentVelocity = Vector3.zero;
        m_TouchStopFlag = false;
        m_CurrentRotate = Quaternion.identity;
    }

    // Use this for initialization
    void Start()
    {
        m_AnmMgr.ChangeAnimationLoopInFixedTime("Idle");
    }

    // Update is called once per frame
    void Update()
    {
        dbg_State = state.current;
        dbg_AnmState = m_AnmMgr.GetState();
        dbg_BattleState = GameData.GetBattleManager().m_ResultPhase.current;

        // 最大速度を変更
        rigidbody.maxVelocitySpeed = maxSpeed;

        // 前フレーム情報を設定
        rigidbody.prevPosition = transform.position;
        rigidbody.prevVelocity = rigidbody.velocity;

        // 状態別更新
        CharactorUpdata();
    }

    #endregion

    #region 状態別更新
    void CharactorUpdata()
    {
        state.Start();
        switch (state.current)
        {
            // ストップ状態
            case State.STOP:
                {
                    if (state.IsFirst())
                    {
                        transform.position = Rigidbody_grgr.RotateToPosition(transform.up, m_Planet.transform.position, m_Planet.transform.localScale.y * 0.5f, HEIGHT_FROM_GROUND);
                        m_AnmMgr.ChangeAnimationLoopInFixedTime("Idle");
                    }

                    // ドラッグ移動条件
                    if (m_TouchType == TouchType.Drag)
                    {
                        Vector3 velocity = DragVelocity();
                        GrgrMove(velocity * Time.deltaTime, 0.0f);
                        state.Change(State.DRAG_MOVE);
                    }
                    // フリック移動条件
                    else if (m_TouchType == TouchType.Frick)
                    {
                        Vector3 velocity = FlickVelocity();
                        FlickMove(velocity);
                        state.Change(State.FLICK_MOVE);
                    }
                }
                break;
            // ドラッグ移動状態
            case State.DRAG_MOVE:
                {
                    Vector3 velocity = DragVelocity();

                    // 現在のドラッグ移動量
                    float mVal = velocity.magnitude;

                    // ストップ移動条件
                    if (mVal < UtilityMath.epsilon)
                    {
                        rigidbody.velocity = Vector3.zero;
                        rigidbody.friction = maxFriction;
                        gear = Gear.First;
                        state.Change(State.STOP);
                        return;
                    }

                    GrgrMove(velocity * Time.deltaTime, 0.0f);
                }
                break;
            // フリック移動状態
            case State.FLICK_MOVE:
                {
                    if (state.IsFirst())
                    {
                        m_AnmMgr.ChangeAnimationLoopInFixedTime("Run");
                    }

                    Vector3 velocity = FlickVelocity();
                    // 現在フレームの移動量
                    float s = (velocity.magnitude + rigidbody.GetSpeed()) * Time.deltaTime;
                    // ストップ移動条件
                    if (s < UtilityMath.epsilon || IsMoveStop())
                    {
                        rigidbody.velocity = Vector3.zero;
                        rigidbody.friction = maxFriction;
                        gear = Gear.First;
                        state.Change(State.STOP);
                        m_TouchStopFlag = false;
                        return;
                    }
                    FlickMove(velocity);
                }
                break;
            // 昇天
            case State.ASCENSION:
                {
                    Ascension();
                }
                break;
            // バトル状態
            case State.BATTLE:
                {
                    BattleUpdate();
                }
                break;
        }
        state.Update();
    }
    #endregion

    #region 回転
    // UPのベクトルを元に向きを回転
    public void GrgrRotateUP(Vector3 up){
        Vector3 front = Vector3.ProjectOnPlane(transform.forward, up);
        transform.rotation = Quaternion.LookRotation(front, up);
    }
    #endregion

    #region 移動

    // 球体ぐるぐる移動
    public void GrgrMove(Vector3 velocity, float jamp, bool constVel = false)
    {
        // 移動量を円弧とし、角度を求めて移動する
        if (velocity.magnitude > Vector3.kEpsilon)
        {
            float arc = velocity.magnitude;
            float angle = arc / (2.0f * Mathf.PI * m_Planet.transform.transform.localScale.y * 0.5f) * 360.0f;
            // 向きを変えずに移動する
            if (constVel){
                Vector3 tmpFront = transform.forward;
                transform.rotation = Quaternion.LookRotation(velocity.normalized, transform.up);
                transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;
                Vector3 front = Vector3.ProjectOnPlane(tmpFront, transform.up);
                transform.rotation = Quaternion.LookRotation(front, transform.up);
            }
            else{
                transform.rotation = Quaternion.LookRotation(velocity.normalized, transform.up);
                transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;
            }
        }
        transform.position = Rigidbody_grgr.RotateToPosition(transform.up, m_Planet.transform.position, m_Planet.transform.localScale.y * 0.5f, HEIGHT_FROM_GROUND + jamp);
    }

    #region フリック移動
    // フリック移動処理
    void FlickMove(Vector3 velocity)
    {
        // フリックをgrgrリジッドボディの移動量に加算
        {
            FrickAddForce(velocity);
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

            rigidbody.friction = (float)friction * Time.deltaTime;
        }
        GrgrMove(rigidbody.velocity * Time.deltaTime, 0.0f);

        rigidbody.Update();
    }

    // フリック移動量
    Vector3 FlickVelocity()
    {
        if (m_TouchType != TouchType.Frick)
        {
            return Vector3.zero;
        }

        if (m_CurrentVelocity.magnitude < Vector3.kEpsilon)
        {
            return Vector3.zero;
        }

        float speed = 0.0f;
        // ギアの段階に応じた加速度を設定
        {
            switch (gear)
            {
                case Gear.First: speed = gearFirstAccele; break;
                case Gear.Second: speed = gearSecondAccele; break;
                case Gear.Third: speed = gearThirdAccele; break;
            }
        }

        // 移動ベクトル作成
        {
            Vector3 velocity = m_CurrentRotate * m_CurrentVelocity;

            float tmp = Mathf.Clamp(velocity.magnitude, 0.0f, 1080);
            tmp = Mathf.Clamp(tmp / 1080, 0.1f, 1.0f);

            return Vector3.ProjectOnPlane(velocity, transform.up).normalized * tmp * speed;
        }
    }

    // 球体ぐるぐるrigidbody.velocity加算
    void FrickAddForce(Vector3 velocity)
    {
        // リジッドボディのベクトルを変更
        Vector3 front;
        // フリック方向
        if (velocity.magnitude > UtilityMath.epsilon)
        {
            front = velocity.normalized;
        }
        // リジッドボディのベクトルをプレイヤーの立つ平面に投射したベクトル
        else
        {
            front = Vector3.ProjectOnPlane(rigidbody.velocity, transform.up).normalized;
        }
        rigidbody.velocity = front * rigidbody.GetSpeed();
        rigidbody.AddForce(velocity);
    }
    #endregion

    #region ドラッグ移動
    // ドラッグ移動量
    Vector3 DragVelocity()
    {

        if (m_CurrentVelocity.magnitude < DRAG_PERMISSION)
        {
            return Vector3.zero;
        }

        Vector3 velocity;
        velocity = m_CurrentRotate * m_CurrentVelocity.normalized;
        velocity = Vector3.ProjectOnPlane(velocity, transform.up).normalized;

        return velocity * dragSpeed;
    }
    #endregion

    #endregion

    #region 昇天
    void AscensionIni(Vector3 impact)
    {
        rigidbody.isMove = true;
        rigidbody.velocity = impact;
        ascensionTimer = ASCENSION_TIME;
        m_AnmMgr.ChangeAnimationInFixedTime("Damage_Down", "Idle");
    }

    void Ascension()
    {
        ascensionTimer -= Time.deltaTime;
        if (ascensionTimer < 0)
        {
            rigidbody.isMove = false;
            rigidbody.velocity = Vector3.zero;
            transform.rotation = Random.rotation;
            hp = 100;
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


            other.collider.GetComponent<PillerDeadTime>().isDead = true;
            Vector3 impact = (other.collider.transform.up + rigidbody.velocity);
            other.collider.GetComponent<PillerController>().rigidbody.AddForce(impact * 0.1f);
            return;
        }
    }
    #endregion

    #region ビタ止め
    // ビタ止めフラグ関数
    bool IsMoveStop()
    {
        return (rigidbody.GetSpeed() > UtilityMath.epsilon) && (m_TouchStopFlag || IsMoveFrickStop());
    }

    // フリックビタ止めフラグ関数
    bool IsMoveFrickStop()
    {
        Vector3 frickVel = m_CurrentVelocity;
        Vector3 backVel = new Vector3(0, 0, -1);
        float dot = Vector3.Dot(backVel.normalized, frickVel.normalized);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        return angle < MOVE_FRICK_STOP_ANGLE;
    }
    #endregion

    #region ギア

    // ギアの段階による速度制限
    void GearSpeedLimit()
    {
        switch (gear)
        {
            case Gear.First: rigidbody.velocity = rigidbody.velocity.normalized * Mathf.Min(rigidbody.GetSpeed(), gearSecondBase + 1.0f); break;
            case Gear.Second: rigidbody.velocity = rigidbody.velocity.normalized * Mathf.Min(rigidbody.GetSpeed(), gearThirdBase + 1.0f); break;
        }
    }

    // ギアの切り替え
    void GearChange()
    {
        if (m_GearChangeInterval > 0)
        {
            return;
        }

        Gear type;
        float speed = rigidbody.GetSpeed();
        type = Gear.First;
        if (speed >= gearSecondBase)
            type = Gear.Second;
        if (speed >= gearThirdBase)
            type = Gear.Third;

        if (gear != type)
        {
            gear = type;
            StartCoroutine(GearChangeIntervalUpdate());
        }
    }

    // ギア変更インターバル更新コルーチン
    IEnumerator GearChangeIntervalUpdate()
    {
        m_GearChangeInterval = GEAR_CHANGE_INTERVAL;
        while (m_GearChangeInterval > 0)
        {
            m_GearChangeInterval -= Time.deltaTime;
            yield return null;
        }
    }

    #endregion

    #region バトルモード
    // バトルモード更新
    void BattleUpdate()
    {
        if (state.IsFirst())
        {
            m_CurrentVelocity = Vector3.zero;
        }

        switch (GameData.GetBattleManager().m_Battle.current)
        {
            // エンカウント
            case BattleManager.Battle.BATTLE_ENCOUNT:{
                if (GameData.GetBattleManager().m_Battle.IsFirst()){
                        StartCoroutine(m_Battle);
                }
            }
            break;
            // // バトルスタート
            // case BattleManager.Battle.SKILL_BATTLE_START:
            //     {
            //         if (GameData.GetBattleManager().m_Battle.IsFirst())
            //         {
            //             return;
            //         }

            //         FrickAddForce(Vector3.zero);
            //         GrgrMove(rigidbody.velocity * Time.deltaTime, 0.0f);
            //     }
            //     break;
            // // スキルバトルプレイ
            // case BattleManager.Battle.SKILL_BATTLE_PLAY:
            //     {
            //         if (GameData.GetBattleManager().m_Battle.IsFirst())
            //         {

            //             StartCoroutine(m_Battle);

            //             return;
            //         }
            //     }
            //     break;
            // バトル終了
            case BattleManager.Battle.BATTLE_END:
                {
                    // ライフが0になったら
                    if (hp <= 0)
                    {
                        Vector3 impact = transform.up;
                        AscensionIni(impact);
                        state.Change(State.ASCENSION);
                    }
                    else
                    {
                        //animator.m_Animator.Play("Idle");
                        if (rigidbody.GetSpeed() > Vector3.kEpsilon)
                        {
                            state.Change(State.FLICK_MOVE);
                        }
                        else
                        {
                            state.Change(State.STOP);
                        }
                    }
                }
                break;
        }
    }

    #endregion

    #region スタック式バトル
    // バトル結果更新 
    void BattleResultUpdate(GameObject target)
    {
        BattleManager.ResultPhase phase = GameData.GetBattleManager().m_ResultPhase.current;
        switch (phase)
        {
            case BattleManager.ResultPhase.FIRST:
                {
                    if (GameData.GetBattleManager().m_ResultPhase.IsFirst())
                    {
                        BattleResultSetAnm(target, BattleBoardData.skillChoiceBoard, phase);
                    }
                }
                break;
            case BattleManager.ResultPhase.SECOND:
                {
                    if (GameData.GetBattleManager().m_ResultPhase.IsFirst())
                    {
                        BattleResultSetAnm(target, BattleBoardData.skillChoiceBoard, phase);
                    }
                }
                break;
            case BattleManager.ResultPhase.THIRD:
                {
                    if (GameData.GetBattleManager().m_ResultPhase.IsFirst())
                    {
                        BattleResultSetAnm(target, BattleBoardData.skillChoiceBoard, phase);
                    }
                }
                break;
            case BattleManager.ResultPhase.FOURTH:
                {
                    if (GameData.GetBattleManager().m_ResultPhase.IsFirst())
                    {
                        BattleResultSetAnm(target, BattleBoardData.skillChoiceBoard, phase);
                    }
                }
                break;
        }
    }

    // バトルリザルトセットアニメーション
    void BattleResultSetAnm(GameObject target, SkillChoiceBoardController controller, BattleManager.ResultPhase m_ResultPhase)
    {
        int damage = 0;
        AnimationType anmType = controller.GetResultData(gameObject, m_ResultPhase)._anmType;
        switch (anmType)
        {
            // 通常攻撃
            case AnimationType.ATTACK:
                {
                    SkillData data = controller.GetResultData(gameObject, m_ResultPhase)._skill;

                    m_AnmMgr.ChangeAnimationInFixedTime(data._anmName);

                    damage = data._attack;
                }
                break;
            // 防御
            case AnimationType.GUARD:
                {
                    // ガードの時は相手のタイミングを参照してアニメーションを再生する
                    AnmData data = m_AnmMgr.GetAnmData(controller.GetResultData(target, m_ResultPhase)._skill._anmName, "Common");
                    m_AnmMgr.ChangeAnmDataInFixedTime("Guard", -1, -1, -1, data._slowEndFrame, data._slowStartFrame);
                }
                break;
            // ダメージ
            case AnimationType.DAMAGE:
                    {
                        // ダメージの時は相手のタイミングを参照してアニメーションを再生する
                        StartCoroutine(damageAnm(target));
                    }
                    break;
            // アニメーションなし
            case AnimationType.NONE:
                {
                        m_AnmMgr.ChangeAnimationInFixedTime("Idle");
                }
                break;
        }

        target.GetComponent<GrgrCharCtrl>().hp -= damage;
    }

    // ダメージモーション時
    IEnumerator damageAnm(GameObject target){
        yield return null;  
        GrgrCharCtrl obj = target.GetComponent<GrgrCharCtrl>(); 
        while(obj.m_AnmMgr.GetState() == AnmState.CHANGE){
            yield return null;
        }
        while (obj.m_AnmMgr.GetPlayAnmData()._slowEndFrame >= obj.m_AnmMgr.GetFrame()){
            yield return null;
        }
        m_AnmMgr.ChangeAnimationInFixedTime("DAMAGED00");
    }

    // 正面衝突バトル
    public IEnumerator FrontBattle(Transform target)
    {
        yield return null;

        Vector3 toTarget = target.position - transform.position;
        Vector3 front = Vector3.ProjectOnPlane(toTarget, transform.up).normalized;
        transform.rotation = Quaternion.LookRotation(front, transform.up);
        rigidbody.velocity = front * rigidbody.GetSpeed();

        while (GameData.GetBattleManager().m_Battle.current == BattleManager.Battle.BATTLE_ENCOUNT || GameData.GetBattleManager().m_Battle.current == BattleManager.Battle.SKILL_BATTLE_START)
        {
            rigidbody.velocity = Vector3.ProjectOnPlane(rigidbody.velocity, transform.up).normalized * rigidbody.GetSpeed();
            GrgrMove(rigidbody.velocity * Time.deltaTime, 0.0f);
            yield return null;
        }

        while (GameData.GetBattleManager().m_Battle.current == BattleManager.Battle.SKILL_BATTLE_PLAY)
        {
            BattleResultUpdate(target.gameObject);
            yield return null;
        }
    }

    // 背面バトル
    public IEnumerator BackBattle(Transform target)
    {
        m_AnmMgr.ChangeAnimationLoopInFixedTime("Idle");
        yield return null;

        Vector3 toTarget = target.position - transform.position;
        Vector3 front = Vector3.ProjectOnPlane(toTarget, transform.up).normalized;
        transform.rotation = Quaternion.LookRotation(front, transform.up);

        while (GameData.GetBattleManager().m_Battle.current == BattleManager.Battle.BATTLE_ENCOUNT || GameData.GetBattleManager().m_Battle.current == BattleManager.Battle.SKILL_BATTLE_START)
        {
            yield return null;
        }

        while (GameData.GetBattleManager().m_Battle.current == BattleManager.Battle.SKILL_BATTLE_PLAY)
        {
            BattleResultUpdate(target.gameObject);
            yield return null;
        }
    }
#endregion

}