using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnimationManager))]
public class CharState : StateBase {

	// ギア加速クラス
	private class GearAccele{
		private float GEAR_INTERVAL = 5.0f;

		// ギア状態
		private enum Gear{
			FIRST,	// ギア1
			SECOND,	// ギア2
			THIRD,	// ギア3
		}
		// Brain
		private CharBrain m_Brain;
		// ギア状態
		private Phase<Gear> m_Gear = new Phase<Gear>();
		// 加速ベクトル
		private Vector3 m_AccelVel;

		// 初期化
		public GearAccele(CharBrain brain){
			m_Brain = brain;

			m_Gear.Change(Gear.FIRST);
			m_Gear.Start();
		}

		public void GearInitialize(Vector3 accelVel){
			m_AccelVel = accelVel;

			float speed = accelVel.magnitude;
			Gear gear = Gear.FIRST;
			if (speed >= m_Brain.GetInfo().GEAR_THIRD_BASE){
				gear = Gear.THIRD;
			}
			else if (speed >= m_Brain.GetInfo().GEAR_SECOND_BASE){
				gear = Gear.SECOND;
			}

			m_Gear.Change(gear);
		}

		public void GearFinalize(){
			m_AccelVel = Vector3.zero;
			m_Gear.Change(Gear.FIRST);
			m_Gear.Start();
		}

		// 加速ベクトル取得
		public Vector3 GetAcceleVel(){
			return m_AccelVel;
		}

		// フリック移動更新
		public void FlickMoveUpdate(){
			// フリックベクトルを加速ベクトルに加算
			AddFlickVel(m_Brain.GetInputVelocity());

			// ギア更新
			m_Gear.Update();
		}

		// フリックベクトルを加速ベクトルに加算
		private void AddFlickVel(Vector3 originVel){
			Vector3 velocity = originVel;
			float flickPow = velocity.magnitude;

			// 入力ベクトルを調整
			// ベクトルをカメラ回転で回転
			velocity = m_Brain.GetLookBase().rotation * velocity;
			// ベクトルをプレイヤーの上軸と垂直になるよう修正
			velocity = Vector3.ProjectOnPlane(velocity, m_Brain.transform.up).normalized;

			// 加速ベクトルの方向設定
			Vector3 front;
			// フリックの大きさが0より大きい
			if (flickPow > Vector3.kEpsilon){
				front = velocity;
			}
			else{
				front = m_Brain.transform.forward;
			}

			// ギアの段階から加速度を設定
			// フリックの強さから比率を計算
			float flickPowPer = flickPow / 1080.0f;
			float allSpeed = m_AccelVel.magnitude;
			switch(m_Gear.current){
				// ギア1
				case Gear.FIRST: {
					allSpeed += (flickPowPer * m_Brain.GetInfo().GEAR_FIRST_ACCELE);
					float maxSpeed = m_Brain.GetInfo().GEAR_SECOND_BASE;
					// ギアアップ
					if (allSpeed >= maxSpeed){
						m_Gear.Change(Gear.SECOND);
						m_Gear.Start();
					}
					// 速度制限
					allSpeed = Mathf.Min(allSpeed, maxSpeed);
				}
				break;
				// ギア2
				case Gear.SECOND: {
					allSpeed += (flickPowPer * m_Brain.GetInfo().GEAR_SECOND_ACCELE);
					float maxSpeed = m_Brain.GetInfo().GEAR_THIRD_BASE;
					float minSpeed = m_Brain.GetInfo().GEAR_SECOND_BASE;
					// ギアアップ
					if (allSpeed >= maxSpeed){
						m_Gear.Change(Gear.THIRD);
						m_Gear.Start();
					}
					else if (m_Gear.phaseTime >= GEAR_INTERVAL && allSpeed < minSpeed){
						m_Gear.Change(Gear.FIRST);
						m_Gear.Start();
					}
					// 速度制限
					allSpeed = Mathf.Min(allSpeed, maxSpeed);
				}
				break;
				// ギア3
				case Gear.THIRD: {
					allSpeed += (flickPowPer * m_Brain.GetInfo().GEAR_THIRD_ACCELE);
					float maxSpeed = m_Brain.GetInfo().FLICK_MAX_SPEED;
					float minSpeed = m_Brain.GetInfo().GEAR_THIRD_BASE;
					// ギア変化可能
					if (m_Gear.phaseTime >= GEAR_INTERVAL){
						// ギアダウン
						if (allSpeed < minSpeed){
							m_Gear.Change(Gear.SECOND);
							m_Gear.Start();
						}
					}
					//　速度制限
					allSpeed = Mathf.Min(allSpeed, maxSpeed);
				}
				break;
			}

			// 摩擦軽減処理
			// 最高速度を1として現在の速度との比を計算
			float t = Mathf.Clamp(allSpeed / m_Brain.GetInfo().FLICK_MAX_SPEED, 0.0f, 1.0f);
			// 摩擦の計算
			float friction = (float)UtilityMath.OutExp(t, 1.0f, m_Brain.GetInfo().MIN_FRICTION, m_Brain.GetInfo().MAX_FRICTION);
			// 摩擦を考慮して計算
			allSpeed *= (1-friction);

			// 最低速度制限
			if (allSpeed < 1.0f){
				allSpeed = 0;
			}
			
			// 加速ベクトル設定
			m_AccelVel = front * allSpeed;
		}
	}
	// ダウンクラス
	private class Down{
		private float DOWN_TIME = 2.0f;

		private float m_Timer = 0.0f;
		private CharBrain m_Brain;

		public Down(CharBrain brain){
			m_Brain = brain;
		}

		public void DownUpdate(){
			m_Timer += Time.deltaTime;

			Vector3 front = -m_Brain.transform.forward * 20.0f;
			m_Brain.SetInputVelocity(front);
			m_Brain.SetInputFront(m_Brain.transform.forward);

			if (m_Timer >= DOWN_TIME){
				m_Brain.GetState().UnLock();
				m_Brain.SetNextState(CharState.State.STOP, false);
				m_Brain.SetHP(m_Brain.GetInfo().MAX_HP);
				m_Timer = 0.0f;
			}
		}
	}

	// 状態
	public enum State{
		STOP,			// 立ち止まり
		DRAG_MOVE,		// ドラッグ移動
		FLICK_MOVE,		// フリック移動
		BATTLE,			// バトル
		DOWN,			// ダウン
		NONE,
	}

	//  頭脳コンポーネント参照
	[SerializeField] protected CharBrain m_Brain;
	// アニメーションマネージャー
	[SerializeField] protected AnimationManager m_AnmMgr;
	// スキルマネージャー
	private SkillDataManagerBase m_SkillDataManager;
	
	// キャラクター状態
	private Phase<State> m_State = new Phase<State>();
	// 状態ロックフラグ
	private bool m_StateLock;
	// 状態変更時終了処理
	private System.Action m_StateFinalize;

	// バトルクラス
	private BattleBase m_Battle;
	// ギア加速クラス
	private GearAccele m_GearAccle;
	// ダウンクラス
	private Down m_Down;


	public void Awake(){
		m_SkillDataManager = new DeckManager();
	}

	// 初期化
	public override void StateIni(){
		StateChange(State.STOP);
		m_StateLock = false;
		m_GearAccle = new GearAccele(m_Brain);
		m_Down = new Down(m_Brain);

		m_Battle = new SkillBattle(m_Brain.GetCore().m_BattleManager, m_Brain);
		m_Battle.BattleIni();
	}

	// 更新
	public override void StateUpdate(){

		// 状態別
		switch(m_State.current){
			// 立ち止まり
			case State.STOP:{
				if (m_State.IsFirst()){
					m_Brain.SetInputVelocity(Vector3.zero);
					m_AnmMgr.ChangeAnimationLoopInFixedTime("Idle");
				}
			}
			break;
			// ドラッグ移動
			case State.DRAG_MOVE:{
			}
			break;
			// フリック移動
			case State.FLICK_MOVE:{
				if (m_State.IsFirst()){
					m_AnmMgr.ChangeAnimationLoopInFixedTime("Run");
					m_GearAccle.GearInitialize(m_Brain.GetInfo().m_CurrentVelocity);
					m_StateFinalize = () =>{ m_GearAccle.GearFinalize(); };
				}

				m_GearAccle.FlickMoveUpdate();

				if(m_GearAccle.GetAcceleVel().magnitude <= Vector3.kEpsilon){
					m_Brain.SetNextState(State.STOP, false);
					m_Brain.SetInputVelocity(Vector3.zero);
					return;
				}

				m_Brain.SetInputVelocity(m_GearAccle.GetAcceleVel());
			}
			break;
			// バトル
			case State.BATTLE:{
				if (m_State.IsFirst()){
					m_StateFinalize = () => { ((DeckManager)m_SkillDataManager).DeckReset(); };
				}

				m_Battle.BattleUpdate();
			}
			break;
			// ダウン
			case State.DOWN:{
				if (m_State.IsFirst()){
					m_AnmMgr.ChangeAnimationInFixedTime("Damage_Down");
				}

				m_Down.DownUpdate();
			}
			break;
		}

		m_State.Update();
	}

	// 現在の移動量取得
	public Vector3 GetCurrentVelocity(){
		return m_GearAccle.GetAcceleVel();
	}

	// 状態変更
	public void StateChange(State state){
		if (m_StateLock == true){
			return;
		}
		
		if (m_StateFinalize != null){
			m_StateFinalize();
			m_StateFinalize = null;
		}
		m_State.Change(state);
		m_State.Start();
	}
	
	// ロックをする
	public void Lock(){
		m_StateLock = true;
	}
	// ロックを解除
	public void UnLock(){
		m_StateLock = false;
	}

	// ロック状態確認
	public bool IsLock(){
		return m_StateLock;
	}

	// 状態取得
	public State GetState(){
		return m_State.current;
	}

	// アニメーションマネージャー取得
	public AnimationManager GetAnmMgr(){
		return m_AnmMgr;
	}

	// スキルマネージャー取得
	public SkillDataManagerBase GetSkillDataManager(){
		return m_SkillDataManager;
	}

	// バトルクラスを取得
	public BattleBase GetBattle(){
		return m_Battle;
	}
}
